using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperAttack : MonoBehaviour
{
    public float speed = 30;
    public float slowDownRate = 0.01f;
    public float detectingDistance = 0.1f;
    public float destroyDelay = 5;
    [SerializeField] private LayerMask layer;

    private Rigidbody rb;
    private bool stopped;

    void Start()
    {
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        if (GetComponent<Rigidbody>() != null)
        {
            rb = GetComponent<Rigidbody>();
            StartCoroutine(SlowDown());
        }
        else
            Debug.Log("No Rigidbody");

        Destroy(gameObject, destroyDelay);
    }

    private void FixedUpdate()
    {
        Shooting();
        if (!stopped)
        {
            RaycastHit hit;
            Vector3 distance = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            if (Physics.Raycast(distance, transform.TransformDirection(-Vector3.up), out hit, 4f))
            {
                Debug.Log($"레이 맞음 : {hit.transform.name}");
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            else
            {
                Debug.Log("레이 안 맞음");
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
            Debug.DrawRay(distance, transform.TransformDirection(-Vector3.up * detectingDistance), Color.red);
        }
    }

    IEnumerator SlowDown()
    {
        float t = 1;
        while (t > 0)
        {
            rb.linearVelocity = Vector3.Lerp(Vector3.zero, rb.linearVelocity, t);
            t -= slowDownRate;
            yield return new WaitForSeconds(0.1f);
        }

        stopped = true;
    }

    private void Shooting()
    {
        rb.linearVelocity = transform.forward * speed;
    }
}
