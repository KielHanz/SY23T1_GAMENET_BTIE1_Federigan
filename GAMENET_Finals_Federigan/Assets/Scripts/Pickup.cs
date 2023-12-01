using Photon.Pun;
using UnityEngine;

public class Pickup : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private LayerMask pickupMask;

	[SerializeField]
	private Transform pickupTarget;

	[SerializeField]
	private float pickupRange;

	private GameObject currentObject;

	private Camera playerCamera;

	private bool isPickedUp;

	private void Start()
	{
	}

	private void Update()
	{
		if (playerCamera == null)
		{
			playerCamera = GetComponent<PlayerSetup>().playerCamera;
		}
		if (base.photonView.IsMine && Input.GetKeyDown(KeyCode.Mouse0))
		{
			base.photonView.RPC("PickUp", RpcTarget.All);
		}
		else if (base.photonView.IsMine && Input.GetKeyUp(KeyCode.Mouse0))
		{
			base.photonView.RPC("ReleaseObject", RpcTarget.All);
		}
	}

	private void FixedUpdate()
	{
		if (base.photonView.IsMine && currentObject != null)
		{
			base.photonView.RPC("MoveObject", RpcTarget.AllBuffered);
		}
	}

	[PunRPC]
	private void MoveObject()
	{
		if ((bool)currentObject)
		{
			Vector3 vector = pickupTarget.position - currentObject.transform.position;
			float magnitude = vector.magnitude;
			currentObject.transform.GetComponent<Rigidbody>().velocity = vector * 12f * magnitude;
		}
	}

	[PunRPC]
	private void PickUp()
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(ray, out var hitInfo, pickupRange, pickupMask))
		{
			base.photonView.RPC("SetCurrentObject", RpcTarget.All, hitInfo.transform.GetComponent<PhotonView>().ViewID);
			base.photonView.RPC("SetCurrentObjectGravity", RpcTarget.All, false);
		}
	}

	[PunRPC]
	private void SetCurrentObject(int viewID)
	{
		currentObject = PhotonNetwork.GetPhotonView(viewID).gameObject;
	}

	[PunRPC]
	private void SetCurrentObjectGravity(bool state)
	{
		currentObject.transform.GetComponent<Rigidbody>().useGravity = state;
	}

	[PunRPC]
	private void ReleaseObject()
	{
		if ((bool)currentObject)
		{
			currentObject.transform.GetComponent<Rigidbody>().useGravity = true;
			currentObject = null;
		}
	}

	[PunRPC]
	private void SetIsPickedUp(bool state)
	{
		isPickedUp = state;
	}
}
