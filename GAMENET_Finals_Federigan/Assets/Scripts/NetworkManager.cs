using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	[Header("Login UI")]
	public GameObject LoginUIPanel;

	public InputField PlayerNameInput;

	[Header("Connecting Info Panel")]
	public GameObject ConnectingInfoUIPanel;

	[Header("Creating Room Info Panel")]
	public GameObject CreatingRoomInfoUIPanel;

	[Header("GameOptions  Panel")]
	public GameObject GameOptionsUIPanel;

	[Header("Create Room Panel")]
	public GameObject CreateRoomUIPanel;

	public InputField RoomNameInputField;

	[Header("Inside Room Panel")]
	public GameObject InsideRoomUIPanel;

	public Text RoomInfoText;

	public GameObject PlayerListPrefab;

	public GameObject PlayerListParent;

	public GameObject StartGameButton;

	public Toggle toggleBlueCharacter;

	public Toggle toggleRedCharacter;

	public GameObject InsufficientPlayersText;

	[Header("Join Random Room Panel")]
	public GameObject JoinRandomRoomUIPanel;

	public int playerSelectionNumber;

	private Dictionary<int, GameObject> playerListGameObjects;

	private void Start()
	{
		ActivatePanel(LoginUIPanel.name);
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public void OnLoginButtonClicked()
	{
		string text = PlayerNameInput.text;
		if (!string.IsNullOrEmpty(text))
		{
			ActivatePanel(ConnectingInfoUIPanel.name);
			if (!PhotonNetwork.IsConnected)
			{
				PhotonNetwork.LocalPlayer.NickName = text;
				PhotonNetwork.ConnectUsingSettings();
			}
		}
		else
		{
			Debug.Log("PlayerName is invalid!");
		}
	}

	public void OnCancelButtonClicked()
	{
		ActivatePanel(GameOptionsUIPanel.name);
	}

	public void OnCreateRoomButtonClicked()
	{
		ActivatePanel(CreatingRoomInfoUIPanel.name);
		string text = RoomNameInputField.text;
		if (string.IsNullOrEmpty(text))
		{
			text = "Room " + Random.Range(1000, 10000);
		}
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 2;
		PhotonNetwork.CreateRoom(text, roomOptions);
	}

	public void OnJoinRandomRoomClicked()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public void OnBackButtonClicked()
	{
		ActivatePanel(GameOptionsUIPanel.name);
	}

	public void OnLeaveGameButtonClicked()
	{
		PhotonNetwork.LeaveRoom();
	}

	public void OnStartGameButtonClicked()
	{
		Debug.Log("loading level");
		PhotonNetwork.LoadLevel("GameScene");
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.CustomProperties.TryGetValue("playerSelectionNumber", out var value))
			{
				Debug.Log(player.NickName + " :" + (int)value);
			}
		}
		PhotonNetwork.CurrentRoom.IsOpen = false;
	}

	public override void OnConnected()
	{
		Debug.Log("Connected to Internet");
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
		ActivatePanel(GameOptionsUIPanel.name);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log(PhotonNetwork.CurrentRoom?.ToString() + " has been created");
	}

	public override void OnJoinedRoom()
	{
		Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined " + PhotonNetwork.CurrentRoom.Name);
		Debug.Log("Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
		ActivatePanel(InsideRoomUIPanel.name);
		RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
		if (playerListGameObjects == null)
		{
			playerListGameObjects = new Dictionary<int, GameObject>();
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			GameObject gameObject = Object.Instantiate(PlayerListPrefab);
			gameObject.transform.SetParent(PlayerListParent.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.GetComponent<PlayerListItemInitializer>().Initialize(player.ActorNumber, player.NickName);
			if (player.CustomProperties.TryGetValue("isPlayerReady", out var value))
			{
				gameObject.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool)value);
			}
			playerListGameObjects.Add(player.ActorNumber, gameObject);
			SetToggleCharacter();
		}
		Hashtable propertiesToSet = new Hashtable { { "playerSelectionNumber", playerSelectionNumber } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
		StartGameButton.SetActive(value: false);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		GameObject gameObject = Object.Instantiate(PlayerListPrefab);
		gameObject.transform.SetParent(PlayerListParent.transform);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponent<PlayerListItemInitializer>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);
		playerListGameObjects.Add(newPlayer.ActorNumber, gameObject);
		RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
		StartGameButton.SetActive(CheckAllPlayerReady());
		SetToggleCharacter();
		Hashtable propertiesToSet = new Hashtable { { "playerSelectionNumber", playerSelectionNumber } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Object.Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
		playerListGameObjects.Remove(otherPlayer.ActorNumber);
		RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
	}

	public override void OnLeftRoom()
	{
		ActivatePanel(GameOptionsUIPanel.name);
		foreach (GameObject value in playerListGameObjects.Values)
		{
			Object.Destroy(value);
		}
		playerListGameObjects.Clear();
		playerListGameObjects = null;
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log(message);
		string text = RoomNameInputField.text;
		if (string.IsNullOrEmpty(text))
		{
			text = "Room " + Random.Range(1000, 10000);
		}
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 2;
		PhotonNetwork.CreateRoom(text, roomOptions);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber, out var value) && changedProps.TryGetValue("isPlayerReady", out var value2))
		{
			value.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool)value2);
			if ((bool)value2 && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
			{
				InsufficientPlayersText.SetActive(value: true);
			}
			else
			{
				InsufficientPlayersText.SetActive(value: false);
			}
		}
		if (PhotonNetwork.InRoom)
		{
			int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
			if (PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayers)
			{
				StartGameButton.SetActive(CheckAllPlayerReady());
			}
		}
		Debug.Log(playerSelectionNumber);
		SetToggleCharacter();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
		{
			StartGameButton.SetActive(CheckAllPlayerReady());
		}
	}

	public void ActivatePanel(string panelNameToBeActivated)
	{
		LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
		ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
		CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
		CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
		GameOptionsUIPanel.SetActive(GameOptionsUIPanel.name.Equals(panelNameToBeActivated));
		JoinRandomRoomUIPanel.SetActive(JoinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
		InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
	}

	public void SetCharacter(int playerSelectionNum)
	{
		playerSelectionNumber = playerSelectionNum;
		Hashtable propertiesToSet = new Hashtable { { "playerSelectionNumber", playerSelectionNumber } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
	}

	private bool CheckAllPlayerReady()
	{
		if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
		{
			return false;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.CustomProperties.TryGetValue("isPlayerReady", out var value))
			{
				if (!(bool)value)
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private void SetToggleCharacter()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			toggleBlueCharacter.interactable = true;
			toggleRedCharacter.interactable = true;
			return;
		}
		toggleBlueCharacter.interactable = false;
		toggleRedCharacter.interactable = false;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.CustomProperties.TryGetValue("playerSelectionNumber", out var value))
			{
				if ((int)value == 0)
				{
					toggleRedCharacter.isOn = true;
				}
				else
				{
					toggleBlueCharacter.isOn = true;
				}
				break;
			}
		}
	}
}
