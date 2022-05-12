using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySword : MonoBehaviour
{
    public float damage;
 
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("This sword: " + transform.name + "; object hit: " + other.transform.name);
        if (other.transform.GetComponent<Player>())
        {
            other.transform.GetComponent<Player>().ReceiveDamage(damage, true);
        }
    }
}
