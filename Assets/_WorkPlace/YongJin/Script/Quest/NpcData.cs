using System.Collections.Generic;
using UnityEngine;


public class NPCData
{
    public string NPC_id; // NPC 고유 ID
    public string NPC_name; // NPC 이름
    public string NPC_description; // NPC 설명
    public NPCType NPC_type; // NPC 타입
    public NPCState currentState = NPCState.Common; // NPC 현재 상태
    public string[] dialogue; // 대화 내용
    public List<QuestTable> quests; // 제공 퀘스트용으로 사용하고 리스트로 대체해도 됨.
    public bool isInteractable; // 상호작용 가능 여부
    public string interactionCondition; // 상호작용 조건 설명(에디터용으로만 사용)
    public bool isShop; // 상점 여부
    public bool isQuestGiver; // 퀘스트 제공 여부

    //public Sprite sprite; // NPC 이미지
/*     // 행동/AI 관련
    public bool canMove; // 이동 가능 여부
    public float moveSpeed; // 이동 속도
    public Vector3[] patrolPoints; // 패트롤 경로 (상호작용 하기전 NPC만의 행동패턴)

    // 시간 및 스케줄
    public bool hasSchedule; // 스케줄 여부 ( 패트롤 혹은 시퀀스 타이밍을 위한 스케줄)
    public string activeTime; // 활성화 시간 (예: "09:00-18:00")
 */
/*     // 시각적/청각적 효과 ( 필요한 경우에만 쓰게끔 설정할필요있음 )
    public AudioClip[] voiceLines; // 대사 음성 있을 경우 이런 프로퍼티를 사용
    public ParticleSystem interactionEffect; // 상호작용 시 이펙트가 있을 경우 이런 프로퍼티를 사용 */

    // 상점/퀘스트 세부

    //public int reputationRequirement; // 평판 요구
    //public Item[] items; // 판매/교환 아이템 목록으로 사용하고 리스트로 대체해도 됨.
}





