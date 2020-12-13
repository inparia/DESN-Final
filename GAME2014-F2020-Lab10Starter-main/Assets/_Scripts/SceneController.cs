using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public int buttonNumber;

    public void OnButtonPressed()
    {
        switch (buttonNumber)
        {
            case 1:
                SceneManager.LoadScene("Platformer");
                break;
            case 2:
                SceneManager.LoadScene("Start");
                break;
            case 3:
                break;
            case 4:
                Application.Quit();
                break;
            default:
                break;
    }
    }
}
