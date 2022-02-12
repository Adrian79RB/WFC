using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public Light currentlight;
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
        }
        else
        {
            currentlight.enabled = true;
        }
        buttonPressed = !buttonPressed;
    }
}
