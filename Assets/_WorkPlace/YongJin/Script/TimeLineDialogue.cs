using System.Collections.Generic;
using UnityEngine;

public class TimeLineDialogue : MonoBehaviour
{
    [SerializeField] private List<string> contextName;
    [SerializeField] private List<string> dialogue;
    [SerializeField] private DialogUITest dialogUITest;
    
    private void OnEnable() 
    {
        dialogUITest.StartDialogue(contextName, dialogue);
        UIManager.SystemGameMessage($"", MessageTag.퀘스트);

    }
    private void OnDisable() 
    {
        dialogUITest.DialogueExit();

    }
    
}
