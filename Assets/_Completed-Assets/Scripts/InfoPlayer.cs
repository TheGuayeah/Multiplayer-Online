using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class InfoPlayer : NetworkBehaviour
{
    [SerializeField]
    private Text playerText; 

    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    private ulong steamId;

    public void SetSteamId(ulong steamId_)
    {
        steamId = steamId_;
    }

    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);
        var name = SteamFriends.GetFriendPersonaName(cSteamId);
        if (isLocalPlayer) {
            PlayerPrefs.SetString("NickName", name);
        }
        playerText.text = name;
    }
}
