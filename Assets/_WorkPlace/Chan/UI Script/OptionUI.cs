using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;
    private Button[] buttons;

    private void Start()
    {
        buttons = Buttons.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            AddClickListener(buttons[i], i);
        }
    }

    private void AddClickListener(Button button, int index)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // 3. 버튼 액션 실행
            ExecuteButtonAction(index);
        });
    }

    private void ExecuteButtonAction(int buttonIndex)
    {

        // 버튼 인덱스에 따라 다른 동작 실행 (실제 버튼 순서에 맞게 수정 필요)
        switch (buttonIndex)
        {
            case 0:
                GoToTitleScene();
                break;

            case 1: // 종료 버튼
                    //Debug.Log("게임 종료");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;

          /*  case 2:
                UIManager.Instance.ToggleOptionUIwindow(); // OptionUI 상태 정상 정리
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;*/

            default:
                //Debug.Log($"버튼 {buttonIndex} 클릭됨");
                break;
        }
    }
    private void GoToTitleScene()
    {
        TimerManager.Instance.ResumeGame();
        GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);

        // 커서 상태 명시적 설정 - 씬 전환 전에 커서를 보이게 하고 언락함
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 새 게임이 아님을 명시적으로 표시
        SaveSystemInitializer saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
        if (saveSystem != null)
        {
            SaveSystemInitializer.isNewGame = false;
            Debug.Log("[OptionUI] 타이틀로 돌아갑니다. isNewGame = false로 설정했습니다.");
            SceneManager.LoadScene(saveSystem.titleSceneName);
        }
        else
        {
            Debug.LogWarning("[OptionUI] SaveSystemInitializer를 찾을 수 없습니다.");
            SceneManager.LoadScene("TitleScene");
        }
    }
}
