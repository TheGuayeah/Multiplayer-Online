using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyOnlineController : MonoBehaviour
{
    private PlayFabController playFab;

    private void Start()
    {
        if (FindObjectOfType<PlayFabController>())
        {
            playFab = FindObjectOfType<PlayFabController>().GetComponent<PlayFabController>();
        }
    }

    public void Back()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerPrefs.DeleteAll();
        if(playFab != null) Destroy(playFab.gameObject);
        SceneManager.LoadScene("Login");
    }
}
