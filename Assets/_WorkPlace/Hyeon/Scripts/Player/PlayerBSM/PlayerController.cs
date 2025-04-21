using UnityEngine;
namespace JWSTEST
{
    public class PlayerController : MonoBehaviour
    {
        public PlayerData playerData;
        public PlayerCombat playerCombat;
        private WeaponManager weapon;
        private CharacterController characterController;
        private Animator playerAnimator;
        private PlayerStateMachine stateMachine;
        private GroundState groundState;

        public bool cheatMode;

        private void OnEnable()
        {
            GameManager.playerTransform = this.transform;
        }

        private void OnDestroy()
        {
            if (playerData != null) playerData.OnTakeDamage -= HitCheck;
        }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            groundState = GetComponent<GroundState>();
            playerCombat = GetComponent<PlayerCombat>();
            weapon = playerCombat.weapon;
            playerAnimator = GetComponent<Animator>();
            playerData = CharacterManager.PlayerCharacterData;

            stateMachine = new PlayerStateMachine(this, playerAnimator);
            stateMachine.SetState<IdleState>();

            if (playerData != null) playerData.OnTakeDamage += HitCheck;
        }

        private void Update()
        {
            if (UIManager.Instance.IsUIWindowOpen()) return;

            CheatMode();
            stateMachine.UpdateState();
        }

        private void CheatMode()
        {
            if (InputManager.InputActions.actions["Cheat"].triggered)
            {
                cheatMode = !cheatMode;
                Debug.LogWarning($"CheatMode : {cheatMode}");
            }
        }

        private void HitCheck(Transform attacker)
        {
            stateMachine.OnHit();
        }

        public void HandleGameOver()
        {
            // 플레이어 사망 처리 및 게임오버 UI 활성화
            GameStateMachine.Instance.ChangeState(GameSystemState.GameOver);
            //Debug.Log("플레이어 사망: 게임오버 UI 활성화");
            
            // 모든 입력 비활성화
            InputManager.Instance.SetAllInputs(false);
        }
    }
}