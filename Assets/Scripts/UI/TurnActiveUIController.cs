using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnActiveUIController : InstanceObject<TurnActiveUIController> 
{
    public Button RollDiceBtn;
    public Button AttackBtn;
    public GameObject PanelSkills;
    public Button CancelSkillBtn;
    public Button ChangeViewBtn;
    public Button SkipBtn;
    public Button UltimateSkillBtn;

    public List<ButtonGroup> BtnGroup = new List<ButtonGroup>();

    private PrototypeGameMode gameMode = null;

    private List<BaseSkill> SkillList = new List<BaseSkill>();

    public bool isActiveSkill = false;
    public bool isActiveAttack = false;
    public SkillDescriptionPanel descriptPanel;

    public void SetActivePanel(bool isEnable, TileType type)
    {
        if(gameMode.CurrentGameState == eStateGameMode.EndGame) return;
        if(!CheckingEnableButtonByStatus(currentPoring))
        {
            // if(descriptPanel.gameObject.activeInHierarchy) descriptPanel.ClosePanel();
            OnClickSkip();
            return;
        }
        RollDiceBtn.gameObject.SetActive(isEnable);
        if(type != TileType.Sanctuary)
        {
            AttackBtn.gameObject.SetActive(isEnable);
            PanelSkills.SetActive(isEnable);
            
            UltimateSkillBtn.interactable = (currentPoring.Property.UltimatePoint == 5 && !isActiveSkill);
            UltimateSkillBtn.gameObject.SetActive(isEnable);
        }
        else
        {
            AttackBtn.gameObject.SetActive(false);
            PanelSkills.SetActive(false);
            UltimateSkillBtn.gameObject.SetActive(false);
        }

        CancelSkillBtn.gameObject.SetActive(!isEnable);
        ChangeViewBtn.gameObject.SetActive(false);
        SkipBtn.gameObject.SetActive(false);

        if(!CheckingEnableButtonByStatus(currentPoring))
        {
            if(descriptPanel.gameObject.activeInHierarchy) descriptPanel.ClosePanel();
            OnClickSkip();
        }
    }

    public void NotMyTurn()
    {
        RollDiceBtn.gameObject.SetActive(false);
        AttackBtn.gameObject.SetActive(false);
        PanelSkills.SetActive(false);
        UltimateSkillBtn.gameObject.SetActive(false);
        CancelSkillBtn.gameObject.SetActive(false);
        SkipBtn.gameObject.SetActive(false);
        
        ChangeViewBtn.gameObject.SetActive(true);
    }

    private Poring currentPoring;
    public void ActiveCurrentPoringTurn(Poring poring, int index, PrototypeGameMode gameMode)
    {
        if (this.gameMode == null) this.gameMode = gameMode;
        currentPoring = poring;
        isActiveSkill = false;
        if(gameMode.IndexCurrentPlayer == PhotonNetwork.LocalPlayer.GetPlayerNumber())
        {
            SetActivePanel(true, currentPoring.Node.TileProperty.Type);
            SkillList = poring.Property.SkillList;
            SetEventToDiceRollBtn(poring, index);
            SetEventToAttackBtn(poring);
            SetSkillsEvent(SkillList, poring, index);
        }
        else
        {
            NotMyTurn();
            SetEventToChangeViewBtn();
        }
    }

    private bool CheckingEnableButtonByStatus(Poring poring)
    {
        // Skip turn.
        SkillStatus condition = SkillStatus.Sleep | SkillStatus.Freeze | SkillStatus.Stun;
        if(poring.CheckHasStatus(condition)) return false;

        // Can't move. 
        condition = SkillStatus.Root;
        if(poring.CheckHasStatus(condition))
        {
            RollDiceBtn.gameObject.SetActive(false);
            if(!CancelSkillBtn.gameObject.activeInHierarchy)
                SkipBtn.gameObject.SetActive(true);
        }

        return true;
    }

    private void SetEventToChangeViewBtn()
    {
        ChangeViewBtn.onClick.RemoveAllListeners();
        ChangeViewBtn.onClick.AddListener(() =>
        {
            var cam = CameraController.Instance;
            cam.Show((cam.CurrentType == CameraType.TopDown) ? CameraType.Action : CameraType.TopDown);
            // SetEventToChangeViewBtn((cameraType == CameraType.Default) ? CameraType.TopDown : CameraType.Default);
        });
    }

    private void SetEventToAttackBtn(Poring poring)
    {
        AttackBtn.onClick.RemoveAllListeners();
        AttackBtn.onClick.AddListener(() =>
        {
            isActiveAttack = true;
            CameraController.Instance.Show(CameraType.TopDown);

            gameMode.PhotonNetworkRaiseEvent(EventCode.HighlightNodeAttack, new object[] { poring.Property.AttackRange});

            SetActivePanel(false, currentPoring.Node.TileProperty.Type);
            CancelSkillBtn.onClick.RemoveAllListeners();
            CancelSkillBtn.onClick.AddListener(OnClickCancelAttack);
        });
    }

    private void SetEventToDiceRollBtn(Poring poring, int index)
    {
        RollDiceBtn.onClick.RemoveAllListeners();
        RollDiceBtn.onClick.AddListener(() =>
        {
            UltimateSkillBtn.gameObject.SetActive(false);
            BtnGroup.ForEach(group => group.BtnObject.gameObject.SetActive(false));    
            AttackBtn.gameObject.SetActive(false);
            RollDiceBtn.gameObject.SetActive(false);
            int result = UnityEngine.Random.Range(0,5);
            // Debug.LogError($"Move > >>>> >> > {result}");
            gameMode.PhotonNetworkRaiseEvent(EventCode.BeginRollMove, new object[] { index, result });

            RollDiceBtn.onClick.RemoveAllListeners();
        });
    }

    private void SetSkillsEvent(List<BaseSkill> skillList, Poring poring, int index)
    {
        BaseSkill ultimateSkill = poring.Property.UltimateSkill;
        UltimateSkillBtn.GetComponentInChildren<Text>().text = ultimateSkill.name;
        UltimateSkillBtn.GetComponent<SkillDetail>().SetDetail(ultimateSkill, poring);
        UltimateSkillBtn.onClick.RemoveAllListeners();
        UltimateSkillBtn.onClick.AddListener(() =>
        {
            SetActivePanel(false, currentPoring.Node.TileProperty.Type);
            ChangeModeToSelectTarget(ultimateSkill, poring, index);
        });

        BtnGroup.ForEach(group => {
            group.BtnObject.onClick.RemoveAllListeners();
            group.BtnObject.gameObject.SetActive(false);
        });

        for (int i = 0; i < skillList.Count; i++)
        {
            ButtonGroup group = BtnGroup[i];
            BaseSkill skill = skillList[i];

            skill.CurrentCD--;
            group.BtnName.text = skill.name;
            group.BtnObject.gameObject.SetActive(true);

            var skillDetail = group.BtnObject.gameObject.GetComponent<SkillDetail>();
            skillDetail.SetDetail(skill, poring);

            group.BtnObject.interactable = (skill.CurrentCD <= 0 && skill.SkillMode == SkillMode.Activate);

            if(skill.MoveToTarget && poring.CheckHasStatus(SkillStatus.Root))
            {
                group.BtnObject.interactable = false;
            }
            
            group.BtnObject.onClick.AddListener(() =>
            {
                BtnGroup.ForEach(g => {
                    g.BtnObject.interactable = false;
                });
                UltimateSkillBtn.interactable = false;
                SetActivePanel(false, currentPoring.Node.TileProperty.Type);
                ChangeModeToSelectTarget(skill, poring, index);
            });
        }
    }

    private void ChangeModeToSelectTarget(BaseSkill skill, Poring poring, int index)
    {
        isActiveSkill = true;
        CameraController.Instance.Show(CameraType.TopDown);

        gameMode.PhotonNetworkRaiseEvent(EventCode.HighlightNodeSkill, new object[]{ skill.name, index});

        CancelSkillBtn.onClick.RemoveAllListeners();
        CancelSkillBtn.onClick.AddListener(OnClickCancelSkill);
    }

    private void OnClickCancelSkill()
    {
        
        CancelSkillBtn.onClick.RemoveAllListeners();
        isActiveSkill = false;
        UltimateSkillBtn.interactable = (currentPoring.Property.UltimatePoint == 5);
        for (int i = 0; i < SkillList.Count; i++)
        {
            BtnGroup[i].BtnObject.interactable = (SkillList[i].CurrentCD <= 0 && SkillList[i].SkillMode == SkillMode.Activate);
        }
        
        CameraController.Instance.Show(CameraType.Default);
        SetActivePanel(true, currentPoring.Node.TileProperty.Type);

        gameMode.PhotonNetworkRaiseEvent(EventCode.OnClickCancel);
    }

    private void OnClickCancelAttack()
    {
        CancelSkillBtn.onClick.RemoveAllListeners();
        isActiveAttack = false;
        
        CameraController.Instance.Show(CameraType.Default);
        SetActivePanel(true, currentPoring.Node.TileProperty.Type);
        
        gameMode.PhotonNetworkRaiseEvent(EventCode.OnClickCancel);
        // gameMode.ResetNodeColor();
    }

    public void OnClickSkip()
    {
        BtnGroup.ForEach(group => group.BtnObject.gameObject.SetActive(false));    
        AttackBtn.gameObject.SetActive(false);
        RollDiceBtn.gameObject.SetActive(false);
        SkipBtn.gameObject.SetActive(false);
        gameMode.PhotonNetworkRaiseEvent(EventCode.SkipToEndTurn);
    }

    [System.Serializable]
    public struct ButtonGroup
    {
        public Text BtnName;
        public Button BtnObject;
    }
}
