using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemInitializer : MonoBehaviour
{
	[Header("UI References")]
	public Text PlayerNameText;

	public Button PlayerReadyButton;

	public Image PlayerReadyImage;

	private bool isPlayerReady = false;

	public void Initialize(int playerId, string playerName)
	{
		PlayerNameText.text = playerName;
		if (PhotonNetwork.LocalPlayer.ActorNumber != playerId)
		{
			PlayerReadyButton.gameObject.SetActive(value: false);
			return;
		}
		Hashtable propertiesToSet = new Hashtable { { "isPlayerReady", isPlayerReady } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
		PlayerReadyButton.onClick.AddListener(delegate
		{
			isPlayerReady = !isPlayerReady;
			SetPlayerReady(isPlayerReady);
			Hashtable propertiesToSet2 = new Hashtable { { "isPlayerReady", isPlayerReady } };
			PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet2);
		});
	}

	public void SetPlayerReady(bool playerReady)
	{
		PlayerReadyImage.enabled = playerReady;
		if (playerReady)
		{
			PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
		}
		else
		{
			PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready?";
		}
	}
}
