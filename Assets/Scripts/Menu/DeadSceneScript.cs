using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadSceneScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartButtonPressed()
    {
        SceneManager.LoadScene("Scene1");
    }

    public void ExitButtonPressed()
    {
        SceneManager.LoadScene("Menu");
    }
}
