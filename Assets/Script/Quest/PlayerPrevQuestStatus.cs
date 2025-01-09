using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerPrevQuestStatus : MonoBehaviour
{

    

    
    public static bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void Update()
    {
        // Example usage
        if (IsMouseOverUI())
        {
            if(Input.GetKey(KeyCode.E))
            {

            }
        }
       
    }
    public void Interact()
    {
        /* if (dialogueLines.Length > 0 && currentDialogueIndex < dialogueLines.Length)
        {
            DisplayDialogue(dialogueLines[currentDialogueIndex]);
            currentDialogueIndex++;
        }
        else if (questToGive != null && !questToGive.isCompleted)
        {
            GiveQuest();
        } */
    }
        
    

}
