using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections.Generic;

namespace Complete
{
    public class GameManagerLocal : MonoBehaviour
    {
        public GameObject mainCamera;
        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks
        public InputActionAsset[] m_InputTanks;     // Array to put the inputs to every tank
        public Text m_NumPlayersTxt;                // Menu Text number of tanks
        public GameObject m_PanelNumPlayersUI;      // Set active and false menu numbre of tanks
        public CinemachineVirtualCamera[] m_PlayersFollowCams;
        public GameObject[] m_PlayersCams;
        public GameObject[] m_PanelsEnterPlayers;
        public GameObject Player3;
        public GameObject Player4;

        private LocalTeamsManager localTeamsManager;
        private int m_RoundNumber;                  // Which round the game is currently on
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won
        private int m_NumPlayers = 2;               // Default number of tanks
        private TankManager[] m_TanksStore;
        List<int> m_deadTanks = new List<int>();

        private bool gameRunning = false;


        private void Start()
        {
            m_PanelNumPlayersUI.SetActive(true);
            m_MessageText.gameObject.SetActive(false);
            m_NumPlayersTxt.text = m_NumPlayers.ToString();
            m_TanksStore = m_Tanks;
            localTeamsManager = FindObjectOfType<LocalTeamsManager>().GetComponent<LocalTeamsManager>();
        }

        private void StartGame()
        {
            gameRunning = true;
            m_MessageText.gameObject.SetActive(true);

            // Create the delays so they only have to be made once
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTanks();

            SetCamerasLayout();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game
            if (!localTeamsManager.activeTeams || !OneTeamLeft())
                StartCoroutine(GameLoop());
        }

		
		private void SpawnAllTanks()
		{
			Camera mainCam = mainCamera.GetComponent<Camera>();

			// For all the tanks...
			for (int i = 0; i < m_Tanks.Length; i++)
			{
                // ... create them, set their player number and references needed for control
                if (m_Tanks[i] != null)
                {
                    m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation);
                    m_Tanks[i].m_PlayerNumber = i + 1;
                    m_Tanks[i].m_Instance.GetComponent<PlayerInput>().actions = m_InputTanks[i];
                    m_Tanks[i].Setup();

                    TankHealthLocal tankHealth = m_Tanks[i].m_Instance.GetComponent<TankHealthLocal>();
                    tankHealth.isTeamGame = m_Tanks[i].isTeamGame;
                    tankHealth.team1 = m_Tanks[i].team1;

                    m_PlayersFollowCams[i].gameObject.SetActive(true);
                    m_PlayersFollowCams[i].Follow = m_Tanks[i].m_Instance.transform;
                    m_PlayersFollowCams[i].LookAt = m_Tanks[i].m_Instance.transform;

                    //AddCamera(i, mainCam);
                }
			}

            foreach (var tank in m_Tanks)
            {
                tank.m_Instance.GetComponent<TankMovementLocal>().CheckTanksDistance();
            }

            mainCam.gameObject.SetActive (false);
		}

		private void AddCamera (int i, Camera mainCam)
        {
			GameObject childCam = new GameObject ("Camera" + (i + 1));
			Camera newCam = childCam.AddComponent<Camera>();		
			newCam.CopyFrom (mainCam);

            childCam.transform.parent = m_Tanks[i].m_Instance.transform;

            if (i == 0)
            {
                newCam.rect = new Rect (0.0f, 0.5f, 0.89f, 0.5f);
            }
            else
            {
                newCam.rect = new Rect (0.11f, 0.0f, 0.89f, 0.5f);
            }
		}

        private void SetCamerasLayout()
        {
            if (m_NumPlayers == 1)
            {
                m_PlayersCams[0].gameObject.SetActive(true);
                m_PlayersCams[0].GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                m_PlayersCams[1].gameObject.SetActive(false);
                m_PlayersCams[2].gameObject.SetActive(false);
                m_PlayersCams[3].gameObject.SetActive(false);
                m_PlayersCams[4].gameObject.SetActive(false);

                m_PanelsEnterPlayers[0].SetActive(false);
                m_PanelsEnterPlayers[1].SetActive(true);
                m_PanelsEnterPlayers[2].SetActive(true);
                m_PanelsEnterPlayers[3].SetActive(true);
            }
            else if (m_NumPlayers == 2)
            {
                m_PlayersCams[0].gameObject.SetActive(true);
                m_PlayersCams[0].GetComponent<Camera>().rect = new Rect(0, 0.5f, 1, 1);
                m_PlayersCams[1].gameObject.SetActive(true);
                m_PlayersCams[1].GetComponent<Camera>().rect = new Rect(0, -0.5f, 1, 1);
                m_PlayersCams[2].gameObject.SetActive(false);
                m_PlayersCams[3].gameObject.SetActive(false);
                m_PlayersCams[4].gameObject.SetActive(false);

                m_PanelsEnterPlayers[0].SetActive(false);
                m_PanelsEnterPlayers[1].SetActive(false);
                m_PanelsEnterPlayers[2].SetActive(true);
                m_PanelsEnterPlayers[3].SetActive(true);
            }
            else if(m_NumPlayers == 3)
            {
                if (m_Tanks[2].m_PlayerNumber == 3)
                {
                    m_PlayersCams[0].gameObject.SetActive(true);
                    m_PlayersCams[0].GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 1);
                    m_PlayersCams[1].gameObject.SetActive(true);
                    m_PlayersCams[1].GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 1);
                    m_PlayersCams[2].gameObject.SetActive(true);
                    m_PlayersCams[2].GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                    m_PlayersCams[4].gameObject.SetActive(true);
                    m_PlayersCams[4].GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                    m_PlayersCams[3].gameObject.SetActive(false);

                    m_PanelsEnterPlayers[0].SetActive(false);
                    m_PanelsEnterPlayers[1].SetActive(false);
                    m_PanelsEnterPlayers[2].SetActive(false);
                    m_PanelsEnterPlayers[3].SetActive(true);
                }
                else
                {
                    m_PlayersCams[0].gameObject.SetActive(true);
                    m_PlayersCams[0].GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 1);
                    m_PlayersCams[1].gameObject.SetActive(true);
                    m_PlayersCams[1].GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 1);
                    m_PlayersCams[2].gameObject.SetActive(true);
                    m_PlayersCams[2].GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                    m_PlayersCams[3].gameObject.SetActive(true);
                    m_PlayersCams[3].GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                    m_PlayersCams[4].gameObject.SetActive(false);

                    m_PanelsEnterPlayers[0].SetActive(false);
                    m_PanelsEnterPlayers[1].SetActive(false);
                    m_PanelsEnterPlayers[2].SetActive(true);
                    m_PanelsEnterPlayers[3].SetActive(false);
                }
                
            }
            else if (m_NumPlayers == 4)
            {
                m_PlayersCams[0].gameObject.SetActive(true);
                m_PlayersCams[0].GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.5f, 1);
                m_PlayersCams[1].gameObject.SetActive(true);
                m_PlayersCams[1].GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 1);
                m_PlayersCams[2].gameObject.SetActive(true);
                m_PlayersCams[2].GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 0.5f);
                m_PlayersCams[3].gameObject.SetActive(true);
                m_PlayersCams[3].GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                m_PlayersCams[4].gameObject.SetActive(false);

                m_PanelsEnterPlayers[0].SetActive(false);
                m_PanelsEnterPlayers[1].SetActive(false);
                m_PanelsEnterPlayers[2].SetActive(false);
                m_PanelsEnterPlayers[3].SetActive(false);
            }
        }

        public void P1EnterPlayer(InputAction.CallbackContext context)
        {
            //This tank will always be created
            Debug.Log("Tank 1 already created");
        }

        public void P2EnterPlayer(InputAction.CallbackContext context)
        {
            //This tank will always be created
            Debug.Log("Tank 2 already created");
        }

        public void P3EnterPlayer(InputAction.CallbackContext context)
        {
            if (gameRunning) {
                var val = context.ReadValue<float>();
                if (val == 1) {
                    try {
                        //Check if the tank already exists
                        var num = m_Tanks[2].m_PlayerNumber;

                        if (num != 3)
                            AddPlayer(3);

                    } catch {
                        //If the tank do not exist, create it.
                        if (m_TanksStore[2] != null)
                            AddPlayer(3);
                    }
                }
            }
        }

        

        public void P4EnterPlayer(InputAction.CallbackContext context)
        {
            if (gameRunning) {
                var val = context.ReadValue<float>();
                if (val == 1) {
                    try {
                        //Check if the tank already exists
                        var num = m_Tanks[2].m_PlayerNumber;

                        if (m_NumPlayers == 3)
                            AddPlayer(4);
                    } catch {
                        AddPlayer(3);
                    }
                }
            }
        }

        private void AddPlayer(int player) {
            bool oneTeamLeft = OneTeamLeft();

            CheckPlayerTeam(player-1);
            
            int wins = 0;
            if (localTeamsManager.activeTeams) {
                for(int i = 0; i <m_Tanks.Length; i++) {
                    if (m_Tanks[i].team1 == localTeamsManager.playerTeams[player - 1])
                        wins = m_Tanks[i].m_Wins;
                }
            }

            m_NumPlayers++;
            SetActiveTanks();

            m_TanksStore[player-1].m_Instance = Instantiate(m_TankPrefab, m_TanksStore[player - 1].m_SpawnPoint.position, m_TanksStore[player - 1].m_SpawnPoint.rotation) as GameObject;
            m_TanksStore[player - 1].m_PlayerNumber = player;
            m_TanksStore[player - 1].m_Instance.GetComponent<PlayerInput>().actions = m_InputTanks[player - 1];
            m_TanksStore[player - 1].Setup();

            TankHealthLocal tankHealth = m_Tanks[player - 1].m_Instance.GetComponent<TankHealthLocal>();
            tankHealth.isTeamGame = m_Tanks[player - 1].isTeamGame;
            tankHealth.team1 = m_Tanks[player - 1].team1;

            m_PlayersFollowCams[player - 1].gameObject.SetActive(true);
            m_PlayersFollowCams[player - 1].Follow = m_TanksStore[player - 1].m_Instance.transform;
            m_PlayersFollowCams[player - 1].LookAt = m_TanksStore[player - 1].m_Instance.transform;

            if (localTeamsManager.activeTeams) {
                m_Tanks[player - 1].m_Wins = wins;
            }

            SetCamerasLayout();

            if (localTeamsManager.activeTeams && player == 3 && oneTeamLeft) 
                StartCoroutine(GameLoop());
        }

        private void CheckPlayerTeam(int pos) {
            if (localTeamsManager.activeTeams) {
                bool team1 = localTeamsManager.playerTeams[pos];
                int sameTeam = 0;
                for (int i = 0; i < pos; i++)
                    if (m_Tanks[i].team1 == team1)
                        sameTeam++;
                if (sameTeam > 1)
                    localTeamsManager.playerTeams[pos] = !localTeamsManager.playerTeams[pos];
            }
        }

        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform
                if(m_Tanks[i] != null) targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another
        private IEnumerator GameLoop()
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

        private void RestartGame() {
            m_PanelNumPlayersUI.SetActive(true);
            m_MessageText.gameObject.SetActive(false);

            m_RoundNumber = 0;
            mainCamera.SetActive(true);
            m_CameraControl.m_Targets = new Transform[0];
            for (int i = 0; i < m_Tanks.Length; i++) {
                m_PlayersFollowCams[i].gameObject.SetActive(false);
                m_PlayersCams[i].gameObject.SetActive(false);
                m_Tanks[i].m_Wins = 0;
                if (m_Tanks[i].m_Instance.activeSelf)
                    Destroy(m_Tanks[i].m_Instance);
            }
            m_PanelsEnterPlayers[2].gameObject.SetActive(false);
            m_PanelsEnterPlayers[3].gameObject.SetActive(false);

            UpdateMenuUI();
        }

        private void UpdateMenuUI() {
            m_NumPlayersTxt.text = m_NumPlayers.ToString();
            if (m_NumPlayers > 2) 
                Player3.SetActive(true);
            if (m_NumPlayers > 3)
                Player4.SetActive(true);
            localTeamsManager.UpdateTeamUIPositions();
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

            // Clear the text from the screen
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while ((!localTeamsManager.activeTeams && !OneTankLeft()) || (localTeamsManager.activeTeams && !OneTeamLeft()))
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
            if (m_RoundWinner != null) {
                m_RoundWinner.m_Wins++;
                if(localTeamsManager.activeTeams){
                    for (int i = 0; i < m_Tanks.Length; i++) 
                        if (m_Tanks[i].m_PlayerNumber != m_RoundWinner.m_PlayerNumber && m_RoundWinner.team1 == m_Tanks[i].team1)
                            m_Tanks[i].m_Wins++;
                }
            }

            // Now the winner's score has been incremented, see if someone has one the game
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it
            string message = localTeamsManager.activeTeams ? EndMessageTeams() : EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false
            return numTanksLeft <= 1;
        }

        private bool OneTeamLeft() {
            // Start the count of tanks left at zero
            int team1TanksLeft = 0;
            int team2TanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++) {
                // ... and if they are active, increment the counter
                if (m_Tanks[i].m_Instance.activeSelf) {
                    if (m_Tanks[i].team1)
                        team1TanksLeft++;
                    else
                        team2TanksLeft++;
                }
            }

            return team1TanksLeft == 0 || team2TanksLeft == 0;
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
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it
                if (m_Tanks[i].m_Wins >= m_NumRoundsToWin)
                {
                    return m_Tanks[i];
                }
            }

            // If no tanks have enough rounds to win, return null
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null)
            {
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
            }

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null)
            {
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
            }

            return message;
        }

        private string EndMessageTeams() {
            // By default when a round ends there are no winners so the default end message is a draw
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null) {
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
            }

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            
            for (int i = 0; i < m_Tanks.Length; i++) {
                if (m_Tanks[i].team1) {
                    message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
                    break;
                }
            }

            for (int i = 0; i < m_Tanks.Length; i++) {
                if (!m_Tanks[i].team1) {
                    message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
                    break;
                }
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null) {
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
            }

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_PlayersFollowCams[i].Follow = m_Tanks[i].m_Instance.transform;
                m_PlayersFollowCams[i].LookAt = m_Tanks[i].m_Instance.transform;
                m_Tanks[i].Reset();
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

        public void SubsNumPlayers()
        {
            if (m_NumPlayers > 2)
            {
                m_NumPlayers--;
                m_NumPlayersTxt.text = m_NumPlayers.ToString();
                Player3.SetActive(m_NumPlayers > 2);
                Player4.SetActive(m_NumPlayers > 3);
            }
        }

        public void PlusNumPlayers()
        {
            if (m_NumPlayers < 4)
            {
                m_NumPlayers++;
                m_NumPlayersTxt.text = m_NumPlayers.ToString();
                Player3.SetActive(m_NumPlayers > 2);
                Player4.SetActive(m_NumPlayers > 3);
            }
        }

        private void SetActiveTanks()
        {
            TankManager[] newTanks = new TankManager[m_NumPlayers];
            for (int i = 0; i < m_TanksStore.Length; i++)
            {
                Color colorTeam1 = Color.red;
                Color colorTeam2 = Color.blue;

                if (i < m_NumPlayers)
                {
                    newTanks[i] = m_TanksStore[i];
                    newTanks[i].isTeamGame= localTeamsManager.activeTeams;
                    newTanks[i].team1 = localTeamsManager.playerTeams[i];

                    if (newTanks[i].isTeamGame)
                        newTanks[i].m_PlayerColor = newTanks[i].team1 ?  colorTeam1 : colorTeam2;
                }
            }
            m_Tanks = newTanks;
        }

        private void SetActiveTanksWith3()
        {
            TankManager[] newTanks = new TankManager[3];
            newTanks[0] = m_TanksStore[0];
            newTanks[1] = m_TanksStore[1];
            newTanks[2] = m_TanksStore[3];
            m_Tanks = newTanks;
        }

        public void PlayGameBtn()
        {
            m_PanelNumPlayersUI.SetActive(false);

            SetActiveTanks();
            StartGame();
        }

        public void BackBtn() {
            SceneManager.LoadScene(0);
        }

        public void ChangeDeadCamera(int deadPlayerNum)
        {
            m_deadTanks.Add(deadPlayerNum);
            foreach (TankManager tank in m_Tanks)
            {
                if (m_deadTanks.Count+1 < m_NumPlayers && !m_deadTanks.Contains(tank.m_PlayerNumber))
                {
                    m_PlayersFollowCams[deadPlayerNum - 1].Follow = m_Tanks[tank.m_PlayerNumber-1].m_Instance.transform;
                    m_PlayersFollowCams[deadPlayerNum - 1].LookAt = m_Tanks[tank.m_PlayerNumber-1].m_Instance.transform;
                    foreach (int dead in m_deadTanks)
                    {
                        m_PlayersFollowCams[dead - 1].Follow = m_Tanks[tank.m_PlayerNumber - 1].m_Instance.transform;
                        m_PlayersFollowCams[dead - 1].LookAt = m_Tanks[tank.m_PlayerNumber - 1].m_Instance.transform;
                    }
                    return;
                }
            }
            
        }
    }
}