using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable_Objects/Movement/Settings")]
public class MoveSettings : ScriptableObject
{
	[SerializeField]
	private float _speed = 5f;

	[SerializeField]
	private float _jumpForce = 13f;

	[SerializeField]
	private float _antiBump = 4.5f;

	[SerializeField]
	private float _gravity = 30f;

	public float speed
	{
		get
		{
			return _speed;
		}
		private set
		{
			_speed = value;
		}
	}

	public float jumpForce
	{
		get
		{
			return _jumpForce;
		}
		private set
		{
			_jumpForce = value;
		}
	}

	public float antiBump
	{
		get
		{
			return _antiBump;
		}
		private set
		{
			_antiBump = value;
		}
	}

	public float gravity
	{
		get
		{
			return _gravity;
		}
		private set
		{
			_gravity = value;
		}
	}
}
