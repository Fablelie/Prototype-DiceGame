using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnActiveUIController : InstanceObject<TurnActiveUIController> 
{
    public Button RollDiceBtn;
    public Button AttackBtn;
    public GameObject PanelSkills;
    public Button CancelSkillBtn;

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
    }

    public void ActiveCurrentPoringTurn(Poring poring, int index, PrototypeGameMode gameMode)
    {
        SetActivePanel(true);
        if (this.gameMode == null) this.gameMode = gameMode;
        RollDiceBtn.onClick.RemoveAllListeners();
        RollDiceBtn.onClick.AddListener(() =>
        {
            BtnGroup.ForEach(group => group.BtnObject.gameObject.SetActive(false));    
            AttackBtn.gameObject.SetActive(false);
            RollDiceBtn.gameObject.SetActive(false);
            poring.MoveRoll.SetRoll(poring.Property.MoveDices[0].FaceDiceList, index);
            RollDiceBtn.onClick.RemoveAllListeners();
        });

        AttackBtn.onClick.RemoveAllListeners();
        AttackBtn.onClick.AddListener(() =>
        {
            isActiveAttack = true;
            CameraController.Instance.Show(CameraType.TopDown);
            gameMode.CheckHasTargetInRange(poring.Property.AttackRange);
            gameMode.DisplayNodeHeatByAttackRange();
            StartCoroutine(gameMode.WaitForSelectTarget());
            SetActivePanel(false);
            CancelSkillBtn.onClick.RemoveAllListeners();
            CancelSkillBtn.onClick.AddListener(OnClickCancelAttack);
        });
        
        SkillList = poring.Property.SkillList;
        SetSkillsEvent(SkillList, poring);
    }

    private void SetSkillsEvent(List<BaseSkill> skillList, Poring poring)
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
            
            group.BtnObject.onClick.AddListener(() =>
            {
                BtnGroup.ForEach(g => {
                    g.BtnObject.interactable = false;
                });
                SetActivePanel(false);
                ChangeModeToSelectTarget(skill, poring);
            });
        }
    }

    private void ChangeModeToSelectTarget(BaseSkill skill, Poring poring)
    {
        isActiveSkill = true;
        CameraController.Instance.Show(CameraType.TopDown);
        gameMode.ParseSelectableNode(skill);
        gameMode.DisplayNodeHeatBySkill(skill);
        StartCoroutine(gameMode.WaitForSelectTarget(skill));

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
        gameMode.ResetNodeColor();
    }

    private void OnClickCancelAttack()
    {
        CancelSkillBtn.onClick.RemoveAllListeners();
        isActiveAttack = false;
        
        CameraController.Instance.Show(CameraType.Default);
        SetActivePanel(true);
        gameMode.ResetNodeColor();
    }

    [System.Serializable]
    public struct ButtonGroup
    {
        public Text BtnName;
        public Button BtnObject;
    }
}
