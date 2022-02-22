using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public Light currentlight;
    public GridTile currentTile;
    public int weight;
    public GameManager GM;
    
    bool buttonPressed;

    // Start is called before the first frame update
    void Start()
    {
        buttonPressed = false;
    }

    public void ButtonPressed()
    {
        if (buttonPressed)
        {
            currentlight.enabled = false;
            if (transform.name == "Generate")
                GM.ClearTileSet();
            else if (transform.name.Contains("Tile"))
                GM.ClearTile(currentTile, weight);
        }
        else
        {
            currentlight.enabled = true;
            if (transform.name == "Generate")
                GM.GenerateTileSet();
            else if (transform.name.Contains("Tile"))
                GM.AddNewTile(currentTile, weight);
        }
        buttonPressed = !buttonPressed;
    }
}
