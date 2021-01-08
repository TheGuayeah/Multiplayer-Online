using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game
        public float m_StartDelay = 1.5f;           // The delay between the start of RoundStarting and RoundPlaying phases
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks
        public NumberPlayers numberPlayersScript;
        public GameObject canvasTeams;
        public PlayNetworking playNetworking;

        private int m_RoundNumber;                  // Which round the game is currently on
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won
        private bool localTankInit;
        
        
        private bool initGame = false;
        private int numberCurrentPlayers= 0;


        private void Start()
        {
            // Create the delays so they only have to be made once
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            InvokeRepeating(nameof(SetPlayersTanks), 0, 0.5f);
        }

        public void SetPlayersTanks()
        {
            int numberPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

            if(numberPlayers == 4)
            {
                CancelInvoke(nameof(SetPlayersTanks));
            }

            if (numberPlayers > numberCurrentPlayers)
            {
                numberCurrentPlayers = numberPlayers;

                GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
                int team1 = 0;
                if (!localTankInit)
                {
                    foreach (var tank in tanksInGame)
                    {
                        var playerInfo = tank.GetComponent<InfoPlayer>();
                        if (playerInfo.LocalPlayer())
                        {
                            if (team1 >= 2) playerInfo.team1 = false;
                            playerInfo.CmdSetTeamBool(playerInfo.team1);
                            if (playNetworking.gameStart) playerInfo.StartGameClient(true);
                            break;
                        }
                        else if (playerInfo.team1)
                        {
                            team1++;
                        }
                    }
                    localTankInit = true;
                }

                //Al comienzo de iniciar la partida el jugador, añade todos los tanques Player al array.
                if (!initGame)
                {
                    foreach (var tank in tanksInGame)
                    {
                        AddToTankList(tank);
                    }

                    //CheckTanksDistance();

                    initGame = true;
                }
                else //Una vez iniciada la partida, si entra un nuevo jugador, solo añade ese jugador al array
                {
                    AddToTankList(tanksInGame[tanksInGame.Length - 1]);
                }

                SetCameraTargets();

                if (playNetworking.gameStart) numberPlayersScript.ShowUI(false);
                else Invoke("UpdateUI", 2f);
            }
        }

        void RestartGame() {
            m_MessageText.gameObject.SetActive(false);

            //m_CameraControl.m_Targets = new Transform[0];
            if (GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<InfoPlayer>().LocalPlayer()) {
                GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < tanksInGame.Length; i++) {
                    tanksInGame[i].SetActive(true);
                    tanksInGame[i].GetComponent<TankMovement>().UpdateWins(0);
                }
            }

            numberPlayersScript.ShowUI(true);
        }

        private void UpdateUI() {
            numberPlayersScript.UiSetup();
        }

        private bool OneTeamLeft()
        {
            // Start the count of tanks left at zero
            bool team1ExistsYet = false;
            bool team2ExistsYet = false;

            int teamsLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter
                if (m_Tanks[i].m_Instance.activeSelf)
                {
                    if (m_Tanks[i].m_Instance.GetComponent<InfoPlayer>() != null) {
                        if (m_Tanks[i].m_Instance.GetComponent<InfoPlayer>().team1) {
                            if (!team1ExistsYet) {
                                teamsLeft++;
                                team1ExistsYet = true;
                            }
                        } else {
                            if (!team2ExistsYet) {
                                teamsLeft++;
                                team2ExistsYet = true;
                            }
                        }
                    } else
                        teamsLeft++;
                    
                } 
            }
            return teamsLeft <= 1;
        }

        private void AddToTankList(GameObject gameObj)
        {
            TankManager tank = new TankManager();
            // Hacemos referencia al objeto del tanque
            tank.m_Instance = gameObj;
            tank.m_PlayerNumber = 0;

            // Cambiamos el color del tanque según si es el del usuario o el de otro jugador
            //tank.m_PlayerColor = gameManager.m_AiColor;

            // Configuramos los componentes del tanque
            tank.Setup();

            // Añadimos el tanque completamente configurado a la lista del GameManager
            List<TankManager> tempTanks = m_Tanks.ToList();
            tempTanks.Add(tank);
            m_Tanks = tempTanks.ToArray();

            // Reconfiguramos la lista de objetivos de la cámara
            //gameManager.SetCameraTargets();
        }

        public void RemoveFromTankList(TankManager tankObj)
        {
            // Eliminamos el tanque completamente configurado a la lista del GameManager
            List<TankManager> tempTanks = m_Tanks.ToList();
            tempTanks.Remove(tankObj);
            m_Tanks = tempTanks.ToArray();

            // Reconfiguramos la lista de objetivos de la cámara
            //gameManager.SetCameraTargets();
        }

        public void StartGame() {
            StartCoroutine(GameLoop());
        }

        public void SetCameraTargets()
        {
            if (m_Tanks.Length == 0)
            {
                TankHealth[] tempPlayers = FindObjectsOfType<TankHealth>();
                TankManager[] tempAllTanks = new TankManager[tempPlayers.Length];

                foreach (var item in tempPlayers)
                {
                    tempAllTanks.ToList().Add(item.GetComponent<TankManager>());
                }
                m_Tanks = tempAllTanks;
            }

            // Create a collection of transforms the same size as the number of tanks
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... set it to the appropriate tank transform
                if (m_Tanks[i].m_Instance != null) targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another
        public IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished
            yield return StartCoroutine (RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level
                RestartGame();
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end
                StartCoroutine (GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move
            ResetAllTanks();
            DisableTankControl();
            // Snap the camera's zoom and position to something appropriate for the reset tanks
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;
            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks
            EnableTankControl();

            GameObject[] tanksEnemiesInGame = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var tank in tanksEnemiesInGame)
            {
                tank.GetComponent<NPC_AI_Script>().gameStarted = true;
            }

            // Clear the text from the screen
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while ((!playNetworking.activeTeams && !OneTankLeft()) || (playNetworking.activeTeams && !OneTeamLeft()))
            {

                // ... return on the next frame
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving
            DisableTankControl();

            // Clear the winner from the previous round
            m_RoundWinner = null;

            // See if there is a winner now the round is over
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score
            if (m_RoundWinner != null)
            {             
                int wins = m_RoundWinner.m_Movement.wins;

                if (m_RoundWinner.m_Instance.CompareTag("Enemy"))
                    m_RoundWinner.m_Movement.wins = wins+1;
                else if (GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<InfoPlayer>().LocalPlayer()) {//if (m_RoundWinner.m_Instance.GetComponent<InfoPlayer>().LocalPlayer()) {
                    if (playNetworking.activeTeams)
                        wins = GetTeamHighestWin(wins, m_RoundWinner.m_Instance.GetComponent<InfoPlayer>().team1);

                    m_RoundWinner.m_Movement.UpdateWins(wins+1);
                }
            }

            // Now the winner's score has been incremented, see if someone has one the game
            m_GameWinner = GetGameWinner(m_RoundWinner);


            yield return new WaitForSeconds(1f);
            // Get a message based on the scores and whether or not there is a game winner and display it
            string message = playNetworking.activeTeams ? EndMessageTeams() : EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_EndWait;
        }


        private int GetTeamHighestWin(int currentWin, bool team1) {
            GameObject[] tanksInGame = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < tanksInGame.Length; i++)
                if (team1 == tanksInGame[i].GetComponent<InfoPlayer>().team1)
                    if (tanksInGame[i].GetComponent<TankMovement>().wins > currentWin)
                        currentWin = tanksInGame[i].GetComponent<TankMovement>().wins;
            return currentWin;
        }

        // This is used to check if there is one or fewer tanks remaining and thus the round should end
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }
        
        
        // This function is to find out if there is a winner of the round
        // This function is called with the assumption that 1 or fewer tanks are currently active
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them is active, it is the winner so return it
                if (m_Tanks[i].m_Instance.activeSelf)
                {
                    return m_Tanks[i];
                }
            }

            // If none of the tanks are active it is a draw so return null
            return null;
        }


        // This function is to find out if there is a winner of the game
        private TankManager GetGameWinner(TankManager roundWinner)
        {
            if (roundWinner.m_Movement.wins >= m_NumRoundsToWin) {
                return roundWinner;
            }

            return null;
        }


        // Returns a string message to display at the end of each round
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null)
            {
                if(m_RoundWinner.m_playerName == "")
                {
                    message = "NPC WINS THE ROUND!";
                }
                else
                {
                    message = GetPlayerColorText(m_RoundWinner) + " WINS THE ROUND!";
                }
            }
                

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if(m_Tanks[i].m_playerName == "")
                {
                    message += "NPC "+ i + ": " + m_Tanks[i].m_Wins + " WINS\n";
                }
                else
                {
                    message += GetPlayerColorText(m_Tanks[i]) + ": " + m_Tanks[i].m_Wins + " WINS\n";
                }
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null)
            {
                if(m_GameWinner.m_playerName == "")
                {
                    message = "NPC WINS THE GAME!";
                }
                else
                {
                    message = GetPlayerColorText(m_GameWinner) + " WINS THE GAME!";
                }
            }

            return message;
        }

        private string EndMessageTeams() {
            // By default when a round ends there are no winners so the default end message is a draw
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null) {
                message = GetPlayerColorText(m_RoundWinner) + " WINS THE ROUND!";
            }

            // Add some line breaks after the initial message
            message += "\n\n\n\n";


            for (int i = 0; i < m_Tanks.Length; i++) {
                if (!m_Tanks[i].m_Instance.CompareTag("Enemy") && m_Tanks[i].m_Instance.GetComponent<InfoPlayer>().team1) {
                    message += GetPlayerColorText(m_Tanks[i]) + ": " + GetTeamHighestWin(0, true) + " WINS\n";
                    break;
                }
            }

            for (int i = 0; i < m_Tanks.Length; i++) {
                if (!m_Tanks[i].m_Instance.CompareTag("Enemy") && !m_Tanks[i].m_Instance.GetComponent<InfoPlayer>().team1) {
                    message += GetPlayerColorText(m_Tanks[i]) + ": " + GetTeamHighestWin(0, false) + " WINS\n";
                    break;
                }
            }

            for (int i = 0; i < m_Tanks.Length; i++) {
                if (m_Tanks[i].m_Instance.GetComponent<InfoPlayer>() == null) {
                    message += "NPC " + i + ": " + m_Tanks[i].m_Wins + " WINS\n";
                }
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null) {
                if (m_GameWinner.m_Instance.CompareTag("Enemy")) {
                    message = "NPC WINS THE GAME!";
                } else {
                    message = GetPlayerColorText(m_GameWinner) + " WINS THE GAME!";
                }
            }

            return message;
        }

        string GetPlayerColorText(TankManager player) {
            string coloredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(player.m_Instance.GetComponent<InfoPlayer>().myColor) + ">";
            if (playNetworking.activeTeams)
                coloredPlayerText += "TEAM " + (player.m_Instance.GetComponent<InfoPlayer>().team1 ? "1" : "2");
            else
                coloredPlayerText += player.m_Movement.playerName;
            coloredPlayerText += " </color>";

            return coloredPlayerText;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
                m_Tanks[i].m_Instance.GetComponent<TankMovement>().CheckTanksDistance();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}