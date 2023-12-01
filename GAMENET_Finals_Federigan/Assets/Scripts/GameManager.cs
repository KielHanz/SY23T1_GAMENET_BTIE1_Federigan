using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	public GameObject[] playerCharacters;

	public Transform[] startingPositions;

	public List<GameObject> checkpoints = new List<GameObject>();

	public GameObject diedListPrefab;

	public GameObject diedListParent;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("playerSelectionNumber", out var value))
		{
			int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			Vector3 position = startingPositions[actorNumber - 1].position;
			PhotonNetwork.Instantiate(playerCharacters[(int)value].name, position, Quaternion.identity, 0);
		}
	}

	private void Update()
	{
	}
}
