using TMPro;
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

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public PlayerData playerData;
    

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
    }


}


