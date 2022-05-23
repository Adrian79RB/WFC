using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAVariable : ScriptableObject
{
    public bool[] domain;
    public int domainCount;
    public int[] visited;
    public BehaviourBlock blockChoosen;
    public float entropy;
    public List<IAVariable> children;

    private const int connectionsNum = 4;

    /// <summary>
    /// Established the variable in the grid
    /// </summary>
    /// <param name="size">Length of the block set array</param>
    public void SetVariable(int size)
    {
        children = new List<IAVariable>();
        domain = new bool[size];
        entropy = 0f;

        for (int i = 0; i < size; i++)
            domain[i] = true;

        visited = new int[size];
        blockChoosen = null;
        domainCount = size;
    }

    /// <summary>
    /// Create the object that represent the final behaviour block that takes up a grid position
    /// </summary>
    /// <param name="currentBlock">Block selected from the block set</param>
    /// <param name="index">Index of the block in the block set array</param>
    internal void SetBlock(Block currentBlock, int index)
    {
        // Create the block object
        int[] Connections;
        switch (currentBlock.type.ToString())
        {
            case "Patrol":
                blockChoosen = new Patrol();
                Connections = new int[connectionsNum] { -1, -1, 0, 0 }; break;
            case "Retreat":
                blockChoosen = new Retreat();
                Connections = new int[connectionsNum] { 0, 0, 0, 0 }; break;
            case "StrategicPos":
                blockChoosen = new StrategicPositioning();
                Connections = new int[connectionsNum] { 0, 0, 0, 0 }; break;
            case "GetClose":
                blockChoosen = new GetClose();
                Connections = new int[connectionsNum] { 0, 0, 1, 1 }; break;
            case "GetAway":
                blockChoosen = new GetAway();
                Connections = new int[connectionsNum] { 0, 0, 1, 1 }; break;
            case "Attack":
                blockChoosen = new Attack();
                Connections = new int[connectionsNum] { 1, 1, -1, -1 }; break;
            case "Shoot":
                blockChoosen = new Shoot();
                Connections = new int[connectionsNum] { 1, 1, -1, -1 }; break;
            default:
                Connections = new int[0]; break;
        }

        blockChoosen.SetConnections(Connections);

        //Remove the other blocks from the domain
        if(domainCount > 1)
        {
            domainCount = 1;
            for (int i = 0; i < domain.Length; i++)
            {
                if (i != index && domain[i])
                    domain[i] = false;
            }
        }
        else if(index < 0)
        {
            domainCount = 1;
            for (int i = 0; i < domain.Length; i++)
            {
                domain[i] = false;
            }
        }
    }

    /// <summary>
    /// Method that calculates the entropy value for each variable
    /// </summary>
    /// <param name="tileSet">Complete block set that it is being used in the generation</param>
    public void CalculateEntropy(Block[] tileSet)
    {
        float auxiliar = 0f;
        float maxWeight = 0f;

        //Calculate the total weight of the active block in the domain
        for (int i = 0; i < tileSet.Length; i++)
        {
            if (domain[i])
                maxWeight += tileSet[i].weight;
        }

        // Calculate the entropy value using the entropy formula
        for (int i = 0; i < domain.Length; i++)
        {
            if (domain[i])
            {
                auxiliar += (tileSet[i].weight/maxWeight) * Mathf.Log((tileSet[i].weight/maxWeight));
            }
        }

        entropy = auxiliar * (-1);
    }
}
