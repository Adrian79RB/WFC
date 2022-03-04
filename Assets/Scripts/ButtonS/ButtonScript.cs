using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public Light currentlight;
    public GridTile[] tiles;
    public int weight;
    public GameManager GM;
    public Color[] lightColors;
    public bool buttonPressed;
    public GameObject[] rotableTile;

    [Header("Audio Sruff")]
    public AudioSource buttonSound;
    public AudioClip buttonUnlocked;
    public AudioClip buttonLocked;

    bool buttonActivate;

    // Start is called before the first frame update
    void Start()
    {
        buttonPressed = false;

        if (transform.name.Contains("Generate"))
        {
            buttonActivate = false;
            buttonSound.clip = buttonLocked;
        }
        else
        {
            buttonActivate = true;
            buttonSound.clip = buttonUnlocked;
        }
    }

    public void ButtonPressed()
    {
        if (transform.name.Contains("Set"))
        {
            GM.ChangeTileSet();
            currentlight.color = lightColors[GM.tileSetChoosen];

        }
        else
        {
            if (buttonPressed)
            {
                if (transform.name == "Generate" && buttonActivate)
                {
                    GM.ClearTileSet();
                    currentlight.color = lightColors[0];
                }
                else if (transform.name.Contains("Tile"))
                {
                    currentlight.enabled = false;
                    GM.ClearTile(tiles[GM.tileSetChoosen], weight);
                    rotableTile[GM.tileSetChoosen].GetComponent<RotateTilesExposition>().rotate = false;
                }
            }
            else
            {
                if (transform.name == "Generate" && buttonActivate)
                {
                    GM.GenerateTileSet();
                    currentlight.color = lightColors[1];
                }
                else if (transform.name.Contains("Tile"))
                {
                    currentlight.enabled = true;
                    GM.AddNewTile(tiles[GM.tileSetChoosen], weight);
                    rotableTile[GM.tileSetChoosen].GetComponent<RotateTilesExposition>().rotate = true;
                }
            }
        }

        if(buttonActivate)
            buttonPressed = !buttonPressed;

        buttonSound.Play();
    }

    public void ActivateLight()
    {
        if(currentlight.color != lightColors[0])
            currentlight.color = lightColors[0];

        currentlight.enabled = true;
        buttonActivate = true;
        buttonSound.clip = buttonUnlocked;
    }

    internal void DiactivateLight()
    {
        if (currentlight.color != lightColors[0])
            currentlight.color = lightColors[0];
        currentlight.enabled = false;
        buttonActivate = false;
        buttonSound.clip = buttonLocked;
    }
}
