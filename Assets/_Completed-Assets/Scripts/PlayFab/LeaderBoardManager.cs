using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class LeaderBoardManager : MonoBehaviour {

    public GameObject leaderboardPanel;
    public GameObject listingPrefab;
    public Transform listingContainer;

    public void GetLeaderboard() {
        var requestLeaderBoard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerHighScore", MaxResultsCount = 20 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderBoard, OnGetLeaderBoard, OnErrorLeaderBoard);
    }

    public void AddPlayerScoreToLeaderBoard() {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerHighScore", Value = 100 }
            }
        },
        result => { Debug.Log("User leaderboard updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void OnGetLeaderBoard(GetLeaderboardResult result) {
        leaderboardPanel.SetActive(true);
        foreach (PlayerLeaderboardEntry player in result.Leaderboard) {
            GameObject tempListing = Instantiate(listingPrefab, listingContainer);
            LeaderBoardListing ll = tempListing.GetComponent<LeaderBoardListing>();
            ll.playerPositionText.text = (player.Position+1).ToString();
            ll.playerNameText.text = player.DisplayName;
            ll.playerScoreText.text = player.StatValue.ToString();
        }
    }

    public void CloseLeaderBoard() {
        leaderboardPanel.SetActive(false);
        for (int i = listingContainer.childCount-1; i>= 0; i--) {
            Destroy(listingContainer.GetChild(i).gameObject);
        }
    }

    void OnErrorLeaderBoard(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

}
