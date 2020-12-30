using System.Collections;
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

        public void SetTeamBool(bool newTeam1) {
            team1 = newTeam1;
            CmdUpdateUIPos();
        }

        private void ChangeTeamBool(bool oldTeam1, bool newTeam1) {
            team1 = newTeam1;
            CmdUpdateUIPos();
        }

        [Command]
        private void CmdUpdateUIPos() {
            RpcUpdateUIPos();
        }

        [ClientRpc]
        private void RpcUpdateUIPos() {
            if (myTeamItem != null) {
                Transform parent = team1 ? numberPlayers.team1Panel : numberPlayers.team2Panel;
                myTeamItem.transform.SetParent(parent);
            }
        }

        public bool LocalPlayer() {
            return isLocalPlayer;
        }
    }
}
