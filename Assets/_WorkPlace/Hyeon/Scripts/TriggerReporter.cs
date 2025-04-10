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
        if (other.gameObject.layer == 3)
        {
            waterBlocker?.TriggerEnter();
        }
    }
            

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            waterBlocker?.TriggerStay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            waterBlocker?.TriggerExit();
        }
    }
}
