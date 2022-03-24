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
    bool isBlocking;
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

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
            else
            {
                StartCoroutine("SwordAttack");
                if(!weaponGUIisChanging)
                    StartCoroutine(ChangeWeaponGUI(swordGUI, swordGUI));
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine("SwordBlock");
            if(!weaponGUIisChanging)
                StartCoroutine(ChangeWeaponGUI(swordGUI, shieldGUI));
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
        stepSound.clip = stepSoundsClip[GM.tileSetChoosen];
        EnterFightingPhase();
        Respawn(newPos);
    }

    public void Respawn(Vector3 newPos)
    {
        transform.position = newPos;
        StartCoroutine("ActivateCharacterController");
    }

    public void EnterFightingPhase()
    {
        handGUI.SetActive(false);
        swordGUI.SetActive(true);
        isInGenerationRoom = false;
    }

    public void EnterInteractivPhase()
    {
        swordGUI.SetActive(false);
        handGUI.SetActive(true);
        isInGenerationRoom = true;
    }

    IEnumerator ActivateCharacterController()
    {
        GetComponent<CharacterController>().enabled = false;
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

            ChangeLifeBarGUI();
        }
        else if (isBlocking && !effectSound.isPlaying)
        {
            effectSound.clip = blockSound;
            effectSound.Play();
        }
    }

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
