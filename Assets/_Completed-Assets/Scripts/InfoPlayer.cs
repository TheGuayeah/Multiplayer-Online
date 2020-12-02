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
    }

    private void HandleSteamIdUpadated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);
        var name = SteamFriends.GetFriendPersonaName(cSteamId);
        PlayerPrefs.SetString("NickName", name);
    }
}
