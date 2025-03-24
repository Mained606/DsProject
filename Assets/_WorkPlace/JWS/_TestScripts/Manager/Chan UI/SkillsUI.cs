using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 스킬 퀵슬롯ui
/// </summary>
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

    // UI가 비활성화되어도 타이머 상태를 유지하기 위한 변수들
    // 버그 수정: UI 창을 열고 닫을 때 타이머 상태가 유지되지 않는 문제 해결을 위한 캐시 딕셔너리
    private Dictionary<Skills, float> lastFillAmounts = new Dictionary<Skills, float>();
    private bool isVisible = true;

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
      /*  if (InputManager.InputActions.actions["SkillUI"].triggered)
        {
            if (registedSkillList.Count > 2) return;
            registedSkillList.Add(playerSkillList[skillIndex]);
            InitskillSlotItems();
            skillIndex++;
        }*/
        // ========== 250312 SH 추가 ==========
    }

    // 2025-03-24 HYO 추가 --------------------------------------------------------------------------------------
    // 버그 수정: UI가 활성화될 때 호출되는 Unity 라이프사이클 메서드
    private void OnEnable()
    {
        isVisible = true;
    }

    // 버그 수정: UI가 비활성화될 때 호출되는 Unity 라이프사이클 메서드
    // UI가 닫힐 때 현재 쿨타임 상태를 캐시에 저장
    private void OnDisable()
    {
        isVisible = false;
        // UI가 비활성화되어도 타이머 상태를 캐시하여 저장
        SaveTimerStates();
    }

    // 버그 수정: UI가 비활성화될 때 각 스킬의 쿨타임 상태를 저장하는 메서드
    private void SaveTimerStates()
    {
        if (registedSkillList.Count > 0)
        {
            for (int i = 0; i < registedSkillList.Count; i++)
            {
                if (registedSkillList[i].cooldownTimer.IsRunning)
                {
                    lastFillAmounts[registedSkillList[i]] = registedSkillList[i].cooldownTimer.RemainingPercent;
                }
            }
        }
    }
    // -----------------------------------------------------------------------------------------------------------

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

        // 2025-03-24 HYO 추가 --------------------------------------------------------------------------------------
        if (registedSkillList.Count > 0)
        {
            for (int i = 0; i < registedSkillList.Count; i++)
            {
                Skills currentSkill = registedSkillList[i];
                skillSlotImages[i].sprite = ItemManager.Instance.GetSkillSprite(currentSkill.skillName);
                
                if (skillSlotsAnimator[i] != null)
                {
                    // 버그 수정: 쿨타임이 있는 스킬인 경우 기존 RemainingPercent 뿐만 아니라
                    // 캐시된 값도 함께 확인하여 UI 상태가 끊기지 않게 함
                    if (currentSkill.cooldownTimer.RemainingPercent > 0f || 
                        (lastFillAmounts.ContainsKey(currentSkill) && lastFillAmounts[currentSkill] > 0f))
                    {
                        // 타이머가 실행 중이면 Hover 트리거 사용, 아니면 Normal
                        if (currentSkill.cooldownTimer.IsRunning) 
                        {
                            skillSlotsAnimator[i].SetTrigger("Hover");
                        }
                        else 
                        {
                            skillSlotsAnimator[i].SetTrigger("Normal");
                        }

                        // 버그 수정: 타이머가 실행 중이면 RemainingPercent 사용, 아니면 캐시된 값 사용
                        float fillAmount = 0f;
                        
                        if (currentSkill.cooldownTimer.IsRunning)
                        {
                            fillAmount = currentSkill.cooldownTimer.RemainingPercent;
                            // 최신 값을 캐시에 저장
                            lastFillAmounts[currentSkill] = fillAmount;
                        }
                        else if (lastFillAmounts.ContainsKey(currentSkill))
                        {
                            fillAmount = lastFillAmounts[currentSkill];
                            
                            // 버그 수정: TimerManager에 등록되지 않았지만 쿨다운이 완료되지 않은 경우
                            // 직접 시간이 경과한 만큼 fillAmount를 감소시킴
                            if (fillAmount > 0f && Time.deltaTime > 0)
                            {
                                // 남은 시간 계산 (쿨다운 기간 * 남은 비율)
                                float remainingTime = currentSkill.cooldown * fillAmount;
                                remainingTime -= Time.deltaTime;
                                
                                // 남은 비율 재계산
                                fillAmount = Mathf.Max(0, remainingTime / currentSkill.cooldown);
                                lastFillAmounts[currentSkill] = fillAmount;
                                
                                // 쿨다운이 완료되었으면 타이머 초기화
                                if (fillAmount <= 0f)
                                {
                                    lastFillAmounts.Remove(currentSkill);
                                }
                            }
                        }

                        // UI에 fillAmount 적용
                        Image fillImage = skillSlotImages[i].transform.GetChild(1).GetComponent<Image>();
                        if (fillImage != null)
                        {
                            fillImage.fillAmount = fillAmount;
                        }
                    }
                    else
                    {
                        // 쿨타임이 없거나 완료된 스킬
                        skillSlotsAnimator[i].SetTrigger("Normal");
                        Image fillImage = skillSlotImages[i].transform.GetChild(1).GetComponent<Image>();
                        if (fillImage != null)
                        {
                            fillImage.fillAmount = 0;
                        }
                        
                        // 캐시된 값이 있으면 제거
                        if (lastFillAmounts.ContainsKey(currentSkill))
                        {
                            lastFillAmounts.Remove(currentSkill);
                        }


                    }
                }
            }
        }
        // -----------------------------------------------------------------------------------------------------------
    }
}
