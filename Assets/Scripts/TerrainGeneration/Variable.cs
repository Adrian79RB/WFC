using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variable : ScriptableObject
{
    public bool[] domain;
    public int domainCount;
    public int[] visited;
    public GridTile tileChosen;
    public bool[,] rotDomain;
    public int[,] rotVisited;
    public int rotDirection = 0;
    public float entropy;
    public GameObject tileReference;

    public void SetVariable(int size)
    {
        int rotSize = 4;
        domain = new bool[size];
        rotDomain = new bool[size, rotSize];
        entropy = 0f;
        for (int i = 0; i < size; i++)
        {
            domain[i] = true;
            for (int j = 0; j < rotSize; j++)
            {
                rotDomain[i, j] = true;
            }
        }
        visited = new int[size];
        rotVisited = new int[size, rotSize];
        tileChosen = null;
        domainCount = size;
    }

    internal void SetTile(GridTile tile, int chosenIndex, Vector3 position, Transform parent)
    {
        tileChosen = tile;

        if(domainCount > 1)
        {
            domainCount = 1;
            for (int i = 0; i < domain.Length; i++)
            {
                if (i != chosenIndex)
                    domain[i] = false;
            }
        }

        tileReference = Instantiate(tile.gameObject, position, tileChosen.transform.rotation, parent);
    }

    public void CalculateEntropy(Tile[] tileSet)
    {
        float auxiliar = 0f;
        float maxWeight = 0f;

        for (int i = 0; i < tileSet.Length; i++)
        {
            if (domain[i])
                maxWeight += tileSet[i].weight;
        }

        for(int i = 0; i < domain.Length; i++)
        {
            if (domain[i])
            {
                auxiliar += (tileSet[i].weight/maxWeight) * Mathf.Log((tileSet[i].weight/maxWeight));
            }
        }

        entropy = auxiliar * (-1);
        //entropy = ((-1) * active) / tileSet.Length * Mathf.Log(active / tileSet.Length) - inactive / tileSet.Length * Mathf.Log(inactive / tileSet.Length);
    }
}
