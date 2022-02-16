using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed;
    public float gravity = -9.81f;
    public float jumpHigh = 3.0f;
    public CharacterController controller;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Button Pressing")]
    public float rayDistance;

    [Header("Combat Variables")]
    public float attackTime;
    public GameObject sword;
    public float blockTime;
    public Animator anim;

    Vector3 velocity;
    bool isGrounded;
    bool isInGenerationRoom;
    Transform playerCamera;

    private void Start()
    {
        isInGenerationRoom = true;
        playerCamera = transform.Find("Main Camera");
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
            anim.SetFloat("GoingRight", x);
            anim.SetFloat("GoingForward", z);

            if (((z > 0 || z < 0) && Mathf.Round(z) == 0) || ((x > 0 || x < 0) && Mathf.Round(x) == 0))
                anim.SetBool("IsIdle", true);
            else
                anim.SetBool("IsIdle", false);

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

        if(Input.GetMouseButtonDown(0))
        {
            if (isInGenerationRoom)
            {
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, rayDistance, LayerMask.GetMask("InteractuableObject")))
                {
                    hit.transform.GetComponent<ButtonScript>().ButtonPressed();
                }
            }
            else
            {
                StartCoroutine("SwordAttack");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine("SwordBlock");
        }
    }

    IEnumerator SwordAttack()
    {
        sword.GetComponent<BoxCollider>().enabled = true;
        anim.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(attackTime);
        anim.SetBool("IsAttacking", false);
        sword.GetComponent<BoxCollider>().enabled = false;
    }

    IEnumerator SwordBlock()
    {
        sword.GetComponent<BoxCollider>().enabled = true;
        anim.SetBool("IsBlocking", true);
        yield return new WaitForSeconds(blockTime);
        anim.SetBool("IsBlocking", false);
        sword.GetComponent<BoxCollider>().enabled = false;
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
