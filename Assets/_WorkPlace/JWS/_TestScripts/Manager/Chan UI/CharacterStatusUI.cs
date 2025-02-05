using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusUI : MonoBehaviour
{
    private enum CharType { Player, Dragon }
    private CharType chraterType;
    [SerializeField] private Transform character_Stat;
    [SerializeField] private Transform statsNamePosition;
    [SerializeField] private Transform statsValuePosition;
    [SerializeField] private Transform ImagePanel;
    [SerializeField] private Transform NamePanel;
    private TextMeshProUGUI[] statsNames;
    private string[] defaultNames;
    private TextMeshProUGUI[] statsValues;
    private Image[] Edge;
    private Button[] buttons;
    private int currentButtonIndex = 0;

    #region 네임판넬에 캐릭터이름 / 경험치&유대감 추가 구현 부분 
    [SerializeField] private TextMeshProUGUI experienceText; // 경험치 텍스트 
    [SerializeField] private Image experienceFillBar; // 경험치 바 이미지

    private TextMeshProUGUI CharacterName;
    #endregion

    void Awake()
    {
        statsNames = statsNamePosition.GetComponentsInChildren<TextMeshProUGUI>();
        statsValues = statsValuePosition.GetComponentsInChildren<TextMeshProUGUI>();
        Edge = ImagePanel.GetComponentsInChildren<Image>();
        CharacterName = NamePanel.GetComponentInChildren<TextMeshProUGUI>();
        buttons = transform.GetComponentsInChildren<Button>();
        defaultNames = new string[statsNames.Length];
        for (int i = 0; statsNames.Length > i; i++)
        {
            defaultNames[i] = statsNames[i].text;
        }
    }

    private void OnEnable()
    {
        AddButtonListeners();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void Start()
    {
        OnButtonClick(currentButtonIndex);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void UpdateUI()
    {
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }
        switch (currentButtonIndex)
        {
            case 2:
                GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
                break;

            default:
                ShowCharacterInfo();
                break;
        }
    }

    private void ShowCharacterInfo()
    {
        switch (chraterType)
        {
            case CharType.Player:
                CameraManager.Instance.HandleUIviewState(GameManager.playerTransform);
                GetPlayerStatsValue();
                break;
            case CharType.Dragon:
                CameraManager.Instance.HandleUIviewState(GameManager.DragonTransform);
                GetDragonStatsValue();
                break;
        }
    }

    public void AddButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public void RemoveButtonListeners()
    {
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        if (currentButtonIndex >= 0 && currentButtonIndex < 2)
        {
            Animator buttonAnimator = buttons[buttonIndex].animator;
            buttonAnimator.SetTrigger("Hover");
            chraterType = (CharType)buttonIndex;
        }
        UpdateUI();
    }

    private void GetPlayerStatsValue()
    {
        PlayerData playerData = CharacterManager.PlayerCharacterData;
        for (int i = 0; statsNames.Length > i; i++)
        {
            statsNames[i].text = defaultNames[i];
        }
        CharacterName.text = playerData.characterName;
     
        statsValues[0].text = playerData.currentHp.ToString();
        statsValues[1].text = playerData.currentMp.ToString();
        statsValues[2].text = playerData.staminaCurrent.ToString();
        statsValues[3].text = playerData.strength.ToString();
        statsValues[4].text = playerData.intelligence.ToString();
        statsValues[5].text = playerData.agility.ToString();
        statsValues[6].text = playerData.physicalDamage.ToString();
        statsValues[7].text = playerData.magicDamage.ToString();
        statsValues[8].text = playerData.physicalDefense.ToString();
        statsValues[9].text = playerData.magicDefense.ToString();

        #region 추가 스탯 수치 표현 & 경험치 추가 
        statsValues[10].text = playerData.vitality.ToString();
        statsValues[11].text = (playerData.criticalChance * 100).ToString("F1") + "%";
        statsValues[12].text = (playerData.dodgeChance * 100).ToString("F1") + "%";
        statsValues[13].text = (playerData.physicalDamageReduction * 100).ToString("F1") + "%";
        statsValues[14].text = (playerData.magicDamageReduction * 100).ToString("F1") + "%";

        // 경험치 UI 갱신 추가
        experienceText.text = $"{playerData.currentExperience} / {playerData.experienceToLevelUp}";
        experienceFillBar.fillAmount = (float)playerData.currentExperience / playerData.experienceToLevelUp;
        #endregion

        // 특정 이미지들의 알파값을 1로 설정 (다시 보이게 만듦)
        for (int i = 0; i < Edge.Length; i++)
        {
            Color tempColor = Edge[i].color;
            tempColor.a = 1f; // 알파값 1 (완전히 보이게)
            Edge[i].color = tempColor;
        }

    }

    private void GetDragonStatsValue()
    {
        DragonData dragonData = CharacterManager.DragonData;

        experienceText.text = $"{dragonData.bondExperience} / {dragonData.bondThresholds}";
     //   experienceFillBar.fillAmount = (float)dragonData.bondExperience / (float)dragonData.bondThresholds;

        CharacterName.text = dragonData.characterName;

        statsNames[0].text = "LV";
        statsNames[1].text = "Form";
        statsNames[2].text = "";
        statsNames[3].text = "STR";
        statsNames[4].text = "INT";
        statsNames[5].text = "Dex";
        statsNames[6].text = "Vitality";
      //  statsNames[7].text = "";
     //   statsNames[8].text = "";
        statsNames[9].text = "criticalChance";
        statsNames[10].text = "";
        statsNames[11].text = "";
        statsNames[12].text = "";
        statsNames[13].text = "";
        statsNames[14].text = "";
        

        statsValues[0].text = dragonData.bondLevel.ToString();
        statsValues[1].text = "Baby";
        statsValues[2].text = "";  //dragonData.speed.ToString(); 
        statsValues[3].text = dragonData.strength.ToString();
        statsValues[4].text = dragonData.intelligence.ToString();
        statsValues[5].text = dragonData.speed.ToString();
        statsValues[6].text = dragonData.vitality.ToString();
        statsValues[7].text = dragonData.physicalDamage.ToString();
        statsValues[8].text = dragonData.magicDamage.ToString();
        statsValues[9].text = (dragonData.criticalChance*100).ToString("F1")+"%";
        statsValues[10].text = "";
        statsValues[11].text = "";
        statsValues[12].text = "";
        statsValues[13].text = "";
        statsValues[14].text = "";
        // 특정 이미지들의 알파값을 0으로 설정 (투명하게 만듦)
        int[] imagesToFade = { 2,10,11,12,13,14}; // 투명하게 만들 이미지 인덱스
        foreach (int index in imagesToFade)
        {
            Color tempColor = Edge[index].color;
            tempColor.a = 0f; // 알파값 0 (투명)
            Edge[index].color = tempColor;
        }

    }
}
