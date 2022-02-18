using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Transform shotPos;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Vector3 dir = other.transform.position - shotPos.position;
            dir.y += .5f;
            dir = dir.normalized;

            RaycastHit hit;
            Debug.DrawRay(shotPos.position, dir * 3f, Color.red, 10f);
            if (Physics.Raycast(shotPos.position, dir, out hit) && hit.transform.tag == "Player")
            {
                GetComponentInParent<EnemyAgent>().PlayerDetected();
            }
        }
    }
}
