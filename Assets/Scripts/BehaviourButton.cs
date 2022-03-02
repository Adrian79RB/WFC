using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourButton : MonoBehaviour
{
    public Light currentLight;
    public BlockType[] type;
    public int weight;
    public GameManager GM;
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
        if (buttonPressed)
        {
            currentLight.enabled = false;
            if (transform.name.Contains("Block"))
                GM.ClearBlock(type, weight);
        }
        else
        {
            currentLight.enabled = true;
            if (transform.name.Contains("Block"))
                GM.AddNewBlock(type, weight);
        }

        buttonPressed = !buttonPressed;
    }
}
