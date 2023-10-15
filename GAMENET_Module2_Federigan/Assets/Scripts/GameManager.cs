using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject loadingPanel;

    public static GameManager Instance;

    public bool gameOver;

    public int killsToWin = 10;

    public Text winnerText;

    public List<GameObject> spawnPoints = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        gameOver = false;

        if (PhotonNetwork.IsConnected)
        {
            loadingPanel.SetActive(true);
            StartCoroutine(DelayedPlayerSpawn());
        }
    }

    private void Update()
    {
        if (gameOver && PhotonNetwork.InRoom)
        {
            StartCoroutine(DisplayBeforeEnd());
        }
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    IEnumerator DelayedPlayerSpawn()
    {
        yield return new WaitForSeconds(3);
        int randomSpawnPoint = Random.Range(0, spawnPoints.Count);
        
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[randomSpawnPoint].transform.position, Quaternion.identity);
        loadingPanel.SetActive(false);

    }

    IEnumerator DisplayBeforeEnd()
    {
        yield return new WaitForSeconds(5);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
        }
        
    }


}
