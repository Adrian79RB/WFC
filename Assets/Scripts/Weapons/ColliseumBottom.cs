using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliseumBottom : MonoBehaviour
{

    public float damage;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>())
        {
            other.GetComponent<Player>().ReceiveDamage(damage);
        }
    }
}
