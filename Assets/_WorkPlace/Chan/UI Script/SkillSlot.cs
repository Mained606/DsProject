using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;
    private Animator animator;

    private Image cooldownOverlay; // Image[1] 기준으로 동적으로 가져오기
    private int slotIndex;
    private Skills currentSkill;
    private Sprite currentIcon;

    private void Awake()
    {
       // animator = transform.GetComponent<Animator>();
        cooldownOverlay = GetComponentsInChildren<Image>()[1];
    }

    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }

    public void SetSkill(Skills skill, Sprite icon = null)
    {
        currentSkill = skill;
        currentIcon = icon ?? ItemManager.Instance.GetSkillSprite(skill.skillName);

        iconImage.sprite = currentIcon;
        iconImage.enabled = true;

      //  animator?.SetTrigger("Set");
    }

    public void ClearSlot()
    {
        currentSkill = null;
        currentIcon = null;
        iconImage.sprite = null;
        iconImage.enabled = false;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        SkillDrag dragItem = eventData.pointerDrag?.GetComponent<SkillDrag>();
        if (dragItem != null)
        {
            SetSkill(dragItem.SkillData, dragItem.Icon);
        }
    }

    private void Update()
    {
        if (currentSkill == null || currentSkill.cooldownTimer == null) return;

        float percent = currentSkill.cooldownTimer.RemainingPercent;
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = percent;

       /* if (percent > 0f)
        {
            if (currentSkill.cooldownTimer.IsRunning)
                animator?.SetTrigger("Hover");
            else
                animator?.SetTrigger("Normal");
        }*/
    }
}
