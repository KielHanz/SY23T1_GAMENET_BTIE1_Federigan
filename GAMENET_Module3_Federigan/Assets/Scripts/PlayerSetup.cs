using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public Text playerName;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;
            GetComponent<LapController>().enabled = photonView.IsMine;
            camera.enabled = photonView.IsMine;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;
            camera.enabled = photonView.IsMine;

            DeathRaceGameManager.instance.cameras.Add(camera);
            playerName.text = photonView.Owner.NickName;

        }
    }

    public void RemoveCamera()
    {
        DeathRaceGameManager.instance.cameras.Remove(camera);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        { 
            if (DeathRaceGameManager.instance.isLastPlayer)
            {
                GetComponent<Spectate>().leaveButton.SetActive(photonView.IsMine);
            }
        }
    }
}
