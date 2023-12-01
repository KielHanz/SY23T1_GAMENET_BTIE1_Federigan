using UnityEngine;

public class DoorSystem : MonoBehaviour
{
	public GameObject door;

	private bool doorOpen;

	private Vector3 originalDoorPos;

	[SerializeField]
	private bool moveUp;

	[SerializeField]
	private bool moveSide;

	[SerializeField]
	private float distanceToReach;

	private void Start()
	{
		originalDoorPos = door.transform.position;
	}

	private void Update()
	{
		if (moveUp)
		{
			if (doorOpen && door.transform.position.y <= originalDoorPos.y + distanceToReach)
			{
				door.transform.position += new Vector3(0f, 2f * Time.deltaTime, 0f);
			}
			else if (!doorOpen && door.transform.position.y >= originalDoorPos.y)
			{
				door.transform.position -= new Vector3(0f, 2f * Time.deltaTime, 0f);
			}
		}
		if (moveSide)
		{
			if (doorOpen && door.transform.position.z <= originalDoorPos.z + distanceToReach)
			{
				door.transform.position += new Vector3(0f, 0f, 2f * Time.deltaTime);
			}
			else if (!doorOpen && door.transform.position.z >= originalDoorPos.z)
			{
				door.transform.position -= new Vector3(0f, 0f, 2f * Time.deltaTime);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.gameObject.GetComponent<Rigidbody>())
		{
			doorOpen = true;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if ((bool)collision.gameObject.GetComponent<Rigidbody>())
		{
			doorOpen = false;
		}
	}
}
