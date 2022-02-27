using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Portal Stuff")]
    public Camera entranceCam;
    public Material cameraMat;
    public GameObject arenaPortal;

    [Header("Map types Stuff")]
    public GameObject[] predefinedPaths;
    public GameObject[] entrances;
    public GameObject[] fortress;

    [Header("Predefine tiles needed")]
    public GridTile[] grassPredefinnedTilesNeeded;
    public GridTile[] desertPredefinnedTilesNeeded;
    public GridTile[] snowPredefinnedTilesNeeded;
    public GridTile[] rarePredefinnedTilesNeeded;

    [Header("Other Stuff")]
    public TileSetGenerator tileSetGenerator;
    public EnemyAgent[] enemies;
    public int tileSetChoosen = 0; // 0 -> Normal; 1 -> Desert; 2 -> Snow; 3 -> Rare
    public GameObject[] buttons;

    GridTile[][] predefinedPathNeededTiles;

    // Start is called before the first frame update
    void Start()
    {
        if(entranceCam.targetTexture != null)
        {
            entranceCam.targetTexture.Release();
        }

        entranceCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMat.mainTexture = entranceCam.targetTexture;

        predefinedPathNeededTiles = new GridTile[4][];
        predefinedPathNeededTiles[0] = grassPredefinnedTilesNeeded;
        predefinedPathNeededTiles[1] = desertPredefinnedTilesNeeded;
        predefinedPathNeededTiles[2] = snowPredefinnedTilesNeeded;
        predefinedPathNeededTiles[3] = rarePredefinnedTilesNeeded;
    }

    public void GenerateTileSet()
    {
        // Set the max steps number of the algorith
        if (tileSetGenerator.tileSet.Count <= 4)
            tileSetGenerator.maxSteps = 15;
        else if(tileSetGenerator.tileSet.Count <= 9)
            tileSetGenerator.maxSteps = 12;
        else if(tileSetGenerator.tileSet.Count <= 13)
            tileSetGenerator.maxSteps = 9;

        // Discover if the predefined path can be initialized
        ActivatePredefinedPath();

        // Generate the tile map
        tileSetGenerator.Generate();

        // Activate the Enemies
        ActivateEnemies();

        // Activate portal
        arenaPortal.SetActive(true);
    }

    public void ClearTileSet()
    {
        tileSetGenerator.ClearTiles();

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].strategicalPosition.Clear();
            enemies[i].gameObject.SetActive(false);
        }

        // Disactivate arena portal
        arenaPortal.SetActive(false);
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

    public void DeactivateAllButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Debug.Log("Entra bucle");
            ButtonScript button = buttons[i].GetComponent<ButtonScript>();
            Debug.Log("Boton: " + button.name + "; activate: " + button.buttonPressed);
            if (button.buttonPressed)
            {
                button.ButtonPressed(); // Imitates that the player press again the button and deactivate them
            }
        }
    }

    public void ChangeTileSet()
    {
        DeactivateAllButtons();
        tileSetChoosen++;
        if (tileSetChoosen > 3)
            tileSetChoosen = 0;
    }

    private void ActivateEnemies()
    {
        var enemyCount = 0;
        int k = 0;

        while (enemyCount < 4 && k < enemies.Length)
        {
            if (!enemies[k].gameObject.activeSelf && Random.value > 0.7)
            {
                enemyCount++;
                enemies[k].gameObject.SetActive(true);
            }

            k++;
            if (k >= enemies.Length && enemyCount < 4)
                k = 0;
        }
    }

    private void ActivatePredefinedPath()
    {
        var counter = 0;
        for (int i = 0; i < tileSetGenerator.tileSet.Count; i++)
        {
            for (int j = 0; j < predefinedPathNeededTiles.Length; j++)
            {
                if (tileSetGenerator.tileSet[i].tile == predefinedPathNeededTiles[tileSetChoosen][j])
                {
                    counter++;
                    break;
                }
            }
        }

        if (counter == predefinedPathNeededTiles.Length)
            predefinedPaths[tileSetChoosen].SetActive(true);
        else
            predefinedPaths[tileSetChoosen].SetActive(false);

        tileSetGenerator.predefinedPath = predefinedPaths[tileSetChoosen].transform;

        entrances[tileSetChoosen].SetActive(true);
        fortress[tileSetChoosen].SetActive(true);
    }
}
