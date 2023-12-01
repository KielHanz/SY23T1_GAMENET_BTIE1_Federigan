using Photon.Pun;
using UnityEngine;

public class ColoredBlocks : MonoBehaviour
{
	private enum BlockColor
	{
		RED = 0,
		BlUE = 1
	}

	[SerializeField]
	private BlockColor blockColor;

	private void OnCollisionEnter(Collision collision)
	{
		if (!collision.gameObject.GetComponent<PlayerSetup>())
		{
			return;
		}
		if (blockColor == BlockColor.RED)
		{
			if (collision.gameObject.GetComponent<PlayerSetup>().playerColor == PlayerSetup.PlayerColor.BLUE)
			{
				collision.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 100f);
				Debug.Log("blue damaged");
			}
		}
		else if (blockColor == BlockColor.BlUE && collision.gameObject.GetComponent<PlayerSetup>().playerColor == PlayerSetup.PlayerColor.RED)
		{
			collision.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 100f);
			Debug.Log("red damaged");
		}
	}
}
