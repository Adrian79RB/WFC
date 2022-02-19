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
            collision.transform.GetComponent<EnemyAgent>().ReceiveDamage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("This sword: " + transform.name + "; object hit: " + other.transform.name);
        if (other.transform.GetComponent<Player>())
        {
            other.transform.GetComponent<Player>().ReceiveDamage(damage);
        }
    }
}
