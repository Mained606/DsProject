using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class TestStatUP : MonoBehaviour
{
    #region 
    /*   [SerializeField] private Transform NamePanel;
       [SerializeField] private Transform ValuePanel;

       private TextMeshProUGUI[] name;
       private TextMeshProUGUI[] value;
       private Button[] button;


       void Start()
       {
           name = NamePanel.GetComponentsInChildren<TextMeshProUGUI>();
           value = ValuePanel.GetComponentsInChildren<TextMeshProUGUI>();
           button = transform.GetComponentsInChildren<Button>();

       }

       // Update is called once per frame
       void Update()
       {

       }*/
    #endregion
    public TextMeshProUGUI STR;
    public TextMeshProUGUI DEX;
    public TextMeshProUGUI INT;
    public TextMeshProUGUI VATAL;
    public TextMeshProUGUI SkillLevel;

    public TextMeshProUGUI STR2;
    public TextMeshProUGUI DEX2;
    public TextMeshProUGUI INT2;
    public TextMeshProUGUI VATAL2;
    public TextMeshProUGUI SkillLevel2;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public Button button7;
    public Button button8;
    public Button button9;
    
    public Button button10;
    public Button button11;
    
    public Button button12;
    public Button button13;
    public Button button14;

    public PlayerData playerData;
    

    private void Start()
    {
        playerData = CharacterManager.PlayerCharacterData;
            
        STR.text = playerData.strength.ToString();
        DEX.text = playerData.agility.ToString();
        INT.text = playerData.intelligence.ToString();
        VATAL.text = playerData.vitality.ToString();
        SkillLevel.text = SkillManager.SkillDatabase.playerSkills[0].skillLevel.ToString();

        button1.onClick.AddListener(() => OnClickButton(StatType.Strength, 1));
        button2.onClick.AddListener(() => OnClickButton(StatType.Agility, 1));
        button3.onClick.AddListener(() => OnClickButton(StatType.Intelligence, 1));
        button4.onClick.AddListener(() => OnClickButton(StatType.Vitality, 1));
        
        button6.onClick.AddListener(() => OnClickButton(StatType.Strength, -1));
        button7.onClick.AddListener(() => OnClickButton(StatType.Agility, -1));
        button8.onClick.AddListener(() => OnClickButton(StatType.Intelligence, -1));
        button9.onClick.AddListener(() => OnClickButton(StatType.Vitality, -1));

        button5.onClick.AddListener(() => OnClickButtonSkill(SkillManager.SkillDatabase.playerSkills[0], 1));
        button12.onClick.AddListener(() => OnClickButtonSkill(SkillManager.SkillDatabase.playerSkills[0], -1));

        button10.onClick.AddListener(() => OnClickComfirm());
        button11.onClick.AddListener(() => OnClickCancel());
        
        button13.onClick.AddListener(() => OnClickSkillComfirm());
        button14.onClick.AddListener(() => OnClickSkillCancel());
    }

    private void OnClickButton(StatType statType, int delta)
    {
        if(playerData.AdjustTempStat(statType, delta))
        {
            Debug.Log("버튼 클릭");
            UpdateUI();

            int previewStrength = playerData.GetPreviewStat(StatType.Strength);
            STR2.text = previewStrength.ToString();

            
        }
    }

    private void OnClickComfirm()
    {
        playerData.ConfirmTempAllocation();
        UpdateUI();
    }

    private void OnClickSkillComfirm()
    {
        playerData.ConfirmTempSkillAllocation();
        UpdateUI();
    }

    private void OnClickCancel()
    {
        playerData.CancelTempAllocation();
        UpdateUI();
        int previewStrength = playerData.GetPreviewStat(StatType.Strength);
        STR2.text = previewStrength.ToString();
    }

    private void OnClickSkillCancel()
    {
        playerData.CancelTempSkillAllocation();
        UpdateUI();
      //  int previewSkillLevel = playerData.GetPreviewSkillLevel(skill);
      //  SkillLevel2.text = previewSkillLevel.ToString();
    }
    
    void OnClickButtonSkill(Skills skill, int delta)
    {
       if( playerData.AdjustTempSkill(skill, delta))
       {
           Debug.Log("버튼 클릭");
           UpdateUI();

            int previewSkillLevel = playerData.GetPreviewSkillLevel(skill);
            SkillLevel2.text = previewSkillLevel.ToString();
        }
    }

    private void UpdateUI()
    {
        STR.text = playerData.strength.ToString();
        DEX.text = playerData.agility.ToString();
        INT.text = playerData.intelligence.ToString();
        VATAL.text = playerData.vitality.ToString();

        SkillLevel.text = SkillManager.SkillDatabase.playerSkills[0].skillLevel.ToString();

    }
    
    // 예: 플레이어의 Strength(힘) 미리보기 값을 가져와서 UI 텍스트에 표시
    // int previewStrength = playerData.GetPreviewStat(StatType.Strength);
    // strengthUIText.text = previewStrength.ToString();
    
    // 예: 선택한 스킬의 미리보기 레벨을 가져와서 UI에 표시
    // int previewSkillLevel = playerData.GetPreviewSkillLevel(selectedSkill);
    // skillLevelUIText.text = previewSkillLevel.ToString();

}


