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
                SoundManager.Instance.Play("Button");
                break;
            case 2:
                SceneManager.LoadScene("Start");
                SoundManager.Instance.Play("Button");
                break;
            case 3:
                SceneManager.LoadScene("Instruction");
                SoundManager.Instance.Play("Button");
                break;
            case 4:
                Application.Quit();
                SoundManager.Instance.Play("Button");
                break;
    }
    }
}
