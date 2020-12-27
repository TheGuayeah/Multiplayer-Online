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
        private Text playerText;

        [SyncVar]
        public bool team1 = true;

        [SerializeField]
        private GameObject teamItemPrefab;
        [SerializeField]
        private GameObject teamCanvasPrefab;
        [SerializeField]
        private GameObject teamCanvas;
        [SerializeField]
        private Transform team1Panel, team2Panel;
        [SerializeField]
        private Button teamBtn;

        public GameObject myTeamItem;

        [SyncVar(hook = nameof(HandleSteamIdUpdated))]
        private ulong steamId;

        private GameManager gameManager;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
            UiSetup();
        }

        private void UiSetup()
        {
            if(isLocalPlayer)
                teamCanvas = Instantiate(teamCanvasPrefab);
            teamBtn = teamCanvas.GetComponentInChildren<Button>();
            teamBtn.onClick.AddListener(ChangeTeam);
            team1Panel = GameObject.Find("Team1").transform;
            team2Panel = GameObject.Find("Team2").transform;
            Transform parent = team1 ? team1Panel : team2Panel;
            GameObject item = Instantiate(teamItemPrefab, parent);
            item.name = PlayerPrefs.GetString("NickName");
            myTeamItem = item;
            myTeamItem.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
        }

        public void ChangeTeam()
        {
            Debug.Log("ChangeTeam");
            int team1Count = 0;
            int team2Count = 0;
            foreach (var item in gameManager.m_Tanks)
            {
                InfoPlayer player = item.m_Instance.GetComponent<InfoPlayer>();
                if (player.team1)
                    team1Count++;
                else
                    team2Count++;
            }
            if(!team1 && team1Count < 3)
                team1 = true;
            else if (team1 && team2Count < 3)
                team1 = false;

            Transform parent = team1 ? team1Panel : team2Panel;
            myTeamItem.transform.parent = parent;
        }

        public void SetSteamId(ulong steamId_)
        {
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
    }
}
