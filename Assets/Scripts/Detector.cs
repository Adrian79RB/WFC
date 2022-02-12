using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log("Entra Player");
            Vector3 dir = other.transform.position - transform.position;
            dir.y += .5f;
            dir = dir.normalized;

            RaycastHit hit;
            Debug.DrawRay(transform.position, dir, Color.red);
            if (Physics.Raycast(transform.position, dir, out hit) && hit.transform.tag == "Player")
            {
                GetComponentInParent<EnemyAgent>().PlayerDetected();
            }
            if(hit.transform.name != null)
                Debug.Log("Hit: " + hit.transform.name);
        }
    }
}
