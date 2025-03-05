using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestStatUP : MonoBehaviour
{
    #region 
    [SerializeField] private Transform NamePanel;
    [SerializeField] private Transform ValuePanel;

    private TextMeshProUGUI[] Statname;
    private TextMeshProUGUI[] Statvalue;
    private Button[] button;


    void Start()
    {
        Statname = NamePanel.GetComponentsInChildren<TextMeshProUGUI>();
        Statvalue = ValuePanel.GetComponentsInChildren<TextMeshProUGUI>();
        button = transform.GetComponentsInChildren<Button>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region Test1
    /*public TextMeshProUGUI STR;
    public TextMeshProUGUI DEX;
    public TextMeshProUGUI INT;
    public TextMeshProUGUI VATAL;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public PlayerData playerData = CharacterManager.PlayerCharacterData;

    private void Start()
    {
        playerData = CharacterManager.PlayerCharacterData;

        STR.text = playerData.strength.ToString();
        DEX.text = playerData.agility.ToString();
        INT.text = playerData.intelligence.ToString();
        VATAL.text = playerData.vitality.ToString();



        button1.onClick.AddListener(() => OnClickButton(StatType.Strength));
        button2.onClick.AddListener(() => OnClickButton(StatType.Agility));
        button3.onClick.AddListener(() => OnClickButton(StatType.Intelligence));
        button4.onClick.AddListener(() => OnClickButton(StatType.Vitality));
    }

    private void OnClickButton(StatType statType)
    {
        playerData.UpgradeStat(statType);
        Debug.Log("버튼 클릭");
    }*/
    #endregion
}


