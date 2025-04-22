using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleStatusUI : MonoBehaviour
{
    [SerializeField] private Image[] fillbars; 
    [SerializeField] private TextMeshProUGUI[] valueTexts;

    private void Update()
    {
        UpdateStatus(); 
    }

    private void UpdateStatus()
    {
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = CharacterManager.PlayerCharacterData.characterName;
        transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"LV.{CharacterManager.PlayerCharacterData.level}";
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        if (fillbars.Length < 3) return;

        valueTexts[0].text = $"{CharacterManager.PlayerCharacterData.currentHp:F0} / {CharacterManager.PlayerCharacterData.maxHp}";
        fillbars[0].fillAmount = (float)CharacterManager.PlayerCharacterData.currentHp / CharacterManager.PlayerCharacterData.maxHp;

        valueTexts[1].text = $"{CharacterManager.PlayerCharacterData.currentMp:F0} / {CharacterManager.PlayerCharacterData.maxMp}";
        fillbars[1].fillAmount = (float)CharacterManager.PlayerCharacterData.currentMp / CharacterManager.PlayerCharacterData.maxMp;

        valueTexts[2].text = $"{CharacterManager.PlayerCharacterData.staminaCurrent:F0} / {CharacterManager.PlayerCharacterData.stamina}";
        fillbars[2].fillAmount = (float)CharacterManager.PlayerCharacterData.staminaCurrent / CharacterManager.PlayerCharacterData.stamina;
    }
}
