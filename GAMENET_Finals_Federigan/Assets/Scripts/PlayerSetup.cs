using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
	public enum PlayerColor
	{
		BLUE = 0,
		RED = 1
	}

	public PlayerColor playerColor;

	public GameObject fpModel;

	public GameObject nonFpModel;

	public Camera playerCamera;

	public GameObject currentCheckpoint;

	private void Start()
	{
		fpModel.SetActive(base.photonView.IsMine);
		nonFpModel.SetActive(!base.photonView.IsMine);
		GetComponent<PlayerMovement>().enabled = base.photonView.IsMine;
		playerCamera.enabled = base.photonView.IsMine;
		currentCheckpoint = GameManager.instance.checkpoints[0];
	}

	private void Update()
	{
	}
}
