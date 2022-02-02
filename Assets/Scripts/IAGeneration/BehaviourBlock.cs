using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourBlock : MonoBehaviour
{
    [Tooltip("Enter conections used to get in this behaviour")]
    public int[] enterConnections; // 0 -> up side of the block; 1 -> left side of the block

    [Tooltip("Exit conections used to get out this behaviour")]
    public int[] exitConnections; // 0 -> right side of the block; 1 -> down side of the block

    public List<BehaviourBlock> children;
    
    public void SetChildren(List<IAVariable> childrenVariables)
    {
        children = new List<BehaviourBlock>();
        for (int i = 0; i < childrenVariables.Count; i++)
        {
            children.Add(childrenVariables[i].blockChoosen);
        }
    }

    public void SetConnections(int[] Connections)
    {
        for (int i = 0; i < Connections.Length; i++)
        {
            if (i < Connections.Length / 2)
                enterConnections[i] = Connections[i];
            else
                exitConnections[i - Connections.Length / 2] = Connections[i];
        }
    }

    public abstract bool RunCondition(Dictionary<string, float> gameData);

    public abstract void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData);
}

public class Patrol : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDeteted"] == 0f;
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GoPatrolling();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}

public class Retreat : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["health"] < gameData["maxHealth"] / 3 || gameData["enemyNum"] < 5f || gameData["ammo"] < gameData["maxAmmo"] / 3;
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.RetreatToHome();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}


public class StrategicPositioning : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["distanceToPlayer"] > gameData["safeDistance"];
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.SearchStrategicPos();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}

public class GetClose : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["distanceToPlayer"] < gameData["AttackDistance"];
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GetCloseToPlayer();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}

public class GetAway : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["distanceToPlayer"] < gameData["AttackDistance"];
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GetAwayFromPlayer();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}

public class Attack : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["DistanceToPlayer"] < gameData["hitDistance"];
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.Attack();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}

public class Shoot : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["DistanceToPlayer"] > gameData["shootDistance"];
    }

    public override void Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.Shoot();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                children[i].Run(enemyAgent, gameData);
            }
        }
    }
}


