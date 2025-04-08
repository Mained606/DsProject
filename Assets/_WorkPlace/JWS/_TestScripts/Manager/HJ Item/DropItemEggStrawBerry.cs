using UnityEngine;
using System.Collections.Generic;

public class DropItemEggStrawBerry : MonoBehaviour
{
    public List<string> dropItemIds = new List<string>();
    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private string itemName = "용의알";
    [SerializeField] private int itemAmount = 1;

    private void Start()
    {
        Destroy(this.gameObject, DsConstValue.DROP_ITEM_DESTROY_INTERVAL);
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            OpenBox(itemName);
        }
    }

    public void OpenBox(string name)
    {
        if (!IsNearPlayer())
            return;

        //Debug.Log("희정님 아이템메니저 이용시");
        ItemManager.Instance.AddItemLogic(name, itemAmount);
        Destroy(gameObject); //박스 삭제
    }

    private bool IsNearPlayer()
    {
        float disatance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
        return disatance <= detectionDistance;
    }
}
