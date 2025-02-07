using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    private Collider weaponCollider;
    public string weaponId;

    public string toolTag;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();

    private void Start()
    {
        weaponCollider = GetComponentInParent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableOb interactable = other.GetComponent<InteractableOb>();
        if (interactable != null)
        {
            interactable.Interact(toolTag); // 상호작용 호출
        }
        if (!weaponCollider.enabled) return;
        if (!DamagedTargets.Contains(other.gameObject))
        {
            DamagedTargets.Add(other.gameObject);
            
            // ================ 2025-02-07 09:18 HYO 코드 추가 ====================================================================================================================================
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                CharacterData enemyData = other.GetComponent<BaseMonsterData>()?.monster ?? other.GetComponent<BaseMonsterData>()?.bossData;

                if (enemyData != null)
                {
                    StartCoroutine(GameManager.playerTransform.GetComponent<PlayerController>().StopPlayer(0.05f));
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyData, other.transform, true, false);
                }
            }
            // ===================================================================================================================================================================================
            else
            {
                //Debug.Log("암튼 뭔갈 침");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        DamagedTargets.Remove(other.gameObject);
    }



}
