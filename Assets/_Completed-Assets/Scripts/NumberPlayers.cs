using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NumberPlayers : NetworkBehaviour
{
    [SyncVar]
    public int currentNumberPlayers;

    private void Update()
    {
        currentNumberPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
    }
}
