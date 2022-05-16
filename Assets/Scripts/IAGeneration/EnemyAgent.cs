using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Archer, Swordman}

public class EnemyAgent : MonoBehaviour
{
    [Header("Select Enemy Type")]
    public EnemyType type;

    [Header("Game Manager")]
    public GameManager GM;

    [Header("Making Decisions")]
    // Constants used to decide in the tree
    public float maxHealth;
    public float maxAmmo;
    public float safeDistance;
    public float AttackDistance;
    public float ShootDistance;
    public float hitDistance;

    // Behaviour block Set
    [Header("Block Set")]
    public List<Block> enemyBlockSet;

    // Enemy Functional stuff
    [Header("Enemy Functionality stuff")]
    public float shootForce;
    public Transform tileMap;
    public NavMeshAgent agent;
    public Transform shootPos;
    public GameObject arrow;
    public GameObject sword;
    public Transform currentWaypoint;
    public Transform[] waypoints;
    public List<Transform> homePositions;
    public List<Vector3> strategicalPosition;

    [Header("Animation Stuff")]
    public Animator anim;

    [Header("Sound Stuff")]
    public AudioSource stepSound;
    public AudioSource effectSound;
    public AudioClip enemyHurt;
    public AudioClip enemyAttack;
    public AudioClip enemyBlock;
    public AudioClip bowCharge;
    public AudioClip[] stepsSoundEffect;

    [Header("Tutorial Stuff")]
    public GameObject endTutorialButton;



    // Game data container to use the BehviourBlocks
    Dictionary<string, float> gameData; // keys: {playerDetected, health, maxHealth, allyNum, ammo, maxAmmo, distanceToPlayer, 
                                        // safeDistance, AttackDistance, hitDistance, shootDistance}

    // Variables used to check the state of the game 
    public bool playerDetected;
    float ammo;
    float health;
    EnemyAgent[] allies;
    int allyNum;
    GameObject player;
    bool isBlocking;
    bool isAttacking;

    // Enemy Functional variables
    int waypointIndex;
    float waitTimer = 5.0f;
    Transform homeWaypoint;
    bool retreating = false;
    bool strategicallyHide = false;
    float shootCoolDown = 5.0f;
    bool reloading = false;
    bool gettingAway = true;
    bool headingPlayer = false;
    GameObject detector;

    // Variables that check the gameData Update
    float gameDataUpdateTimer = 2.0f;
    float gameDataUpdateTime = 0.0f;

    // BehaviourGenerator and BehaviourTree
    BehaviourBlockGeneration treeGenerator;
    BehaviourBlock  rootVariable;
    [SerializeField]BehaviourBlock currentBlock;

    // Tiles Grid to search the strategical positions
    Transform predefinedPath;
    GameObject[,] gameObjectGrid;

    // Damage control variables
    bool damaged = false;
    float damagedCoolDown = 1.25f;
    float damagedTime = 0f;

    void Start()
    {
        // Creating the Decision Tree
        treeGenerator = new BehaviourBlockGeneration();
        treeGenerator.blockSet = enemyBlockSet.ToArray();
        if (rootVariable != null)
        {
            ClearBlockTree(rootVariable);
            treeGenerator.ClearTree();
        }

        while (rootVariable == null)
        {
            rootVariable = treeGenerator.Generate();
            if (rootVariable == null)
                treeGenerator.ClearTree();
        }
        currentBlock = rootVariable;

        DebugArbol(); // Method that shows the tree blocks
    }

    private void OnEnable()
    {
        // Initializing the Game Variables
        playerDetected = false;
        ammo = maxAmmo;
        health = maxHealth;
        allies = FindObjectsOfType<EnemyAgent>();
        homeWaypoint = GameObject.Find("homeWaypoint").transform;
        player = GameObject.Find("Player");
        detector = transform.Find("Detector").gameObject;

        for(int i = 0; i < tileMap.childCount; i++)
        {
            if(tileMap.GetChild(i).name.Contains("Predefined") && tileMap.GetChild(i).gameObject.activeSelf)
                predefinedPath = tileMap.GetChild(i);
        }

        gameObjectGrid = new GameObject[tileMap.GetComponent<TileSetGenerator>().numRow, tileMap.GetComponent<TileSetGenerator>().numCol];

        // Initializing the game data structure
        gameData = new Dictionary<string, float>();
        gameDataUpdateTime = gameDataUpdateTimer;

        // Searching for covers in the arena
        if (!GM.isInTutorial)
        {
            GetGameObjectGrid();
            GetStrategicalPositions();
        }

        stepSound.clip = stepsSoundEffect[GM.tileSetChoosen];
    }

    private void DebugArbol()
    {
        Queue<BehaviourBlock> visited = new Queue<BehaviourBlock>();
        BehaviourBlock auxBlock = currentBlock;
        visited.Enqueue(auxBlock);

        while (visited.Count > 0)
        {
            auxBlock = visited.Dequeue();
            Debug.Log("Bloque actual: " + auxBlock + "; num hijos: " + auxBlock.children.Count);

            for (int i = 0; i < auxBlock.children.Count; i++)
            {
                Debug.Log("Child " + i + ": " + auxBlock.children[i]);
                visited.Enqueue(auxBlock.children[i]);
            }
        }
    }

    /*private void DebugArbol(BehaviourBlock block)
    {
        Debug.Log("Bloque actual: " + block + "; num hijos: " + block.children.Count);

        if (block.children.Count <= 0)
            return;

        for(int i = 0; i < block.children.Count; i++)
            Debug.Log("Child: " + i + ": " + block.children[i]);

        for (int i = 0; i < block.children.Count; i++)
            DebugArbol(block.children[i]);
    }*/

    private void ClearBlockTree(BehaviourBlock block)
    {
        for (int i = 0; i < block.children.Count; i++)
        {
            ClearBlockTree(block.children[i]);
        }

        block.children.Clear();
        block = null;
    }

    private void GetGameObjectGrid()
    {
        if (predefinedPath != null && predefinedPath.gameObject.activeSelf)
        {
            for (int i = 0; i < predefinedPath.childCount; i++)
            {
                int[] coor = { Mathf.FloorToInt(predefinedPath.GetChild(i).position.z), Mathf.FloorToInt(predefinedPath.GetChild(i).position.x) };
                gameObjectGrid[coor[0], coor[1]] = predefinedPath.GetChild(i).gameObject;
            }
        }

        for (int i = 0; i < tileMap.childCount; i++)
        {
            int[] coor = { Mathf.FloorToInt(tileMap.GetChild(i).position.z), Mathf.FloorToInt(tileMap.GetChild(i).position.x) };
            gameObjectGrid[coor[0], coor[1]] = tileMap.GetChild(i).gameObject;
        }
    }

    /// <summary>
    /// This method is needed to find the positions near to cover in the procedural generated environment
    /// </summary>
    private void GetStrategicalPositions()
    {
        // Search trhough all the tiles grid
        for (int i = 0; i < gameObjectGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gameObjectGrid.GetLength(1); j++)
            {
                if (gameObjectGrid[i, j].name.Contains("Cover")) // Checking that the current tile is a cover
                {
                    // Checking that the neighbour tiles are not covers, and they are inside the grid
                    if ((i - 1) > 0 && !gameObjectGrid[i - 1, j].name.Contains("Cover"))
                    {
                        var pos = new Vector3(j * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, 
                            tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, 
                            (i - 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((i + 1) < (gameObjectGrid.GetLength(0) - 1) && !gameObjectGrid[i + 1, j].name.Contains("Cover"))
                    {
                        var pos = new Vector3(j * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, 
                            tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, 
                            (i + 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j - 1) > 0 && !gameObjectGrid[i, j - 1].name.Contains("Cover"))
                    {
                        var pos = new Vector3((j - 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, 
                            tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, 
                            i * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j + 1) < (gameObjectGrid.GetLength(1) - 1) && !gameObjectGrid[i, j + 1].name.Contains("Cover"))
                    {
                        var pos = new Vector3((j + 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, 
                            tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, 
                            i * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                }
            }
        }
    }

    void Update()
    {
        // Timer that check the game data Update
        gameDataUpdateTime += Time.deltaTime;
        if (gameDataUpdateTime >= gameDataUpdateTimer)
        {
            gameDataUpdateTime = 0.0f;
            GetGameData();
        }

        var aux = currentBlock;
        currentBlock = currentBlock.Run(this, gameData);

        if(currentBlock != aux)
        {
            Debug.Log(transform.name + "run a new Behaviour Block: " + currentBlock);
        }

        // Restarting some behaviour variables
        if (currentBlock.ToString() != "Retreat" && retreating)
            retreating = false;
        else if (currentBlock.ToString() != "StrategicPositioning" && strategicallyHide)
            strategicallyHide = false;
        else if (currentBlock.ToString() != "GetClose" && headingPlayer)
            headingPlayer = false;

        if (damaged)
        {
            damagedTime += Time.deltaTime;
            if(damagedTime >= damagedCoolDown)
            {
                damaged = false;
                damagedTime = 0f;
            }
        }
    }

    private void RotateEnemy(Vector3 targetPos)
    {
        // Calculate the rotation to face the player
        Vector3 playerDirection = (targetPos - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(playerDirection, transform.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.8f);
    }

    /// <summary>
    /// This is the clasical patrolling behaviour, it alternates between a patrolling and stay-still
    /// </summary>
    public void GoPatrolling()
    {
        if (currentWaypoint == null)
        {
            waypointIndex = 0;
            currentWaypoint = waypoints[waypointIndex];
            anim.SetBool("IsMoving", true);

            if (!stepSound.isPlaying)
                stepSound.Play();
        }

        agent.SetDestination(currentWaypoint.position);

        // Moving among the patrol waypoints
        if (!agent.isStopped && Vector3.Distance(currentWaypoint.position, transform.position) < agent.stoppingDistance)
        {
            if (UnityEngine.Random.value > 0.2f)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Length)
                    waypointIndex = 0;

                currentWaypoint = waypoints[waypointIndex];
                anim.SetBool("IsMoving", true);

                if (!stepSound.isPlaying)
                    stepSound.Play();
            }
            else
            {
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);

                if (stepSound.isPlaying)
                    stepSound.Stop();
            }
        }
        else if (agent.isStopped)// Waiting time until start patrolling again
        {
            if (waitTimer == 5.0f)
            {
                Vector3 pos = transform.position + transform.forward * 20;
                RotateEnemy(pos);
            }

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waitTimer = 5.0f;
                agent.isStopped = false;
                anim.SetBool("IsMoving", true);

                if (!stepSound.isPlaying)
                    stepSound.Play();
            }
        }
    }

    /// <summary>
    /// This behaviour makes the enemy go back to a fortificate position to wait until the player arrives.
    /// </summary>
    internal void RetreatToHome()
    {
        // Going to the Fortificate position in the arena
        if(currentWaypoint != homeWaypoint && !retreating)
        {
            if (agent.isStopped)
                agent.isStopped = false;

            if (type == EnemyType.Archer && anim.GetBool("IsAiming"))
                anim.SetBool("IsAiming", false);

            currentWaypoint = homeWaypoint;
            anim.SetBool("IsMoving", true);
            retreating = true;

            if (!stepSound.isPlaying)
                stepSound.Play();
        }

        // Selecting an Strategical position in the fortificate area
        if (!homePositions.Contains(currentWaypoint) 
            && Vector3.Distance(transform.position, homeWaypoint.position) < agent.stoppingDistance)
        {
            if (type == EnemyType.Archer)
            {
                var furthestPosition = -Mathf.Infinity;
                for (int i = 0; i < homePositions.Count; i++)
                {
                    var selected = false;
                    foreach (EnemyAgent ally in allies)
                    {
                        if (homePositions[i] == ally.currentWaypoint)
                        {
                            selected = true;
                            break;
                        }
                    }

                    if (!selected) 
                    {
                        var distance = Vector3.Distance(homePositions[i].position, player.transform.position);
                        if (distance > furthestPosition)
                        {
                            currentWaypoint = homePositions[i];
                            furthestPosition = distance;
                        }
                    }
                }
            }
            else
            {
                var closestPosition = Mathf.Infinity;
                for (int i = 0; i < homePositions.Count; i++)
                {
                    var selected = false;
                    foreach (EnemyAgent ally in allies)
                    {
                        if (homePositions[i] == ally.currentWaypoint)
                        {
                            selected = true;
                            break;
                        }
                    }

                    if (!selected)
                    {
                        var distance = Vector3.Distance(homePositions[i].position, player.transform.position);
                        if (distance < closestPosition)
                        {
                            currentWaypoint = homePositions[i];
                            closestPosition = distance;
                        }
                    }
                }
            }

            anim.SetBool("IsMoving", true);
            if (!stepSound.isPlaying)
                stepSound.Play();
        }

        agent.SetDestination(currentWaypoint.position);

        // Waiting for the player to arrive
        if (currentWaypoint != homeWaypoint 
            && Vector3.Distance(transform.position, currentWaypoint.position) < agent.stoppingDistance)
        {            
            agent.isStopped = true;
            anim.SetBool("IsMoving", false);
            if (stepSound.isPlaying)
                stepSound.Stop();
            RotateEnemy(player.transform.position);
        }
    }

    /// <summary>
    /// This behaviour makes the enemy search among some predefined position which is the best one to attack the player.
    /// </summary>
    internal void SearchStrategicPos()
    {
        if (!strategicallyHide)
        {
            if (agent.isStopped)
                agent.isStopped = false;

            var nextPos = currentWaypoint.position;
            // Searching a strategical position in the arena
            if (!strategicalPosition.Contains(agent.destination))
            {
                var bestDistance = Mathf.Infinity;
                for (int i = 0; i < strategicalPosition.Count; i++)
                {
                    var selected = false;
                    foreach (EnemyAgent ally in allies)
                    {
                        if (strategicalPosition[i] == ally.currentWaypoint.position)
                        {
                            selected = true;
                            break;
                        }
                    }

                    if (!selected)
                    {
                        var distance = Vector3.Distance(strategicalPosition[i], player.transform.position);
                        if (distance > safeDistance && distance < bestDistance)
                        {
                            nextPos = strategicalPosition[i];
                            bestDistance = distance;
                        }
                    }
                }

                anim.SetBool("IsMoving", true);
                if (!stepSound.isPlaying)
                    stepSound.Play();
            }

            agent.SetDestination(nextPos);

            // Waiting the player to arrive
            if (Vector3.Distance(transform.position, nextPos) < 1f)
            {
                anim.SetBool("IsMoving", false);
                agent.isStopped = true;
                strategicallyHide = true;

                if (stepSound.isPlaying)
                    stepSound.Stop();
            }
        }
        else
        {
            var pos = new Vector3(player.transform.position.x, player.transform.position.y + .5f, player.transform.position.z);
            RotateEnemy(pos);
        }
    }

    /// <summary>
    /// A kind of Pursuit Behaviour that makes the enemy get closer to the player
    /// </summary>
    internal void GetCloseToPlayer()
    {
        if(!headingPlayer)
        {
            if (agent.isStopped)
                agent.isStopped = false;

            float time = gameData["distanceToPlayer"] / agent.speed; // Time that las the enemy to arrive to the player

            Vector3 futurePlayerPosition = player.transform.position + 
                player.GetComponent<Player>().movement * player.GetComponent<Player>().movementSpeed * time; // Future position of the player in that time

            if (!anim.GetBool("IsMoving"))
                anim.SetBool("IsMoving", true);

            if (!stepSound.isPlaying)
                stepSound.Play();

            agent.SetDestination(futurePlayerPosition);

            // If the enemy is close enought go for the player
            if (Vector3.Distance(transform.position, futurePlayerPosition) >= gameData["distanceToPlayer"])
            {
                agent.SetDestination(player.transform.position);
                headingPlayer = true;
            }
        }
        else
        {
            if (!anim.GetBool("IsMoving"))
                anim.SetBool("IsMoving", true);

            agent.SetDestination(player.transform.position);
            RotateEnemy(player.transform.position);
        }
    }

    /// <summary>
    /// A kind of Flee Behaviour that makes the enemy get away from the player
    /// </summary>
    internal void GetAwayFromPlayer()
    {
        if (gettingAway)
        {
            if (anim.GetBool("IsAiming"))
                anim.SetBool("IsAiming", false);

            if (agent.isStopped)
                agent.isStopped = false;

            Vector3 direction = (player.transform.position - transform.position).normalized; // Get the opposite direction to the player
            Vector3 targetPos = -direction * safeDistance; // Calculate an appropriate position to get distance from the player

            targetPos.x = Mathf.Clamp(targetPos.x, 1f, gameObjectGrid.GetLength(1) - 2);
            targetPos.z = Mathf.Clamp(targetPos.z, 1f, gameObjectGrid.GetLength(0) - 2);

            if (!anim.GetBool("IsMoving"))
                anim.SetBool("IsMoving", true);
            if (!stepSound.isPlaying)
                stepSound.Play();

            agent.SetDestination(targetPos);

            if (Vector3.Distance(transform.position, targetPos) < agent.stoppingDistance)
            {
                gettingAway = false;
                agent.isStopped = true;

                if (anim.GetBool("IsMoving"))
                    anim.SetBool("IsMoving", false);
                if (stepSound.isPlaying)
                    stepSound.Stop();
            }
        }
    }

    internal void Attack()
    {
        if (gameData["distanceToPlayer"] > 1.7f) // Get to hit distance of the player 
        {
            if (agent.isStopped)
                agent.isStopped = false;

            currentWaypoint = player.transform;
            anim.SetBool("IsMoving", true);
            if (!stepSound.isPlaying)
                stepSound.Play();

            agent.SetDestination(currentWaypoint.position);
        }
        else if(!isAttacking && !isBlocking)
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            anim.SetBool("IsMoving", false);
            if (stepSound.isPlaying)
                stepSound.Stop();

            var randomValue = UnityEngine.Random.value;
            if ( randomValue > 0.8) // Attack the player
            {
                StartCoroutine("attackAnimation");
            }
            else if(randomValue < 0.2) // Block the player attack
            {
                StartCoroutine("blockAnimation");
            }
            var pos = new Vector3(player.transform.position.x, player.transform.position.y + .5f, player.transform.position.z);
            RotateEnemy(pos);
        }
    }

    internal void Shoot()
    {
        // The enemy stops moving
        if (!agent.isStopped)
            agent.isStopped = true;
        if (anim.GetBool("IsMoving"))
            anim.SetBool("IsMoving", false);
        if (stepSound.isPlaying)
            stepSound.Stop();

        if (!anim.GetBool("IsAiming"))
            anim.SetBool("IsAiming", true);

        // Calculate the rotation to face the player
        var pos = new Vector3(player.transform.position.x, player.transform.position.y + .5f, player.transform.position.z);
        RotateEnemy(pos);

        Vector3 playerDirection = player.transform.position - transform.position;
        playerDirection.y += .5f;
        playerDirection = playerDirection.normalized;

        // Check if there is enought ammo to shoot and the player is visible to shoot them
        RaycastHit hit;
        if (!reloading && ammo > 0 && Physics.Raycast(shootPos.position, playerDirection, out hit, ShootDistance) && hit.transform.tag == "Player")
        {
            StartCoroutine(shootAnimation(playerDirection));
        }
        else if (reloading)
        {
            shootCoolDown -= Time.deltaTime;
            if (shootCoolDown < 0)
            {
                reloading = false;
                shootCoolDown = 5.0f;
            }
        }
    }

    public void ReceiveDamage(float damage)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        if (!isBlocking && !damaged)
        {
            effectSound.clip = enemyHurt;
            effectSound.Play();

            damaged = true;
            health -= damage;
            if (health <= 0f)
            {
                StartCoroutine("DeathAnimation");
                if (GM.isInTutorial)
                    endTutorialButton.SetActive(true);
            }
        }
        else if (isBlocking)
        {
            effectSound.clip = enemyBlock;
            effectSound.Play();
        }
    }

    internal void PlayerDetected()
    {
        playerDetected = true;
        detector.SetActive(false);

        Collider[] allies = Physics.OverlapSphere(transform.position, 20f, LayerMask.GetMask("Enemy"));
        foreach (Collider ally in allies)
        {
            if (!ally.GetComponent<EnemyAgent>().playerDetected)
            {
                ally.GetComponent<EnemyAgent>().PlayerDetected();
            }
        }
    }

    IEnumerator attackAnimation()
    {
        effectSound.clip = enemyAttack;
        effectSound.Play();

        isAttacking = true;
        anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(.5f);
        sword.GetComponent<BoxCollider>().enabled = true;
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("IsAttacking", false);
        sword.GetComponent<BoxCollider>().enabled = false;
        isAttacking = false;
    }

    IEnumerator blockAnimation()
    {
        isBlocking = true;
        anim.SetBool("IsBlocking", true);
        yield return new WaitForSeconds(1.5f);
        isBlocking = false;
        anim.SetBool("IsBlocking", false);
    }

    IEnumerator shootAnimation(Vector3 playerDirection)
    {
        effectSound.clip = bowCharge;
        effectSound.Play();

        Debug.Log("Shooting arrow");

        anim.SetBool("IsShooting", true);
        Rigidbody rgbdArrow = Instantiate(arrow, shootPos.position, shootPos.rotation, shootPos).GetComponent<Rigidbody>();
        rgbdArrow.AddForce(playerDirection * shootForce, ForceMode.Impulse);
        reloading = true;
        ammo--;
        yield return new WaitForSeconds(.5f);
        anim.SetBool("IsShooting", false);
    }

    IEnumerator DeathAnimation()
    {
        agent.isStopped = true;
        RotateEnemy(transform.forward);
        GetComponent<Rigidbody>().isKinematic = true;

        anim.SetBool("IsMoving", false);
        if (type == EnemyType.Archer)
        {
            reloading = true;
            anim.SetBool("IsShooting", false);
            anim.SetBool("IsAiming", false);
        }
        else
        {
            sword.GetComponent<BoxCollider>().enabled = false;
            anim.SetBool("IsBlocking", false);
            anim.SetBool("IsAttacking", false);
        }

        anim.SetBool("IsDead", true);
        if (!effectSound.isPlaying)
            effectSound.Play();

        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
        GM.AddDeadEnemy();
    }

    private void GetGameData()
    {
        if (gameData.Count > 0) // Update the game data
        {
            if (playerDetected)
                gameData["playerDetected"] = 1.0f;
            else
                gameData["playerDetected"] = 0.0f;

            gameData["health"] = health;
            gameData["ammo"] = ammo;

            allyNum = 0;
            for (int i = 0; i < allies.Length; i++)
            {
                if (allies[i].gameObject.activeSelf)
                    allyNum++;
            }
            gameData["allyNum"] = allyNum;

            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            gameData["distanceToPlayer"] = distanceToPlayer;

            var dir = (player.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(shootPos.position, dir, out hit) && hit.transform.tag == "Player")
                gameData["playerVisible"] =  1.0f;
            else
                gameData["playerVisible"] = 0.0f;
        }
        else // Create the game data for the first time
        {
            if (playerDetected)
                gameData.Add("playerDetected", 1.0f);
            else
                gameData.Add("playerDetected", 0.0f);

            gameData.Add("health", health);
            gameData.Add("maxHealth", maxHealth);
            gameData.Add("ammo", ammo);
            gameData.Add("maxAmmo", maxAmmo);

            for (int i = 0; i < allies.Length; i++)
            {
                if (allies[i].gameObject.activeSelf)
                    allyNum++;
            }
            gameData.Add("allyNum", allyNum);

            gameData.Add("safeDistance", safeDistance);
            gameData.Add("AttackDistance", AttackDistance);
            gameData.Add("hitDistance", hitDistance);
            gameData.Add("ShootDistance", ShootDistance);

            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            gameData.Add("distanceToPlayer", distanceToPlayer);

            var dir = (player.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(shootPos.position, dir, out hit) && hit.transform.tag == "Player")
                gameData.Add("playerVisible", 1.0f);
            else
                gameData.Add("playerVisible", 0.0f);
        }
    }
}
