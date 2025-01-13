public enum RequireDataType
{
    KillMosters,
    CollectItems,
    TalkToNPC,
    UseItems,
    ReachLocation
}
public enum NPCType
{
    Dialogue,        // 특별한 역할 없이 대화만 가능
    Shop,        // 아이템을 판매하거나 구매 가능
    Quest,      // 퀘스트 제공
    Event,      // 특정 이벤트 트리거
    InformationProvider,    // 게임 정보 제공 (지형, 몬스터 약점 등)
    Interect,    // 상호작용 트리거 (문 열기, 장치 활성화 등)
    Enemy,       // 적으로 변할 수 있는 NPC
    Partner,        // 플레이어의 동료가 될 수 있는 NPC
}
public enum NPCState
{
    Common,       // 기본 상태
    Partner,       // 플레이어의 동료
    Enemy          // 적대 상태
} 
public enum QuestInterectType
{
    PassiveInterect,
    ActiveInterect,
}


