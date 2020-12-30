using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

namespace Complete {
    public class NumberPlayers : NetworkBehaviour {

        [SyncVar]
        public int currentNumberPlayers;

        private GameManager gameManager;


        public GameObject teamCanvas;
        [SerializeField]
        private GameObject teamItemPrefab;
        [SerializeField]
        public Transform team1Panel, team2Panel;
        [SerializeField]
        private Button teamBtn;

        int team1Count = 0;
        int team2Count = 0;

        private void Start() {
            gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
            if (isLocalPlayer)
                teamCanvas.SetActive(true);
            teamBtn.onClick.AddListener(ChangeTeam);
            team1Panel = GameObject.Find("Team1").transform;
            team2Panel = GameObject.Find("Team2").transform;
        }

        private void Update() {
            currentNumberPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
        }

        public void UiSetup() {
            UpdateTeamCount();
            foreach (var tank in gameManager.m_Tanks) {
                InfoPlayer player = tank.m_Instance.GetComponent<InfoPlayer>();
                if (player != null && player.myTeamItem == null) {
                    player.numberPlayers = this;
                    //if (player.LocalPlayer())
                        //player.SetTeamBool(team1Count <= team2Count);
                    Transform parent = player.team1 ? team1Panel : team2Panel;
                    GameObject item = Instantiate(teamItemPrefab, parent);
                    item.name = tank.m_Movement.playerName;
                    player.myTeamItem = item;
                    player.myTeamItem.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
                }
            }
        }

        public void ChangeTeam() {
            UpdateTeamCount();
            foreach (var item in gameManager.m_Tanks) {
                InfoPlayer player = item.m_Instance.GetComponent<InfoPlayer>();
                if (player != null && player.LocalPlayer()) {
                    if (!player.team1 && team1Count < 3)
                        player.SetTeamBool(true);
                    else if (player.team1 && team2Count < 3)
                        player.SetTeamBool(false);
                    break;
                }
            }
        }

        private void UpdateTeamCount() {
            team1Count = 0;
            team2Count = 0;
            foreach (var item in gameManager.m_Tanks) {
                InfoPlayer player = item.m_Instance.GetComponent<InfoPlayer>();
                if (player != null && !player.LocalPlayer()) {
                    if (player.team1)
                        team1Count++;
                    else
                        team2Count++;
                }
            }
        }
    }
}