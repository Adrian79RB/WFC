using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public float damage;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("This sword: " + transform.name + "; object hit: "+ collision.transform.name);
        if (collision.transform.GetComponent<EnemyAgent>())
        {
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.transform.GetComponent<EnemyAgent>().ReceiveDamage(damage);
        }
    }
}
