using Photon.Pun;
using UnityEngine;

public class Laser : MonoBehaviourPunCallbacks
{
	private LineRenderer lineLaser;

	public Transform laserPoint;

	public float damage;

	private void Start()
	{
		lineLaser = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		RaycastLaser();
	}

	private void RaycastLaser()
	{
		if (Physics.Raycast(laserPoint.transform.position, laserPoint.transform.forward, out var hitInfo, 50f))
		{
			lineLaser.SetPosition(0, laserPoint.position);
			lineLaser.SetPosition(1, hitInfo.point);
			lineLaser.enabled = true;
			if (hitInfo.collider.CompareTag("Player") && (bool)hitInfo.collider.gameObject.GetComponent<PhotonView>())
			{
				hitInfo.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 50f);
				Debug.Log("hit");
			}
		}
		else
		{
			lineLaser.SetPosition(0, laserPoint.position);
			lineLaser.SetPosition(1, laserPoint.transform.forward * 50f);
		}
	}

	[PunRPC]
	private void SetLineRendererState(bool state)
	{
		lineLaser.enabled = state;
	}

	[PunRPC]
	private void UpdateLineRenderer(Vector3 startPosition, Vector3 endPosition)
	{
		lineLaser.SetPosition(0, startPosition);
		lineLaser.SetPosition(1, endPosition);
	}
}
