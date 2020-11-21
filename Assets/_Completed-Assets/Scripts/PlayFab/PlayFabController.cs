using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabController : MonoBehaviour
{
    [SerializeField] private string titleId = "F28E8";
    
    [Header("GUI")]
    public TMP_InputField userNameInput;
    public TMP_InputField userEmailInput;
    public TMP_InputField userPasswordInput;

    [Header("User Settings")]
    [SerializeField] private string userId;
    [SerializeField] private string userName = "test";
    [SerializeField] private string userEmail;
    [SerializeField] private string userPassword;

    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = titleId;
        }

        PlayerPrefs.DeleteKey("USERID");

        if (PlayerPrefs.HasKey("USERID"))
        {
            userId = PlayerPrefs.GetString("USERID");
            Debug.Log("Player’s UserId is: " + userId);

            var loginRequest = new LoginWithCustomIDRequest
            {
                CustomId = userId,
                CreateAccount = false
            };
            PlayFabClientAPI.LoginWithCustomID(loginRequest, OnLoginSuccess, OnLoginFailure);
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
        PlayerPrefs.SetString("EMAIL", userEmail);

        var request = new LoginWithEmailAddressRequest
        {
            Email = userEmail,
            Password = userPassword
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");

        userId = result.InfoResultPayload.AccountInfo.TitleInfo.TitlePlayerAccount.Id;
        PlayerPrefs.SetString("USERID", userId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("OnLoginFailure: " + error.GenerateErrorReport());
        var request = new RegisterPlayFabUserRequest
        {
            Username = userName,
            Email = userEmail,
            Password = userPassword
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
        GuiLogIn();
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Congratulations, new user has been registered!");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}