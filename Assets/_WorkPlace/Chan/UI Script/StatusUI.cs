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
       // playerData.CancelTempAllocation();
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
        Animator playerAnimator = playerTabButton.GetComponent<Animator>();
        if (playerAnimator != null) ButtonReset(playerAnimator);

        Animator dragonAnimator = dragonTabButton.GetComponent<Animator>();
        if (dragonAnimator != null) ButtonReset(dragonAnimator);
    }

    private void ButtonReset(Animator animator)
    {
        Image image1 = animator.transform.GetChild(0).GetComponent<Image>();
        Color color1 = image1.color;
        color1.a = 0f;
        image1.color = color1;

        Image image2 = animator.transform.GetChild(1).GetComponent<Image>();
        Color color2 = image1.color;
        color2.a = 0f;
        image2.color = color2;

        Image image3 = animator.transform.GetChild(3).GetComponent<Image>();
        Color color3 = image3.color;
        color3.a = 0f;
        image3.color = color3;
    }

    private void OnTabClick(CharType type)
    {
        characterType = type;
        UpdateUI();
    }

    private void OnClickStatButton(StatType stat, int delta)
    {

        if (!playerData.AdjustTempStat(stat, delta)) return;
        UpdateUI();
    }

    private void OnClickConfirm()
    {
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
        UpdateDefaultPanel(CharType.Player);
        playerPanel.SetActive(true);
        dragonPanel.SetActive(false);

        playerStatTexts[0].text = playerData.strength.ToString();
        playerStatTexts[1].text = playerData.intelligence.ToString();
        playerStatTexts[2].text = playerData.agility.ToString();
        playerStatTexts[3].text = playerData.vitality.ToString();

        playerStatTexts[4].text = playerData.currentHp.ToString();
        playerStatTexts[5].text = playerData.currentMp.ToString();
        playerStatTexts[6].text = playerData.staminaCurrent.ToString();
        playerStatTexts[7].text = playerData.physicalDamage.ToString();
        playerStatTexts[8].text = playerData.magicDamage.ToString();
        playerStatTexts[9].text = playerData.physicalDefense.ToString();
        playerStatTexts[10].text = playerData.magicDefense.ToString();
        playerStatTexts[11].text = (playerData.criticalChance * 100).ToString("F1") + "%";
        playerStatTexts[12].text = (playerData.dodgeChance * 100).ToString("F1") + "%";
        playerStatTexts[13].text = (playerData.physicalDamageReduction * 100).ToString("F1") + "%";
        playerStatTexts[14].text = (playerData.magicDamageReduction * 100).ToString("F1") + "%";
        playerStatTexts[15].text = $"스탯포인트: {playerData.availableStatPoints}";

        UpdatePlayerStatUI(StatType.Strength, 0);
        UpdatePlayerStatUI(StatType.Intelligence, 1);
        UpdatePlayerStatUI(StatType.Agility, 2);
        UpdatePlayerStatUI(StatType.Vitality, 3);
    }

    private void ShowDragonStats()
    {
        UpdateDefaultPanel(CharType.Dragon);
        playerPanel.SetActive(false);
        dragonPanel.SetActive(true);

        // 값 텍스트 설정
        DragonStats[0].text = dragonData.evolutionStage.ToString();
        DragonStats[1].text = dragonData.strength.ToString();
        DragonStats[2].text = dragonData.intelligence.ToString();
        DragonStats[3].text = dragonData.agility.ToString();
        DragonStats[4].text = dragonData.vitality.ToString();

        DragonStats[5].text = dragonData.physicalDamage.ToString();
        DragonStats[6].text = dragonData.magicDamage.ToString();
        DragonStats[7].text = dragonData.attackSpeed.ToString();
        DragonStats[8].text = (dragonData.criticalChance * 100f).ToString("F1") + "%";
    }

    private void UpdatePlayerStatUI(StatType stat, int index)
    {
        int baseValue = GetStatByType(stat); // 현재 실제값
        int preview = playerData.GetPreviewStat(stat); // 임시 포함값
        int delta = preview;

        playerStatTexts[index].text = baseValue.ToString();

        if (delta != 0)
        {
            previewStatTexts[index].text = delta > 0 ? $"+{delta}" : delta.ToString();
            previewStatTexts[index].gameObject.SetActive(true);
        }
        else
        {
            previewStatTexts[index].text = "";
            previewStatTexts[index].gameObject.SetActive(false); 
        }


    }

    private void UpdateDefaultPanel(CharType type)
    {
        if (type == CharType.Player)
        {
            DefaultText[0].text = playerData.characterName;
            DefaultText[1].text  = $"Lv. {playerData.level}";
            DefaultText[2].text = $"EXP {playerData.currentExperience} / {playerData.experienceToLevelUp}";
            DefaultImages[2].fillAmount = (float)playerData.currentExperience / playerData.experienceToLevelUp;

            // 무기 속성
            Item weapon = ItemEffectManager.Instance?.GetEquippedItem(EquipmentSlot.손);
            ElementalAttribute weaponAttr = weapon?.itemSkill?.element ?? ElementalAttribute.None;
            string weaponColor = GetElementColor(weaponAttr);
            DefaultText[3].text = weaponAttr == ElementalAttribute.None ? "" : $"무기속성: <color={weaponColor}>{weaponAttr}</color>";


            // 방어구 속성
            Item armor = ItemEffectManager.Instance?.GetEquippedItem(EquipmentSlot.몸);
            ElementalAttribute armorAttr = armor?.itemSkill?.element ?? ElementalAttribute.None;
            string armorColor = GetElementColor(armorAttr);
            DefaultText[4].text = armorAttr == ElementalAttribute.None ? "" : $"방어구속성: <color={armorColor}>{armorAttr}</color>";

            DefaultText[3].gameObject.SetActive(true);
            DefaultText[4].gameObject.SetActive(true);
        }
        else if (type == CharType.Dragon)
        {
            DefaultText[0].text = dragonData.characterName;
            DefaultText[1].text = $"Lv. {dragonData.bondLevel}";
            DefaultText[2].text = $"유대감 {dragonData.bondExperience} / {dragonData.CalculateExperienceToLevelUp(dragonData.bondLevel)}";
            DefaultImages[2].fillAmount = (float)dragonData.bondExperience / dragonData.CalculateExperienceToLevelUp(dragonData.bondLevel);

            // 드래곤은 장비 없음
            DefaultText[3].text = "";
            DefaultText[4].text = "";

            DefaultText[3].gameObject.SetActive(false);
            DefaultText[4].gameObject.SetActive(false);
        }
    }
    private string GetElementColor(ElementalAttribute attr)
    {
        return attr switch
        {
            ElementalAttribute.Fire => "#FF4500",
            ElementalAttribute.Water => "#1E90FF",
            ElementalAttribute.Electric => "#FFFF00",
            ElementalAttribute.Earth => "#8B4513",
            _ => "#FFFFFF"
        };
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
