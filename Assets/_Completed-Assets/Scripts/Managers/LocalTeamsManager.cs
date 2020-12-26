using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalTeamsManager : MonoBehaviour
{
    public GameObject[] TextTeam1;
    public GameObject[] TextTeam2;

    [HideInInspector]
    public bool activeTeams = false;

    public bool[] playerTeams= new bool[4] {true, false, true, false};

    public void ActiveTeams(bool activeTeams)
    {
        this.activeTeams = activeTeams;
    }

    public void ChangeTeam(int player)
    {
        int countTrue = 0; 
        int countFalse = 0; 

        for (int i = 0; i < playerTeams.Length; i++)
        {
            if(playerTeams[i] == true)
            {
                countTrue++;
            }

            if (playerTeams[i] == false)
            {
                countFalse++;
            }
        }

        bool newTeam = !playerTeams[player - 1];

        if ((newTeam && countTrue < 3) || (!newTeam && countFalse < 3))
        {
            playerTeams[player - 1] = !playerTeams[player - 1];
            TextTeam1[player - 1].SetActive(playerTeams[player - 1]);
            TextTeam2[player - 1].SetActive(!playerTeams[player - 1]);
        }
    }

    public void UpdateTeamUIPositions() {
        for(int i = 0; i< playerTeams.Length; i++) {
            TextTeam1[i].SetActive(playerTeams[i]);
            TextTeam2[i].SetActive(!playerTeams[i]);
        }
    }
}
