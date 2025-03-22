using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    private enum CharType { Player, Dragon }
    private CharType characterType;

    [Header("공통 패널")]
    [SerializeField] private GameObject defaultPanel;
    private TextMeshProUGUI[] DefaultText;
    private Image[] DefaultImages;

    [Header("플레이어 패널")]
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private Transform statValuePosition;
    [SerializeField] private Transform statPreviewPosition;
    private TextMeshProUGUI[] playerStatTexts;
    private TextMeshProUGUI[] previewStatTexts;

    [Header("드래곤 패널")]
    [SerializeField] private GameObject dragonPanel;
    private TextMeshProUGUI[] DragonStats;

    [Header("버튼 영역")]

    private Button[] buttons;
    private Button playerTabButton;
    private Button dragonTabButton;
    private Button confirmButton;
    private Dictionary<StatType, (Button plus, Button minus)> statButtons = new();

    private PlayerData playerData;
    private DragonData dragonData;

    private void Awake()
    {
        playerData = CharacterManager.PlayerCharacterData;
        dragonData = CharacterManager.DragonData;

        DefaultText = defaultPanel.GetComponentsInChildren<TextMeshProUGUI>();
        DefaultImages = defaultPanel.GetComponentsInChildren<Image>();
        playerStatTexts = statValuePosition.GetComponentsInChildren<TextMeshProUGUI>();
        previewStatTexts = statPreviewPosition.GetComponentsInChildren<TextMeshProUGUI>();
        DragonStats = dragonPanel.GetComponentsInChildren<TextMeshProUGUI>();

        AssignButtonsByIndex();
    }

    private void OnEnable()
    {
        playerData.CancelTempAllocation();
        OnTabClick(CharType.Player);
        AddButtonListeners();
       
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void AssignButtonsByIndex()
    {
        buttons = transform.GetComponentsInChildren<Button>(true);
        if (buttons.Length < 11)
        {
            Debug.LogError("버튼 개수가 부족합니다. 총 11개 필요.");
            return;
        }

        playerTabButton = buttons[0];
        dragonTabButton = buttons[1];
        confirmButton = buttons[10];

        statButtons[StatType.Strength] = (buttons[2], buttons[6]);
        statButtons[StatType.Intelligence] = (buttons[3], buttons[7]);
        statButtons[StatType.Agility] = (buttons[4], buttons[8]);
        statButtons[StatType.Vitality] = (buttons[5], buttons[9]);
    }

    private void AddButtonListeners()
    {
        Debug.Log("[AddButtonListeners 호출됨]");
        playerTabButton?.onClick.AddListener(() => OnTabClick(CharType.Player));
        dragonTabButton?.onClick.AddListener(() => OnTabClick(CharType.Dragon));
        confirmButton?.onClick.AddListener(OnClickConfirm);

        foreach (var pair in statButtons)
        {
            pair.Value.plus?.onClick.AddListener(() => OnClickStatButton(pair.Key, 1));
            pair.Value.minus?.onClick.AddListener(() => OnClickStatButton(pair.Key, -1));
        }
    }

    private void RemoveButtonListeners()
    {
        playerTabButton?.onClick.RemoveAllListeners();
        dragonTabButton?.onClick.RemoveAllListeners();
        confirmButton?.onClick.RemoveAllListeners();

        foreach (var pair in statButtons)
        {
            pair.Value.plus?.onClick.RemoveAllListeners();
            pair.Value.minus?.onClick.RemoveAllListeners();
        }
    }

    private void OnTabClick(CharType type)
    {
        characterType = type;
        UpdateUI();
    }

    private void OnClickStatButton(StatType stat, int delta)
    {
        Debug.Log("버튼 눌림");
        if (!playerData.AdjustTempStat(stat, delta)) return;
        UpdateUI();
    }

    private void OnClickConfirm()
    {
        Debug.Log("버튼 눌림");
        playerData.ConfirmTempAllocation();
        UpdateUI();
    }

    private void UpdateUI()
    {
        switch (characterType)
        {
            case CharType.Player:
                ShowPlayerStats();
                break;
            case CharType.Dragon:
                ShowDragonStats();
                break;
        }
    }

    private void ShowPlayerStats()
    {
        playerPanel.SetActive(true);
        dragonPanel.SetActive(false);

      /*  DefaultText[0].text = playerData.characterName;
        DefaultText[1].text = $"Lv. {playerData.level}";
        DefaultText[2].text = $"EXP {playerData.currentExperience} / {playerData.experienceToLevelUp}";
        DefaultImages[0].fillAmount = (float)playerData.currentExperience / playerData.experienceToLevelUp;*/

        UpdatePlayerStatUI(StatType.Strength, 0);
        UpdatePlayerStatUI(StatType.Intelligence, 1);
        UpdatePlayerStatUI(StatType.Agility, 2);
        UpdatePlayerStatUI(StatType.Vitality, 3);
    }

    private void ShowDragonStats()
    {
        playerPanel.SetActive(false);
        dragonPanel.SetActive(true);

     /*   DefaultText[0].text = dragonData.characterName;
        DefaultText[1].text = $"Lv. {dragonData.bondLevel}";
        DefaultText[2].text = $"유대감 {dragonData.bondExperience} / {dragonData.CalculateExperienceToLevelUp(dragonData.bondLevel)}";
        DefaultImages[0].fillAmount = (float)dragonData.bondExperience / dragonData.CalculateExperienceToLevelUp(dragonData.bondLevel);*/

        DragonStats[0].text = dragonData.strength.ToString();
        DragonStats[1].text = dragonData.intelligence.ToString();
        DragonStats[2].text = dragonData.agility.ToString();
        DragonStats[3].text = dragonData.vitality.ToString();
    }

    private void UpdatePlayerStatUI(StatType stat, int index)
    {
        int baseValue = GetStatByType(stat); // 현재 실제값
        int preview = playerData.GetPreviewStat(stat); // 임시 포함값

        playerStatTexts[index].text = baseValue.ToString();
        previewStatTexts[index].text = preview.ToString();


    }

    private int GetStatByType(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => playerData.strength,
            StatType.Intelligence => playerData.intelligence,
            StatType.Agility => playerData.agility,
            StatType.Vitality => playerData.vitality,
            _ => 0
        };
    }
}
