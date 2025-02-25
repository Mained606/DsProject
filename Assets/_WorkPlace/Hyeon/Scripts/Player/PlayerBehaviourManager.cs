using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviourManager : BaseManager<PlayerBehaviourManager>
{
    private List<IBehaviour> behaviours = new List<IBehaviour>();
    private PlayerController controller;

    private Dictionary<Func<bool>, Type> behaviourMappings;

    [SerializeField] private bool canMove;
    [SerializeField] private bool canJump;
    [SerializeField] private bool canClimb;
    [SerializeField] private bool canGliding;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool canUseSkill;
    [SerializeField] private bool canBlock;
    //[SerializeField] private bool canSprint;
    [SerializeField] private bool canWeaponSwitch;

    public bool CanMove { get => canMove; set { if (canMove != value) { canMove = value; UpdateBehaviours(); } } }
    public bool CanJump { get => canJump; set { if (canJump != value) { canJump = value; UpdateBehaviours(); } } }
    public bool CanClimb { get => canClimb; set { if (canClimb != value) { canClimb = value; UpdateBehaviours(); } } }
    public bool CanGliding { get => canGliding; set { if (canGliding != value) { canGliding = value; UpdateBehaviours(); } } }
    public bool CanAttack { get => canAttack; set { if (canAttack != value) { canAttack = value; UpdateBehaviours(); } } }
    public bool CanUseSkill { get => canUseSkill; set { if (canUseSkill != value) { canUseSkill = value; UpdateBehaviours(); } } }
    public bool CanBlock { get => canBlock; set { if (canBlock != value) { canBlock = value; UpdateBehaviours(); } } }
    //public bool CanSprint { get => canSprint; set { if (canSprint != value) { canSprint = value; UpdateBehaviours(); } } }
    public bool CanWeaponSwitch { get => canWeaponSwitch; set { if (canWeaponSwitch != value) { canWeaponSwitch = value; UpdateBehaviours(); } } }

    protected override void Start()
    {
        base.Start();

        controller = GameManager.playerTransform.GetComponent<PlayerController>();

        behaviourMappings = new Dictionary<Func<bool>, Type>()
        {
            { () => CanMove, typeof(MoveBehaviour) },
            { () => CanJump, typeof(JumpBehaviour) },
            //{ () => CanClimb, typeof(ClimbBehaviour) },
            //{ () => CanGliding, typeof(GlidingBehaviour) },
            //{ () => CanAttack, typeof(AttackBehaviour) },
            //{ () => CanUseSkill, typeof(SkillBehaviour) },
            //{ () => CanBlock, typeof(BlockBehaviour) },
            //{ () => CanSprint, typeof(SprintBehaviour) },
            //{ () => CanWeaponSwitch, typeof(WeaponSwitchBehaviour) }
        };
    }

    private void Update()
    {
        foreach(var behaviour in behaviours)
        {
            behaviour.Execute();
            //Debug.Log($"{behaviour} 행동 중");
        }
    }

    public void AddBehaviour(IBehaviour behaviour)
    {
        if (!behaviours.Contains(behaviour))
        {
            behaviours.Add(behaviour);
            behaviour.Enter();
        }
    }

    public void RemoveBehaviour(IBehaviour behaviour)
    {
        if (behaviours.Contains(behaviour))
        {
            behaviour.Exit();
            behaviours.Remove(behaviour);
        }
    }

    private void UpdateBehaviours()
    {
        Debug.Log("UpdateBehaviours 함수 진입");
        foreach (var mapping in behaviourMappings)
        {
            bool isActive = mapping.Key();
            Type behaviourType = mapping.Value;

            IBehaviour existing = behaviours.Find(b => b.GetType() == behaviourType);

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

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}
