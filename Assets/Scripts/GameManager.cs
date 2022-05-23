using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Portal Stuff")]
    public Camera entranceCam;
    public Camera exitCam;
    public Material cameraMat;
    public Material cameraMat2;
    public GameObject arenaPortal;
    public GameObject initRoomPortal;
    

    [Header("Map types Stuff")]
    public GameObject[] predefinedPaths;
    public GameObject[] entrances;
    public GameObject[] fortress;

    [Header("Predefine tiles needed")]
    public GridTile[] grassPredefinnedTilesNeeded;
    public GridTile[] desertPredefinnedTilesNeeded;
    public GridTile[] snowPredefinnedTilesNeeded;
    public GridTile[] rarePredefinnedTilesNeeded;

    [Header("Music Audio Clips")]
    public AudioClip tenseMusic;
    public AudioClip epicMusic;

    [Header("Other Stuff")]
    public TileSetGenerator tileSetGenerator;
    public EnemyAgent[] enemies;
    public int tileSetChoosen = 0; // 0 -> Normal; 1 -> Desert; 2 -> Snow; 3 -> Rare
    public GameObject[] buttons;
    public GameObject generateButton;
    public int phase = 0;
    public bool isInTutorial = false;

    GridTile[][] predefinedPathNeededTiles;
    List<Block> behaviourBlockSelectedArchers;
    List<Block> behaviourBlockSelectedSwordman;
    AudioSource environmentalMusic;
    int enemiesInGame = 0;
    int enemiesDead = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            isInTutorial = true;
        }


        if (!isInTutorial)
        {
            if (entranceCam.targetTexture != null)
            {
                entranceCam.targetTexture.Release();
            }
            if (exitCam.targetTexture != null)
            {
                exitCam.targetTexture.Release();
            }

            entranceCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            exitCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            cameraMat.mainTexture = entranceCam.targetTexture;
            cameraMat2.mainTexture = exitCam.targetTexture;

            predefinedPathNeededTiles = new GridTile[4][];
            predefinedPathNeededTiles[0] = grassPredefinnedTilesNeeded;
            predefinedPathNeededTiles[1] = desertPredefinnedTilesNeeded;
            predefinedPathNeededTiles[2] = snowPredefinnedTilesNeeded;
            predefinedPathNeededTiles[3] = rarePredefinnedTilesNeeded;
        }

        environmentalMusic = GetComponent<AudioSource>();
        behaviourBlockSelectedArchers = new List<Block>();
        behaviourBlockSelectedSwordman = new List<Block>();
    }

    /// <summary>
    /// Method called by the generate button from the initial room
    /// </summary>
    public void GenerateTileSet()
    {
        // Set the max steps number of the algorith
        if (tileSetGenerator.tileSet.Count <= 4)
            tileSetGenerator.maxSteps = 15;
        else if(tileSetGenerator.tileSet.Count <= 9)
            tileSetGenerator.maxSteps = 13;
        else if(tileSetGenerator.tileSet.Count <= 13)
            tileSetGenerator.maxSteps = 9;

        // Discover if the predefined path can be initialized
        if (!isInTutorial)
        {
            ActivatePredefinedPath();
            environmentalMusic.clip = epicMusic;
            environmentalMusic.Play();
        }

        // Generate the tile map
        tileSetGenerator.Generate();

        // Activate the Enemies
        ActivateEnemies();

        // Activate portal
        if(!isInTutorial)
            arenaPortal.SetActive(true);
    }

    /// <summary>
    /// This method eliminates the tile map created by the generate method, it is called from the generate button in the initial room
    /// </summary>
    public void ClearTileSet()
    {
        // Eliminate tiles from tileMap
        tileSetGenerator.ClearTiles();
        
        environmentalMusic.clip = tenseMusic;
        environmentalMusic.Play();

        // Deactivate the enemies
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].strategicalPosition.Clear();
            enemies[i].gameObject.SetActive(false);
        }

        if (!isInTutorial)
        {
            // Disactivate arena portal
            arenaPortal.SetActive(false);
        }
    }

    /// <summary>
    /// Add a new tile to the Tile set that is going to be used to generate the Tile map
    /// </summary>
    /// <param name="tile">Tile selected to be intoruced in the TileSet</param>
    /// <param name="weight">Weigth related to the tile selected</param>
    public void AddNewTile(GridTile tile, int weight)
    {
        // Creating the object that represents the tile
        Tile newTile = new Tile();
        newTile.tile = tile;
        newTile.weight = weight;
        tileSetGenerator.tileSet.Add(newTile);

        // Changing the state of the generate button
        if (!isInTutorial && tileSetGenerator.tileSet.Count > 6 && behaviourBlockSelectedArchers.Count > 2 && behaviourBlockSelectedSwordman.Count > 2)
        {
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
        else if(isInTutorial && tileSetGenerator.tileSet.Count > 0 && phase == 0)
        {
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
        else if(isInTutorial && tileSetGenerator.tileSet.Count > 5 && behaviourBlockSelectedSwordman.Count > 0){
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
    }

    /// <summary>
    /// Add a new block to the Block set that is going to be used to generate the decision tree for the NPCs
    /// </summary>
    /// <param name="type">Block selected to be introduce in the BlockSet</param>
    /// <param name="weight">Weight related to the selected block</param>
    public void AddNewBlock(BlockType[] type, int weight)
    {
        // Create the object that represent the block selected
        Block newBlock = new Block();
        newBlock.weight = weight;

        if (type.Length == 1) // The block is appropriate for both enemy type
        {
            newBlock.type = type[0];
            behaviourBlockSelectedArchers.Add(newBlock);
            behaviourBlockSelectedSwordman.Add(newBlock);
        }
        else // Each enemy type has a different block but represented by the same button in the initial room
        {
            for (int i = 0; i < type.Length; i++)
            {
                newBlock.type = type[i];
                if (type[i] == BlockType.GetClose || type[i] == BlockType.Attack)
                    behaviourBlockSelectedSwordman.Add(newBlock);
                else
                    behaviourBlockSelectedArchers.Add(newBlock);
            }
        }

        // Changing the state of the generate button
        if (!isInTutorial && tileSetGenerator.tileSet.Count > 6 && behaviourBlockSelectedArchers.Count > 2 && behaviourBlockSelectedSwordman.Count > 2)
        {
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
        else if(isInTutorial && tileSetGenerator.tileSet.Count > 0 && phase == 0)
        {
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
        else if (isInTutorial && tileSetGenerator.tileSet.Count > 5 && behaviourBlockSelectedSwordman.Count > 0)
        {
            if (!generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().ActivateLight();
        }
    }

    /// <summary>
    /// Remove a tile from the Tile set that has been selected
    /// </summary>
    /// <param name="tile">Tile selected to be removed from the tile set</param>
    /// <param name="weight">Weight related to the selected tile</param>
    public void ClearTile(GridTile tile, int weight)
    {
        // Object created to represent the selected tile
        Tile tileToRemove = new Tile();
        tileToRemove.tile = tile;
        tileToRemove.weight = weight;
        tileSetGenerator.tileSet.Remove(tileToRemove);

        // Changing the state of the generate button from the initial room
        if (!isInTutorial && ( tileSetGenerator.tileSet.Count <= 6 || behaviourBlockSelectedArchers.Count < 3 || behaviourBlockSelectedSwordman.Count < 3 ) )
        {
            if (generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().DiactivateLight();
        }
        else if (isInTutorial && tileSetGenerator.tileSet.Count < 1 && phase == 0)
        {
            if (generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().DiactivateLight();
        }
        else if (isInTutorial && phase != 0 && (behaviourBlockSelectedSwordman.Count < 1 || tileSetGenerator.tileSet.Count < 6) ){ 
            if (generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().DiactivateLight();
        }
    }

    /// <summary>
    /// Remove a block from the Block set that has been selected
    /// </summary>
    /// <param name="type">Block selected to be removed from the block set</param>
    /// <param name="weight">Weight related to the block selected</param>
    public void ClearBlock(BlockType[] type, int weight)
    {
        // Creating the object that represents the block selected
        Block blockToRemove = new Block();
        blockToRemove.weight = weight;

        if(type.Length == 1) // The block is appropriate for both enemy types
        {
            blockToRemove.type = type[0];
            behaviourBlockSelectedArchers.Remove(blockToRemove);
            behaviourBlockSelectedSwordman.Remove(blockToRemove);
        }
        else // Each enemy type has a different block 
        {
            for (int i = 0; i < type.Length; i++)
            {
                blockToRemove.type = type[i];
                if (type[i] == BlockType.GetClose || type[i] == BlockType.Attack)
                    behaviourBlockSelectedSwordman.Remove(blockToRemove);
                else
                    behaviourBlockSelectedArchers.Remove(blockToRemove);
            }
        }

        // Changing the state of the generate button from the initial room
        if (!isInTutorial && (tileSetGenerator.tileSet.Count < 6 || behaviourBlockSelectedArchers.Count < 3 || behaviourBlockSelectedSwordman.Count < 3) )
        {
            if (generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().DiactivateLight();
        }
        else if (isInTutorial && (tileSetGenerator.tileSet.Count < 1 || (behaviourBlockSelectedSwordman.Count < 1 && phase != 0) ))
        {
            if (generateButton.GetComponent<ButtonScript>().currentlight.enabled)
                generateButton.GetComponent<ButtonScript>().DiactivateLight();
        }
    }

    /// <summary>
    /// Change the type of the tiles that could generate the tile map (Four types: Grass, Sand, Snow, Rare)
    /// </summary>
    public void ChangeTileSet()
    {
        DeactivateAllButtons();
        ChangeRotableTiles();
        tileSetChoosen++;
        if (tileSetChoosen > 3)
            tileSetChoosen = 0;
    }

    /// <summary>
    /// Deselect all the buttons that determine the tile set and block set from the initial room
    /// </summary>
    private void DeactivateAllButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ButtonScript button = buttons[i].GetComponent<ButtonScript>();
            if (button.buttonPressed)
            {
                button.ButtonPressed(); // Imitates that the player press again the button and deactivate them
            }
        }
    }

    /// <summary>
    /// Change the object that represent the tile type in the buttons from the initial room
    /// </summary>
    private void ChangeRotableTiles()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ButtonScript button = buttons[i].GetComponent<ButtonScript>();
            button.rotableTile[tileSetChoosen].SetActive(false);
            if (tileSetChoosen < 3)
                button.rotableTile[tileSetChoosen + 1].SetActive(true);
            else
                button.rotableTile[0].SetActive(true);
        }
    }

    /// <summary>
    /// Enable the enemies and establish the blocks that are going to be used to generate each decision tree
    /// </summary>
    private void ActivateEnemies()
    {
        var enemyCount = 0;
        int k = 0;
        
        if (!isInTutorial)
        {
            // Activate the enemies randomly
            while (enemyCount < 4 || k < enemies.Length)
            {
                if (!enemies[k].gameObject.activeSelf && UnityEngine.Random.value > 0.7)
                {
                    enemyCount++;
                    // Establish the block set needed by each enemy based on its type
                    if (enemies[k].type == EnemyType.Archer)
                    {
                        enemies[k].enemyBlockSet = behaviourBlockSelectedArchers;
                    }
                    else
                    {
                        enemies[k].enemyBlockSet = behaviourBlockSelectedSwordman;
                    }

                    enemies[k].gameObject.SetActive(true);
                }

                k++;
                if (k >= enemies.Length && enemyCount < 4)
                    k = 0;
            }

            enemiesInGame = enemyCount;
        }
        else
        {
            // In the tutorial there is only a unique enemy and it has to be activated always
            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i].gameObject.activeSelf && enemies[i].type == EnemyType.Swordman)
                {
                    enemies[i].enemyBlockSet = behaviourBlockSelectedSwordman;
                    enemies[i].gameObject.SetActive(true);
                }
            }
        }
       
    }

    /// <summary>
    /// Select which is the predefined path that has to be activated based on the tile type selected
    /// </summary>
    private void ActivatePredefinedPath()
    {
        // Determining if all the needed tiles to generate the predefined path have beem selected
        var counter = 0;
        for (int i = 0; i < tileSetGenerator.tileSet.Count; i++)
        {
            for (int j = 0; j < predefinedPathNeededTiles[tileSetChoosen].Length; j++)
            {
                if (tileSetGenerator.tileSet[i].tile == predefinedPathNeededTiles[tileSetChoosen][j])
                {
                    counter++;
                    break;
                }
            }
        }

        // If all the needed tiles have been selected, activate the predefined path
        if (counter == predefinedPathNeededTiles[tileSetChoosen].Length)
            predefinedPaths[tileSetChoosen].SetActive(true);
        else
            predefinedPaths[tileSetChoosen].SetActive(false);

        tileSetGenerator.predefinedPath = predefinedPaths[tileSetChoosen].transform;

        // Activate the appropriate entrance and frotificate position in the arena
        entrances[tileSetChoosen].SetActive(true);
        fortress[tileSetChoosen].SetActive(true);
    }

    /// <summary>
    /// Add an enemy to the dead list
    /// </summary>
    public void AddDeadEnemy()
    {
        enemiesDead += 1;
        if (!isInTutorial && enemiesDead >= enemiesInGame) // If all the enemies are dead, activate the end game portal
            initRoomPortal.SetActive(true);
    }

    /// <summary>
    /// Player cross through the end game portal
    /// </summary>
    public void CrossingEndPortal()
    {
        StartCoroutine(ChangeScene("WinScene"));
    }

    /// <summary>
    /// Restart the game scene
    /// </summary>
    public void PlayerIsDead()
    {
        StartCoroutine(ChangeScene("Scene1"));
    }

    /// <summary>
    /// Change to the next scene in the game flow
    /// </summary>
    /// <param name="scene">Next scene reference</param>
    /// <returns></returns>
    IEnumerator ChangeScene(string scene)
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(scene);
    }
}
