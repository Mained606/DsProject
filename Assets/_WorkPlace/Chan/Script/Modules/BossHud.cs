using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    /* [SerializeField] private TextMeshProUGUI BossName;
     [SerializeField] private TextMeshProUGUI BossHP;
     [SerializeField] private Image BossHpBar;*/

    private TextMeshProUGUI BossName;
    private TextMeshProUGUI BossHP;
    private Image BossHpBar;

    private void Start()
    {
        BossName= GetComponentInChildren<TextMeshProUGUI>();
        BossHP = GetComponentInChildren<TextMeshProUGUI>();
        BossHpBar = GetComponentInChildren<Image>();
    }


    private void OnEnable()
    {
        BossName.text = UIManager.Instance.CurrentBossData.characterName;
        BossHP.text = $"{UIManager.Instance.CurrentBossData.currentHp} / {UIManager.Instance.CurrentBossData.maxHp}"; 
        BossHpBar.fillAmount = UIManager.Instance.CurrentBossData.currentHp / UIManager.Instance.CurrentBossData.maxHp;
    }
    private void OnDisable()
    {

    }


}
