using System.Collections.Generic;
using UnityEngine;

public class UpdateCheckpoint : MonoBehaviour
{
	public List<GameObject> checkpoints = new List<GameObject>();

	private void Start()
	{
		foreach (GameObject checkpoint in GameManager.instance.checkpoints)
		{
			checkpoints.Add(checkpoint);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (checkpoints.Contains(other.gameObject))
		{
			int index = checkpoints.IndexOf(other.gameObject);
			Debug.Log("entered");
			if ((bool)base.transform.GetComponent<PlayerSetup>())
			{
				base.transform.GetComponent<PlayerSetup>().currentCheckpoint = checkpoints[index];
			}
		}
	}
}
