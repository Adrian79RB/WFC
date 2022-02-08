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
    public bool isBlocking;
    public NavMeshAgent agent;
    public Transform shootPos;
    public GameObject arrow;
    public Transform[] waypoints;
    public Transform[] homePositions;
    public List<Vector3> strategicalPosition; //TODO: Falta implementar como obtenemos las posiciones del mapa


    // Game data container to use the BehviourBlocks
    Dictionary<string, float> gameData; // keys: {playerDetected, health, maxHealth, allyNum, ammo, maxAmmo, distanceToPlayer, 
                                        // safeDistance, AttackDistance, hitDistance, shootDistance}

    // Variables used to check the state of the game 
    bool playerDetected;
    float ammo;
    float health;
    EnemyAgent[] allies;
    int allyNum;
    GameObject player;

    // Enemy Functional variables
    int waypointIndex;
    float waitTimer = 5.0f;
    Transform currentWaypoint;
    Transform homeWaypoint;
    bool retreating = false;
    bool strategicallyHide = false;

    // Variables that check the gameData Update
    float gameDataUpdateTimer = 5.0f;
    float gameDataUpdateTime = 0.0f;

    // BehaviourGenerator and BehaviourTree
    BehaviourBlockGeneration treeGenerator;
    BehaviourBlock rootBlock;
    BehaviourBlock currentBlock;

    // Tiles Grid to search the strategical positions
    TileSetGenerator tileGenerator;
    Variable[,] tilesGrid;

    void Start()
    {
        // Initializing the Game Variables
        playerDetected = false;
        ammo = maxAmmo;
        health = maxHealth;
        allies = FindObjectsOfType<EnemyAgent>();
        homeWaypoint = GameObject.Find("homeWaypoint").transform;
        player = GameObject.FindGameObjectWithTag("Player");
        tileGenerator = FindObjectOfType<TileSetGenerator>();
        tilesGrid = tileGenerator.grid;

        // Initializing the game data structure
        gameData = new Dictionary<string, float>();

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

        GetStrategicalPositions();

        //DebugArbol(currentBlock); // Method that shows the tree blocks
    }

    private void DebugArbol()
    {
        Queue<BehaviourBlock> visited = new Queue<BehaviourBlock>();

        while (visited.Count > 0)
        {
            Debug.Log("Bloque actual: " + currentBlock + "; num hijos: " + currentBlock.children.Count);
            for (int i = 0; i < currentBlock.children.Count; i++)
            {
                visited.Enqueue(currentBlock.children[i]);
            }

            currentBlock = visited.Dequeue();
        }
    }

    /// <summary>
    /// This method is needed to find the positions near to cover in the procedural generated environment
    /// </summary>
    private void GetStrategicalPositions()
    {
        // Search trhough all the tiles grid
        for (int i = 0; i < tilesGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tilesGrid.GetLength(1); j++)
            {
                if (tilesGrid[i, j].tileChosen.name.Contains("Cover")) // Checking that the current tile is a cover
                {
                    // Checking that the neighbour tiles are not covers, and they are inside the grid
                    if ((i - 1) > 0 && !tilesGrid[i - 1, j].tileChosen.name.Contains("Cover"))
                    {
                        var pos = new Vector3(j * tileGenerator.tileSize.x + tileGenerator.tileSize.x / 2, tileGenerator.tileSize.y / 2, (i - 1) * tileGenerator.tileSize.z + tileGenerator.tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((i + 1) < (tilesGrid.GetLength(0) - 1) && !tilesGrid[i + 1, j].tileChosen.name.Contains("Cover"))
                    {
                        var pos = new Vector3(j * tileGenerator.tileSize.x + tileGenerator.tileSize.x / 2, tileGenerator.tileSize.y / 2, (i + 1) * tileGenerator.tileSize.z + tileGenerator.tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j - 1) > 0 && !tilesGrid[i, j - 1].tileChosen.name.Contains("Cover"))
                    {
                        var pos = new Vector3((j - 1) * tileGenerator.tileSize.x + tileGenerator.tileSize.x / 2, tileGenerator.tileSize.y / 2, i * tileGenerator.tileSize.z + tileGenerator.tileSize.z / 2);
                        if (!strategicalPosition.Contains(pos))
                            strategicalPosition.Add(pos);
                    }
                    if ((j + 1) < (tilesGrid.GetLength(1) - 1) && !tilesGrid[i, j + 1].tileChosen.name.Contains("Cover"))
                    {
                        var pos = new Vector3((j + 1) * tileGenerator.tileSize.x + tileGenerator.tileSize.x / 2, tileGenerator.tileSize.y / 2, i * tileGenerator.tileSize.z + tileGenerator.tileSize.z / 2);
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
        if(gameDataUpdateTime >= gameDataUpdateTimer)
        {
            gameDataUpdateTime = 0.0f;
            GetGameData();
        }

        currentBlock = currentBlock.Run(this, gameData);

        // Restarting some behaviour variables
        if (currentBlock.GetType().ToString() != "RetreatToHome" && retreating)
            retreating = false;
        else if (currentBlock.GetType().ToString() != "SearchStrategicPos" && strategicallyHide)
            strategicallyHide = false;
    }

    private void RotateEnemy(Vector3 targetPos)
    {
        // Calculate the rotation to face the player
        Vector3 playerDirection = (targetPos - transform.position).normalized;
        Quaternion rot = Quaternion.FromToRotation(transform.forward, playerDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, agent.angularSpeed);
    }

    /// <summary>
    /// This is the clasical patrolling behaviour, it alternates between a patrolling and stay-still
    /// </summary>
    public void GoPatrolling()
    {
        if (!playerDetected)
        {
            // Moving among the patrol waypoints
            if(Vector3.Distance(currentWaypoint.position, transform.position) < agent.stoppingDistance)
            {
                if (waypointIndex >= waypoints.Length)
                    waypointIndex = 0;

                if (UnityEngine.Random.value > 0.2f)
                    currentWaypoint = waypoints[waypointIndex];
                else
                    agent.isStopped = true;
            }

            // Waiting time until start patrolling again
            if (agent.isStopped)
            {
                waitTimer -= Time.deltaTime;
                if(waitTimer <= 0f)
                {
                    waitTimer = 5.0f;
                    agent.isStopped = false;
                }
            }
            else 
            {
                agent.Move(currentWaypoint.position);
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
            currentWaypoint = homeWaypoint;
            retreating = true;
        }

        // Selecting an Strategical position in the fortificate area
        if (Vector3.Distance(transform.position, homeWaypoint.position) < agent.stoppingDistance)
        {
            if (type == EnemyType.Archer)
            {
                var furthestPosition = -Mathf.Infinity;
                for (int i = 0; i < homePositions.Length; i++)
                {
                    var distance = Vector3.Distance(homePositions[i].position, player.transform.position);
                    if (distance > furthestPosition)
                    {
                        currentWaypoint = homePositions[i];
                        furthestPosition = distance;
                    }
                }
            }
            else
            {
                var closestPosition = Mathf.Infinity;
                for (int i = 0; i < homePositions.Length; i++)
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

        agent.Move(currentWaypoint.position);

        // Waiting for the player to arrive
        if (currentWaypoint != homeWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) < agent.stoppingDistance)
        {
            agent.isStopped = true;
            RotateEnemy(player.transform.position);
        }
        else
            RotateEnemy(currentWaypoint.position);
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
            if (!strategicalPosition.Contains(nextPos))
            {
                var bestDistance = Mathf.Infinity;
                for (int i = 0; i < strategicalPosition.Count; i++)
                {
                    var distance = Vector3.Distance(strategicalPosition[i], player.transform.position);
                    if(distance > safeDistance && distance < bestDistance)
                    {
                        nextPos = strategicalPosition[i];
                        bestDistance = distance;
                    }
                }
            }

            agent.Move(nextPos);

            // Waiting the player to arrive
            if (Vector3.Distance(transform.position, nextPos) < agent.stoppingDistance)
            {
                agent.isStopped = true;
                strategicallyHide = true;
                RotateEnemy(player.transform.position);
            }
            else
                RotateEnemy(nextPos);
        }
    }

    /// <summary>
    /// A kind of Pursuit Behaviour that makes the enemy get closer to the player
    /// </summary>
    internal void GetCloseToPlayer()
    {
        float time = gameData["distanceToPlayer"] / agent.speed; // Time that las the enemy to arrive to the player
        Vector3 futurePlayerPosition = player.transform.position + player.GetComponent<Rigidbody>().velocity * time; // Future position of the player in that time

        agent.Move(futurePlayerPosition);


        // If the enemy is close enought go for the player
        if (Vector3.Distance(transform.position, futurePlayerPosition) >= gameData["distanceToPlayer"])
        {
            agent.Move(player.transform.position);
            RotateEnemy(player.transform.position);
        }
        else
            RotateEnemy(futurePlayerPosition);
    }

    /// <summary>
    /// A kind of Evade Behaviour that makes the enemy get away from the player
    /// </summary>
    internal void GetAwayFromPlayer()
    {
        float time = gameData["distanceToPlayer"] / agent.speed; // Time that las the enemy to arrive to the player
        Vector3 futurePlayerPosition = player.transform.position + player.GetComponent<Rigidbody>().velocity * time; // Future position of the player in that time

        Vector3 direction = (futurePlayerPosition - transform.position).normalized; // Get the opposite direction to the player
        Vector3 targetPos = -direction * ShootDistance; // Calculate an appropriate position to shoot to the player
        
        agent.Move(targetPos);
        RotateEnemy(targetPos);
    }

    internal void Attack()
    {
        if(gameData["distanceToPlayer"] > hitDistance) // Get to hit distance of the player 
        {
            currentWaypoint = player.transform;
            agent.Move(currentWaypoint.position);
            RotateEnemy(currentWaypoint.position);
        }
        else
        {
            var randomValue = UnityEngine.Random.value;
            if ( randomValue > 0.2) // Attack the player
            {
                // Ejecutar animación de ataque
            }
            else if(randomValue < 0.1) // Block the player attack
            {
                isBlocking = true;
                StartCoroutine("blockAnimation");
            }
            RotateEnemy(player.transform.position);
        }
    }

    internal void Shoot()
    {
        // Calculate the rotation to face the player
        RotateEnemy(player.transform.position);
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;

        // Check if there is enought ammo to shoot and the player is visible to shoot them
        RaycastHit hit;
        if(ammo > 0 && Physics.Raycast(shootPos.position, playerDirection, out hit, ShootDistance) && hit.transform.tag == "Player")
        {
            Instantiate(arrow, shootPos.position, shootPos.rotation, shootPos);
            ammo--;
        }
    }

    public void ReceiveDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
            StartCoroutine("DeathAnimation");
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

    IEnumerator blockAnimation()
    {
        // Shoot block animation
        yield return new WaitForSeconds(1.0f);
        isBlocking = false;
        // Stop block animation
    }

    IEnumerator DeathAnimation()
    {
        // Shoot death animation
        yield return new WaitForSeconds(2.0f);
        // Stop animation
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
