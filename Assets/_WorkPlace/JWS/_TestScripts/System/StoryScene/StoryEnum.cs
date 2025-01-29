public enum StoryState
{
    Idle,       // 대기 상태
    Playing,    // 현재 시퀀스 실행 중
    Waiting,    // 플레이어 입력 대기 중
    Transition, // 씬 전환 중
    Completed   // 스토리 종료
}

public enum SequenceTriggerType
{
    None, OnDialogEnd, OnCharacterMove, OnAnimationEnd, OnInteraction
}

public enum CharacterActionType
{
    None,
    Walk,
    Run,
    Attack,
    Emote
}