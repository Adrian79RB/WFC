using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public Transform player;
    public Transform receiver;

    bool playerIsOverlapping = false;

    // Update is called once per frame
    void Update()
    {
        if (playerIsOverlapping)
        {
            Vector3 portalToPlayer = player.position - transform.position;
            float dotProduct = Vector3.Dot(transform.up, portalToPlayer);
            Debug.Log("Player pos: " + player.position + "; portal pos: " + transform.position + "; dot product: " + dotProduct);
            if(dotProduct < 0f)
            {
                float rotDifference = -Quaternion.Angle(transform.rotation, receiver.rotation);
                player.Rotate(Vector3.up, rotDifference);

                Vector3 positionOffset = Quaternion.Euler(0f, rotDifference, 0f) * portalToPlayer;
                player.GetComponent<Player>().PortalTeleport(receiver.position + positionOffset);

                playerIsOverlapping = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log("Player Colliding");
            playerIsOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerIsOverlapping = false;
        }
    }
}
