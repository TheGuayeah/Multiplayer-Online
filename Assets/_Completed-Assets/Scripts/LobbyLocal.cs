using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using UnityEngine.UI;
using PlayFab;
using UnityEngine.SceneManagement;

public class LobbyLocal : MonoBehaviour {


    public void CreateGame() {
        SceneManager.LoadScene("Main");
    }

    public void Back()
    {
        SceneManager.LoadScene("SelectGameMode");
    }
}
