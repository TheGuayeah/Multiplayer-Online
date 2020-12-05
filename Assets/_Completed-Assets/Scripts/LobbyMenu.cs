using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using UnityEngine.UI;
using PlayFab;
using UnityEngine.SceneManagement;

public class ServerMenuData {
    public ServerMenuData(ServerResponse response, int id, GameObject menuId) {
        this.response = response;
        this.id = id;
        this.menuId = menuId;
    }

    public ServerResponse response;
    public int id;
    public GameObject menuId;
}

public class LobbyMenu : MonoBehaviour {
    private NetworkManager networkManager;
    private NetworkDiscovery networkDiscovery;
    readonly Dictionary<long, ServerMenuData> discoveredServersList = new Dictionary<long, ServerMenuData>();

    public string serverIP = "localhost";
    public InputField nickName;                         // Value of the nickname

    public GameObject serverListUI;
    public GameObject serverUISpawner;
    public GameObject serverUIRow;

    public Image colorButton;

    public static string[] tankColors = { "red", "black", "white", "blue", "green", "yellow" };
    int currentColorIdx;

    void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        networkDiscovery = FindObjectOfType<NetworkDiscovery>();
        currentColorIdx = Random.Range(0, tankColors.Length - 1);
        Color tmpColor;
        ColorUtility.TryParseHtmlString(tankColors[currentColorIdx], out tmpColor);
        colorButton.color = tmpColor;
    }

    public void ChangeColor() {
        currentColorIdx = (currentColorIdx + 1) % tankColors.Length;
        Color tmpColor;
        ColorUtility.TryParseHtmlString(tankColors[currentColorIdx], out tmpColor);
        colorButton.color = tmpColor;
    }

    public void CreateGame() {
        if (!NetworkClient.isConnected && !NetworkServer.active) {
            if (!NetworkClient.active) {
                SetNickName();
                SetColor();
                networkManager.StartHost();
                networkDiscovery.AdvertiseServer();
            }
        }
    }

    public void FindServer() {
        ShowServerList(true);
        networkDiscovery.StartDiscovery();
    }

    public void JoinGame(string uri) {
        if (!NetworkClient.isConnected && !NetworkServer.active) {
            if (!NetworkClient.active) {
                SetNickName();
                SetColor();
                networkManager.networkAddress = uri;
                networkManager.StartClient();
            }
        }
    }

    public void RunServer()
    {
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (!NetworkClient.active)
            {
                SetNickName();
                networkManager.StartServer();
                networkDiscovery.AdvertiseServer();
            }
        }
    }

    public void AddressData()
    {
        if (NetworkServer.active)
        {
            Debug.Log("Server: active. IP: " + networkManager.networkAddress + " - Transport: " + Transport.activeTransport);
        }
        else
        {
            Debug.Log("Attempted to join server " + serverIP);
        }
        Debug.Log("Local IP Address: " + GetLocalIPAddress());
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void SetNickName()
    {
        if(nickName.text.Equals(""))
        {
            PlayerPrefs.SetString("NickName", "Player");
        }
        else
        {
            PlayerPrefs.SetString("NickName", nickName.text);
        }
    }

    public void SetColor() {
        PlayerPrefs.SetInt("Color", currentColorIdx);
    }

    public void OnDiscoveredServer(ServerResponse info) {
        int i = discoveredServersList.Count;
        if (!discoveredServersList.ContainsKey(info.serverId)) {
            var server = Instantiate(serverUIRow, serverUISpawner.transform);
            server.GetComponentsInChildren<Text>()[0].text = info.EndPoint.Address.ToString();
            server.GetComponentInChildren<Button>().onClick.AddListener(delegate { JoinGame(info.EndPoint.Address.ToString()); });
            RectTransform rt = server.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 15 + (40 * i), rt.rect.height);
            discoveredServersList.Add(info.serverId, new ServerMenuData(info, i, server));
        }
    }

    public void ShowServerList(bool show) {
        serverListUI.SetActive(show);
        if (!show) {
            networkDiscovery.StopDiscovery();
        }
    }

    public void Back()
    {
        SceneManager.LoadScene("SelectGameMode");
    }
}
