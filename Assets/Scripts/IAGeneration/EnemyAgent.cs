using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour
{
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
    public NavMeshAgent agent;
    public Transform[] waypoints;

    // Game data container to use the BehviourBlocks
    Dictionary<string, float> gameData; // keys: playerDetected, health, maxHealth, allyNum, ammo, maxAmmo, distanceToPlayer, safeDistance, AttackDistance, hitDistance, shootDistance

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

    // Variables that check the gameData Update
    float gameDataUpdateTimer = 5.0f;
    float gameDataUpdateTime = 0.0f;

    // BehaviourGenerator and BehaviourTree
    BehaviourBlockGeneration treeGenerator;
    BehaviourBlock rootBlock;
    BehaviourBlock currentBlock;

    // Dictionary Update

    void Start()
    {
        // Initializing the Game Variables
        playerDetected = false;
        ammo = maxAmmo;
        health = maxHealth;
        allies = FindObjectsOfType<EnemyAgent>();

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

        //DebugArbol(currentBlock); // Method that shows the tree blocks
    }

    private void DebugArbol()
    {
        Queue<BehaviourBlock> visited = new Queue<BehaviourBlock>();

        while (currentBlock != null)
        {
            Debug.Log("Bloque actual: " + currentBlock + "; num hijos: " + currentBlock.children.Count);
            for (int i = 0; i < currentBlock.children.Count; i++)
            {
                visited.Enqueue(currentBlock.children[i]);
            }

            if (visited.Count > 0)
                currentBlock = visited.Dequeue();
            else
                currentBlock = null;
        }
    }

    void Update()
    {
        gameDataUpdateTime += Time.deltaTime;
        if(gameDataUpdateTime >= gameDataUpdateTimer)
        {
            gameDataUpdateTime = 0.0f;
            GetGameData();
        }
        
        //currentBlock = currentBlock.Run(this, gameData);
    }

    private void GetGameData()
    {
        if(gameData.Count > 0)
        {
            if (playerDetected)
                gameData["playerDetected"] = 1.0f;
            else
                gameData["playerDetected"] = 0.0f;

            gameData["health"] = health;
            gameData["ammo"] = ammo;
            gameData["allyNum"] = allyNum;

            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            gameData["distanceToPlayer"] = distanceToPlayer;
        }
        else
        {
            if (playerDetected)
                gameData.Add("playerDetected", 1.0f);
            else
                gameData.Add("playerDetected", 0.0f);

            gameData.Add("health", health);
            gameData.Add("maxHealth", maxHealth);
            gameData.Add("ammo", ammo);
            gameData.Add("maxAmmo", maxAmmo);
            gameData.Add("allyNum", allyNum);
            gameData.Add("safeDistance", safeDistance);
            gameData.Add("AttackDistance", AttackDistance);
            gameData.Add("hitDistance", hitDistance);
            gameData.Add("ShootDistance", ShootDistance);

            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            gameData.Add("distanceToPlayer", distanceToPlayer);
        }
    }

    public void GoPatrolling()
    {
        if (!playerDetected)
        {
            if(Vector3.Distance(currentWaypoint.position, transform.position) < agent.stoppingDistance)
            {
                if (waypointIndex >= waypoints.Length)
                    waypointIndex = 0;

                if (UnityEngine.Random.value > 0.2f)
                    currentWaypoint = waypoints[waypointIndex];
                else
                    agent.isStopped = true;
            }

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

    internal void RetreatToHome()
    {
        throw new NotImplementedException();
    }

    internal void SearchStrategicPos()
    {
        throw new NotImplementedException();
    }

    internal void GetCloseToPlayer()
    {
        throw new NotImplementedException();
    }

    internal void GetAwayFromPlayer()
    {
        throw new NotImplementedException();
    }

    internal void Attack()
    {
        throw new NotImplementedException();
    }

    internal void Shoot()
    {
        throw new NotImplementedException();
    }
}
