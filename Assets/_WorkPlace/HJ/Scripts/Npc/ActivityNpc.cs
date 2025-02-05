using System.Collections;
using UnityEngine;

public class ActivityNpc : MonoBehaviour
{
    private string fishingState = "IsFishing";
    private string farmingState = "IsFarming";
    private string craftingState = "IsCrafting";
    private string sittingState = "IsSitting";

    private string wipeSweatTrigger = "WipeSweatTrigger";
    private string[] sittingTriggers = { "SittingTalkingTrigger", "SittingClapTrigger" };

    private int sittingTalkingTrigger = Animator.StringToHash("SittingTalkingTrigger");

    private Animator animator;
    private NpcController npcController;

    [SerializeField] private bool isNearNpc = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        npcController = GetComponent<NpcController>();

        if (npcController.npcType == NpcType.Fishing)
        {
            StartCoroutine(Fishing());
        }
        else if(npcController.npcType == NpcType.Farmer)
        {
            StartCoroutine(Farming());
        }
        else if(npcController.npcType == NpcType.Craft)
        {
            StartCoroutine(Crafting());
        }
        else if(npcController.npcType == NpcType.Sitting)
        {
            StartCoroutine(Sitting());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNpc = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNpc = false;
        }
    }

    private IEnumerator Fishing()
    {
        while(true)
        {
            animator.SetBool(fishingState, true);

            yield return new WaitForSeconds(Random.Range(20f, 60f));

            animator.SetBool(fishingState, false);

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }        
    }

    private IEnumerator Farming()
    {
        while(true)
        {
            animator.SetBool(farmingState, true);

            yield return new WaitForSeconds(Random.Range(20, 40f));

            animator.SetTrigger(wipeSweatTrigger);

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetBool(farmingState, false);

            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }
    }

    private IEnumerator Crafting()
    {
        while(true)
        {
            if(!animator.GetBool(craftingState))
            {
                animator.SetBool(craftingState, true);
            }

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetTrigger(wipeSweatTrigger);
        }
    }

    private IEnumerator Sitting()
    {
        while (true)
        {
            if(!animator.GetBool(sittingState))
            {
                animator.SetBool(sittingState, true);
            }

            yield return new WaitForSeconds(Random.Range(5f, 10f));

            if (isNearNpc)
            {
                PlayRandomTrigger(sittingTriggers);
            }
        }
    }

    private void PlayRandomTrigger(string[] triggers)
    {
        int randomIndex = Random.Range(0, triggers.Length);
        string randomTrigger = triggers[randomIndex];

        if(randomTrigger != null)
        {
            animator.SetTrigger(randomTrigger);
        }
    }
}
