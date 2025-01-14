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
    private int currentNPCDesaIndex = 0; // 현재 출력중인 일반 대사 인덱스 
    private int currentQuestDesaIndex = 0; // 현재 출력중인 일반 대사 인덱스 
    
    private Coroutine typingCoroutine;
    private NPCController NPCData;

    #region Npc일반대사
    public void StartDialogue(NPCController contextNPC, List<string> dialogueList)
    {
        if(currentNPCDesaIndex == 0)
        {
            DialogueStart();            
        }
        
        NPCData = contextNPC;
        dialogueName.text = NPCManager.Instance.GetNPCStringData(contextNPC.contextNPC.NPC_stringIdx).NPCString_name;
        this.dialogueList = dialogueList;
        if(currentNPCDesaIndex >= dialogueList.Count)
        {
            NpcButtonActive();
            return;
        }
        if(typingCoroutine == null)
        {
            string typingDialogyeData = dialogueList[currentNPCDesaIndex];
            typingCoroutine = StartCoroutine(TypeNpcDialogue(typingDialogyeData));
        }
        else
        {
            SkipNpcDialogue();
        }
    }
    private IEnumerator TypeNpcDialogue(string dialgue)
    {
        dialogueText.text = "";
        
        foreach(char i in dialgue.ToCharArray())
        {
            dialogueText.text += i;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        yield return new WaitForSeconds(1f);
        SkipNpcDialogue();

    }
    private void SkipNpcDialogue()
    {
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);            
        dialogueText.text = dialogueList[currentNPCDesaIndex];
        currentNPCDesaIndex++;
        typingCoroutine = null;
    }

    #endregion
    
    #region 퀘스트 대사
    public void StartDialogue(List<QuestDialogue> questDialogue)//퀘스트 데이터
    {
        
        if(currentQuestDesaIndex == 0)
        {
            DialogueStart();
            dialogueList = new List<string>();
            foreach(var questDesa in questDialogue)
            {
                dialogueList.Add(questDesa.dialogue);
            }           
        }
        if(currentQuestDesaIndex >= dialogueList.Count)
        {
            Debug.Log("Quest대사 끝");
            QuestButtonActive();
            return;
        }

        if(questDialogue[currentQuestDesaIndex].context == 1)
            dialogueName.text = "시안";
        else
            dialogueName.text = NPCData.NPCName;
        
        
        if(currentQuestDesaIndex >= dialogueList.Count)
        {
            NpcButtonActive();
            return;
        } 

        if(typingCoroutine == null)
        {
            string typingDialogyeData = dialogueList[currentQuestDesaIndex];
            typingCoroutine = StartCoroutine(TypeQuestDialogue(typingDialogyeData));
        }
        else
        {
            SkipQuestDialogue();
        }
    }

    
    private IEnumerator TypeQuestDialogue(string dialgue)
    {
        dialogueText.text = "";
        
        foreach(char i in dialgue.ToCharArray())
        {
            dialogueText.text += i;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        yield return new WaitForSeconds(1f);
        SkipQuestDialogue();

    }
    
    private void SkipQuestDialogue()
    {
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);            
        dialogueText.text = dialogueList[currentQuestDesaIndex];
        currentQuestDesaIndex++;
        typingCoroutine = null;
    }


    #endregion




    
    
    public void DialogueStart()
    {
        gameObject.SetActive(true);
        ClearChoiceButton();
    }
    


    private void NpcButtonActive()
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
    private void QuestButtonActive()
    {
        baseParent.GetChild(4).gameObject.SetActive(true); // 수락 버튼
        baseParent.GetChild(5).gameObject.SetActive(true); // 나가기 버튼
    }


    public void ClickMainQuest()
    {
        if(NPCData.SameQuestCheck())
        {
            NPCData.ChangeState(NPCState.MainQuestGiver);
            UIManagerTest.Instance.DialogUI.StartDialogue(QuestManager.Instance.GetQuestStringData(QuestManager.Instance.nextMainQuestId).questString_diologue);
            ClearChoiceButton();
        }
        else
        {
            DialogueExit();
        }
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
    public void ClickAccept()
    {
        NPCData.GiveQuest();
        DialogueExit();

    }
    public void ClickRefuse()
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
    public void DialogueExit()
    {
        NPCData.ChangeState(NPCState.Common);
        gameObject.SetActive(false);
        ClearChoiceButton();
        currentNPCDesaIndex = 0;
        currentQuestDesaIndex = 0;
    }


    
    

    
    
    
}
