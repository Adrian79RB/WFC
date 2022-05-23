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
        //enemies[0].gameObject.SetActive(true);

        // Activate portal
        if(!isInTutorial)
            arenaPortal.SetActive(true);
    }

    public void ClearTileSet()
    {
        tileSetGenerator.ClearTiles();
        
        environmentalMusic.clip = tenseMusic;
        environmentalMusic.Play();

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

    public void AddNewTile(GridTile tile, int weight)
    {
        Tile newTile = new Tile();
        newTile.tile = tile;
        newTile.weight = weight;
        tileSetGenerator.tileSet.Add(newTile);
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

    public void AddNewBlock(BlockType[] type, int weight)
    {
        Block newBlock = new Block();
        newBlock.weight = weight;
        if (type.Length == 1)
        {
            newBlock.type = type[0];
            behaviourBlockSelectedArchers.Add(newBlock);
            behaviourBlockSelectedSwordman.Add(newBlock);
        }
        else
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

    public void ClearTile(GridTile tile, int weight)
    {
        Tile tileToRemove = new Tile();
        tileToRemove.tile = tile;
        tileToRemove.weight = weight;
        tileSetGenerator.tileSet.Remove(tileToRemove);

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

    public void ClearBlock(BlockType[] type, int weight)
    {
        Block blockToRemove = new Block();
        blockToRemove.weight = weight;
        if(type.Length == 1)
        {
            blockToRemove.type = type[0];
            behaviourBlockSelectedArchers.Remove(blockToRemove);
            behaviourBlockSelectedSwordman.Remove(blockToRemove);
        }
        else
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

    public void ChangeTileSet()
    {
        DeactivateAllButtons();
        ChangeRotableTiles();
        tileSetChoosen++;
        if (tileSetChoosen > 3)
            tileSetChoosen = 0;
    }

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

    private void ActivateEnemies()
    {
        var enemyCount = 0;
        int k = 0;
        
        if (!isInTutorial)
        {
            while (enemyCount < 4 || k < enemies.Length)
            {
                if (!enemies[k].gameObject.activeSelf && UnityEngine.Random.value > 0.7)
                {
                    enemyCount++;
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

    private void ActivatePredefinedPath()
    {
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

        if (counter == predefinedPathNeededTiles[tileSetChoosen].Length)
            predefinedPaths[tileSetChoosen].SetActive(true);
        else
            predefinedPaths[tileSetChoosen].SetActive(false);

        tileSetGenerator.predefinedPath = predefinedPaths[tileSetChoosen].transform;

        entrances[tileSetChoosen].SetActive(true);
        fortress[tileSetChoosen].SetActive(true);
    }

    public void AddDeadEnemy()
    {
        enemiesDead += 1;
        if (!isInTutorial && enemiesDead >= enemiesInGame)
            initRoomPortal.SetActive(true);
    }

    public void CrossingEndPortal()
    {
        StartCoroutine(ChangeScene("WinScene"));
    }

    public void PlayerIsDead()
    {
        StartCoroutine(ChangeScene("Scene1"));
    }

    IEnumerator ChangeScene(string scene)
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(scene);
    }
}
