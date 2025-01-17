using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleStatusUI : MonoBehaviour
{

    private void Update()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = CharacterManager.PlayerCharacterData.characterName;
        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Lv\n{CharacterManager.PlayerCharacterData.level}";
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            TextMeshProUGUI amount = transform.GetChild(0).GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            Image fillbar = amount.transform.parent.GetComponent<Image>();
            switch (i)
            {
                case 0:
                    amount.text = $"{CharacterManager.PlayerCharacterData.currentHp} / {CharacterManager.PlayerCharacterData.maxHp}";
                    break;
                case 1:
                    amount.text = $"{CharacterManager.PlayerCharacterData.currentMp} / {CharacterManager.PlayerCharacterData.maxMp}";
                    break;
                case 2:
                    amount.text = $"{CharacterManager.PlayerCharacterData.staminaCurrent} / {CharacterManager.PlayerCharacterData.stamina}";
                    break;
            }
        }
    }
}
