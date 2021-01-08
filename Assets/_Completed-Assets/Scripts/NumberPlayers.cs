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
            //if (isLocalPlayer)
                teamCanvas.SetActive(true);
            teamBtn.onClick.AddListener(ChangeTeam);
            team1Panel = GameObject.Find("Team1").transform;
            team2Panel = GameObject.Find("Team2").transform;
        }

        public void UiSetup()
        {
            UpdateTeamCount();
            int count = 0;
            foreach (var tank in gameManager.m_Tanks)
            {
                InfoPlayer player = tank.m_Instance.GetComponent<InfoPlayer>();
                if (player != null)
                {
                    if (player.myTeamItem == null)
                    {
                        player.numberPlayers = this;
                        //if (player.LocalPlayer())
                        //player.SetTeamBool(team1Count <= team2Count);
                        Transform parent = team1Panel.parent;
                        GameObject item = Instantiate(teamItemPrefab, parent);
                        item.name = tank.m_Movement.playerName;
                        //tank.m_Movement.enabled = false;
                        RectTransform rt = item.GetComponent<RectTransform>();
                        rt.localPosition = new Vector3(
                            player.team1 ? team1Panel.localPosition.x : team2Panel.localPosition.x,
                            team1Panel.localPosition.y - (count * rt.sizeDelta.y),
                            0.0f);
                        player.myTeamItem = item;
                        player.myTeamItem.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
                    }
                    count++;
                }                
            }
        }



        public void ShowUI(bool show)
        {
            teamCanvas.SetActive(show);
        }

        [Command]
        public void CmdUIServerSetup()
        {
            RpcUIClientsSetup();
        } 

        [ClientRpc]
        public void RpcUIClientsSetup()
        {
            UiSetup();
        } 

        public void ChangeTeam() {
            UpdateTeamCount();
            foreach (var item in gameManager.m_Tanks) {
                InfoPlayer player = item.m_Instance.GetComponent<InfoPlayer>();
                if (player != null && player.LocalPlayer() && item.m_Instance.activeSelf) {
                    if (!player.team1 && team1Count < 3)
                    {
                        player.CmdSetTeamBool(true);
                        item.team1 = true;
                    }
                    else if (player.team1 && team2Count < 3)
                    {
                        player.CmdSetTeamBool(false);
                        item.team1 = false;
                    }
                        
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