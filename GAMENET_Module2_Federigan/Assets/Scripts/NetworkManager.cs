using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status Panel")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject loginUiPanel;

    [Header("Game Options Panel")]
    public GameObject gameOptionsPanel;

    [Header("Create Room Panel")]
    public GameObject createRoomPanel;
    public InputField roomNameInputField;
    public InputField playerCountInputField;

    [Header("Join Rnadom Room Panel")]
    public GameObject joinRandomRoomPanel;

    [Header("Show Room List Panel")]
    public GameObject showRoomListPanel;

    [Header("Inside Room Panel")]
    public GameObject insideRoomPanel;
    public Text roomInfoText;
    public GameObject playerListItemPrefab;
    public GameObject playerListViewParent;
    public GameObject startGameButton;

    [Header("Room List Panel")]
    public GameObject roomListPanel;
    public GameObject roomItemPrefab;
    public GameObject roomListParent;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;

    #region Unity Functions
    // Start is called before the first frame update
    void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        ActivatePanel(loginUiPanel);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }

    #endregion

    //functions being called through the listeners of buttons in game
    #region UI Callbacks

    //create player username
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player name is invalid");
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    //creation of room with room name and max player count
    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(playerCountInputField.text);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    //return to game options panel once cancel button is clicked
    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsPanel);
    }

    // show the room list
    public void OnShowRoomListButtonClicked()
    {
        //just to make sure the player is in lobby
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(showRoomListPanel);
    }

    //return to game options panel once back button is clicked
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(gameOptionsPanel);
    }

    //leave the room
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    //join a random room
    public void OnJoinRandomRoomClicked()
    {
        ActivatePanel(joinRandomRoomPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    #endregion

    //built in functions of Photon
    #region PUN Callbacks

    //check if player is connected
    public override void OnConnected()
    {
        Debug.Log("Connected to the internet!");
    }

    //check if player is connected to photon server
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has connected to the Photon Servers!");
        ActivatePanel(gameOptionsPanel);
    }

    //if room is created then..
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " created!");
    }

    //if player joins the room then...
    public override void OnJoinedRoom()
    {

        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(insideRoomPanel);

        //a checker... if they are the host of the room then give them a start game button
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        //create the player list prefab
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab);
            playerItem.transform.SetParent(playerListViewParent.transform);
            playerItem.transform.localScale = Vector3.one;

            playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

            //populate player list
            playerListGameObjects.Add(player.ActorNumber, playerItem);
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListGameObjects();
        Debug.Log("OnRoomListUpdate Called");

        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        foreach (RoomInfo info in roomList)
        {

            Debug.Log(info.Name);

            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }

        }

        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject listItem = Instantiate(roomItemPrefab);
            listItem.transform.SetParent(roomListParent.transform);
            listItem.transform.localScale = Vector3.one;

            if (info.PlayerCount == info.MaxPlayers)
            {
                listItem.transform.Find("JoinRoomButton").GetComponent<Button>().gameObject.SetActive(false);
            }

            listItem.transform.Find("RoomNameText").GetComponent<Text>().text = info.Name;
            listItem.transform.Find("RoomPlayersText").GetComponent<Text>().text = "Player count: " + info.PlayerCount + "/" + info.MaxPlayers;
            listItem.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinedRoomClicked(info.Name));

            roomListGameObjects.Add(info.Name, listItem);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListGameObjects();
        cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerItem = Instantiate(playerListItemPrefab);
        playerItem.transform.SetParent(playerListViewParent.transform);
        playerItem.transform.localScale = Vector3.one;

        playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

        playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        playerListGameObjects.Add(player.ActorNumber, playerItem);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(playerListGameObjects[otherPlayer.ActorNumber]);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {

        foreach (var gameObject in playerListGameObjects.Values)
        {
            Destroy(gameObject);
        }
        playerListGameObjects.Clear();
        playerListGameObjects = null;

        ActivatePanel(gameOptionsPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);

        string roomName = "Room " + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    #endregion

    #region Public Methods

    public void ActivatePanel(GameObject panelToBeActivated)
    {
        loginUiPanel.SetActive(panelToBeActivated.Equals(loginUiPanel));
        gameOptionsPanel.SetActive(panelToBeActivated.Equals(gameOptionsPanel));
        createRoomPanel.SetActive(panelToBeActivated.Equals(createRoomPanel));
        joinRandomRoomPanel.SetActive(panelToBeActivated.Equals(joinRandomRoomPanel));
        showRoomListPanel.SetActive(panelToBeActivated.Equals(showRoomListPanel));
        insideRoomPanel.SetActive(panelToBeActivated.Equals(insideRoomPanel));
        roomListPanel.SetActive(panelToBeActivated.Equals(roomListPanel));
    }
    #endregion

    #region Private Methods

    private void OnJoinedRoomClicked(string roomName)
    {

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListGameObjects()
    {
        foreach (var item in roomListGameObjects.Values)
        {
            Destroy(item);
        }

        roomListGameObjects.Clear();
    }

    #endregion
}
