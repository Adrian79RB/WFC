using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct Tile
{
    public int weight;
    public GridTile tile;
}

public class TileSetGenerator : MonoBehaviour
{
    public int numCol;
    public int numRow;
    public int maxSteps;
    public Vector3 tileSize;
    public Tile[] tileSet;

    Variable[,] grid;
    Vector3 originalPos;
    Vector3 currentPos;
    bool gridCleared = false;
    bool preDefinedPath = false;
    int[,] predefinedPathCoor;

    // Initialization method whcih prepare all the variables needed
    private void Initialize()
    {
        originalPos = transform.position;
        currentPos = transform.position;
        grid = new Variable[numRow, numCol];

        for ( int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = ScriptableObject.CreateInstance<Variable>();
                grid[i, j].SetVariable(tileSet.Length);
            }
        }


        if (transform.childCount > 0)
        {
            preDefinedPath = true;
            predefinedPathCoor = new int[transform.childCount, 2];
            for (int i = 0; i < transform.childCount; i++)
            {
                int[] coor = { Mathf.FloorToInt(transform.GetChild(i).position.z), Mathf.FloorToInt(transform.GetChild(i).position.x) };
                predefinedPathCoor[i, 0] = coor[0];
                predefinedPathCoor[i, 1] = coor[1];
                int choosenTile = -1;

                for (int k = 0; k < tileSet.Length; k++)
                {
                    if (transform.GetChild(i).name.StartsWith(tileSet[k].tile.transform.name))
                        choosenTile = k;
                }

                for (int j = 0; j < grid[coor[0], coor[1]].domain.Length; j++)
                {
                    if (j != choosenTile)
                        grid[coor[0], coor[1]].domain[j] = false;
                }

                grid[coor[0], coor[1]].visited[choosenTile]++;
                grid[coor[0], coor[1]].domainCount = 1;
                grid[coor[0], coor[1]].tileChosen = tileSet[choosenTile].tile;
                grid[coor[0], coor[1]].CalculateEntropy(tileSet);
            }
        }
        else
        {
            preDefinedPath = false;
        }
    }

    // Clear the grid
    public void ClearTiles()
    {
        if (!gridCleared)
        {
            gridCleared = true;
            currentPos = originalPos;

            var count = transform.childCount;
            int i = 0;

            while (i < count)
            {
                DestroyImmediate(transform.GetChild(i).gameObject, true);
                count--;
            }

            grid = null;
        }
        else
        {
            Debug.Log("Grid has already been cleared, try to generate a new landscape.");
        }
    }

    // Grid Generation method
    public void Generate()
    {
        if (gridCleared)
        {
            gridCleared = false;
            Initialize();
            if (preDefinedPath)
            {
                for(int index = 0; index < predefinedPathCoor.GetLength(0); index++)
                {
                    if (predefinedPathCoor[index, 1] > 0)
                        ConstraintPropagation(predefinedPathCoor[index, 0], predefinedPathCoor[index, 1] - 1, 3, grid[predefinedPathCoor[index, 0], predefinedPathCoor[index, 1]], 0);
                    if (predefinedPathCoor[index, 1] < (numCol - 1))
                        ConstraintPropagation(predefinedPathCoor[index, 0], predefinedPathCoor[index, 1] + 1, 1, grid[predefinedPathCoor[index, 0], predefinedPathCoor[index, 1]], 0);
                    if (predefinedPathCoor[index, 0] > 0)
                        ConstraintPropagation(predefinedPathCoor[index, 0] - 1, predefinedPathCoor[index, 1], 2, grid[predefinedPathCoor[index, 0], predefinedPathCoor[index, 1]], 0);
                    if (predefinedPathCoor[index, 0] < (numRow - 1))
                        ConstraintPropagation(predefinedPathCoor[index, 0] + 1, predefinedPathCoor[index, 1], 0, grid[predefinedPathCoor[index, 0], predefinedPathCoor[index, 1]], 0);
                }

                int[] nextCel = SearchNextGridCell();
                if (nextCel[0] != 1 && nextCel[1] != 0)
                    TileElection(nextCel[0], nextCel[1]);
            }
            else
            {
                var firstCell = SearchNextGridCell();
                TileElection(firstCell[0], firstCell[1]);
            }
        }
        else
        {
            Debug.Log("You need to clear the grid to generate a new landscape.");
        }
    }

    public void RotateTile(int chosenIndex, int rowIndex, int colIndex)
    {
        switch (grid[rowIndex, colIndex].rotDirection)
        {
            case 1:
                tileSet[chosenIndex].tile.transform.Rotate(new Vector3(0, 0, 90f)); break;
            case 2:
                tileSet[chosenIndex].tile.transform.Rotate(new Vector3(0, 0, 180f)); break;
            case 3:
                tileSet[chosenIndex].tile.transform.Rotate(new Vector3(0, 0, 270)); break;
        }
    }

    public int[] SearchNextGridCell()
    {
        float finalEntropy = Mathf.Infinity;
        int nextCol = -1;
        int nextRow = -1;

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j].tileChosen == null && grid[i, j].domainCount == 1)
                {
                    int[] nextGridCell = new int[2] { i, j };
                    return nextGridCell;
                }
                else if (grid[i, j].domainCount > 1)
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

    private void InstantiateTile(int rowIndex, int colIndex, int chosenIndex)
    {
        currentPos = new Vector3(colIndex * tileSize.x + tileSize.x / 2, tileSize.y / 2, rowIndex * tileSize.z + tileSize.z / 2);
        grid[rowIndex, colIndex].SetTile(tileSet[chosenIndex].tile, chosenIndex, currentPos, transform);
    }

    /// <summary>
    /// Tile setting when the constraint propagation has ended
    /// </summary>
    /// <param name="rowIndex">Row of the matrix</param>
    /// <param name="colIndex">Col of the matrix</param>
    private void TileElection(int rowIndex, int colIndex)
    {
        // Select the only tile available in the domain of the cell if there is only one
        // if not it makes a random election bearing in mind the weights of the tiles
        int chosenIndex = -1;
        if (grid[rowIndex, colIndex].tileChosen == null)
        {
            if (grid[rowIndex, colIndex].domainCount == 1)
            {
                for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
                {
                    if (grid[rowIndex, colIndex].domain[i])
                    {
                        chosenIndex = i;
                        break;
                    }
                }
            }
            else if (grid[rowIndex, colIndex].domainCount > 1)
            {
                Tile[] availableTiles = new Tile[grid[rowIndex, colIndex].domainCount];
                int j = 0;
                for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
                {
                    if (grid[rowIndex, colIndex].domain[i] && j < availableTiles.Length)
                    {
                        availableTiles[j] = tileSet[i];
                        j++;
                    }
                }
                chosenIndex = weightedRandom(availableTiles);
            }

            InstantiateTile(rowIndex, colIndex, chosenIndex);
        }

        // Propagates the constraints bearing in mind the current tile
        if(colIndex > 0)
            ConstraintPropagation(rowIndex, colIndex -1, 3, grid[rowIndex, colIndex], 0);
        if(colIndex < (numCol - 1))
            ConstraintPropagation(rowIndex, colIndex + 1, 1, grid[rowIndex, colIndex], 0);
        if(rowIndex > 0)
            ConstraintPropagation(rowIndex - 1, colIndex, 2, grid[rowIndex, colIndex], 0);
        if(rowIndex < (numRow - 1))
            ConstraintPropagation(rowIndex + 1, colIndex, 0, grid[rowIndex, colIndex], 0);

        // Search if any cel has one tile in its domain
        // If can't find any, then choose the one with minimal entropy
        int[] nextGridCell = SearchNextGridCell();

        if(nextGridCell[0] != -1 && nextGridCell[1] != -1)
            TileElection(nextGridCell[0], nextGridCell[1]);
    }

    /// <summary>
    /// Propagation of the constraint from the current tile to the four directions
    /// </summary>
    /// <param name="rowIndex">Row of the matrix</param>
    /// <param name="colIndex">Col of the matrix</param>
    /// <param name="direction">Last cell propagation direction</param>
    /// <param name="lastCell">Last cell of the grid</param>
    /// <param name="step">Step limitation</param>
    private void ConstraintPropagation(int rowIndex, int colIndex, int direction, Variable lastCell, int step)
    {
        if (grid[rowIndex, colIndex].domainCount == 1 || step >= maxSteps || rowIndex < 0 || colIndex < 0 || rowIndex >= numRow || colIndex >= numCol)
            return;

        step++;
        if (lastCell.domainCount == 1)
        {
            for (int i = 0; i < grid[rowIndex, colIndex].domain.Length; i++)
            {
                if (grid[rowIndex, colIndex].domain[i] && grid[rowIndex, colIndex].domainCount > 1)
                {
                    //Siempre que no sean iguales, como solo hay una posibilidad de conexi�n, se elimina del dominio
                    if ((direction == 0 && tileSet[i].tile.sideIndex[2] != lastCell.tileChosen.sideIndex[direction])
                        || (direction == 1 && tileSet[i].tile.sideIndex[3] != lastCell.tileChosen.sideIndex[direction])
                        || (direction == 2 && tileSet[i].tile.sideIndex[0] != lastCell.tileChosen.sideIndex[direction])
                        || (direction == 3 && tileSet[i].tile.sideIndex[1] != lastCell.tileChosen.sideIndex[direction]))
                    {
                        grid[rowIndex, colIndex].domain[i] = false;
                        grid[rowIndex, colIndex].domainCount--;
                    }
                }
            }
        }
        else
        {
            // Buscar un metodo para eliminar las teselas que no se pueden
            for (int i = 0; i < lastCell.domain.Length; i++)
            {
                if (lastCell.domain[i])
                {
                    for (int j = 0; j < grid[rowIndex, colIndex].domain.Length; j++)
                    {
                        if (grid[rowIndex, colIndex].domain[j])
                        {
                            //Si las dos partes coinciden se cuenta como una visita (hay una manera de conectarlas)
                            if ((direction == 0 && tileSet[j].tile.sideIndex[2] == tileSet[i].tile.sideIndex[direction])
                            || (direction == 1 && tileSet[j].tile.sideIndex[3] == tileSet[i].tile.sideIndex[direction])
                            || (direction == 2 && tileSet[j].tile.sideIndex[0] == tileSet[i].tile.sideIndex[direction])
                            || (direction == 3 && tileSet[j].tile.sideIndex[1] == tileSet[i].tile.sideIndex[direction]))
                            {
                                grid[rowIndex, colIndex].visited[j]++;
                            }

                        }
                    }
                }
            }

            for (int i = 0; i < grid[rowIndex, colIndex].visited.Length; i++)
            {
                if (grid[rowIndex, colIndex].domain[i] && grid[rowIndex, colIndex].visited[i] < 1) {
                    grid[rowIndex, colIndex].domain[i] = false;
                    grid[rowIndex, colIndex].domainCount--;
                }
                else
                    grid[rowIndex, colIndex].visited[i] = 0;
            }
        }

        if(grid[rowIndex, colIndex].domainCount == 1)
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

            InstantiateTile(rowIndex, colIndex, chosenIndex);
        }

        //Calculate entropy after the constraint propagation
        grid[rowIndex, colIndex].CalculateEntropy(tileSet);

        // Propagates the constraints bearing in mind the current tile
        if (colIndex > 0 && direction != 1)
            ConstraintPropagation(rowIndex, colIndex - 1, 3, grid[rowIndex, colIndex], step);
        if (colIndex < (numCol - 1) && direction != 3)
            ConstraintPropagation(rowIndex, colIndex + 1, 1, grid[rowIndex, colIndex], step);
        if (rowIndex > 0 && direction != 0)
            ConstraintPropagation(rowIndex - 1, colIndex, 2, grid[rowIndex, colIndex], step);
        if (rowIndex < (numRow - 1) && direction != 2)
            ConstraintPropagation(rowIndex + 1, colIndex, 0, grid[rowIndex, colIndex], step);
    }

    private int weightedRandom(Tile[] availableTiles) {
        int max = availableTiles[0].weight;

        for( int k = 1; k < availableTiles.Length; k++)
        {
            max += availableTiles[k].weight;
        }

        int target = Mathf.RoundToInt(UnityEngine.Random.Range(0, max));
        int choosenNumber = 0;
        while(target > availableTiles[choosenNumber].weight && choosenNumber < availableTiles.Length)
        {
            target -= availableTiles[choosenNumber].weight;
            choosenNumber++;
        }

        for( int k = 0; k < tileSet.Length; k++)
        {
            if (availableTiles[choosenNumber].tile.name == tileSet[k].tile.name)
            {
                choosenNumber = k;
                break;
            }
        }

        return choosenNumber;
    }

    private void OnDrawGizmos()
    {
        // Draw Grid Edges
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(numCol, 0, 0));
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, numRow));
        Gizmos.DrawLine(new Vector3(0, 0, numRow), new Vector3(numCol, 0, numRow));
        Gizmos.DrawLine(new Vector3(numCol, 0, 0), new Vector3(numCol, 0, numRow));

        // Draw Grid
        for (int i = 0; i < numCol; i++)
        {
            Gizmos.DrawLine(new Vector3(i * tileSize.x, 0, 0), new Vector3(i * tileSize.x, 0, numRow));
        }

        for (int i = 0; i < numRow; i++)
        {
            Gizmos.DrawLine(new Vector3(0, 0, i * tileSize.x), new Vector3(numRow, 0, i * tileSize.x));
        }
    }
}
