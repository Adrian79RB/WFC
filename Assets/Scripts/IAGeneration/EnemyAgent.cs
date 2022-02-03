using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgent : MonoBehaviour
{
    // Constants used to decide in the tree
    public float maxHealth;
    public float maxAmmo;
    public float safeDistance;
    public float AttackDistance;
    public float ShootDistance;
    public float hitDistance;

    // Behaviour block Set
    public Block[] enemyBlockSet;

    // Game data container to use the BehviourBlocks
    Dictionary<string, float> gameData; // keys: playerDetected, health, maxHealth, allyNum, ammo, maxAmmo, distanceToPlayer, safeDistance, AttackDistance, hitDistance, shootDistance

    // Variables used to check the state of the game 
    bool playerDetected;
    float ammo;
    float health;
    EnemyAgent[] allies;

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

        //currentBlock = currentBlock.Run(this, gameData);
    }

    public void GoPatrolling()
    {
        throw new NotImplementedException();
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
