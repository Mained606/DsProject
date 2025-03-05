using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 불은 밸런스 / 얼음은 공격력 / 전기는 공속
/// 불은 지속 뎀
/// 얼음은 동결
/// 전기 지속 뎀
/// 속성 무기는 여러번 때리면 카운트가 올라가고 카운트 다 쓰면 일반 무기처럼... 일정 시간이 지나면 다시 충전
/// </summary>
[Serializable]
public class ItemSkill
{
    public string skillName;        //이름
    public string description;      //설명
    public ElementType element;     //속성
    public int level = 0;           //아이템 레벨 (강화)
    public GameObject weaponEffect; //무기 자체에 적용된 효과
    public GameObject attackEffect; //공격 시 나오는 효과 (타겟에 적용)

    public float power = 10;        //공격력 + (무기)
    //public float deffence = 1;      //방어력 + (방어구)
    public float attackSpeed = 0;   //공격 속도 +

    [HideInInspector] public float count;
    [HideInInspector] public bool isActive = true;

    //아이템 강화, 습득시에 초기화
    public void Initialize()
    {
        AdjustElementValue();
    }

    public void AdjustElementValue()
    {
        switch (element)
        {
            case ElementType.Fire:  //공격력, 공속 밸런스
                power += 5;
                attackSpeed += 2;   
                break;

            case ElementType.Ice:   //공격력 증가, 공속 감소
                power += 10;
                attackSpeed -= 2;    
                break;

            case ElementType.Electric:  //공격력 소폭 증가, 공속 증가
                power += 3;
                attackSpeed += 5;
                break;
        }
    }    
}

//속성 타격 누적 카운트
public class CountCoroutine
{
    public int count;
    public Coroutine resetCoroutine;
}

public enum ElementType
{
    None,
    Fire,
    Ice,
    Electric
}