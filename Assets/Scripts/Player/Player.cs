using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float movementSpeed;
    public float gravity = -9.81f;
    public float jumpHigh = 3.0f;
    public CharacterController controller;
    
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float rayDistance;

    Vector3 velocity;
    bool isGrounded;
    bool isInGenerationRoom;
    Transform camera;

    private void Start()
    {
        isInGenerationRoom = true;
        camera = transform.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (GetComponent<CharacterController>().enabled)
        {
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * movementSpeed * Time.deltaTime);

            // Jump method
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHigh * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        if(isInGenerationRoom && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(camera.position, camera.forward, out hit, rayDistance, LayerMask.GetMask("InteractuableObject")))
            {
                hit.transform.GetComponent<ButtonScript>().ButtonPressed();
            }
        }
    }

    public void PortalTeleport(Vector3 newPos)
    {
        transform.position = newPos;
        StartCoroutine("ActivateCharacterController");
    }

    IEnumerator ActivateCharacterController()
    {
        GetComponent<CharacterController>().enabled = false;
        isInGenerationRoom = false;
        yield return new WaitForSeconds(1.0f);
        GetComponent<CharacterController>().enabled = true;
    }
}
