using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeathRaceGameManager : MonoBehaviourPunCallbacks
{
    public GameObject[] vehiclePrefabs;
    public Transform[] startingPositions;

    public Text timeText;

    public static DeathRaceGameManager instance = null;

    public GameObject killListPrefab;
    public GameObject killListParent;

    public List<Camera> cameras = new List<Camera>();

    public List<int> playerAlive = new List<int>();

    public Text winnerName;

    public bool isLastPlayer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            object playerSelectionNumber;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out playerSelectionNumber))
            {
                Debug.Log((int)playerSelectionNumber);

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                Vector3 instantiatePosition = startingPositions[actorNumber - 1].position;
                Quaternion instantiateRotation = startingPositions[actorNumber - 1].rotation;
                PhotonNetwork.Instantiate(vehiclePrefabs[(int)playerSelectionNumber].name, instantiatePosition, instantiateRotation);

                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    playerAlive.Add(player.ActorNumber);
                }
            }
        }
    }

    public void CheckPlayers()
    {
        Debug.Log(playerAlive.Count);
        if (playerAlive.Count <= 1)
        {
            int lastPlayerId = playerAlive[0];
            string lastPlayerName = PhotonNetwork.CurrentRoom.GetPlayer(lastPlayerId).NickName;

            Debug.Log(lastPlayerName + " is the last man standing!");
            winnerName.gameObject.SetActive(true);
            winnerName.text = lastPlayerName + " is the last man standing!";
            isLastPlayer = true;
        }
    }

    public void PlayerEliminated(int playerId)
    {
        playerAlive.Remove(playerId);
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
