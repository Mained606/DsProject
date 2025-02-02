using UnityEngine;
using System.Collections;

public class GolemAI : BaseMonsterAI
{
    private WeaponCollider weaponCollider;

    protected override void Start()
    {
        base.Start();
        weaponCollider = GetComponentInChildren<WeaponCollider>();
    }
    
    // 애니메이션 이벤트에서 호출
    public void GolemEnableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.EnableWeaponCollider(true);
        }
    }
    
    public void GolemDisableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.EnableWeaponCollider(false);
        }
    }
}
