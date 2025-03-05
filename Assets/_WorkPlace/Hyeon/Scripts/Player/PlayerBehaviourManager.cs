using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviourManager : BaseManager<PlayerBehaviourManager>
{
    private List<IBehaviour> activeBehaviours = new List<IBehaviour>();       // 현재 가동 가능한 행동 리스트
    [SerializeField] private IBehaviour exclusiveBehaviour = null;
    private Queue<IBehaviour> removeQueue = new Queue<IBehaviour>();    // 지워져야 할 행동 큐
    private PlayerController controller;

    private Dictionary<Func<bool>, Type> behaviourMappings;             // can bool 변수에 따라 각자에게 맞는 행동 매핑

    [SerializeField] private bool canMove;
    [SerializeField] private bool canJump;
    [SerializeField] private bool canClimb;
    [SerializeField] private bool canGliding;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool canUseSkill;
    [SerializeField] private bool canBlock;
    [SerializeField] private bool canParry;
    [SerializeField] private bool canDodge;
    [SerializeField] private bool canWeaponSwitch;

    #region Bool Properties
    public bool CanMove { get => canMove; set { if (canMove != value) { canMove = value; UpdateBehaviours(); } } }
    public bool CanJump { get => canJump; set { if (canJump != value) { canJump = value; UpdateBehaviours(); } } }
    public bool CanClimb { get => canClimb; set { if (canClimb != value) { canClimb = value; UpdateBehaviours(); } } }
    //public bool CanGliding { get => canGliding; set { if (canGliding != value) { canGliding = value; UpdateBehaviours(); } } }
    public bool CanAttack { get => canAttack; set { if (canAttack != value) { canAttack = value; UpdateBehaviours(); } } }
    public bool CanUseSkill { get => canUseSkill; set { if (canUseSkill != value) { canUseSkill = value; UpdateBehaviours(); } } }
    public bool CanBlock { get => canBlock; set { if (canBlock != value) { canBlock = value; UpdateBehaviours(); } } }
    public bool CanParry { get => canParry; set { if (canParry != value) { canParry = value; UpdateBehaviours(); } } }
    public bool CanDodge { get => canDodge; set { if (canDodge != value) { canDodge = value; UpdateBehaviours(); } } }
    //public bool CanWeaponSwitch { get => canWeaponSwitch; set { if (canWeaponSwitch != value) { canWeaponSwitch = value; UpdateBehaviours(); } } }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        controller = GameManager.playerTransform.GetComponent<PlayerController>();

        behaviourMappings = new Dictionary<Func<bool>, Type>()  // 행동매핑
        {
            { () => CanMove, typeof(MoveBehaviour) },
            { () => CanJump, typeof(JumpBehaviour) },
            { () => CanClimb, typeof(ClimbBehaviour) },
            //{ () => CanGliding, typeof(GlidingBehaviour) },
            { () => CanAttack, typeof(AttackBehaviour) },
            { () => CanUseSkill, typeof(SkillBehaviour) },
            { () => CanBlock, typeof(BlockBehaviour) },
            { () => CanParry, typeof(ParryBehaviour) },
            { () => CanDodge, typeof(DodgeBehaviour) },
            //{ () => CanWeaponSwitch, typeof(WeaponSwitchBehaviour) }
        };
    }

    // 행동가능 리스트에 있는 요소들의 Execute() 함수를 실행시켜줌
    private void Update()
    {
        if (controller.uiCheck) return;

        if(activeBehaviours.Count > 0)    // 리스트가 비어있지 않을 때
        {
            for (int i = activeBehaviours.Count - 1; i >= 0; i--)
            {
                activeBehaviours[i].Execute();
            }
        }

        while (removeQueue.Count > 0)   // removeQueue에 요소가 있으면 Dequeue후, 리스트에서도 삭제
        {
            IBehaviour behaviourToRemove = removeQueue.Dequeue();
            activeBehaviours.Remove(behaviourToRemove);
        }
    }

    public void RequestBehaviour(IBehaviour behaviour, bool isExclusive)
    {
        if (isExclusive)
        {
            ClearBehaviours();
            exclusiveBehaviour = behaviour;
            behaviour.Enter();
        }
        else if (!activeBehaviours.Contains(behaviour))
        {
            activeBehaviours.Add(behaviour);
            behaviour.Enter();
        }
    }

    // 행동 가능 리스트에 추가
    public void AddBehaviour(IBehaviour behaviour)
    {
        if (!activeBehaviours.Contains(behaviour))
        {
            activeBehaviours.Add(behaviour);
            behaviour.Enter();
        }
    }

    // 행동 가능 리스트에서 삭제
    public void RemoveBehaviour(IBehaviour behaviour)
    {
        if(exclusiveBehaviour == behaviour)
        {   
            exclusiveBehaviour = null;
            UpdateBehaviours();
        }
        else if(activeBehaviours.Contains(behaviour))
        {
            behaviour.Exit();
            removeQueue.Enqueue(behaviour);     // 바로 리스트에서 Remove하지 않는 이유 : 리스트 순회하는 도중에 리스트 요소가 삭제 됐을 때 에러가 뜨는 것을 방지하기 위함
        }
    }

    private void ClearBehaviours()
    {
        foreach(var behaviour in activeBehaviours)
        {
            behaviour.Exit();
        }
        activeBehaviours.Clear();
    }

    // bool 변수에 변화가 있을 때 호출되는 함수.
    // 매핑된 맵을 돌며 true면 Add, false면 Remove
    private void UpdateBehaviours()
    {
        if(exclusiveBehaviour == null)
        {
            foreach (var mapping in behaviourMappings)
            {
                bool isActive = mapping.Key();
                Type behaviourType = mapping.Value;

                IBehaviour existing = activeBehaviours.Find(b => b.GetType() == behaviourType);

                if (isActive && existing == null)
                {
                    IBehaviour newBehaviour = (IBehaviour)Activator.CreateInstance(behaviourType, controller);
                    AddBehaviour(newBehaviour);
                }
                else if (!isActive && existing != null)
                {
                    RemoveBehaviour(existing);
                }
            }
        }
        
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}
