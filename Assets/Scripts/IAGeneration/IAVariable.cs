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

    internal void SetBlock(BehaviourBlock currentBlock, int index)
    {
        blockChoosen = currentBlock;
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

    public void CalculateEntropy(Block[] tileSet)
    {
        float auxiliar = 0f;
        float maxWeight = 0f;

        for (int i = 0; i < tileSet.Length; i++)
        {
            if (domain[i])
                maxWeight += tileSet[i].weight;
        }

        for (int i = 0; i < domain.Length; i++)
        {
            if (domain[i])
            {
                auxiliar += (tileSet[i].weight/maxWeight) * Mathf.Log(tileSet[i].weight/maxWeight);
            }
        }

        entropy = auxiliar * (-1);
    }
}
