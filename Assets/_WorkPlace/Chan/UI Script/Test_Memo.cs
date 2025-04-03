using UnityEngine;
using UnityEngine.UI;

public class Test_Memo : MonoBehaviour
{
    private Image memo;

    private void Awake()
    {
        memo = GetComponent<Image>();
        GameStateMachine.Instance.ChangeState(GameSystemState.InventoryChange);
    }


    public void btnDown()
   {
     memo.gameObject.SetActive(false);
     UIManager.Instance.ToggleinfoMessageWindow("지도를 받으러 가자.");
    }

}
