using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BlockType { Retreat, StrategicPos, GetClose, GetAway, Attack, Shoot }

[System.Serializable]
public struct Block
{
    public BlockType type;
    public int weight;
    public BehaviourBlock block;
}

public class BehaviourBlockGeneration
{
    public int numCol = 4;
    public int numRow = 4;
    public int maxSteps = 4;
    public const int connectionsNum = 4;
    public Block[] blockSet;

    IAVariable rootVariable;
    IAVariable[,] grid;
    bool treeCleared = true;

    private void Initialize()
    {
        for (int i = 0; i < blockSet.Length; i++)
        {
            int[] Connections;
            switch (blockSet[i].type.ToString())
            {
                case "Retreat":
                    blockSet[i].block = new Retreat();
                    Connections = new int[connectionsNum] { 0, 0, 0, 0 }; break;
                case "StrategicPos":
                    blockSet[i].block = new StrategicPositioning();
                    Connections = new int[connectionsNum] { 0, 0, 0, 0 }; break;
                case "GetClose":
                    blockSet[i].block = new GetClose();
                    Connections = new int[connectionsNum] { 0, 0, 1, 1 }; break;
                case "GetAway":
                    blockSet[i].block = new GetAway();
                    Connections = new int[connectionsNum] { 0, 0, 1, 1 }; break;
                case "Attack":
                    blockSet[i].block = new Attack();
                    Connections = new int[connectionsNum] { 1, 1, -1, -1 }; break;
                case "Shoot":
                    blockSet[i].block = new Shoot();
                    Connections = new int[connectionsNum] { 1, 1, -1, -1 }; break;
                default:
                    Connections = new int[0]; break;
            }

            blockSet[i].block.SetConnections(Connections);
        }

        grid = new IAVariable[numRow, numCol];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = ScriptableObject.CreateInstance<IAVariable>();
                grid[i, j].SetVariable(blockSet.Length);
                grid[i, j].CalculateEntropy(blockSet);
            }
        }

        BehaviourBlock firstBlock = new Patrol();
        firstBlock.SetConnections(new int[connectionsNum] { -1, -1, 0, 0 });
        grid[0, 0].SetBlock(firstBlock, -1);
        rootVariable = grid[0, 0];
    }

    public void ClearTree()
    {
        if (!treeCleared){
            treeCleared = true;

            ClearNode(rootVariable);
            rootVariable = null;

            grid = null;
        }
        else
        {
            Debug.Log("Tree already cleared");
        }
    }

    void ClearNode(IAVariable currentNode)
    {
        if(currentNode.children.Count > 0)
        {
            for (int i = 0; i < currentNode.children.Count; i++)
            {
                ClearNode(currentNode.children[i]);
                currentNode.children[i] = null;
            }
        }

        currentNode.entropy = 0;
        currentNode.domainCount = 0;
        currentNode.blockChoosen = null;
        currentNode.domain = null;
        currentNode.visited = null;
    }

    public BehaviourBlock Generate()
    {
        if (treeCleared)
        {
            treeCleared = false;
            Initialize();
            BlockElection(0, 0);

            //Generate behaviour blocks children
            GenerateBehaviourBlocksChildren(rootVariable);

            return rootVariable.blockChoosen;
        }
        else
        {
            Debug.Log("Tree must be cleared before generating a new one");
            return null;
        }
    }

    private void GenerateBehaviourBlocksChildren(IAVariable currentVariable)
    {
        Debug.Log("Variable: " + currentVariable.blockChoosen + "; children: " + currentVariable.children.Count);
        currentVariable.blockChoosen.SetChildren(currentVariable.children);

        for (int i = 0; i < currentVariable.children.Count; i++)
        {
            if (currentVariable.children[i].blockChoosen != null)
            {
                Debug.Log("Variable: "+ currentVariable.blockChoosen + "; Hijo " + (i + 1) + ": " + currentVariable.children[i].blockChoosen);
                GenerateBehaviourBlocksChildren(currentVariable.children[i]);
            }
        }
    }

    public int[] SearchNextGridCell()
    {
        float finalEntropy = Mathf.Infinity;
        int nextCol = -1;
        int nextRow = -1;

        Debug.Log("Valor entropia: ");

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j].blockChoosen == null && grid[i, j].domainCount == 1)
                {
                    Debug.Log("Bloque (" + i + ", " + j + "): dominioCount: " + grid[i, j].domainCount);
                    int[] nextGridCell = new int[2] { i, j };
                    return nextGridCell;
                }
                else if (grid[i, j].domainCount > 1)
                {
                    var entropy = grid[i, j].entropy;
                    Debug.Log("Bloque (" + i + ", " + j + "): " + entropy + "; dominio: " + grid[i, j].domain[0] + ", " + grid[i, j].domain[1] + ", " + grid[i, j].domain[2] + ", " + grid[i, j].domain[3]);
                    if (entropy < finalEntropy)
                    {
                        finalEntropy = entropy;
                        nextRow = i;
                        nextCol = j;
                    }
                }
            }
        }

        return new int[2] { nextRow, nextCol };
    }

    public void BlockElection(int rowIndex, int colIndex)
    {
        int chosenIndex = -1;
        if(grid[rowIndex, colIndex].blockChoosen == null)
        {
            if(grid[rowIndex, colIndex].domainCount == 1)
            {
                for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
                {
                    if(grid[rowIndex, colIndex].domain[i])
                    {
                        chosenIndex = i;
                        break;
                    }
                }
            }
            else if(grid[rowIndex, colIndex].domainCount > 1)
            {
                Block[] availableBlocks = new Block[grid[rowIndex, colIndex].domainCount];
                int j = 0;
                for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
                {
                    if(grid[rowIndex, colIndex].domain[i])
                    {
                        availableBlocks[j] = blockSet[i];
                        j++;
                    }
                }

                chosenIndex = weightedRandom(availableBlocks);
            }

            grid[rowIndex, colIndex].SetBlock(blockSet[chosenIndex].block,  chosenIndex);
        }

        //Propagate Constraints
        //Directions = 0 -> up; 1 -> right; 2 -> down> 3 -> left
        if (colIndex < (numCol - 1))
        {
            ConstraintPropagation(rowIndex, colIndex + 1, 1, grid[rowIndex, colIndex], 1);

            if (grid[rowIndex, colIndex + 1].domainCount > 0 
                && grid[rowIndex, colIndex + 1].blockChoosen == null
                && !grid[rowIndex, colIndex].children.Contains(grid[rowIndex, (colIndex + 1)]))
                grid[rowIndex, colIndex].children.Add(grid[rowIndex, (colIndex + 1)]);
        }
        if (rowIndex < (numRow - 1))
        {
            ConstraintPropagation(rowIndex + 1, colIndex, 2, grid[rowIndex, colIndex], 1);

            if (grid[rowIndex + 1, colIndex].domainCount > 0
                && grid[rowIndex + 1, colIndex].blockChoosen == null
                && !grid[rowIndex, colIndex].children.Contains( grid[(rowIndex + 1), colIndex] ))
                grid[rowIndex, colIndex].children.Add(grid[(rowIndex + 1), colIndex]);
        }

        Debug.Log("Estoy en el bloque: " + rowIndex + ", " + colIndex);

        int[] nextGridCell = SearchNextGridCell();

        Debug.Log("Voy a: " + nextGridCell[0] + ", " + nextGridCell[1]);

        Debug.Log("Hijos de (" + rowIndex + ", " + colIndex + ") size: " + grid[rowIndex, colIndex].children.Count);

        if (nextGridCell[0] != -1 && nextGridCell[1] != -1)
            BlockElection(nextGridCell[0], nextGridCell[1]);
    }

    private void ConstraintPropagation(int rowIndex, int colIndex, int direction, IAVariable lastCell, int step)
    {
        if (lastCell.domainCount < 1 || grid[rowIndex, colIndex].blockChoosen != null || grid[rowIndex, colIndex].domainCount < 1 || step >= maxSteps)
            return;

        step++;
        if(lastCell.blockChoosen != null)  //lastCell.domainCount == 1)
        {
            for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
            {
                if(grid[rowIndex, colIndex].domain[i] )
                {
                    if( (direction == 1 && lastCell.blockChoosen.exitConnections[0] != blockSet[i].block.enterConnections[1]) 
                        || (direction == 2 && lastCell.blockChoosen.exitConnections[1] != blockSet[i].block.enterConnections[0]))
                    {
                        grid[rowIndex, colIndex].domain[i] = false;
                        grid[rowIndex, colIndex].domainCount--;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < lastCell.domain.Length; i++)
            {
                if (lastCell.domain[i])
                {
                    for (int j = 0; j < grid[rowIndex, colIndex].domain.Length; j++)
                    {
                        if(grid[rowIndex, colIndex].domain[j])
                        {
                            if((direction == 1 && blockSet[i].block.exitConnections[0] == blockSet[j].block.enterConnections[1])
                                || (direction == 2 && blockSet[i].block.exitConnections[1] == blockSet[j].block.enterConnections[0]))
                            {
                                grid[rowIndex, colIndex].visited[j]++;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < grid[rowIndex, colIndex].visited.Length; i++)
            {
                if(grid[rowIndex, colIndex].domain[i] && grid[rowIndex, colIndex].visited[i] < 1)
                {
                    grid[rowIndex, colIndex].domain[i] = false;
                    grid[rowIndex, colIndex].domainCount--;
                }
                else
                {
                    grid[rowIndex, colIndex].visited[i] = 0;
                }
            }
        }

        /*if(grid[rowIndex, colIndex].domainCount == 1)
        {
            int chosenIndex = -1;
            for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
            {
                if (grid[rowIndex, colIndex].domain[i])
                {
                    chosenIndex = i;
                    break;
                }
            }

            grid[rowIndex, colIndex].SetBlock(blockSet[chosenIndex].block, chosenIndex);
        }*/

        //Calculates the entropy of the block
        grid[rowIndex, colIndex].CalculateEntropy(blockSet);

        //Propagates the constraints
        if(colIndex < (numCol - 1))
        {
            ConstraintPropagation(rowIndex, colIndex + 1, 1, grid[rowIndex, colIndex], step);
        }

        if(rowIndex < (numRow - 1))
        {
            ConstraintPropagation(rowIndex + 1, colIndex, 2, grid[rowIndex, colIndex], step);
        }
    }

    private int weightedRandom(Block[] availableBlocks)
    {
        int max = availableBlocks[0].weight;

        for (int k = 1; k < availableBlocks.Length; k++)
        {
            max += availableBlocks[k].weight;
        }

        int target = Mathf.RoundToInt(UnityEngine.Random.Range(0, max));
        int choosenNumber = 0;
        while (target > availableBlocks[choosenNumber].weight && choosenNumber < availableBlocks.Length)
        {
            target -= availableBlocks[choosenNumber].weight;
            choosenNumber++;
        }

        for (int k = 0; k < blockSet.Length; k++)
        {
            if (availableBlocks[choosenNumber].type == blockSet[k].type)
            {
                choosenNumber = k;
                break;
            }
        }

        return choosenNumber;
    }
}
