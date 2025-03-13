using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class NPCData : ISheetData
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

    public void ParseData(IList<object> row)
    {
        if (row.Count < 12) throw new Exception("NPC 데이터 부족");

        id = row[0].ToString();
        name = row[1].ToString();
        description = row[2].ToString();

        // NPC 타입 변환
        npcType = Enum.TryParse(row[3].ToString(), out NPCType parsedType) ? parsedType : NPCType.일반;

        // NPC 상태 변환
        currentState = Enum.TryParse(row[4].ToString(), out NPCState parsedState) ? parsedState : NPCState.중립;

        // 인터랙션 가능 여부 및 조건
        isInteractable = bool.TryParse(row[5].ToString(), out bool interact) ? interact : true;
        interactionCondition = row[6].ToString();

        // 이동 관련
        canMove = bool.TryParse(row[7].ToString(), out bool move) ? move : false;
        moveSpeed = float.TryParse(row[8].ToString(), out float speed) ? speed : 0f;

        // 이동 경로 변환 (쉼표로 구분된 Vector3 좌표 리스트)
        string[] patrolData = row[9].ToString().Split('|');
        patrolPoints = new Vector3[patrolData.Length];
        for (int i = 0; i < patrolData.Length; i++)
        {
            string[] coords = patrolData[i].Split(',');
            if (coords.Length == 3 &&
                float.TryParse(coords[0], out float x) &&
                float.TryParse(coords[1], out float y) &&
                float.TryParse(coords[2], out float z))
            {
                patrolPoints[i] = new Vector3(x, y, z);
            }
        }

        // 활동 시간
        hasSchedule = bool.TryParse(row[10].ToString(), out bool schedule) ? schedule : false;
        activeTime = row[11].ToString();

        // 상점 여부, 퀘스트 제공 여부
        isShop = bool.TryParse(row[12].ToString(), out bool shop) ? shop : false;
        isQuestGiver = bool.TryParse(row[13].ToString(), out bool questGiver) ? questGiver : false;
        reputationRequirement = int.TryParse(row[14].ToString(), out int rep) ? rep : 0;

        // 대화 내용 (쉼표로 구분된 배열)
        dialogue = row[15].ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        conditionalDialogue = row[16].ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        // 퀘스트 목록 (쉼표로 구분된 ID 리스트)
        string[] questIds = row[17].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        quests = new Quest[questIds.Length];
        for (int i = 0; i < questIds.Length; i++)
        {
            quests[i] = new Quest { id = questIds[i] }; // 기본적으로 ID만 설정
        }

        // 아이템 목록 (쉼표로 구분된 ID 리스트)
        string[] itemIds = row[18].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        items = new Item[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            items[i] = new Item { id = itemIds[i] }; // 기본적으로 ID만 설정
        }

        // 상점 데이터 (TODO: ShopData 클래스를 활용하여 로드)
        shopData = new ShopData();

        // 오디오 (쉼표로 구분된 파일 이름 -> Resources 폴더에서 로드)
        string[] voiceFiles = row[19].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        voiceLines = new AudioClip[voiceFiles.Length];
        for (int i = 0; i < voiceFiles.Length; i++)
        {
            voiceLines[i] = Resources.Load<AudioClip>($"Audio/NPC/{voiceFiles[i]}");
        }

        // 이펙트 (Resources에서 로드)
        string effectName = row[20].ToString();
        if (!string.IsNullOrEmpty(effectName))
        {
            interactionEffect = Resources.Load<ParticleSystem>($"Effects/NPC/{effectName}");
        }

        Debug.Log($"[NPCData] {name} 데이터 로드 완료!");
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
