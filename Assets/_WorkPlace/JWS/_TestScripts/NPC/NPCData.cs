using System;
using UnityEngine;

[Serializable]
public class NPCData
{
    public string id;
    public string name;
    public string description;
    public NPCType npcType;
    public Sprite sprite;
    public NPCState currentState = NPCState.중립;
    public GameObject currentNPC;

    public string[] dialogue;
    public string[] conditionalDialogue;
    public Quest[] quests;
    public Item[] items;
    public ShopData shopData;
    public bool isInteractable;
    public string interactionCondition;

    public bool canMove;
    public float moveSpeed;
    public Vector3[] patrolPoints;

    public bool hasSchedule;
    public string activeTime;

    public AudioClip[] voiceLines;
    public ParticleSystem interactionEffect;

    public bool isShop;
    public bool isQuestGiver;
    public int reputationRequirement;

    public NPCData Clone(bool deep)
    {
        NPCData clone = new NPCData();
        clone.id = id;
        clone.name = name;
        clone.description = description;
        clone.npcType = npcType;
        clone.sprite = null;
        clone.currentState = currentState;
        clone.currentNPC = null;
        clone.isInteractable = isInteractable;
        clone.interactionCondition = interactionCondition;
        clone.canMove = canMove;
        clone.moveSpeed = moveSpeed;
        clone.hasSchedule = hasSchedule;
        clone.activeTime = activeTime;
        clone.isShop = isShop;
        clone.isQuestGiver = isQuestGiver;
        clone.reputationRequirement = reputationRequirement;
        clone.shopData = shopData;

        if (deep)
        {
            clone.dialogue = (dialogue != null) ? (string[])dialogue.Clone() : null;
            clone.conditionalDialogue = (conditionalDialogue != null) ? (string[])conditionalDialogue.Clone() : null;
            clone.quests = (quests != null) ? quests : null;
            clone.items = (items != null) ? items : null;
            clone.patrolPoints = (patrolPoints != null) ? (Vector3[])patrolPoints.Clone() : null;
            clone.voiceLines = (voiceLines != null) ? (AudioClip[])voiceLines.Clone() : null;
        }
        else
        {
            clone.dialogue = dialogue;
            clone.conditionalDialogue = conditionalDialogue;
            clone.quests = quests;
            clone.items = items;
            clone.patrolPoints = patrolPoints;
            clone.voiceLines = voiceLines;
        }

        return clone;
    }
}

public enum NPCType
{
    일반,        // 특별한 역할 없이 대화만 가능
    상점,        // 아이템을 판매하거나 구매 가능
    퀘스트,      // 퀘스트 제공
    이벤트,      // 특정 이벤트 트리거
    정보제공,    // 게임 정보 제공 (지형, 몬스터 약점 등)
    상호작용,    // 상호작용 트리거 (문 열기, 장치 활성화 등)
    힐러,        // 체력을 일부 회복시키는 나중에 축복같은 버프부여로 연결가능
    적NPC,       // 적으로 변할 수 있는 NPC
    동료,        // 플레이어의 동료가 될 수 있는 NPC
}

public enum NPCState
{
    중립,       // 기본 상태
    동료,       // 플레이어의 동료
    적          // 적대 상태
}
