using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera entranceCam;
    public Material cameraMat;
    public GameObject predefinedPath;

    public TileSetGenerator tileSetGenerator;
    public GridTile[] predefinedPathNeededTiles;
    public EnemyAgent[] enemies;


    // Start is called before the first frame update
    void Start()
    {
        if(entranceCam.targetTexture != null)
        {
            entranceCam.targetTexture.Release();
        }

        entranceCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMat.mainTexture = entranceCam.targetTexture;
    }

    public void GenerateTileSet()
    {
        if (tileSetGenerator.tileSet.Count <= 4)
            tileSetGenerator.maxSteps = 15;
        else if(tileSetGenerator.tileSet.Count <= 9)
            tileSetGenerator.maxSteps = 12;
        else if(tileSetGenerator.tileSet.Count <= 13)
            tileSetGenerator.maxSteps = 9;

        var counter = 0;
        for(int i = 0; i < tileSetGenerator.tileSet.Count; i++)
        {
            for (int j = 0; j < predefinedPathNeededTiles.Length; j++)
            {
                if (tileSetGenerator.tileSet[i].tile == predefinedPathNeededTiles[j])
                {
                    counter++;
                    break;
                }
            }
        }

        if (counter == predefinedPathNeededTiles.Length)
            predefinedPath.SetActive(true);
        else
            predefinedPath.SetActive(false);

        tileSetGenerator.Generate();

        for (int k = 0; k < enemies.Length; k++)
        {
            enemies[k].gameObject.SetActive(true);
        }
    }

    public void ClearTileSet()
    {
        tileSetGenerator.ClearTiles();

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].strategicalPosition.Clear();
            enemies[i].gameObject.SetActive(false);
        }
    }

    public void AddNewTile(GridTile tile, int weight)
    {
        Tile newTile = new Tile();
        newTile.tile = tile;
        newTile.weight = weight;
        tileSetGenerator.tileSet.Add(newTile);
    }

    public void ClearTile(GridTile tile, int weight)
    {
        Tile tileToRemove = new Tile();
        tileToRemove.tile = tile;
        tileToRemove.weight = weight;
        tileSetGenerator.tileSet.Remove(tileToRemove);
    }
}
