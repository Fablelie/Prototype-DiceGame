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

    public List<ButtonGroup> BtnGroup = new List<ButtonGroup>();

    private PrototypeGameMode gameMode = null;

    private List<BaseSkill> SkillList = new List<BaseSkill>();

    public bool isActiveSkill = false;
    public bool isActiveAttack = false;

    public void SetActivePanel(bool isEnable)
    {
        RollDiceBtn.gameObject.SetActive(isEnable);
        AttackBtn.gameObject.SetActive(isEnable);
        PanelSkills.SetActive(isEnable);
        CancelSkillBtn.gameObject.SetActive(!isEnable);
        ChangeViewBtn.gameObject.SetActive(false);
        SkipBtn.gameObject.SetActive(false);

        if(!CheckingEnableButtonByStatus(currentPoring))
        {
            OnClickSkip();
        }
    }

    public void NotMyTurn()
    {
        RollDiceBtn.gameObject.SetActive(false);
        AttackBtn.gameObject.SetActive(false);
        PanelSkills.SetActive(false);
        CancelSkillBtn.gameObject.SetActive(false);
        ChangeViewBtn.gameObject.SetActive(true);
    }

    private Poring currentPoring;
    public void ActiveCurrentPoringTurn(Poring poring, int index, PrototypeGameMode gameMode)
    {
        if (this.gameMode == null) this.gameMode = gameMode;
        currentPoring = poring;
        if(gameMode.IndexCurrentPlayer == PhotonNetwork.LocalPlayer.GetPlayerNumber())
        {
            SetActivePanel(true);
            SkillList = poring.Property.SkillList;
            SetEventToDiceRollBtn(poring, index);
            SetEventToAttackBtn(poring);
            SetSkillsEvent(SkillList, poring, index);
        }
        else
        {
            NotMyTurn();
            SetEventToChangeViewBtn(CameraType.Default);
        }
    }

    private bool CheckingEnableButtonByStatus(Poring poring)
    {
        int input = poring.GetCurrentStatus();

        // Skip turn.
        SkillStatus condition = SkillStatus.Sleep | SkillStatus.Freeze | SkillStatus.Stun;
        if(ExtensionSkillStatus.CheckResultInCondition(input, (int)condition)) return false;

        // Can't move. 
        condition = SkillStatus.Root;
        if(ExtensionSkillStatus.CheckResultInCondition(input, (int)condition))
        {
            RollDiceBtn.gameObject.SetActive(false);
            if(!CancelSkillBtn.gameObject.activeInHierarchy)
                SkipBtn.gameObject.SetActive(true);
        }

        return true;
    }

    private void SetEventToChangeViewBtn(CameraType cameraType)
    {
        ChangeViewBtn.onClick.RemoveAllListeners();
        ChangeViewBtn.onClick.AddListener(() =>
        {
            CameraController.Instance.Show(cameraType);
            SetEventToChangeViewBtn((cameraType == CameraType.Default) ? CameraType.TopDown : CameraType.Default);
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

            SetActivePanel(false);
            CancelSkillBtn.onClick.RemoveAllListeners();
            CancelSkillBtn.onClick.AddListener(OnClickCancelAttack);
        });
    }

    private void SetEventToDiceRollBtn(Poring poring, int index)
    {
        RollDiceBtn.onClick.RemoveAllListeners();
        RollDiceBtn.onClick.AddListener(() =>
        {
            BtnGroup.ForEach(group => group.BtnObject.gameObject.SetActive(false));    
            AttackBtn.gameObject.SetActive(false);
            RollDiceBtn.gameObject.SetActive(false);

            gameMode.PhotonNetworkRaiseEvent(EventCode.BeginRollMove, new object[] { index });

            // poring.MoveRoll.SetRoll(poring.Property.MoveDices[0].FaceDiceList, index);
            RollDiceBtn.onClick.RemoveAllListeners();
        });
    }

    

    private void SetSkillsEvent(List<BaseSkill> skillList, Poring poring, int index)
    {
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

            group.BtnObject.interactable = (skill.CurrentCD <= 0 && skill.SkillMode == SkillMode.Activate);

            if(skill.MoveToTarget && ExtensionSkillStatus.CheckResultInCondition(poring.GetCurrentStatus(), (int)SkillStatus.Root))
            {
                group.BtnObject.interactable = false;
            }
            
            group.BtnObject.onClick.AddListener(() =>
            {
                BtnGroup.ForEach(g => {
                    g.BtnObject.interactable = false;
                });
                SetActivePanel(false);
                ChangeModeToSelectTarget(skill, poring, index);
            });
        }
    }

    private void ChangeModeToSelectTarget(BaseSkill skill, Poring poring, int index)
    {
        isActiveSkill = true;
        CameraController.Instance.Show(CameraType.TopDown);

        gameMode.PhotonNetworkRaiseEvent(EventCode.HighlightNodeSkill, new object[]{ skill.name, index});
        // gameMode.ParseSelectableNode(skill);
        // gameMode.DisplayNodeHeatBySkill(skill);
        // StartCoroutine(gameMode.WaitForSelectTarget(skill));

        CancelSkillBtn.onClick.RemoveAllListeners();
        CancelSkillBtn.onClick.AddListener(OnClickCancelSkill);
    }

    private void OnClickCancelSkill()
    {
        CancelSkillBtn.onClick.RemoveAllListeners();
        isActiveSkill = false;
        for (int i = 0; i < SkillList.Count; i++)
        {
            BtnGroup[i].BtnObject.interactable = (SkillList[i].CurrentCD <= 0 && SkillList[i].SkillMode == SkillMode.Activate);
        }
        
        CameraController.Instance.Show(CameraType.Default);
        SetActivePanel(true);

        gameMode.PhotonNetworkRaiseEvent(EventCode.OnClickCancel);
        // gameMode.ResetNodeColor();
    }

    private void OnClickCancelAttack()
    {
        CancelSkillBtn.onClick.RemoveAllListeners();
        isActiveAttack = false;
        
        CameraController.Instance.Show(CameraType.Default);
        SetActivePanel(true);
        
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
