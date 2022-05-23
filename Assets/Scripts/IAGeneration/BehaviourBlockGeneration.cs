using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BlockType {Retreat, StrategicPos, GetClose, GetAway, Attack, Shoot, Patrol } // All behaviour block types

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

    /// <summary>
    /// Method that established the initial conditions of the grid where the generation is going to be performed
    /// </summary>
    private void Initialize()
    {
        // Create the blocks objects that are going to be used as block set
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

        // AI variables grid creation and initialization of each AIVariable
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

        // Creation of the first grid block and the root node of the decision tree
        Block firstBlock = new Block();
        firstBlock.type = BlockType.Patrol;
        firstBlock.block = new Patrol();
        firstBlock.block.SetConnections(new int[connectionsNum] { -1, -1, 0, 0 });

        grid[0, 0].SetBlock(firstBlock, -1);
        rootVariable = grid[0, 0];
    }

    /// <summary>
    /// Remove all the nodes from the decision tree
    /// </summary>
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

    // Set to initial state all the elements in the Node(AIVariable)
    void ClearNode(IAVariable currentNode)
    {
        // Firstly clearing all the children of the current AIVariable
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

    /// <summary>
    /// Method that generates a decision tree for one agent
    /// </summary>
    /// <returns></returns>
    public BehaviourBlock Generate()
    {
        if (treeCleared)
        {
            treeCleared = false;
            Initialize(); // Initialise the grid for the current agent
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

    /// <summary>
    /// Transform the grid connections into behaviour block connected that form the decision tree
    /// </summary>
    /// <param name="currentVariable">Current variable of the grid</param>
    private void GenerateBehaviourBlocksChildren(IAVariable currentVariable)
    {
        currentVariable.blockChoosen.SetChildren(currentVariable.children);

        // Go through the connected AIVariables and established them as children of the current Behaviour block
        for (int i = 0; i < currentVariable.children.Count; i++)
        {
            if (currentVariable.children[i].blockChoosen != null)
            {
                IAVariable nextChild = currentVariable.children[i];
                GenerateBehaviourBlocksChildren(nextChild);
            }
        }

    }

    /// <summary>
    /// Select the next grid of the cell to be analised depending on their entropy value
    /// </summary>
    /// <returns></returns>
    public int[] SearchNextGridCell()
    {
        float finalEntropy = Mathf.Infinity;
        int nextCol = -1;
        int nextRow = -1;

        // Go through the entire grid
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j].blockChoosen == null && grid[i, j].domainCount == 1) // If a cell with one block in its domain is found, select this one
                {
                    int[] nextGridCell = new int[2] { i, j };
                    return nextGridCell;
                }
                else if (grid[i, j].domainCount > 1) // Search for the cell with minimum entropy value
                {
                    var entropy = grid[i, j].entropy;
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

    /// <summary>
    /// Method that selects the block which is going to take up the current AIVariable of the grid
    /// </summary>
    /// <param name="rowIndex">Index that represent the row of the grid</param>
    /// <param name="colIndex">Index that represent the colum of the grid</param>
    public void BlockElection(int rowIndex, int colIndex)
    {
        int chosenIndex = -1;
        if(grid[rowIndex, colIndex].blockChoosen == null) // The AIVariable hasn't got established a block yet
        {
            if(grid[rowIndex, colIndex].domainCount == 1) // There is one block remaining in the AIVariable domain
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
            else if(grid[rowIndex, colIndex].domainCount > 1) // There are more than one block remaining in the AIVariable domain
            {
                // Create an array that contains only the active blocks in the domain
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

                // Randomly choose an index among the active blocks
                chosenIndex = weightedRandom(availableBlocks);
            }

            // Establish the selected blocked in the AIVariable position of the grid
            grid[rowIndex, colIndex].SetBlock(blockSet[chosenIndex],  chosenIndex);
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

        // Search the next AIVariable that has to be analised
        int[] nextGridCell = SearchNextGridCell();
        if (nextGridCell[0] != -1 && nextGridCell[1] != -1)
            BlockElection(nextGridCell[0], nextGridCell[1]);
    }

    /// <summary>
    /// Propagation of the constraints determined by a block that has been established by the BlockElection method
    /// </summary>
    /// <param name="rowIndex">Index that represents the row of the grid</param>
    /// <param name="colIndex">Index that represents the colum of the grid</param>
    /// <param name="direction">Index that represents which was the direction took for the propagation in the last variable</param>
    /// <param name="lastCell">Reference to the last cell analised</param>
    /// <param name="step">Number that represents the depth of the propagation</param>
    private void ConstraintPropagation(int rowIndex, int colIndex, int direction, IAVariable lastCell, int step)
    {
        //Condition limiting the depth of the propagation
        if (lastCell.domainCount < 1 || grid[rowIndex, colIndex].blockChoosen != null || grid[rowIndex, colIndex].domainCount < 1 || step >= maxSteps)
            return;

        step++;
        if(lastCell.blockChoosen != null) // Last cell analised has been taken up by a behaviour block
        {
            for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
            {
                if(grid[rowIndex, colIndex].domain[i] )
                {
                    // Check if the block breaks the constraints of the last cell block
                    if( (direction == 1 && lastCell.blockChoosen.exitConnections[0] != blockSet[i].block.enterConnections[1]) 
                        || (direction == 2 && lastCell.blockChoosen.exitConnections[1] != blockSet[i].block.enterConnections[0]))
                    {
                        grid[rowIndex, colIndex].domain[i] = false;
                        grid[rowIndex, colIndex].domainCount--;
                    }
                }
            }
        }
        else // Last cell has several blocks available in its domain
        {
            for (int i = 0; i < lastCell.domain.Length; i++) // Go through the last cell domain
            {
                if (lastCell.domain[i])
                {
                    for (int j = 0; j < grid[rowIndex, colIndex].domain.Length; j++) // Go through the current cell domain
                    {
                        if(grid[rowIndex, colIndex].domain[j])
                        {
                            // Check if the block respects the constraints of the last cell block
                            if((direction == 1 && blockSet[i].block.exitConnections[0] == blockSet[j].block.enterConnections[1])
                                || (direction == 2 && blockSet[i].block.exitConnections[1] == blockSet[j].block.enterConnections[0]))
                            {
                                grid[rowIndex, colIndex].visited[j]++;
                            }
                        }
                    }
                }
            }

            // Check which block from the current cell don't respect any constraint
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

    /// <summary>
    /// Random election among the blocks inside a block set biased by their weight
    /// </summary>
    /// <param name="availableBlocks">Block set formed by the active blocks</param>
    /// <returns>Index of the block chosen</returns>
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
