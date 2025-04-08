using UnityEngine;

public class TriggerReporter : MonoBehaviour
{
    private WaterDepthBlocker waterBlocker;

    private void Start()
    {
        waterBlocker = GetComponentInParent<WaterDepthBlocker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        waterBlocker?.TriggerEnter(this.gameObject, other);
    }

    private void OnTriggerStay(Collider other)
    {
        waterBlocker?.TriggerStay(this.gameObject, other);
    }

    private void OnTriggerExit(Collider other)
    {
        waterBlocker?.TriggerExit(this.gameObject, other);
    }
}
