using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spectate : MonoBehaviourPunCallbacks
{
    public GameObject leaveButton;
    public GameObject spectatePrev;
    public GameObject spectateNext;

    private int cameraIndex;

    private DeathRaceGameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        leaveButton.SetActive(false);
        spectatePrev.SetActive(false);
        spectateNext.SetActive(false);

        gameManager = DeathRaceGameManager.instance;

    }

    public void OnClickLeaveGameButton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }


    public void SwitchCamera(int x)
    {
        foreach (Camera cam in gameManager.cameras)
        {
           cam.enabled = false;
        }
        this.GetComponent<PlayerSetup>().camera = gameManager.cameras[x];
        gameManager.cameras[x].enabled = true;

    }

    public void OnClickPrevSpectate()
    {
        cameraIndex--;

        if (cameraIndex < 0)
        {
            cameraIndex = gameManager.cameras.Count - 1;
        }

        SwitchCamera(cameraIndex);
    }

    public void OnClickNextSpectate()
    {
        cameraIndex++;

        if (cameraIndex >= gameManager.cameras.Count)
        {
            cameraIndex = 0;
        }

        SwitchCamera(cameraIndex);

    }
}
