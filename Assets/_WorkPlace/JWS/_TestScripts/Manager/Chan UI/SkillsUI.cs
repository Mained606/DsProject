using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    [SerializeField] private Animator[] skillSlotsAnimator;
    [SerializeField] private Image[] skillSlotImages;
    [SerializeField] private TextMeshProUGUI[] skillSlotText;
    [SerializeField] private Transform buffDisplayPosition;
    private PlayerData playerData;
    private List<Skills> playerSkillList = new List<Skills>();
    private int skillCount = 0;

    private void Start()
    {

        if (playerData == null) { playerData = CharacterManager.PlayerCharacterData; }
        if (SkillManager.Instance != null)
        {
            playerSkillList = SkillManager.SkillDatabase.playerSkills;
        }
        skillCount = playerSkillList.Count;
        InitskillSlotItems();
    }

    private void Update()
    {
        if (ItemManager.Instance != null) CheckskillSlot();
    }

    private void InitskillSlotItems()
    {
        for (int i = 0; i < skillCount; i++)
        {
            skillSlotText[i].text = playerSkillList[i].skillName;
        }
    }

    private void CheckskillSlot()
    {
        for (int i = 0; i < playerSkillList.Count; i++)
        {
            skillSlotImages[i].sprite = ItemManager.Instance.GetSkillSprite(playerSkillList[i].skillName);
            if (skillSlotsAnimator[i] != null)
            {
                // skillSlotText[i].transform.parent.gameObject.SetActive(playerSkillList[i].cooldownTimer.IsRunning ? true : false);
                if (playerSkillList[i].cooldownTimer.RemainingPercent > 0f)
                {
                    if (playerSkillList[i].cooldownTimer.IsRunning) skillSlotsAnimator[i].SetTrigger("Hover");
                    else skillSlotsAnimator[i].SetTrigger("Normal");
                    skillSlotImages[i].transform.GetChild(1).GetComponent<Image>().fillAmount = playerSkillList[i].cooldownTimer.IsRunning ? playerSkillList[i].cooldownTimer.RemainingPercent : 0;
                }
            }
        }
    }

}
