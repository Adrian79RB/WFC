using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit) && hit.transform.tag == "Player")
            {
                GetComponentInParent<EnemyAgent>().PlayerDetected();
            }
        }
    }
}
