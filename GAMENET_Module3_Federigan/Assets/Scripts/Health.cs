using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviourPunCallbacks
{
    public int MaxHealth
    {
        get => _maxHealth;
    }

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;

    }

    public enum RaiseEventsCode
    {
        WhoGotEliminatedEventCode = 0,
    }

    [SerializeField] private int _maxHealth;
    [SerializeField] private float _currentHealth;

    public Image otherHealthBarImage;
    public Image myHealthBarImage;

    public GameObject otherHealthBar;
    public GameObject myHealthBar;

    private bool isDead;

    GameObject killList;

    PhotonMessageInfo deathInfo;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnDeath;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnDeath;
    }

    void Start()
    {
        _currentHealth = _maxHealth;

        otherHealthBarImage.fillAmount = _currentHealth / _maxHealth;

        otherHealthBar.SetActive(!photonView.IsMine);
        myHealthBar.SetActive(photonView.IsMine);

    }


    [PunRPC]
    public void TakeDamage(float damage, PhotonMessageInfo info)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (!photonView.IsMine)
        {
            otherHealthBarImage.fillAmount = _currentHealth / _maxHealth;
        }
        else
        {
            myHealthBarImage.fillAmount = _currentHealth / _maxHealth;
        }

        deathInfo = info;

     

        if (_currentHealth <= 0 && !isDead)
        {
            Die();

            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);

            killList = Instantiate(DeathRaceGameManager.instance.killListPrefab);
            DeathRaceGameManager.instance.killListParent = GameObject.Find("WhoKilledUIList");
            killList.transform.SetParent(DeathRaceGameManager.instance.killListParent.transform);
            killList.transform.localScale = Vector3.one;

            killList.transform.Find("KillListText").GetComponent<Text>().text = info.Sender.NickName + " has killed " + info.photonView.Owner.NickName;

            Destroy(killList, 5.0f);

            photonView.RPC("deadState", RpcTarget.All, true);

            DeathRaceGameManager.instance.PlayerEliminated(photonView.OwnerActorNr);
            DeathRaceGameManager.instance.CheckPlayers();

            GetComponent<Spectate>().leaveButton.SetActive(photonView.IsMine);
            GetComponent<Spectate>().spectateNext.SetActive(photonView.IsMine);
            GetComponent<Spectate>().spectatePrev.SetActive(photonView.IsMine);

            GetComponent<PlayerSetup>().RemoveCamera();

            GetComponent<Spectate>().SwitchCamera(0);

        }
    }

    [PunRPC]
    void deadState(bool deadState)
    {
        isDead = deadState;
    }

    void Die()
    {
        
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<VehicleMovement>().enabled = false;
        GetComponent<Shoot>().enabled = false;

        string nickName = photonView.Owner.NickName;
        int viewId = photonView.ViewID;

        //event data
        object[] data = new object[] { nickName, viewId };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.WhoGotEliminatedEventCode, data, raiseEventOptions, sendOption);

    }

    void OnDeath(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.WhoGotEliminatedEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string nickNameOfEliminatedPlayer = (string)data[0];
            int viewId = (int)data[1];


            if (viewId == photonView.ViewID)
            {

                killList.transform.Find("EliminatedText").GetComponent<Text>().text = nickNameOfEliminatedPlayer + " has been eliminated";
                killList.transform.Find("EliminatedText").GetComponent<Text>().color = Color.red;

            }
            else
            {
                Debug.Log(nickNameOfEliminatedPlayer + " has been eliminated");
          
            }

        }
    }
}
