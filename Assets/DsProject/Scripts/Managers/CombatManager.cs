using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : BaseManager<CombatManager>
{
    private List<string> blockSound = new List<string> { "Shield_Block_1", "Shield_Block_2", "Shield_Block_3" };
    private List<string> parrySound = new List<string> { "Parry_1", "Parry_2" };

    [SerializeField] private GameObject hitEffectNone;
    [SerializeField] private GameObject hitEffectFire;
    [SerializeField] private GameObject hitEffectWater;
    [SerializeField] private GameObject hitEffectElectric;
    [SerializeField] private GameObject hitEffectEarth;
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        
    }
    
    // 공격 처리 메서드
    public void ProcessAttack(CharacterData playerData, CharacterData monsterData, Transform defenderTransform, bool isPlayerAttacking, bool isMagicAttack, Skills skills = null, bool isBossAttacking = false, ElementalAttribute debuffType = ElementalAttribute.None, float debuffDuration = 0f, float debuffValue = 0f)
    {
        // 공격자와 방어자 설정
        CharacterData actualAttacker = isPlayerAttacking ? playerData : monsterData;
        CharacterData actualDefender = isPlayerAttacking ? monsterData : playerData;
        Transform attackerTransform = isPlayerAttacking ? GameManager.playerTransform : defenderTransform;
        Transform defenderPosition = isPlayerAttacking ? defenderTransform : GameManager.playerTransform;
        
        if (actualDefender == null)
        {
            Debug.LogWarning("actualDefender가 null입니다!");
            return;
        }

        if (isBossAttacking)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        }
        else
        {
            //Debug.Log($"현재 대상 타입: {actualDefender.characterType}");
            // 공격 대상이 보스인지 확인
            GameStateMachine.Instance.ChangeState(actualDefender.characterType == CharacterType.Boss
                ? GameSystemState.BossBattle
                : GameSystemState.Combat);
        }
        
        // 현재 타겟의 실제높이 계산을 위한부분
        Collider collider = defenderTransform.GetComponent<Collider>();
        float characterHeight = collider != null ? collider.bounds.size.y : 0f;
        float targetHeight = defenderTransform.position.y + characterHeight;// ( (characterHeight == 0f ? 2f : characterHeight));

        // 현재 타겟의 실제높이 계산을 위한부분
        Vector3 targetPosition = new Vector3(defenderTransform.position.x, targetHeight, defenderTransform.position.z);

        if (actualDefender == null || actualDefender.currentHp <= 0)
        {
            // //Debug.Log(actualDefender.currentHp);
            Debug.LogWarning($"{actualDefender.characterName}는 이미 사망했습니다.");
            return;
        }
        
        // 지금 기준은 플레이어만 회피 및 블록 처리
        if (!isPlayerAttacking)
        {
            // 회피율 적용
            if (Random.value < actualDefender.dodgeChance)
            {
                //Debug.Log($"{actualDefender.characterName}는 공격을 회피했습니다.");
                return; // 공격이 회피되어 데미지 없음
            }
            
            // 방패 블락율 처리
            if (actualDefender.hasShield)
            {
                // 방패 블록 확률이 적용되는 경우
                if (Random.value < actualDefender.blockChance)
                {
                    //Debug.Log($"{actualDefender.characterName}는 방패로 공격을 차단했습니다!");
                    return; // 방패 차단
                }
            }
        }
        
        // 크리티컬 체크: 물리 공격 또는 마법 공격에 대해 크리티컬 확률 계산
        bool isCritical = Random.value < actualAttacker.criticalChance;
        
        // 기본 데미지 계산
        float damage = 0f;
        
        if (isMagicAttack)
        {
            // 마법 공격
            damage = actualAttacker.magicDamage * (1 - actualDefender.magicDamageReduction);
        }
        
        else
        {
            // 물리 공격
            damage = actualAttacker.physicalDamage * (1 - actualDefender.physicalDamageReduction);
        }
        
        // 스킬 배율이 있을 때만 적용 (배율이 없으면 기본값 1을 사용)
        if (skills != null)
        {
            damage = skills.currentDamage;
        }
        
        ElementalAttribute attackerEffectiveAttribute = actualAttacker.attribute;
        ElementalAttribute defenderEffectiveAttribute = actualDefender.attribute;
        
        // 만약 공격자가 플레이어라면, 무기/스킬에 따라 속성을 가져옴
        if (isPlayerAttacking && actualAttacker is PlayerData playerAttacker)
        {
            attackerEffectiveAttribute = playerAttacker.GetEffectiveAttackAttribute(skills != null, skills);
        }

        if (isBossAttacking && actualAttacker is BossData bossAttacker && skills != null)
        {
            attackerEffectiveAttribute = skills.attribute;
        }
        
        // 만약 방어자가 플레이어라면, 방어구 속성을 사용
        if (actualDefender is PlayerData playerDefender)
        {
            defenderEffectiveAttribute = playerDefender.GetEffectiveDefenseAttribute();
        }
        
        // 속성 효과 적용
        float elementalMultiplier = AttributeCalculator.GetMultiplier(attackerEffectiveAttribute, defenderEffectiveAttribute);
        
        // 땅 속성 버프 확인 및 적용
        if (attackerEffectiveAttribute == ElementalAttribute.Earth)
        {
            // 공격자에게 적용된 땅 속성 효과가 있는지 확인하고 있으면 데미지 증가
            float earthBuffMultiplier = 1.0f;
            
            // 무기 효과가 활성화 상태인지 확인 (플레이어의 일반 공격인 경우만)
            bool weaponEffectActive = true;
            if (isPlayerAttacking && skills == null && ItemSkillManager.Instance != null)
            {
                weaponEffectActive = ItemSkillManager.Instance.IsActive;
                if (!weaponEffectActive)
                {
                    //Debug.Log("무기 효과가 비활성화 상태입니다. 땅 속성 데미지 증가가 적용되지 않습니다.");
                }
            }
            
            // 무기 효과가 활성화된 경우에만 증가 효과 적용
            if (weaponEffectActive)
            {
                // 통합된 새 시스템 사용
                EarthDamageEffect earthEffect = ElementalEffectManager.Instance?.GetEarthDamageEffect(actualAttacker);
                if (earthEffect != null)
                {
                    earthBuffMultiplier = earthEffect.GetDamageMultiplier();
                }
                // 일반 공격(스킬 없음)이고 플레이어가 공격할 때 무기의 속성 효과 적용
                else if (isPlayerAttacking && !isBossAttacking && skills == null)
                {
                    // 장착된 무기 정보 가져오기
                    Item equippedWeapon = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손);
                    if (equippedWeapon != null && equippedWeapon.itemSkill != null && 
                        equippedWeapon.itemSkill.element == ElementalAttribute.Earth && 
                        equippedWeapon.itemSkill.debuffValue > 0)
                    {
                        // 무기의 땅 속성 효과 적용
                        earthBuffMultiplier = 1f + (equippedWeapon.itemSkill.debuffValue / 100f);
                        actualAttacker.ApplyEarthDamageEffect(equippedWeapon.itemSkill.debuffDuration, equippedWeapon.itemSkill.debuffValue);
                    }
                }
                // 스킬 사용 시 땅 속성 효과 적용
                else if (skills != null && skills.debuffValue > 0)
                {
                    earthBuffMultiplier = 1f + (skills.debuffValue / 100f);
                    
                    // 스킬 사용 시 효과 적용
                    if (isPlayerAttacking && !isBossAttacking)
                    {
                        actualAttacker.ApplyEarthDamageEffect(skills.debuffDuration, skills.debuffValue);
                    }
                }
            }
            
            damage = Mathf.RoundToInt(damage * earthBuffMultiplier);
            //Debug.Log($"땅 속성 데미지 증가 효과: {earthBuffMultiplier}배 증가");
        }
        
        // 최종 데미지에 속성 배율 적용
        damage = Mathf.RoundToInt(damage * elementalMultiplier);
        
        //Debug.Log($"공격자 속성: {attackerEffectiveAttribute} / 방어자 속성: {defenderEffectiveAttribute} / 속성 배율: {elementalMultiplier}");
        
        // 크리티컬 데미지 배율 적용
        if (isCritical)
        {
            damage *= actualAttacker.criticalDamage;
            //Debug.Log($"{actualAttacker.characterName}가 크리티컬 히트를 발생시켰습니다!");
        }
        
        // 최소 데미지는 0으로 처리
        int finalDamage = Mathf.Max(0, (int)damage);
        
        // 데미지 적용
        if (damage > 0)
        {
            // 250205 3:40PM Hyeon : 패링 성공 처리
            if (GameManager.playerTransform.GetComponent<PlayerCombat>().onParry)
            {
                UIManager.DisplayPopupText("패링", targetPosition, isPlayerAttacking ? MessageTag.플레이어_피해 : MessageTag.적_피해);
                SoundManager.Instance.RandomPlay(parrySound, GameManager.playerTransform, 0.5f);
                BaseMonsterAI monsterAI = attackerTransform.GetComponentInParent<BaseMonsterAI>();
                if (monsterAI != null)
                {
                    // 패링 시 기본 스턴 지속시간 적용 (기본값 사용)
                    monsterAI.ApplyStun();
                }
                else
                {
                    // 기존 방식 유지
                    attackerTransform.GetComponentInParent<BaseMonsterAI>().ChangeState(BaseMonsterAI.AIState.Stun);
                }
                return;
            }
            
            if (GameManager.playerTransform.GetComponent<PlayerCombat>().isBlocking && !GameManager.playerTransform.GetComponent<PlayerController>().isParry)
            {
                UIManager.DisplayPopupText("방어", targetPosition, isPlayerAttacking ? MessageTag.플레이어_피해 : MessageTag.적_피해);
                actualDefender.TakeDamage(0, attackerTransform);
                SoundManager.Instance.RandomPlay(blockSound, GameManager.playerTransform, 0.5f);
                // Damage에 따라 방어 성공 or 방어 실패(break) 판단 필요할 수 있음
                return;
            }

            if (GameManager.playerTransform.GetComponent<PlayerController>().isInvincible)
            {
                UIManager.DisplayPopupText("무효", targetPosition, isPlayerAttacking ? MessageTag.플레이어_피해 : MessageTag.적_피해);
                return;
            }
            
            if(!isPlayerAttacking && GameManager.playerTransform.GetComponent<PlayerController>().cheatMode)
            {
                UIManager.DisplayPopupText("무적", targetPosition, isPlayerAttacking ? MessageTag.플레이어_피해 : MessageTag.적_피해);
                return;
            }
            // 250131 2:00PM Hyeon ===============================================
            actualDefender.TakeDamage(finalDamage, attackerTransform);
            
            if (skills != null)
            {
                int levelPoint = isCritical ? 2 : 1;
                skills.AddExperience(levelPoint);
            }

            // 250331 4:00PM SH ====================
            GameObject obj;
            if (!isPlayerAttacking && skills == null)
            {
                obj = Instantiate(hitEffectNone, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                Destroy(obj, 2f);
            }
            else if (skills != null)
            { 
                switch (skills.attribute)
                {
                    case ElementalAttribute.Fire:
                        obj = Instantiate(hitEffectFire, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                        break;
                    case ElementalAttribute.Water:
                        obj = Instantiate(hitEffectWater, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                        break;
                    case ElementalAttribute.Electric:
                        obj = Instantiate(hitEffectElectric, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                        break;
                    case ElementalAttribute.Earth:
                        obj = Instantiate(hitEffectEarth, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                        break;
                    default:
                        obj = Instantiate(hitEffectNone, defenderPosition.position + Vector3.up * 1.5f, Quaternion.identity);
                        break;
                }
                Destroy(obj, 2f);
            }
            // 250331 4:00PM SH ====================

            // 디버프 적용 로직
            if (isPlayerAttacking && skills == null)
            {
                // 플레이어의 경우 아이템 스킬 매니저 사용
                Item equippedWeapon = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손);
                ItemSkillManager.Instance.ElementAttack(equippedWeapon, actualDefender);
            }
            else if (debuffType != ElementalAttribute.None)
            {
                // 몬스터의 경우 새로운 통합 시스템 사용
                //Debug.Log($"속성 효과 적용: {debuffType}, 지속시간 {debuffDuration}초, 수치 {debuffValue}");

                switch (debuffType)
                {
                    case ElementalAttribute.Fire:
                        //Debug.Log($"화상 효과 적용: 지속시간 {debuffDuration}초, 매 초마다 최대 체력의 {debuffValue}% 피해");
                        actualDefender.ApplyFireBurnEffect(debuffDuration, debuffValue);
                        break;
                    case ElementalAttribute.Water:
                        //Debug.Log($"이동속도 감소 효과 적용: 지속시간 {debuffDuration}초, 이동속도 {debuffValue}% 감소");
                        actualDefender.ApplyWaterSlowEffect(debuffDuration, debuffValue);
                        break;
                    case ElementalAttribute.Electric:
                        //Debug.Log($"스턴 효과 적용: 지속시간 {debuffDuration}초");
                        actualDefender.ApplyElectricStunEffect(debuffDuration);
                        break;
                    case ElementalAttribute.Earth:
                        // 보스가 땅 속성 스킬을 사용하면 자신에게 데미지 증가 효과
                        //Debug.Log($"땅 속성 데미지 증가 효과 적용: 지속시간 {debuffDuration}초, 데미지 {debuffValue}% 증가");
                        actualAttacker.ApplyEarthDamageEffect(debuffDuration, debuffValue);
                        break;
                }
            }
        }
        //
        // UI에 데미지 텍스트 표시
        if (damage > 0)
        {
            UIManager.DisplayPopupText(finalDamage.ToString(), targetPosition, isPlayerAttacking ? MessageTag.적_피해 : MessageTag.플레이어_피해);
        }

        // 대상이 사망했는지 확인
        CheckAndHandleDeath(actualDefender, defenderTransform, isPlayerAttacking);
    }
    
    public void ProcessDragonAttack(DragonData dragonData, CharacterData targetData, Transform targetTransform, bool isMagicAttack, float skillMultiplier = 1f)
    {
        
        // 현재 타겟의 실제높이 계산을 위한부분
        Collider collider = targetTransform.GetComponent<Collider>();
        float characterHeight = collider != null ? collider.bounds.size.y : 0f;
        float targetHeight = targetTransform.position.y + characterHeight;// ( (characterHeight == 0f ? 2f : characterHeight));

        // 현재 타겟의 실제높이 계산을 위한부분
        Vector3 targetPosition = new Vector3(targetTransform.position.x, targetHeight, targetTransform.position.z);
        
        // 타겟의 데이터가 없거나 체력이 0이면 공격하지 않음
        if (targetData == null || targetData.currentHp <= 0)
        {
            Debug.LogWarning($"{targetData.characterName}는 이미 사망했습니다.");
            return;
        }

        // 회피율 적용 (타겟이 회피하면 공격하지 않음)
        if (Random.value < targetData.dodgeChance)
        {
            //Debug.Log($"{targetData.characterName}는 공격을 회피했습니다.");
            return;
        }

        // 방패 블락 확률 처리
        if (targetData.hasShield)
        {
            if (Random.value < targetData.blockChance)
            {
                //Debug.Log($"{targetData.characterName}는 방패로 공격을 차단했습니다!");
                return; // 방패 차단
            }
        }

        // 데미지 계산
        float damage = 0f;
        if (isMagicAttack)
        {
            // 마법 공격 처리 + 플레이어 데미지의 10%
            // 복사본을 사용하여 원본 데이터를 변경하지 않음
            float playerMagicDamageBonus = CharacterManager.PlayerCharacterData.magicDamage * 0.1f;
            damage = (dragonData.magicDamage + playerMagicDamageBonus) * (1 - targetData.magicDamageReduction);
        }
        else
        {
            // 물리 공격 처리 용 데미지 + 플레이어 데미지의 10%
            // 복사본을 사용하여 원본 데이터를 변경하지 않음
            float playerPhysicalDamageBonus = CharacterManager.PlayerCharacterData.physicalDamage * 0.1f;
            damage = (dragonData.physicalDamage + playerPhysicalDamageBonus) * (1 - targetData.physicalDamageReduction);
        }
        
        // 스킬 배율이 있을 때만 적용 (배율이 없으면 기본값 1을 사용)
        if (skillMultiplier > 1f)
        {
            damage *= skillMultiplier;
        }
        
        float attributeMultiplier = AttributeCalculator.GetMultiplier(dragonData.dragonAttribute, targetData.attribute);
        damage *= attributeMultiplier;
        //Debug.Log($"드래곤 공격자 속성: {dragonData.dragonAttribute} / 방어자 속성: {targetData.attribute} / 속성 배율: {attributeMultiplier}");

        // 크리티컬 체크: 드래곤의 크리티컬 확률
        bool isCritical = Random.value < dragonData.criticalChance;
        if (isCritical)
        {
            damage *= dragonData.criticalDamage;
            //Debug.Log($"{dragonData.characterName}가 크리티컬 히트를 발생시켰습니다!");
        }

        // 최소 데미지는 0으로 설정
        int finalDamage = Mathf.Max(0, (int)damage);

        // 데미지 적용
        if (damage > 0)
        {
            targetData.TakeDamage(finalDamage, targetTransform);
        }

        // UI에 데미지 표시
        UIManager.DisplayPopupText(finalDamage.ToString(), targetPosition, MessageTag.적_피해);

        // //Debug.Log($"{dragonData.characterName}가 {targetData.characterName}에게 {finalDamage}의 데미지를 입혔습니다.");
        // //Debug.Log($"{targetData.characterName}의 체력이 {targetData.currentHp} 만큼 남았습니다.");

        // 대상이 사망했는지 확인
        if (targetData.currentHp <= 0)
        {
            HandleDefeated(targetData, targetTransform);
            QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Kill, dragonData.characterName, 1);
        }
    }

    public void CheckAndHandleDeath(CharacterData actualDefender, Transform defenderTransform, bool isPlayerAttacking)
    {
        if (actualDefender.currentHp <= 0)
        {
            if (!isPlayerAttacking)
            {
                // 플레이어 사망 처리
                // GameStateMachine.Instance.ChangeState(GameSystemState.GameOver);
                // Debug.LogError("플레이어 사망");
                return;
            }
            HandleDefeated(actualDefender, defenderTransform);
            // //Debug.Log(actualAttacker.ToStringForTMPro());
            QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Kill, actualDefender.characterName, 1);
        }
    }

    // 몬스터 사망 처리
    private void HandleDefeated(CharacterData defeatedCharacter, Transform defenderTransform)
    {
        // defeatedCharacter가 BaseMonsterData를 갖고 있는지 확인
        BaseMonsterData baseMonsterData = defenderTransform.GetComponent<BaseMonsterData>();

        if (baseMonsterData != null)
        {
            switch (baseMonsterData.currentType)
            {
                case SpawnnerType.Monster:
                    CharacterManager.Instance.OnMonsterDefeated(baseMonsterData.GetMonsterData(), defenderTransform.position);
                    break;

                case SpawnnerType.Boss:
                    CharacterManager.Instance.OnBossDefeated(baseMonsterData.GetBossData(), defenderTransform.position);
                    break;
            }
        }
    }
}
