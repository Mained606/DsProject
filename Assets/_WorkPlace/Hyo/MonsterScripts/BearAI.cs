using UnityEngine;
using System.Collections;

public class BearAI : BaseMonsterAI
{
    private WeaponCollider weaponCollider;

    protected override void Start()
    {
        base.Start();
        weaponCollider = GetComponentInChildren<WeaponCollider>();
    }
    
    // 애니메이션 이벤트에서 호출
    public void BearEnableWeaponCollider()
    {
        if (weaponCollider)
        {
            weaponCollider.EnableWeaponCollider(true);
        }
    }
    
    public void BearDisableWeaponCollider()
    {
        if (weaponCollider)
        {
            weaponCollider.EnableWeaponCollider(false);
        }
    }
}
