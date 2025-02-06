using UnityEngine;

public class Bench : MonoBehaviour
{
    public Vector3 leftPosition;
    public Vector3 rightPosition;

    public Vector3 leftOffset;
    public Vector3 rightOffset;

    public bool left = false;
    public bool right = false;

    private void Start()
    {
        leftPosition = transform.position + leftOffset;
        rightPosition = transform.position + rightOffset;
    }

    private void Update()
    {
        leftPosition = transform.position + leftOffset;
        rightPosition = transform.position + rightOffset;
    }
}
