using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using PlayFab;

public class SteamLobby : MonoBehaviour
{
    public static CSteamID LobbyID { get; private set; }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    private const string HostAddresKey = "HostAddress";
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    public void HostLobby()
    {
        GetComponent<LobbyMenu>().SetColor();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t calllback)
    {
        if (calllback.m_eResult != EResult.k_EResultOK) return;

        LobbyID = new CSteamID(calllback.m_ulSteamIDLobby);
        var name = SteamFriends.GetFriendPersonaName(LobbyID);

        PlayerPrefs.SetString("NickName", name);
        networkManager.StartHost();


        SteamMatchmaking.SetLobbyData(
            new CSteamID(calllback.m_ulSteamIDLobby), 
            HostAddresKey, 
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), HostAddresKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }
}
