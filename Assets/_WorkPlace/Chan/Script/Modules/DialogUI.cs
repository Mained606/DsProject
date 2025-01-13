using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DialogUI : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI name;   // 이름 출력칸
    [SerializeField]private TextMeshProUGUI dialogueText; // 대사 출력칸
    private float typingSpeed = 0.05f; // 텍스트 출력 딜레이

    private bool isTyping = false; // 현재 대사 출력중인지 확인
    private bool isWaiting = false; // 출력 완료 후 대기 상태 확인

    private List<string> dialogueList = new List<string>(); // JSON에서 불러온 대사 리스트
    private int currentDialogueIndex = 0; // 현재 출력중인 대사 인덱스 

    //   private Queue<string> dialogueQueue = new Queue<string>(); // 대사 큐 리스트 

    void Start()
    {

        #region 임시 대사 큐
        dialogueList.Add("폴라포 너무 맛있다.");
        dialogueList.Add("폴라포는 왜 이리 맛있는 걸까?");
        dialogueList.Add("그 이유는 네이버에도 나와있지 않다.");

        #endregion

        UIManager.Instance.DialogUI.ShowNext(dialogueList);
    }

    void Update()
    {
       if(isWaiting && Input.GetMouseButtonDown(0))
       {
            UIManager.Instance.DialogUI.ShowNext(dialogueList);
       }
    }

    public void ShowNext(List<string> dialoglist)
    {
        if(isTyping)
        {
            CompleteTyping(dialoglist);
            return;
        }
        // 큐가 비면 종료
        if(currentDialogueIndex >= dialoglist.Count)
        {
            dialogueText.text = "";
            Debug.Log("이제 대사 없엉");
            return;
        }

        string nextDialogue = dialoglist[currentDialogueIndex];
        StartCoroutine(TypeDialogue(nextDialogue));
    }

    private IEnumerator TypeDialogue(string dialgue)
    {
        isTyping = true;
        isWaiting = false;
        dialogueText.text = "";

        // 한글자씩 출력
       foreach(char i in dialgue.ToCharArray())
        {
            dialogueText.text += i;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isWaiting = true; // 대기상태로 전환
    }
    private void CompleteTyping(List<string> dialoglist)
    {
        
        StopAllCoroutines(); // 타이핑 효과 중단

        if(currentDialogueIndex <= dialoglist.Count)
        dialogueText.text = dialoglist[currentDialogueIndex - 1]; // 현재 대사를 완전 출력

        isTyping = false;
        isWaiting = true;
    }
}
