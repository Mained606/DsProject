using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private GameObject defaultPanel;// 공용 네임, 레벨, 경험치 판넬
    [SerializeField] private GameObject playerPanel; // 플레이어 스탯판넬 
    [SerializeField] private GameObject dragonbPanel;// 드래곤 스탯판넬

    PlayerData playerData; 
    DragonData dragonData;

    private TextMeshProUGUI[] PlayerStats;
    private TextMeshProUGUI[] DragonStats;
    private TextMeshProUGUI[] DefaultText; // 공용 판넬
    private Image[] DeFaultImage;
    private Button[] buttons;


    private void Awake()
    {
        // CharacterManager에서 PlayerCharacterData 가져오기
        playerData = CharacterManager.PlayerCharacterData;
        dragonData = CharacterManager.DragonData;
        if (playerData == null && dragonData == null)
        {
            Debug.LogError("캐릭터 데이터가 없어욘");
            return;
        }

        PlayerStats = playerPanel.GetComponentsInChildren<TextMeshProUGUI>();
        DragonStats = dragonbPanel.GetComponentsInChildren<TextMeshProUGUI>();

        DeFaultImage = defaultPanel.GetComponentsInChildren<Image>();
        DefaultText = defaultPanel.GetComponentsInChildren<TextMeshProUGUI>();

    }

    private void OnEnable()
    {
        


    }

}
