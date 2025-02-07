using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHudUI : MonoBehaviour
{
    BossData bossData;
    TextMeshProUGUI[] bossHudText;
    Image bossHealthBar;

    private void Awake()
    {
        bossHudText = transform.GetComponentsInChildren<TextMeshProUGUI>();
        bossHealthBar = transform.GetComponentInChildren<Image>();
    }

    private void OnDisable()
    {
        bossData = null;
    }

    public void Update()
    {
        if (bossData != null)
        {
            bossHudText[0].text = bossData.characterName;
            bossHudText[1].text = $"{bossData.currentHp} / {bossData.maxHp}";
            bossHealthBar.fillAmount = bossData.currentHp / bossData.maxHp;
        }
    }

    public void SetBossData(BossData bossData)
    {
        this.bossData = bossData;
    }
}
