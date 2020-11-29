using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;

public class InfoPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpadated))]
    private ulong steamId;

    public void SetSteamId(ulong steamId_)
    {
        steamId = steamId_;
        var cSteamId = new CSteamID(steamId_);
        var name = SteamFriends.GetFriendPersonaName(cSteamId);
        PlayerPrefs.SetString("NickName", name);
    }

    private void HandleSteamIdUpadated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);
        var name = SteamFriends.GetFriendPersonaName(cSteamId);
        PlayerPrefs.SetString("NickName", name);
    }
}
