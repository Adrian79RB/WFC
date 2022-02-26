using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public float health;
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

    [Header("Sound Stuff")]
    public AudioSource stepSound;
    public AudioSource effectSound;
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip blockSound;
    public AudioClip hurtSound;

    // Movement variables
    bool isGrounded;
    bool isInGenerationRoom;
    Vector3 velocity;
    Transform playerCamera;

    // Combat variables
    bool isBlocking;
    float damageCoolDown = 2.0f;
    float damageTime = 0;
    bool damaged = false;

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

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * movementSpeed * Time.deltaTime);
            if (!stepSound.isPlaying)
                stepSound.Play();
            else if (z == 0 && x == 0)
                stepSound.Stop();

            // Jump method
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHigh * -2f * gravity);
                effectSound.clip = jumpSound;
                effectSound.Play();
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

        if (damaged)
        {
            damageTime += Time.deltaTime;
            if(damageTime >= damageCoolDown)
            {
                damageTime = 0;
                damaged = false;
            }
        }
    }

    IEnumerator SwordAttack()
    {
        effectSound.clip = attackSound;
        effectSound.Play();
        sword.GetComponent<BoxCollider>().enabled = true;
        anim.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(attackTime);
        anim.SetBool("IsAttacking", false);
        sword.GetComponent<BoxCollider>().enabled = false;
    }

    IEnumerator SwordBlock()
    {
        isBlocking = true;
        anim.SetBool("IsBlocking", true);
        yield return new WaitForSeconds(blockTime);
        anim.SetBool("IsBlocking", false);
        isBlocking = false;
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

    public void ReceiveDamage(float damage)
    {
        Debug.Log("Recibe daño. Protegiendo: " + isBlocking);
        if (!isBlocking && !damaged)
        {
            effectSound.clip = hurtSound;
            effectSound.Play();
            damaged = true;
            health -= damage;
            if (health <= 0f)
            {
                health = 0;
                StartCoroutine("DeadAnimation");
            }
        }
        else if (isBlocking)
        {
            effectSound.clip = blockSound;
            effectSound.Play();
        }
    }

    IEnumerator DeadAnimation()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsBlocking", false);
        anim.SetFloat("GoingRight", 0f);
        anim.SetFloat("GoingForward", 0f);
        anim.SetBool("IsDead", true);

        if (!effectSound.isPlaying)
            effectSound.Play();

        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene("Scene1");
    }
}
