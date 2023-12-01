using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField]
	private MoveSettings _settings = null;

	private Vector3 _moveDirection;

	private CharacterController _characterController;

	public Camera playerCamera;

	private float rotationX;

	private float rotationY;

	private bool isJumping;

	public Animator animator;

	[Header("Mouse Sensitivity")]
	public float mouseSensitivity;

	private void Start()
	{
		_characterController = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		DefaultMovement();
		MouseLook();
	}

	private void FixedUpdate()
	{
		_characterController.Move(_moveDirection * Time.deltaTime);
	}

	private void DefaultMovement()
	{
		if (_characterController.isGrounded)
		{
			Vector2 vector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (vector.x != 0f && vector.y != 0f)
			{
				vector *= 0.777f;
			}
			_moveDirection.x = vector.x * _settings.speed;
			_moveDirection.z = vector.y * _settings.speed;
			_moveDirection.y = 0f - _settings.antiBump;
			_moveDirection = base.transform.TransformDirection(_moveDirection);
			animator.SetBool("isGrounded", value: true);
			animator.SetBool("isJumping", value: false);
			animator.SetBool("isFalling", value: false);
			if (Input.GetKey(KeyCode.Space))
			{
				Jump();
			}
			animator.SetFloat("horizontal", vector.x);
			animator.SetFloat("vertical", vector.y);
		}
		else
		{
			_moveDirection.y -= _settings.gravity * Time.deltaTime;
			if ((isJumping && _moveDirection.y < 0f) || _moveDirection.y < -2f)
			{
				animator.SetBool("isFalling", value: true);
				animator.SetBool("isGrounded", value: false);
				animator.SetBool("isJumping", value: false);
			}
		}
	}

	private void Jump()
	{
		_moveDirection.y += _settings.jumpForce;
		animator.SetBool("isJumping", value: true);
		isJumping = true;
	}

	private void MouseLook()
	{
		rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
		rotationX = Mathf.Clamp(rotationX, -90f, 90f);
		playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		rotationY = Input.GetAxis("Mouse X") * mouseSensitivity;
		base.transform.rotation *= Quaternion.Euler(0f, rotationY, 0f);
	}
}
