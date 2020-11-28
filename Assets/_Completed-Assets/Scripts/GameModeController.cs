using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeController : MonoBehaviour
{
    public void GotOScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
