using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager GM;

    [Header("Player Stats")]
    public float health;
    public float movementSpeed;
    public float gravity = -9.81f;
    public float jumpHigh = 3.0f;
    public CharacterController controller;
    [HideInInspector]public Vector3 movement;
    
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
    public AudioClip[] stepSoundsClip;
    public AudioClip concreteStep;

    [Header("Weapon GUI Stuff")]
    public GameObject handGUI;
    public GameObject swordGUI;
    public GameObject shieldGUI;
    public GameObject fingerGUI;
    public RectTransform lifeBar;

    // Movement variables
    bool isGrounded;
    bool isInGenerationRoom;
    Vector3 velocity;
    Transform playerCamera;

    // Combat variables
    bool isBlocking = false;
    bool isAttacking = false;
    float damageCoolDown = 2.0f;
    float damageTime = 0;
    bool damaged = false;

    // GUI variables
    bool weaponGUIisChanging;
    float originalLifeBarWidth;

    private void Start()
    {
        isInGenerationRoom = true;
        weaponGUIisChanging = false;
        originalLifeBarWidth = lifeBar.sizeDelta.x;

        playerCamera = transform.Find("Main Camera");
        stepSound.clip = concreteStep;
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(1).transform.localPosition = new Vector3(0f, 0f, 0f);
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // Check if the player is touching the ground

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Control the movement of the player
        if (GetComponent<CharacterController>().enabled)
        {
            anim.SetFloat("GoingRight", x);
            anim.SetFloat("GoingForward", z);

            movement = transform.right * x + transform.forward * z;
            controller.Move(movement * movementSpeed * Time.deltaTime);

            if ( ((z > -0.5f && z < 0.5 && x > -0.5 && x < 0.5) || !isGrounded) && stepSound.isPlaying)
            {
                stepSound.Stop();
            }
            else if ( (z < -0.5f || z > 0.5 || x < -0.5 || x > 0.5) && !stepSound.isPlaying && isGrounded)
            {
                stepSound.Play();
            }

            // Jump method
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHigh * -2f * gravity);
                effectSound.clip = jumpSound;
                stepSound.Stop();
                effectSound.Play();
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        // Check if the player click the right mouse button
        if(Input.GetMouseButtonDown(0))
        {
            if (isInGenerationRoom)
            {
                if(!weaponGUIisChanging)
                    StartCoroutine(ChangeWeaponGUI(handGUI, fingerGUI));
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, rayDistance, LayerMask.GetMask("InteractuableObject")))
                {
                    if (hit.transform.GetComponent<ButtonScript>() != null)
                        hit.transform.GetComponent<ButtonScript>().ButtonPressed();
                    else if (hit.transform.GetComponent<BehaviourButton>())
                        hit.transform.GetComponent<BehaviourButton>().ButtonPressed();
                }
            }
            else if(!isAttacking && !isBlocking)
            {
                StartCoroutine("SwordAttack");
                if(!weaponGUIisChanging)
                    StartCoroutine(ChangeWeaponGUI(swordGUI, swordGUI));
            }
        }

        // Check if the player click the left mouse button
        if (!isInGenerationRoom && Input.GetMouseButtonDown(1) && !isBlocking && !isAttacking)
        {
            StartCoroutine("SwordBlock");
            if(!weaponGUIisChanging)
                StartCoroutine(ChangeWeaponGUI(swordGUI, shieldGUI));
        }

        // Check if player gets damage to avoid doubling the damage received
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

    /// <summary>
    /// Coroutine that executes the attack animation of the player
    /// </summary>
    /// <returns></returns>
    IEnumerator SwordAttack()
    {
        isAttacking = true;
        effectSound.clip = attackSound;
        effectSound.Play();
        sword.GetComponent<BoxCollider>().enabled = true;
        anim.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(attackTime);
        anim.SetBool("IsAttacking", false);
        sword.GetComponent<BoxCollider>().enabled = false;
        isAttacking = false;
    }

    /// <summary>
    /// Coroutine that executes the block animation of the player
    /// </summary>
    /// <returns></returns>
    IEnumerator SwordBlock()
    {
        isBlocking = true;
        anim.SetBool("IsBlocking", true);
        yield return new WaitForSeconds(blockTime);
        anim.SetBool("IsBlocking", false);
        isBlocking = false;
    }

    /// <summary>
    /// Executing the player teleporting through the portal
    /// </summary>
    /// <param name="newPos">New position where the player is going to teleport to</param>
    public void PortalTeleport(Vector3 newPos)
    {
        stepSound.clip = stepSoundsClip[GM.tileSetChoosen];
        EnterFightingPhase();
        Respawn(newPos);
    }

    /// <summary>
    /// Respawning the player to the new position
    /// </summary>
    /// <param name="newPos">New position where the player</param>
    public void Respawn(Vector3 newPos)
    {
        transform.position = newPos;
        StartCoroutine("ActivateCharacterController");
    }

    /// <summary>
    /// Change fighting phase in the tutorial
    /// </summary>
    public void EnterFightingPhase()
    {
        handGUI.SetActive(false);
        swordGUI.SetActive(true);
        isInGenerationRoom = false;
    }

    /// <summary>
    /// Change interactive phase in the tutorial
    /// </summary>
    public void EnterInteractivPhase()
    {
        swordGUI.SetActive(false);
        handGUI.SetActive(true);
        isInGenerationRoom = true;
    }

    /// <summary>
    /// Coroutine that activate the character controller  object 
    /// </summary>
    /// <returns></returns>
    IEnumerator ActivateCharacterController()
    {
        GetComponent<CharacterController>().enabled = false;
        yield return new WaitForSeconds(1.0f);
        GetComponent<CharacterController>().enabled = true;
    }


    /// <summary>
    /// The player receive damage from the enemy
    /// </summary>
    /// <param name="damage">Amount of damage received by the player</param>
    /// <param name="colliseumBottom">Boolean that control if the player fall in the pit</param>
    public void ReceiveDamage(float damage, bool colliseumBottom)
    {
        if (colliseumBottom || (!isBlocking && !damaged) )
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

            ChangeLifeBarGUI();
        }
        else if (isBlocking && !effectSound.isPlaying)
        {
            effectSound.clip = blockSound;
            effectSound.Play();
        }
    }

    /// <summary>
    /// Changing the life bar size that represents the health amount
    /// </summary>
    private void ChangeLifeBarGUI()
    {
        float newWidth = lifeBar.sizeDelta.x * health / 10f;
        float xOffset = lifeBar.sizeDelta.x - newWidth;

        lifeBar.sizeDelta = new Vector2(newWidth, lifeBar.sizeDelta.y);
        lifeBar.position = new Vector3( lifeBar.position.x + 10 - xOffset/2, lifeBar.position.y, lifeBar.position.z);

        if (health > 6)
            lifeBar.gameObject.GetComponent<Image>().color = Color.green;
        else if (health > 3)
            lifeBar.gameObject.GetComponent<Image>().color = Color.yellow;
        else
            lifeBar.gameObject.GetComponent<Image>().color = Color.red;
    }

    /// <summary>
    /// Coroutine that executes the dying animation
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadAnimation()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsBlocking", false);
        anim.SetFloat("GoingRight", 0f);
        anim.SetFloat("GoingForward", 0f);
        anim.SetBool("IsDead", true);

        if (!effectSound.isPlaying)
            effectSound.Play();

        yield return null;

        GM.PlayerIsDead();
    }

    /// <summary>
    /// Coroutine that change the weapon image in the GUI
    /// </summary>
    /// <param name="currentGUI">Weapon that it is being shown</param>
    /// <param name="nextGUI">Next weapon</param>
    /// <returns></returns>
    IEnumerator ChangeWeaponGUI(GameObject currentGUI, GameObject nextGUI)
    {
        weaponGUIisChanging = true;
        currentGUI.SetActive(false);
        nextGUI.SetActive(true);
        nextGUI.GetComponent<Animator>().SetBool("IsActive", true);
        yield return new WaitForSeconds(1.5f);
        nextGUI.GetComponent<Animator>().SetBool("IsActive", false);
        nextGUI.SetActive(false);
        currentGUI.SetActive(true);
        weaponGUIisChanging = false;
    }

    public bool IsPlayerOnRoom()
    {
        return isInGenerationRoom;
    }
}
