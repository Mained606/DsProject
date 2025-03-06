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

    public PlayerData playerData;
    

    private void Start()
    {
        playerData = CharacterManager.PlayerCharacterData;
            
        STR.text = playerData.strength.ToString();
        DEX.text = playerData.agility.ToString();
        INT.text = playerData.intelligence.ToString();
        VATAL.text = playerData.vitality.ToString();
        SkillLevel.text = SkillManager.SkillDatabase.playerSkills[0].skillLevel.ToString();


        button1.onClick.AddListener(() => OnClickButton(StatType.Strength));
        button2.onClick.AddListener(() => OnClickButton(StatType.Agility));
        button3.onClick.AddListener(() => OnClickButton(StatType.Intelligence));
        button4.onClick.AddListener(() => OnClickButton(StatType.Vitality));
        button5.onClick.AddListener(() => OnClickButtonSkill(SkillManager.SkillDatabase.playerSkills[0]));
    }
    // 버튼 1~4번까지는 스탯 증가
    private void OnClickButton(StatType statType)
    {
       if(playerData.UpgradeStat(statType))
        {
            UpdateUI();
        }
        Debug.Log("버튼 클릭");

    }
    // 버튼 5번 스킬증가 6번 스킬감소 7번 확정버튼
    void OnClickButtonSkill(Skills skill)
    {
       if( playerData.UpgradeSkill(skill))
        {
            UpdateUI();
        }
    }
    // 변경 후 초기화
    private void UpdateUI()
    {
        STR.text = playerData.strength.ToString();
        DEX.text = playerData.agility.ToString();
        INT.text = playerData.intelligence.ToString();
        VATAL.text = playerData.vitality.ToString();

        SkillLevel.text = SkillManager.SkillDatabase.playerSkills[0].skillLevel.ToString();
    }

}


