using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    public Joystick joystick;
    public FixedTouchField fixedTouchField;

    private RigidbodyFirstPersonController rigidbodyFirstPersonController;

    private Animator animator;

    // Start is called before the first frame update
    private void Start()
    {
        rigidbodyFirstPersonController = this.GetComponent<RigidbodyFirstPersonController>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rigidbodyFirstPersonController.joystickInputAxis.x = joystick.Horizontal;
        rigidbodyFirstPersonController.joystickInputAxis.y = joystick.Vertical;

        rigidbodyFirstPersonController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;

        animator.SetFloat("horizontal", joystick.Horizontal);
        animator.SetFloat("vertical", joystick.Vertical);

        if (Mathf.Abs(joystick.Horizontal) > 0.9f || Mathf.Abs(joystick.Vertical) > 0.9f)
        {
            animator.SetBool("isRunning", true);
            rigidbodyFirstPersonController.movementSettings.ForwardSpeed = 10;
        }
        else
        {
            animator.SetBool("isRunning", false);
            rigidbodyFirstPersonController.movementSettings.ForwardSpeed = 5;
        }
    }
}
