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

        button5.onClick.AddListener(() => OnClickButtonSkill(SkillManager.SkillDatabase.playerSkills[0]));

        button10.onClick.AddListener(() => OnClickComfirm());
        button11.onClick.AddListener(() => OnClickCancel());
    }

    private void OnClickButton(StatType statType, int delta)
    {
        if(playerData.AdjustTempStat(statType, delta))
        {
            Debug.Log("버튼 클릭");
            UpdateUI();
        }
    }

    private void OnClickComfirm()
    {
        playerData.ConfirmTempAllocation();
        UpdateUI();
    }

    private void OnClickCancel()
    {
        playerData.CancelTempAllocation();
        UpdateUI();
    }
    
    void OnClickButtonSkill(Skills skill)
    {
       if( playerData.UpgradeSkill(skill))
        {
            UpdateUI();
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

}


