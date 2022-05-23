using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourBlock
{
    [Tooltip("Enter conections used to get in this behaviour")]
    public int[] enterConnections; // 0 -> up side of the block; 1 -> left side of the block

    [Tooltip("Exit conections used to get out this behaviour")]
    public int[] exitConnections; // 0 -> right side of the block; 1 -> down side of the block

    public List<BehaviourBlock> children;
    
    /// <summary>
    /// Establish the block children, that connect each block with the next executable block in the decision tree
    /// </summary>
    /// <param name="childrenVariables">Variables that represent these block in the grid</param>
    public void SetChildren(List<IAVariable> childrenVariables)
    {
        children = new List<BehaviourBlock>();
        for (int i = 0; i < childrenVariables.Count; i++)
        {
            if (childrenVariables[i].blockChoosen != null)
            {
                children.Add(childrenVariables[i].blockChoosen);
            }
        }
    }

    /// <summary>
    /// Establish which are the connection constraint of the block
    /// </summary>
    /// <param name="Connections"></param>
    public void SetConnections(int[] Connections)
    {
        enterConnections = new int[Connections.Length/2];
        exitConnections = new int[Connections.Length / 2];

        for (int i = 0; i < Connections.Length; i++)
        {
            if (i < Connections.Length / 2)
                enterConnections[i] = Connections[i];
            else
                exitConnections[i - Connections.Length / 2] = Connections[i];
        }
    }

    /// <summary>
    /// Method that contains the condition to execute the current behaviour block
    /// </summary>
    /// <param name="gameData">Dictionary that contains the environment information</param>
    /// <returns>The condition is accomplished or not</returns>
    public abstract bool RunCondition(Dictionary<string, float> gameData);

    /// <summary>
    /// Method that calls the enemyAgent appropriate method
    /// </summary>
    /// <param name="enemyAgent">Reference to the agent that is executing the behaviour block</param>
    /// <param name="gameData">Dictionary that contains the environment information</param>
    /// <returns>Returns the next Behaviour block that have to be execute, if any</returns>
    public abstract BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData);
}

// Patrol behaviour which inherit from the basic BehaviourBlock class
public class Patrol : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 0f;
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GoPatrolling();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// Retreat behaviour which inherit from the basic BehaviourBlock class
public class Retreat : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && (gameData["health"] < gameData["maxHealth"] / 3 
            || gameData["allyNum"] < 4f || gameData["ammo"] < gameData["maxAmmo"] / 2);
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.RetreatToHome();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// Strategic Positioning behaviour which inherit from the basic BehaviourBlock class
public class StrategicPositioning : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && (gameData["distanceToPlayer"] < gameData["safeDistance"] || gameData["playerVisible"] == 1.0f);
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.SearchStrategicPos();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// GetClose behaviour which inherit from the basic BehaviourBlock class
public class GetClose : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && gameData["distanceToPlayer"] < gameData["AttackDistance"];
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GetCloseToPlayer();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// GetAway behaviour which inherit from the basic BehaviourBlock class
public class GetAway : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && gameData["distanceToPlayer"] < gameData["safeDistance"];
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.GetAwayFromPlayer();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// Attack behaviour which inherit from the basic BehaviourBlock class
public class Attack : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && gameData["distanceToPlayer"] < gameData["hitDistance"];
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.Attack();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}

// Shoot behaviour which inherit from the basic BehaviourBlock class
public class Shoot : BehaviourBlock
{
    public override bool RunCondition(Dictionary<string, float> gameData)
    {
        return gameData["playerDetected"] == 1f && (gameData["distanceToPlayer"] > gameData["ShootDistance"] || gameData["playerVisible"] == 1.0f);
    }

    public override BehaviourBlock Run(EnemyAgent enemyAgent, Dictionary<string, float> gameData)
    {
        enemyAgent.Shoot();

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].RunCondition(gameData))
            {
                return children[i];
            }
        }

        return this;
    }
}


