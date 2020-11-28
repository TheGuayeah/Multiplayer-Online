using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class PlayFabController : MonoBehaviour
{
    [SerializeField] private string titleId = "F28E8";

    [Header("GUI")]
    public TMP_InputField userNameInput;
    public TMP_InputField userEmailInput;
    public TMP_InputField userPasswordInput;
    public Button loginBtn;
    public Button backBtn;
    public Button registerBtn;
    public TextMeshProUGUI errorTxt;

    [Header("Player Stats")]
    public float playerHealth;
    public float playerDamage;
    public int playerHighScore;
    public static PlayFabController PFC;


    [Header("User Settings")]
    [SerializeField] private string userName;
    [SerializeField] private string userEmail;
    [SerializeField] private string userPassword;


    [Header("Game Settings")]
    public string build;
    public string gameMode;
    public Region region;
    public int serverPort;
    public string secretKey;


    private void OnEnable()
    {
        if (PFC == null)
        {
            PFC = this;
        }
        else
        {
            if (PFC != this)
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        errorTxt.text = "";
        userNameInput.onValueChanged.AddListener(delegate { OnChangeUserName(); });
        userEmailInput.onValueChanged.AddListener(delegate { OnChangeEmail(); });
        userPasswordInput.onValueChanged.AddListener(delegate { OnChangePassword(); });
        loginBtn.onClick.AddListener(delegate { GuiLogIn(); });
        backBtn.onClick.AddListener(delegate { Back(); });
        registerBtn.onClick.AddListener(delegate { GuiRegister(); });

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = titleId;
        }
        if (PlayerPrefs.GetString("EMAIL", "") != "" && PlayerPrefs.GetString("PASSWORD", "") != "")
        {
            userEmail = PlayerPrefs.GetString("EMAIL", "");
            userPassword = PlayerPrefs.GetString("PASSWORD", "");
            GuiLogIn();
        }
    }

    public void OnChangeUserName()
    {
        userName = userNameInput.text;
    }

    public void OnChangeEmail()
    {
        userEmail = userEmailInput.text;
    }

    public void OnChangePassword()
    {
        userPassword = userPasswordInput.text;
    }

    public void GuiLogIn()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = userEmail,
            Password = userPassword
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Successful!");
        errorTxt.text = "";
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        SetStats();
        GetStats();
        SceneManager.LoadScene("LobbyOnline");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("OnLoginFailure: " + error.GenerateErrorReport());
        errorTxt.text = "Error haciendo el login.";
    }

    public void GuiRegister()
    {
        if (userName != "" && userEmail != "" && userPassword != "")
        {
            var request = new RegisterPlayFabUserRequest
            {
                Username = userName,
                Email = userEmail,
                Password = userPassword
            };
            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
        }
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Congratulations, new user has been registered!");
        GuiLogIn();
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
        errorTxt.text = "Error haciendo el registro de usuario.";
    }

    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerHealth", Value = (int)playerHealth },
                new StatisticUpdate { StatisticName = "PlayerDamage", Value = (int)playerDamage },
                new StatisticUpdate { StatisticName = "PlayerHighScore", Value = playerHighScore }
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    public void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(), OnGetStatistics, OnStatiscticsError);
    }

    private void OnStatiscticsError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    public void OnGetStatistics(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch (eachStat.StatisticName)
            {
                case "PlayerHealth":
                    playerHealth = eachStat.Value;
                    break;
                case "PlayerDamage":
                    playerDamage = eachStat.Value;
                    break;
                case "PlayerHighScore":
                    playerHighScore = eachStat.Value;
                    break;
            }
        }
    }

    public void StartGame()
    {
        build = PlayFabSettings.BuildIdentifier;
        gameMode = PlayFabSettings.LocalApiServer;
        secretKey = PlayFabSettings.DeveloperSecretKey;
        serverPort = 7777;

        var serverBuildUpload = new GetServerBuildUploadUrl()
        {
            BuildId = build
        };
        PlayFabClientAPI.GetServerBuildUploadUrl(serverBuildUpload, OnMachmakeSuccess, OnMatchmakeFailure);


        var matchmakeReq = new MatchmakeRequest()
        {
            BuildVersion = "1.0",
            GameMode = "Basic",
            Region = Region.EUWest,
            StartNewIfNoneFound = true
        };
        PlayFabClientAPI.Matchmake(matchmakeReq, OnMachmakeSuccess, OnMatchmakeFailure);
    }

    private void OnMachmakeSuccess(MatchmakeResult result)
    {
        Debug.Log("Matchmake done");
        var req = new StartGameRequest()
        {
            BuildVersion = build,
            GameMode = "Basic",
            Region = Region.EUWest
        };
        PlayFabClientAPI.StartGame(req, OnStartSuccess, OnStartFailure);
    }

    private void OnMatchmakeFailure(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    private void OnStartSuccess(StartGameResult result)
    {
        Debug.Log("STARTED GAME");
        serverPort = (int)result.ServerPort;
    }

    private void OnStartFailure(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    public void Back()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("SelectGameMode");
    }
}