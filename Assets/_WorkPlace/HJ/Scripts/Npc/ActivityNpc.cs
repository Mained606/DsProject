using System.Collections;
using UnityEngine;

public class ActivityNpc : MonoBehaviour
{
    private string fishingState = "IsFishing";
    private string farmingState = "IsFarming";
    private string wipeSweatState = "WipeSweatTrigger";

    private Animator animator;
    private NpcController npcController;


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

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetTrigger(wipeSweatState);

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetBool(farmingState, false);

            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }
    }
}
