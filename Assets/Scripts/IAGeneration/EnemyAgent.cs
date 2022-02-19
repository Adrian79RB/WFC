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
    public Block[] enemyBlockSet;

    // Enemy Functional stuff
    [Header("Enemy Functionality stuff")]
    public float shootForce;
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
    float shootCoolDown = 3.0f;
    bool reloading = false;

    // Variables that check the gameData Update
    float gameDataUpdateTimer = 5.0f;
    float gameDataUpdateTime = 0.0f;

    // BehaviourGenerator and BehaviourTree
    BehaviourBlockGeneration treeGenerator;
    BehaviourBlock rootBlock;
    BehaviourBlock currentBlock;

    // Tiles Grid to search the strategical positions
    Transform tileMap;
    Transform predefinedPath;
    GameObject[,] gameObjectGrid;

    // Damage control variables
    bool damaged = false;
    float damagedCoolDown = 2.0f;
    float damagedTime = 0f;

    void Start()
    {
        // Initializing the Game Variables
        playerDetected = false;
        ammo = maxAmmo;
        health = maxHealth;
        allies = FindObjectsOfType<EnemyAgent>();
        homeWaypoint = GameObject.Find("homeWaypoint").transform;
        player = GameObject.Find("Player");

        tileMap = GameObject.Find("TilesGenerator").transform;
        predefinedPath = tileMap.Find("PredefinedPath");
        gameObjectGrid = new GameObject[tileMap.GetComponent<TileSetGenerator>().numRow, tileMap.GetComponent<TileSetGenerator>().numCol];

        // Initializing the game data structure
        gameData = new Dictionary<string, float>();
        gameDataUpdateTime = gameDataUpdateTimer;

        // Creating the Decision Tree
        treeGenerator = new BehaviourBlockGeneration();
        treeGenerator.blockSet = enemyBlockSet;

        while (rootBlock == null)
        {
            rootBlock = treeGenerator.Generate();
            if (rootBlock == null)
                treeGenerator.ClearTree();
        }
        currentBlock = rootBlock;

        if (tileMap.GetComponent<TileSetGenerator>().terrainGenerated)
        {
            GetGameObjectGrid();
            GetStrategicalPositions();
        }

        DebugArbol(); // Method that shows the tree blocks
    }

    private void DebugArbol()
    {
        Queue<BehaviourBlock> visited = new Queue<BehaviourBlock>();
        visited.Enqueue(currentBlock);

        while (visited.Count > 0)
        {
            currentBlock = visited.Dequeue();
            Debug.Log("Bloque actual: " + currentBlock + "; num hijos: " + currentBlock.children.Count);

            for (int i = 0; i < currentBlock.children.Count; i++)
            {
                Debug.Log("Child " + i + ": " + currentBlock.children[i]);
                visited.Enqueue(currentBlock.children[i]);
            }
        }

        currentBlock = rootBlock;
    }

    /// <summary>
    /// Method called from Game Manager when player generate the terrain in game
    /// </summary>
    public void InGameGameObjectGridGeneration()
    {
        if (tileMap.GetComponent<TileSetGenerator>().terrainGenerated)
        {
            GetGameObjectGrid();
            GetStrategicalPositions();
        }
    }

    private void GetGameObjectGrid()
    {
        if (predefinedPath.gameObject.activeSelf)
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
                        var pos = new Vector3(j * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, (i - 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((i + 1) < (gameObjectGrid.GetLength(0) - 1) && !gameObjectGrid[i + 1, j].name.Contains("Cover"))
                    {
                        var pos = new Vector3(j * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, (i + 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j - 1) > 0 && !gameObjectGrid[i, j - 1].name.Contains("Cover"))
                    {
                        var pos = new Vector3((j - 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, i * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j + 1) < (gameObjectGrid.GetLength(1) - 1) && !gameObjectGrid[i, j + 1].name.Contains("Cover"))
                    {
                        var pos = new Vector3((j + 1) * tileMap.GetComponent<TileSetGenerator>().tileSize.x + tileMap.GetComponent<TileSetGenerator>().tileSize.x / 2, tileMap.GetComponent<TileSetGenerator>().tileSize.y / 2, i * tileMap.GetComponent<TileSetGenerator>().tileSize.z + tileMap.GetComponent<TileSetGenerator>().tileSize.z / 2);
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

        currentBlock = currentBlock.Run(this, gameData);

        // Restarting some behaviour variables
        if (currentBlock.ToString() != "Retreat" && retreating)
            retreating = false;
        else if (currentBlock.ToString() != "StrategicPositioning" && strategicallyHide)
            strategicallyHide = false;

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
        if (!playerDetected)
        {
            if (currentWaypoint == null)
            {
                waypointIndex = 0;
                currentWaypoint = waypoints[waypointIndex];
                anim.SetBool("IsMoving", true);
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
                }
                else
                {
                    agent.isStopped = true;
                    anim.SetBool("IsMoving", false);
                }
            }
            else if (agent.isStopped)// Waiting time until start patrolling again
            {
                if(waitTimer == 5.0f)
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
                }
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

            currentWaypoint = homeWaypoint;
            retreating = true;
        }

        // Selecting an Strategical position in the fortificate area
        if (!homePositions.Contains(currentWaypoint) && Vector3.Distance(transform.position, homeWaypoint.position) < agent.stoppingDistance)
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
        }

        agent.SetDestination(currentWaypoint.position);

        // Waiting for the player to arrive
        if (currentWaypoint != homeWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) < agent.stoppingDistance)
        {
            agent.isStopped = true;
            anim.SetBool("IsMoving", false);
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
            Debug.Log("Executing Strategic Pos");

            if (agent.isStopped)
                agent.isStopped = false;

            var nextPos = currentWaypoint.position;
            // Searching a strategical position in the arena
            if (!strategicalPosition.Contains(agent.destination))
            {
                Debug.Log("Analising Strategical Pos");
                var bestDistance = Mathf.Infinity;
                for (int i = 0; i < strategicalPosition.Count; i++)
                {
                    var selected = false;
                    foreach (EnemyAgent ally in allies)
                    {
                        Debug.Log("Searching among allies");
                        if (strategicalPosition[i] == ally.currentWaypoint.position)
                        {
                            selected = true;
                            break;
                        }
                    }

                    if (!selected)
                    {
                        var distance = Vector3.Distance(strategicalPosition[i], player.transform.position);
                        Debug.Log("Calculating Distance: " + distance + "; best distance: " + bestDistance + "; safe distance: " + safeDistance);
                        if (distance > safeDistance && distance < bestDistance)
                        {
                            nextPos = strategicalPosition[i];
                            bestDistance = distance;
                        }
                    }
                }

                anim.SetBool("IsMoving", true);
            }

            agent.SetDestination(nextPos);

            // Waiting the player to arrive
            if (Vector3.Distance(transform.position, nextPos) < 1f)
            {
                Debug.Log("Stopping");
                anim.SetBool("IsMoving", false);
                agent.isStopped = true;
                strategicallyHide = true;
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
        if (agent.isStopped)
            agent.isStopped = false;

        float time = gameData["distanceToPlayer"] / agent.speed; // Time that las the enemy to arrive to the player
        Vector3 futurePlayerPosition = player.transform.position + player.transform.forward * player.GetComponent<Player>().movementSpeed * time; // Future position of the player in that time

        if(!anim.GetBool("IsMoving"))
            anim.SetBool("IsMoving", true);

        agent.SetDestination(futurePlayerPosition);

        // If the enemy is close enought go for the player
        if (Vector3.Distance(transform.position, futurePlayerPosition) >= gameData["distanceToPlayer"])
        {
            agent.SetDestination(player.transform.position);
        }
    }

    /// <summary>
    /// A kind of Evade Behaviour that makes the enemy get away from the player
    /// </summary>
    internal void GetAwayFromPlayer()
    {
        if (anim.GetBool("IsAiming"))
            anim.SetBool("IsAiming", false);

        float time = gameData["distanceToPlayer"] / agent.speed; // Time that las the enemy to arrive to the player
        Vector3 futurePlayerPosition = player.transform.position + player.transform.forward * player.GetComponent<Player>().movementSpeed * time; // Future position of the player in that time

        Vector3 direction = (futurePlayerPosition - transform.position).normalized; // Get the opposite direction to the player
        Vector3 targetPos = -direction * ShootDistance; // Calculate an appropriate position to shoot to the player

        targetPos.x = Mathf.Clamp(targetPos.x, 0f, gameObjectGrid.GetLength(1));
        targetPos.z = Mathf.Clamp(targetPos.z, 0f, gameObjectGrid.GetLength(0));

        if(!anim.GetBool("IsMoving"))
            anim.SetBool("IsMoving", true);
        agent.SetDestination(targetPos);
    }

    internal void Attack()
    {
        if (gameData["distanceToPlayer"] > hitDistance) // Get to hit distance of the player 
        {
            if (agent.isStopped)
                agent.isStopped = false;

            currentWaypoint = player.transform;
            anim.SetBool("IsMoving", true);
            agent.SetDestination(currentWaypoint.position);
        }
        else if(!isAttacking && !isBlocking)
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            anim.SetBool("IsMoving", false);
            var randomValue = UnityEngine.Random.value;
            if ( randomValue > 0.6) // Attack the player
            {
                StartCoroutine("attackAnimation");
            }
            else if(randomValue < 0.3 && !isBlocking) // Block the player attack
            {
                StartCoroutine("blockAnimation");
            }
            var pos = new Vector3(player.transform.position.x, player.transform.position.y + .5f, player.transform.position.z);
            RotateEnemy(pos);
        }
    }

    internal void Shoot()
    {
        Debug.Log("Shooting");
        if (anim.GetBool("IsMoving"))
            anim.SetBool("IsMoving", false);

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
        Debug.Log("Va a disparar");
        if (!reloading && ammo > 0 && Physics.Raycast(shootPos.position, playerDirection, out hit, ShootDistance) && hit.transform.tag == "Player")
        {
            StartCoroutine(shootAnimation(playerDirection));
            Debug.Log("Flecha creada");
        }
        else if (reloading)
        {
            shootCoolDown -= Time.deltaTime;
            if (shootCoolDown < 0)
            {
                reloading = false;
                shootCoolDown = 3.0f;
            }
        }
    }

    public void ReceiveDamage(float damage)
    {
        if (!isBlocking && !damaged)
        {
            damaged = true;
            health -= damage;
            if (health <= 0f)
                StartCoroutine("DeathAnimation");
        }
    }

    internal void PlayerDetected()
    {
        playerDetected = true;

        // Warn the other allies of the player position
        for (int i = 0; i < allies.Length; i++)
        {
            if (allies[i].gameObject.activeSelf && !allies[i].playerDetected) // Creo que no se hace bucle infinito por que el primero que pille al jugador activa todos los demas
                allies[i].PlayerDetected();
        }
    }

    IEnumerator attackAnimation()
    {
        isAttacking = true;
        sword.GetComponent<BoxCollider>().enabled = true;
        anim.SetBool("IsAttacking", true);
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
        anim.SetBool("IsMoving", false);
        if (type == EnemyType.Archer)
        {
            anim.SetBool("IsShooting", false);
            anim.SetBool("IsAiming", false);
        }
        else
        {
            anim.SetBool("IsBlocking", false);
            anim.SetBool("IsAttacking", false);
        }
        anim.SetBool("IsDead", true);
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
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
        }
    }
}
