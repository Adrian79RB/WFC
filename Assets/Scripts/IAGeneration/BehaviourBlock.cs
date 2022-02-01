using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourBlock : MonoBehaviour
{
    [Tooltip("Enter conections used to get in this behaviour")]
    public int[] enterConnections;

    [Tooltip("Exit conections used to get out this behaviour")]
    public int[] exitConnections;

    public List<BehaviourBlock> children;
    
    // Start is called before the first frame update
    public BehaviourBlock()
    {
        children = new List<BehaviourBlock>();
    }

    public abstract bool RunCondition(Dictionary<string, int> gameData);

    public abstract void Run(Agent agent, Dictionary<string, int> gameData);
}

public class Patrol : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, int> gameData)
    {
        if(gameData["playerDetected"] == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Run(Agent agent, Dictionary<string, int> gameData)
    {
        agent.GoPatrolling();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(agent, gameData);
            }
        }
    }
}


