using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class PlayNetworking : NetworkBehaviour
    {
        public GameObject playBtn;
        public bool activeTeams = true;

        [SyncVar(hook = nameof(StartGameNetwork))]
        public bool gameStart;

        [SerializeField]
        private GameManager gameManager;

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
            gameStart = true;
            GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
            foreach (var tank in tanksInGame)
            {
                tank.GetComponent<TankMovement>().enabled = true;
                tank.GetComponent<TankShooting>().enabled = true;
            }
            tanksInGame[0].GetComponent<InfoPlayer>().RpcStartGame(true);
        }
    }
}

