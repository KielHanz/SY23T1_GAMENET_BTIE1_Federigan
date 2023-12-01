using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviourPunCallbacks
{
	public enum RaiseEventsCode
	{
		WhoGotEliminatedEventCode = 0
	}

	[SerializeField]
	private int _maxHealth;

	[SerializeField]
	private float _currentHealth;

	private bool isDead;

	private GameObject diedList;

	private PhotonMessageInfo deathInfo;

	public int MaxHealth => _maxHealth;

	public float CurrentHealth
	{
		get
		{
			return _currentHealth;
		}
		set
		{
			_currentHealth = value;
		}
	}

	private new void OnEnable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += OnDeath;
	}

	private new void OnDisable()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= OnDeath;
	}

	private void Start()
	{
		_currentHealth = _maxHealth;
	}

	private IEnumerator RespawnCountdown()
	{
		float respawnTime = 5f;
		while (respawnTime > 0f)
		{
			yield return new WaitForSeconds(1f);
			respawnTime -= 1f;
			base.transform.GetComponent<PlayerMovement>().enabled = false;
		}
		base.transform.GetComponent<PlayerMovement>().animator.SetBool("isDead", value: false);
		base.transform.position = base.transform.GetComponent<PlayerSetup>().currentCheckpoint.transform.position;
		base.transform.GetComponent<PlayerMovement>().enabled = true;
		base.transform.GetComponent<CharacterController>().enabled = true;
		base.transform.GetComponent<Collider>().enabled = true;
		base.photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
	}

	[PunRPC]
	public void TakeDamage(float damage)
	{
		_currentHealth -= damage;
		_currentHealth = Mathf.Max(_currentHealth, 0f);
		if (_currentHealth <= 0f && !isDead)
		{
			Die();
			base.photonView.RPC("deadState", RpcTarget.All, true);
		}
	}

	[PunRPC]
	public void RegainHealth()
	{
		_currentHealth = _maxHealth;
		isDead = false;
	}

	[PunRPC]
	private void deadState(bool deadState)
	{
		isDead = deadState;
	}

	private void Die()
	{
		string nickName = base.photonView.Owner.NickName;
		int viewID = base.photonView.ViewID;
		object[] eventContent = new object[2] { nickName, viewID };
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			Receivers = ReceiverGroup.Others,
			CachingOption = EventCaching.AddToRoomCache
		};
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = false;
		SendOptions sendOptions2 = sendOptions;
		PhotonNetwork.RaiseEvent(0, eventContent, raiseEventOptions, sendOptions2);
		GetComponent<PlayerMovement>().animator.SetBool("isDead", value: true);
		diedList = Object.Instantiate(GameManager.instance.diedListPrefab);
		GameManager.instance.diedListParent = GameObject.Find("DiedList");
		diedList.transform.SetParent(GameManager.instance.diedListParent.transform);
		diedList.transform.localScale = Vector3.one;
		diedList.transform.Find("DiedText").GetComponent<Text>().text = nickName + " Died!";
		Object.Destroy(diedList, 5f);
	}

	private void OnDeath(EventData photonEvent)
	{
		if (photonEvent.Code != 0)
		{
			return;
		}
		object[] array = (object[])photonEvent.CustomData;
		string text = (string)array[0];
		int num = (int)array[1];
		if (num == base.photonView.ViewID)
		{
			base.transform.GetComponent<CharacterController>().enabled = false;
			base.transform.GetComponent<Collider>().enabled = false;
			if (base.photonView.IsMine)
			{
				StartCoroutine(RespawnCountdown());
			}
			Debug.Log(text + " has been eliminated");
		}
		else
		{
			Debug.Log(text + " has been eliminated");
		}
	}
}
