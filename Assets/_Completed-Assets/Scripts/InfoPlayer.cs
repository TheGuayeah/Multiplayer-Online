﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;
using UnityEngine.UI;

namespace Complete
{
    public class InfoPlayer : NetworkBehaviour
    {
        [SerializeField]
        public Text playerText;

        [SyncVar(hook = nameof(ChangeTeamBool))]
        public bool team1 = true;
        
        public GameObject myTeamItem;
        public NumberPlayers numberPlayers;

        [SyncVar]
        public Color myColor;

        public Color colorTeam1= Color.green, colorTeam2= Color.blue;

        [SyncVar(hook = nameof(HandleSteamIdUpdated))]
        private ulong steamId;

        public void SetSteamId(ulong steamId_) {
            steamId = steamId_;
        }

        private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
        {
            var cSteamId = new CSteamID(newSteamId);
            var name = SteamFriends.GetFriendPersonaName(cSteamId);
            if (isLocalPlayer)
            {
                PlayerPrefs.SetString("NickName", name);
            }
            playerText.text = name;
        }

        [Command]
        public void CmdSetTeamBool(bool newTeam1) {
            team1 = newTeam1;
            RpcSetTeamBool(newTeam1);
            ChangeColor();
        }

        [ClientRpc]
        public void RpcSetTeamBool(bool newTeam1) {
            team1 = newTeam1;
            UpdateUIPos();
            ChangeColor();
        }

        private void ChangeTeamBool(bool oldTeam1, bool newTeam1)
        {
            team1 = newTeam1;
        }

        private void ChangeColor()
        {
            // Get all of the renderers of the tank
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

            myColor = team1 ? colorTeam1 : colorTeam2;

            // Go through all the renderers...
            for (int i = 0; i < renderers.Length; i++)
            {
                // ... set their material color to the color specific to this tank
                renderers[i].material.color = myColor;
            }
        }

        private void UpdateUIPos() {
            if (myTeamItem != null) {
                RectTransform rt = myTeamItem.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(
                    team1 ? numberPlayers.team1Panel.localPosition.x : numberPlayers.team2Panel.localPosition.x,
                    rt.localPosition.y,
                    rt.localPosition.z);
            }
        }
        

        public bool LocalPlayer() {
            return isLocalPlayer;
        }
    }
}
