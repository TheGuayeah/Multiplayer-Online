using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class PlayNetworking : NetworkBehaviour
    {
        public GameObject playBtn;
        public GameObject teamsCanvas;
        public bool activeTeams = true;

        [SyncVar(hook = nameof(StartGameNetwork))]
        public bool gameStart;

        [SerializeField]
        private GameManager gameManager;

        private int team1Players;
        private int team2Players;

        void Start()
        {
            playBtn.SetActive(isServer);
        }

        void StartGameNetwork(bool oldStart, bool newStart)
        {
            if (gameManager.canvasTeams.activeSelf)
            {
                gameManager.canvasTeams.SetActive(false);
            }
            gameStart = newStart;
        }

        public void StartGame()
        {
            checkNumPlayers();
            if (team1Players < 3 && team2Players < 3)
            {
                gameStart = true;
                teamsCanvas.SetActive(false);
                GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
                foreach (var tank in tanksInGame)
                {
                    tank.GetComponent<TankMovement>().enabled = true;
                    tank.GetComponent<TankShooting>().enabled = true;
                }
                tanksInGame[0].GetComponent<InfoPlayer>().RpcStartGame(true);
            }
        }

        private void checkNumPlayers()
        {
            // Start the count of tanks left at zero
            int team1TanksLeft = 0;
            int team2TanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < gameManager.m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter
                if (gameManager.m_Tanks[i].m_Instance.activeSelf && gameManager.m_Tanks[i].m_Instance.GetComponent<InfoPlayer>() != null)
                {
                    if (gameManager.m_Tanks[i].m_Instance.GetComponent<InfoPlayer>().team1)
                        team1TanksLeft++;
                    else
                        team2TanksLeft++;
                }
            }
            team1Players = team1TanksLeft;
            team2Players = team2TanksLeft;
        }
    }
}

