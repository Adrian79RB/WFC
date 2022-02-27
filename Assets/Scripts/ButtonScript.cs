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
    public AudioSource buttonSound;
    public bool buttonPressed;

    // Start is called before the first frame update
    void Start()
    {
        buttonPressed = false;
    }

    public void ButtonPressed()
    {
        buttonSound.Play();

        if (transform.name.Contains("Set"))
        {
            GM.ChangeTileSet();
            currentlight.color = lightColors[GM.tileSetChoosen];
        }
        else
        {
            if (buttonPressed)
            {
                currentlight.enabled = false;
                if (transform.name == "Generate")
                    GM.ClearTileSet();
                else if (transform.name.Contains("Tile"))
                    GM.ClearTile(tiles[GM.tileSetChoosen], weight);
            }
            else
            {
                currentlight.enabled = true;
                if (transform.name == "Generate")
                    GM.GenerateTileSet();
                else if (transform.name.Contains("Tile"))
                    GM.AddNewTile(tiles[GM.tileSetChoosen], weight);
            }
        }

        buttonPressed = !buttonPressed;
    }
}
