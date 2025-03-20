using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 03.20 스테이터스ui 새로 작성중
/// </summary>
public class StatusUI : MonoBehaviour
{
    private enum CharType { Player, Dragon } // 카테고리 이넘 
    private CharType characterType;

    [SerializeField] private GameObject defaultPanel;// 공용 네임, 레벨, 경험치 판넬
    [SerializeField] private GameObject playerPanel; // 플레이어 스탯판넬 / 버튼
    [SerializeField] private GameObject dragonbPanel;// 드래곤 스탯판넬

    private Dictionary<StatType, (Button plus, Button minus)> statButtons = new Dictionary<StatType, (Button, Button)>();
    PlayerData playerData; 
    DragonData dragonData;

    private TextMeshProUGUI[] PlayerStats;
    private TextMeshProUGUI[] DragonStats;
    private TextMeshProUGUI[] DefaultText; // 공용 판넬
    private Image[] DeFaultImage;
    [SerializeField] private Button confirmButton; // 확정 버튼


    private void Awake()
    {
        // CharacterManager에서 PlayerCharacterData 가져오기
        playerData = CharacterManager.PlayerCharacterData;
        dragonData = CharacterManager.DragonData;
        if (playerData == null)
        {
            Debug.LogError("플레이어 데이터가 없음");
        }
        if (dragonData == null)
        {
            Debug.LogError("드래곤 데이터가 없음");
        }

        PlayerStats = playerPanel.GetComponentsInChildren<TextMeshProUGUI>();
        DragonStats = dragonbPanel.GetComponentsInChildren<TextMeshProUGUI>();

        DeFaultImage = defaultPanel.GetComponentsInChildren<Image>();
        DefaultText = defaultPanel.GetComponentsInChildren<TextMeshProUGUI>();

     Button[] allbuttons = playerPanel.GetComponentsInChildren<Button>();
        foreach(Button btn in allbuttons)
        {
            switch(btn.name)
            {
                case"UP0":
                    if (!statButtons.ContainsKey(StatType.Strength)) statButtons[StatType.Strength] = (btn, null);
                    else statButtons[StatType.Strength] = (btn, statButtons[StatType.Strength].plus);
                    break;
                case "UP1":
                    if (!statButtons.ContainsKey(StatType.Intelligence)) statButtons[StatType.Intelligence] = (btn, null);
                    else statButtons[StatType.Intelligence] = (btn, statButtons[StatType.Intelligence].plus);
                    break;
                case "UP2":
                    if (!statButtons.ContainsKey(StatType.Agility)) statButtons[StatType.Agility] = (btn, null);
                    else statButtons[StatType.Agility] = (btn, statButtons[StatType.Agility].plus);
                    break;
                case "UP3":
                    if (!statButtons.ContainsKey(StatType.Vitality)) statButtons[StatType.Vitality] = (btn, null);
                    else statButtons[StatType.Vitality] = (btn, statButtons[StatType.Vitality].plus);
                    break;
                //=================================================================================================== 
                case "Down0":
                    if (!statButtons.ContainsKey(StatType.Strength)) statButtons[StatType.Strength] = (btn, null);
                    else statButtons[StatType.Strength] = (btn, statButtons[StatType.Strength].minus);
                    break;
                case "Down1":
                    if (!statButtons.ContainsKey(StatType.Intelligence)) statButtons[StatType.Intelligence] = (btn, null);
                    else statButtons[StatType.Intelligence] = (btn, statButtons[StatType.Intelligence].minus);
                    break;
                case "Down2":
                    if (!statButtons.ContainsKey(StatType.Agility)) statButtons[StatType.Agility] = (btn, null);
                    else statButtons[StatType.Agility] = (btn, statButtons[StatType.Agility].minus);
                    break;
                case "Down3":
                    if (!statButtons.ContainsKey(StatType.Vitality)) statButtons[StatType.Vitality] = (btn, null);
                    else statButtons[StatType.Vitality] = (btn, statButtons[StatType.Vitality].minus);
                    break;
                case "Confirm":
                    confirmButton = btn;
                    break;
            }
        }

    }

    private void OnEnable()
    {
        ShowPlayerStats(); // 기본적으로 플레이어 패널 활성화

    }

    private void OnDisable()
    {
        
    }
    private void ShowPlayerStats()
    {
        playerPanel.SetActive(true);
        dragonbPanel.SetActive(false);
       // UpdateCommonPanel(CharType.Player);
    }

    private void ShowDragonStats()
    {
        playerPanel.SetActive(false);
        dragonbPanel.SetActive(true);
     //   UpdateCommonPanel(CharType.Dragon);
    }
}
