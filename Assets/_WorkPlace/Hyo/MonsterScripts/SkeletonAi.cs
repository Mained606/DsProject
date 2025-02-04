using UnityEngine;
using System.Collections;

public class SkeletonAi : BaseMonsterAI
{
    private WeaponCollider weaponCollider;

    protected override void Start()
    {
        base.Start();
        weaponCollider = GetComponentInChildren<WeaponCollider>();
    }
    
    // 애니메이션 이벤트에서 호출
    public void SkeletonAiEnableWeaponCollider()
    {
        if (weaponCollider)
        {
            weaponCollider.EnableWeaponCollider(true);
        }
    }
    
    public void SkeletonAiDisableWeaponCollider()
    {
        if (weaponCollider)
        {
            weaponCollider.EnableWeaponCollider(false);
        }
    }
}