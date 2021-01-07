using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class PlayNetworking : NetworkBehaviour
    {
        public GameObject playBtn;

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
            gameStart = newStart;
        }

        [Command]
        public void CmdStartGame(bool newBool)
        {
            gameStart = newBool;
            RpcStartGame(newBool);
        }

        [ClientRpc]
        public void RpcStartGame(bool newBool)
        {
            gameStart = newBool;
            StartGame();
        }

        public void StartGame()
        {
            GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
            foreach (var tank in tanksInGame)
            {
                tank.GetComponent<TankMovement>().enabled = true;
                tank.GetComponent<TankShooting>().enabled = true;
            }

            StartCoroutine(gameManager.GameLoop());
        }
    }
}

