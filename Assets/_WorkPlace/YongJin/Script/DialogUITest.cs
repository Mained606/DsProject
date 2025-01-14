using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class DialogUITest : MonoBehaviour
{
    [SerializeField] private GameObject ChoicePrefab;
    [SerializeField] private Transform baseParent;

    [SerializeField]private TMP_Text dialogueName;   // 이름 출력칸
    [SerializeField]private TMP_Text dialogueText; // 대사 출력칸
    private float typingSpeed = 0.05f; // 텍스트 출력 딜레이


    private List<string> dialogueList = new List<string>(); // JSON에서 불러온 대사 리스트
    private int currentDialogueIndex = 0; // 현재 출력중인 대사 인덱스 
    
    private Coroutine typingCoroutine;
    private NPCController NPCData;

    public void StartDialogue(NPCController contextNPC, List<string> dialogueList)
    {
        if(currentDialogueIndex == 0)
        {
            DialogueStart();            
        }
        
        NPCData = contextNPC;
        dialogueName.text = NPCManager.Instance.GetNPCStringData(contextNPC.contextNPC.NPC_stringIdx).NPCString_name;
        this.dialogueList = dialogueList;
        if(currentDialogueIndex >= dialogueList.Count)
        {
            ButtonActive();
            return;
        } 

        if(typingCoroutine == null)
        {
            string typingDialogyeData = dialogueList[currentDialogueIndex];
            typingCoroutine = StartCoroutine(TypeDialogue(typingDialogyeData));
        }
        else
        {
            SkipDialogue();
        }
    }
    public void StartDialogue(List<QuestDialogue> questDialogue)//퀘스트 데이터
    {
        if(currentDialogueIndex == 0)
        {
            DialogueStart();
            foreach(var questDesa in questDialogue)
            {
                dialogueList.Add(questDesa.dialogue);
            }           
        }

        if(questDialogue[currentDialogueIndex].context == 1)
            dialogueName.text = "시안";
        else
            dialogueName.text = NPCData.NPCName;
        dialogueList = new List<string>();
        
        if(currentDialogueIndex >= dialogueList.Count)
        {
            ButtonActive();
            return;
        } 

        if(typingCoroutine == null)
        {
            string typingDialogyeData = dialogueList[currentDialogueIndex];
            typingCoroutine = StartCoroutine(TypeDialogue(typingDialogyeData));
        }
        else
        {
            SkipDialogue();
        }
    }

    private IEnumerator TypeDialogue(string dialgue)
    {
        dialogueText.text = "";
        
        foreach(char i in dialgue.ToCharArray())
        {
            dialogueText.text += i;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        yield return new WaitForSeconds(1f);
        SkipDialogue();

    }
    private void SkipDialogue()
    {
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);            
        dialogueText.text = dialogueList[currentDialogueIndex];
        currentDialogueIndex++;
         
        typingCoroutine = null;
        /* if(AutoDialogue)
        {
            if(currentDialogueIndex >= dialogueList.Count)
            {
                Debug.Log("스킵 끝");
            }
            else
            {
                string typingDialogyeData = dialogueList[currentDialogueIndex];
                typingCoroutine = StartCoroutine(TypeDialogue(typingDialogyeData));
            }
        } */
    }
    
    public void DialogueStart()
    {
        gameObject.SetActive(true);
        ClearChoiceButton();
    }
    public void DialogueExit()
    {
        

        gameObject.SetActive(false);
        ClearChoiceButton();
    }


    private void ButtonActive()
    {
        Debug.Log(NPCData.contextNPC.NPC_isMainQuestGiver);
        if(NPCData.contextNPC.NPC_isMainQuestGiver)
        {
            baseParent.GetChild(0).gameObject.SetActive(true); // 메인 퀘스트 버튼
        }
        
        baseParent.GetChild(1).gameObject.SetActive(false); // 서브 퀘스트 버튼
        if(NPCData.contextNPC.NPC_isShop)
        {
            baseParent.GetChild(2).gameObject.SetActive(true); // 상점 버튼
        }        
        baseParent.GetChild(3).gameObject.SetActive(true); // 나가기 버튼
    }


    public void ClickMainQuest()
    {
        NPCData.ChangeState(NPCState.MainQuestGiver);
        
        ClearChoiceButton();
    }
    public void ClickSubQuest()
    {
        DialogueExit();
    }
    public void ClickShop()
    {
        DialogueExit();
    }
    public void ClickExit()
    {
        DialogueExit();
    }
    private void ClearChoiceButton()
    {
        for(int i = 0; i < baseParent.childCount; i++)
        {   
            baseParent.GetChild(i).gameObject.SetActive(false);
        }
        
    }


    
    

    
    
    
}
