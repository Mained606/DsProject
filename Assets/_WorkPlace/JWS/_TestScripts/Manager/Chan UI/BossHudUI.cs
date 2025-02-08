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
        bossHealthBar = transform.GetChild(1).GetComponent<Image>();
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
            bossHealthBar.fillAmount = (float)((float)bossData.currentHp / (float)bossData.maxHp);
            Debug.LogWarning("보스피 : " + bossHealthBar.fillAmount);
        }
    }

    public void SetBossData(BossData bossData)
    {
        this.bossData = bossData;
    }

    public Transform BossBuffZone()
    {
        return transform.GetChild(3);
    }
}
