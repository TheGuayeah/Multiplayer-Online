using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

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
    public GameObject loginForm;
    public GameObject loading;

    [Header("Player Stats")]
    public float playerHealth;
    public float playerDamage;
    public int playerHighScore;
    public static PlayFabController PFC;


    [Header("User Settings")]
    [SerializeField] private string userId;
    [SerializeField] private string userName = "test";
    [SerializeField] private string userEmail;
    [SerializeField] private string userPassword;

    public string build = PlayFabSettings.BuildIdentifier;
    public string gameMode = PlayFabSettings.LocalApiServer;

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
        //NetworkClient _network;
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
        } else {
            loginForm.SetActive(true);
            loading.SetActive(false);
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

        //PlayFab.GetServedrBuildUploadUrl();
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        errorTxt.text = "";
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        SetStats();
        GetStats();
        SceneManager.LoadScene("LobbyOnline");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        loginForm.SetActive(true);
        loading.SetActive(false);
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

    public void Back()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("SelectGameMode");
    }
}