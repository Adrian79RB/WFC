using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Transform shotPos;

    private void OnTriggerEnter(Collider other)
    {
        OntriggerEnterAndStay(other);
    }

    private void OnTriggerStay(Collider other)
    {
        OntriggerEnterAndStay(other);
    }

    private void OntriggerEnterAndStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 dir = new Vector3(other.transform.position.x - shotPos.position.x, 0f, transform.position.z - shotPos.position.z);
            dir = dir.normalized;

            Vector3 originalPos = new Vector3(shotPos.position.x, .5f, shotPos.position.z);

            RaycastHit hit;
            if (Physics.Raycast(originalPos, dir, out hit) && hit.transform.tag == "Player")
            {
                GetComponentInParent<EnemyAgent>().PlayerDetected();
            }
        }
    }
}
