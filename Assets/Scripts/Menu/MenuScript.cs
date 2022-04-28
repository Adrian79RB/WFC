using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void StartButtonPressed()
    {
        SceneManager.LoadScene("Scene1");
    }

    public void TutorialButtonPressed()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }
}
