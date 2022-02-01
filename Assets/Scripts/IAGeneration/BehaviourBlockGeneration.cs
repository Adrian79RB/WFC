using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Block
{
    int weight;
    BehaviourBlock block;
}

public class BehaviourBlockGeneration : MonoBehaviour
{
    public int maxSteps;
    public Block[] blockSet;

    IAVariable rootVariable;
    IAVariable currentVariable;
    bool treeCleared;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
