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
    // ========== 250312 SH 추가 ==========
    public List<Skills> registedSkillList = new List<Skills>();
    private int skillIndex = 0;
    private int targetSkillIndex = 0;
    // ========== 250312 SH 추가 ==========
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

        // ========== 250312 SH 추가 ==========
        if (InputManager.InputActions.actions["SkillUI"].triggered)
        {
            if (registedSkillList.Count > 2) return;
            registedSkillList.Add(playerSkillList[skillIndex]);
            targetSkillIndex = playerSkillList.IndexOf(registedSkillList[skillIndex]);
            playerSkillList[targetSkillIndex].activeTriggerName = $"Skill_{skillIndex + 1}";
            InitskillSlotItems();
            skillIndex++;
        }
        // ========== 250312 SH 추가 ==========
    }

    private void InitskillSlotItems()
    {
        /* ========== 250312 SH 주석처리 ==========
        for (int i = 0; i < skillCount; i++)
        {
            skillSlotText[i].text = playerSkillList[i].skillName;
        }
        */

        // ========== 250312 SH 추가 ==========
        for (int i = 0; i < registedSkillList.Count; i++)
        {
            skillSlotText[i].text = registedSkillList[i].skillName;
        }
        // ========== 250312 SH 추가 ==========

    }

    private void CheckskillSlot()
    {
        /* ========== 250312 SH 주석 처리 ==========
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
            */

        if (registedSkillList.Count > 0)
        {
            for (int i = 0; i < registedSkillList.Count; i++)
            {
                skillSlotImages[i].sprite = ItemManager.Instance.GetSkillSprite(registedSkillList[i].skillName);
                if (skillSlotsAnimator[i] != null)
                {
                    if (registedSkillList[i].cooldownTimer.RemainingPercent > 0f)
                    {
                        if (registedSkillList[i].cooldownTimer.IsRunning) skillSlotsAnimator[i].SetTrigger("Hover");
                        else skillSlotsAnimator[i].SetTrigger("Normal");
                        skillSlotImages[i].transform.GetChild(1).GetComponent<Image>().fillAmount = playerSkillList[i].cooldownTimer.IsRunning ? playerSkillList[i].cooldownTimer.RemainingPercent : 0;
                    }
                }
            }
        }
    }
}
