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
        
        treeGenerator.blockSet = new Block[4];
        treeGenerator.blockSet[0].type = BlockType.Retreat;
        treeGenerator.blockSet[0].weight = 4;
        treeGenerator.blockSet[1].type = BlockType.StrategicPos;
        treeGenerator.blockSet[1].weight = 1;
        treeGenerator.blockSet[2].type = BlockType.GetClose;
        treeGenerator.blockSet[2].weight = 4;
        treeGenerator.blockSet[3].type = BlockType.Attack;
        treeGenerator.blockSet[3].weight = 2;

        while (rootBlock == null)
        {
            rootBlock = treeGenerator.Generate();
            if (rootBlock == null)
                treeGenerator.ClearTree();
        }
        currentBlock = rootBlock;
        DebugArbol(currentBlock);
    }

    private void DebugArbol(BehaviourBlock block)
    {
        if (block == null)
            return;
        Debug.Log("Bloque actual: " + block + "; Hijo 1: " + block.children[0] + "; Hijo 2: " + block.children[1]);
        Debug.Log("Num Hijos: " + block.children.Count);
        for (int i = 0; i < block.children.Count; i++)
        {
            Debug.Log("Hijo " + i + ": " + block.children[i]);
            DebugArbol(block.children[i]);
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
