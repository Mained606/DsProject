using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;
    private Button[] buttons;
    [SerializeField] private string titleSceneName = "TitleScene";

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
                SceneManager.LoadScene(titleSceneName);
                break;

            case 1: // 종료 버튼
                    //Debug.Log("게임 종료");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;

            case 2:
                gameObject.SetActive(false);
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;

            default:
                //Debug.Log($"버튼 {buttonIndex} 클릭됨");
                break;
        }
    }

}
