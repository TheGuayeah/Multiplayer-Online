using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Complete
{
    [Serializable]
    public class TankManager
    {
        // This class is to manage various settings on a tank
        // It works with the GameManager class to control how the tanks behave
        // and whether or not players have control of their tank in the 
        // different phases of the game

        public string m_playerName;
        public Color m_PlayerColor;                             // This is the color this tank will be tinted
        public Transform m_SpawnPoint;                          // The position and direction the tank will have when it spawns
        public int m_PlayerNumber;            // This specifies which player this the manager for
        [HideInInspector] public string m_ColoredPlayerText;    // A string that represents the player with their number colored to match their tank


        public GameObject m_Instance;         // A reference to the instance of the tank when it is created
        public int m_Wins;                    // The number of wins this player has so far
        public bool isTeamGame;
        public bool team1;

        public TankMovement m_Movement;                        // Reference to tank's movement script, used to disable and enable control
        private TankShooting m_Shooting;                        // Reference to tank's shooting script, used to disable and enable control
        private TankMovementLocal m_MovementLocal;                        // Reference to tank's movement script, used to disable and enable control
        private TankShootingLocal m_ShootingLocal;                        // Reference to tank's shooting script, used to disable and enable control
        private GameObject m_CanvasGameObject;                  // Used to disable the world space UI during the Starting and Ending phases of each round


        public void Setup ()
        {
            try
            {
                // Get references to the components
                m_Movement = m_Instance.GetComponent<TankMovement>();
                m_Shooting = m_Instance.GetComponent<TankShooting>();
                m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

                // Set the player numbers to be consistent across the scripts
                m_Movement.m_PlayerNumber = m_PlayerNumber;
                m_Shooting.m_PlayerNumber = m_PlayerNumber;
            }
            catch (Exception)
            {
                // Get references to the components
                m_MovementLocal = m_Instance.GetComponent<TankMovementLocal>();
                m_ShootingLocal = m_Instance.GetComponent<TankShootingLocal>();
                m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

                // Set the player numbers to be consistent across the scripts
                m_MovementLocal.m_PlayerNumber = m_PlayerNumber;
                m_ShootingLocal.m_PlayerNumber = m_PlayerNumber;
            }

            InfoPlayer infoPlayer = m_Instance.GetComponent<InfoPlayer>();

            if (infoPlayer)
            {
                m_PlayerColor = infoPlayer.team1 ? infoPlayer.colorTeam1 : infoPlayer.colorTeam2;
            }
            
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">";
            if (isTeamGame)
                m_ColoredPlayerText += "TEAM " + (team1 ? "1" : "2");
            else
                m_ColoredPlayerText += "PLAYER " + m_PlayerNumber;
            m_ColoredPlayerText += " </color>";

            if(m_Movement)
            {
                m_playerName = m_Movement.playerName;
            }

            // Get all of the renderers of the tank
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();

            // Go through all the renderers...
            for (int i = 0; i < renderers.Length; i++)
            {
                // ... set their material color to the color specific to this tank
                renderers[i].material.color = m_PlayerColor;
            }
        }


        // Used during the phases of the game where the player shouldn't be able to control their tank
        public void DisableControl ()
        {
            try
            {
                m_Movement.enabled = false;
                m_Shooting.enabled = false;
            }
            catch (Exception)
            {
                m_MovementLocal.enabled = false;
                m_ShootingLocal.enabled = false;
            }

            m_CanvasGameObject.SetActive (false);
        }


        // Used during the phases of the game where the player should be able to control their tank
        public void EnableControl ()
        {
            try
            {
                m_Movement.enabled = true;
                m_Shooting.enabled = true;
            }
            catch (Exception)
            {
                m_MovementLocal.enabled = true;
                m_ShootingLocal.enabled = true;
            }
            m_CanvasGameObject.SetActive (true);
        }


        // Used at the start of each round to put the tank into it's default state
        public void Reset ()
        {


            //m_Instance.transform.position = m_SpawnPoint.position;
            //m_Instance.transform.rotation = m_SpawnPoint.rotation;

            Respawn();

            //m_Instance.SetActive (false);
            m_Instance.SetActive (true);
            try
            {
                m_Instance.GetComponent<TankHealth>().ResetHealth();
            }
            catch (Exception)
            {
                m_Instance.GetComponent<TankHealthLocal>().ResetHealth();
            }
        }

        public void Respawn()
        {
            float randomX = Random.Range(-40, 40);
            float randomZ = Random.Range(-40, 40);

            m_Instance.transform.position = new Vector3(randomX, 0, randomZ);

            RaycastHit hit;
            if (Physics.Raycast(new Ray(m_Instance.transform.position, Vector3.down), out hit, 100f))
                m_Instance.transform.position = hit.point + new Vector3(0, 1, 0);
            if(hit.collider != null)
            {
                if (hit.collider.CompareTag("Building"))
                    Respawn();
            }
        }
    }
}