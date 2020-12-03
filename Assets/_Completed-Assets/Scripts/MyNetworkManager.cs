using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class MyNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
            SteamLobby.LobbyID,
            numPlayers - 1);

        var playerInfoDisplay = conn.identity.GetComponent<InfoPlayer>();

        playerInfoDisplay.SetSteamId(steamId.m_SteamID);
    }
}
