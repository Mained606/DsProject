using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    private Image image;
    public ChoiceButtonType type {get; private set;}
    private TMP_Text text;
    private Button button;
    private void Start() 
    {
        image = transform.GetChild(0).GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<TMP_Text>();
    }
    public void Init(ChoiceButtonType type, string questName)// MainQuest, SubQuest, Shop, Exit
    {
        
        if(type == ChoiceButtonType.MainQuest)
        {
            text.text = questName;
            
        }
        if(type == ChoiceButtonType.SubQuest)
        {
            text.text = questName;
        }
        if(type == ChoiceButtonType.Shop)
        {
            text.text = "상점";
        }
        if(type == ChoiceButtonType.Exit)
        {
            text.text = "나가기";
        }
        this.type = type;
    }


}
