using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnActiveUIController : InstanceObject<TurnActiveUIController> 
{
    public Button RollDiceBtn;
    public GameObject PanelSkills;
    public GameObject CancelSkillBtn;

    public List<ButtonGroup> BtnGroup = new List<ButtonGroup>();

    private PrototypeGameMode gameMode = null;

    private List<BaseSkill> SkillList = new List<BaseSkill>();

    public bool SkillMode = false;

    public void SetActivePanel(bool isEnable)
    {
        RollDiceBtn.gameObject.SetActive(isEnable);
        PanelSkills.SetActive(isEnable);
        CancelSkillBtn.SetActive(!isEnable);
    }

    public void ActiveCurrentPoringTurn(Poring poring, int index, PrototypeGameMode gameMode)
    {
        SetActivePanel(true);
        if (this.gameMode == null) this.gameMode = gameMode;
        RollDiceBtn.onClick.RemoveAllListeners();
        RollDiceBtn.onClick.AddListener(() =>
        {
            BtnGroup.ForEach(group => group.BtnObject.gameObject.SetActive(false));    
            poring.MoveRoll.SetRoll(poring.Property.MoveDices[0].FaceDiceList, index);
            RollDiceBtn.onClick.RemoveAllListeners();
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

            group.BtnObject.interactable = (skill.CurrentCD <= 0);
            
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
        SkillMode = true;
        CameraController.Instance.Show(CameraType.TopDown);
        gameMode.ParseSelectableNode(skill);
        gameMode.DisplayNodeHeatBySkill(skill);
        StartCoroutine(gameMode.WaitForSelectTarget(skill));
    }

    public void OnClickCancel()
    {
        SkillMode = false;
        for (int i = 0; i < SkillList.Count; i++)
        {
            BtnGroup[i].BtnObject.interactable = (SkillList[i].CurrentCD <= 0);
        }
        
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
