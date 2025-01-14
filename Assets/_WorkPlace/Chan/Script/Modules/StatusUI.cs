using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> statSlotPrefabs; // 스탯 슬롯 프리팹 리스트
    [SerializeField] private Transform statPanelParent;       // 슬롯 부모 오브젝트

    private CharacterData characterData; // 캐릭터 데이터를 참조할 필드

    // CharacterData를 바인딩
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (characterData == null)
        {
            Debug.LogWarning("CharacterData가 설정되지 않았습니다.");
            return;
        }

        // 기존 슬롯 제거
        ClearStatsUI();

        // 스탯 데이터를 가져와 슬롯 생성
        Dictionary<string, string> stats = new Dictionary<string, string>
        {
            { "체력", $"{characterData.currentHp}/{characterData.maxHp}" },
            { "스태미나", $"{characterData.staminaCurrent}/{characterData.stamina}" },
            { "힘", $"{characterData.strength}" },
            { "지능", $"{characterData.intelligence}" }
        };

        int prefabIndex = 0; // 번갈아가며 선택할 프리팹 인덱스

        foreach (var stat in stats)
        {
            // 프리팹 선택
            GameObject prefabToUse = statSlotPrefabs[prefabIndex];

            // 슬롯 생성
            GameObject slot = Instantiate(prefabToUse, statPanelParent);

            // 슬롯 텍스트 바인딩
            TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = stat.Key;   // 스탯 이름
                texts[1].text = stat.Value; // 스탯 값
            }

            // 다음 프리팹 선택
            prefabIndex = (prefabIndex + 1) % statSlotPrefabs.Count; // 프리팹 인덱스 번갈아 변경
        }
    }

    private void ClearStatsUI()
    {
        foreach (Transform child in statPanelParent)
        {
            Destroy(child.gameObject);
        }
    }
}
