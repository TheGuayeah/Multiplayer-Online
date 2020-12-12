using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawnerLocal : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemies;

    public void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            var spawnPosition = new Vector3(
            Random.Range(-8.0f, 8.0f),
            0.0f,
            Random.Range(-8.0f, 8.0f));
            var spawnRotation = Quaternion.Euler(
            0.0f,
            Random.Range(0, 180),
            0.0f);
            Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        }
    }
}
