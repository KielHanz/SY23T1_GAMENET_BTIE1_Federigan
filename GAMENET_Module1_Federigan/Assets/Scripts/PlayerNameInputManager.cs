using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerNameInputManager : MonoBehaviour
{
    public InputField inputName;
    string playerName;

    public void SetPlayerName()
    {
        playerName = inputName.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Player name is empty");
            return;
        }

        PhotonNetwork.NickName = playerName;
    }
}
