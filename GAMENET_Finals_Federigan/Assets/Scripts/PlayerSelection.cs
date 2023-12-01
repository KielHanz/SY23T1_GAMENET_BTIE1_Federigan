using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
	public GameObject[] SelectablePlayers;

	public int playerSelectionNumber;

	private void Start()
	{
		playerSelectionNumber = 0;
		ActivatePlayer(playerSelectionNumber);
	}

	private void ActivatePlayer(int x)
	{
		GameObject[] selectablePlayers = SelectablePlayers;
		foreach (GameObject gameObject in selectablePlayers)
		{
			gameObject.SetActive(value: false);
		}
		SelectablePlayers[x].SetActive(value: true);
		Hashtable propertiesToSet = new Hashtable { { "playerSelectionNumber", playerSelectionNumber } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
	}

	public void goToNextPlayer()
	{
		playerSelectionNumber++;
		if (playerSelectionNumber >= SelectablePlayers.Length)
		{
			playerSelectionNumber = 0;
		}
		ActivatePlayer(playerSelectionNumber);
	}

	public void goToPrevPlayer()
	{
		playerSelectionNumber--;
		if (playerSelectionNumber < 0)
		{
			playerSelectionNumber = SelectablePlayers.Length - 1;
		}
		ActivatePlayer(playerSelectionNumber);
	}
}
