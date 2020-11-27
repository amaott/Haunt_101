using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float speed = 12f;
	public float gravity = -9.81f;

	bool doJump = false;

	public Transform groundCheck;
	public LayerMask groundMask;

	public float groundDistance = 0.4f;

	[SerializeField] float minJumpInterval = 0.25f;
	[SerializeField] float jumpHeight = 1f;
	[SerializeField] readonly float walkScale = 0.33f;

	[SerializeField] Animator animator;
	[SerializeField] CharacterController controller;

	float jumpTimeStamp = 0;
	float currentX = 0;
	float currentZ = 0;
	readonly float interpolation = 10;
	Vector3 currentDirection = Vector3.zero;

	Vector3 velocity;
	/// <summary>
	/// Assigns the attached gameObject's Animator and Rigidbody compenent values when not provided.
	/// </summary>
	private void Awake()
	{
		if (!animator) animator = gameObject.GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
		/* Update Animator States */
		animator.SetBool("Grounded", isGrounded);

		if (!doJump && Input.GetKey(KeyCode.Space))
		{
			doJump = true;
		}
	}
	

	bool wasGrounded;
	bool isGrounded;
	private void FixedUpdate()
	{

		/* Update movement */
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		if (Input.GetKey(KeyCode.LeftShift))
		{
			x *= walkScale;
			z *= walkScale;
		}

		Transform camera = Camera.main.transform;

		Vector3 move = transform.right * x + transform.forward * z;

		var motion = move * speed * Time.deltaTime;
		controller.Move(motion);
		
		velocity.y += gravity * Time.deltaTime;

		currentX = Mathf.Lerp(currentX, x, Time.deltaTime * interpolation);
		currentZ = Mathf.Lerp(currentZ, z, Time.deltaTime * interpolation);

		/* Update Animator States and reset Input member values */
		Vector3 direction = camera.forward * currentZ + camera.right * currentX;

		float directionLength = direction.magnitude;
		direction.y = 0;
		direction = direction.normalized * directionLength;

		if (direction != Vector3.zero)
		{
			currentDirection = Vector3.Slerp(currentDirection, direction, Time.deltaTime * interpolation);
			
			bool isMovingBackwards = currentZ < 0;

			//ToDo: Discover a better way to do this.
			//Ideally we should use a measured movement distance from the perspective of the player's camera.forward
			float moveSpeedValue = isMovingBackwards
				? -direction.magnitude
				: direction.magnitude;

			animator.SetFloat("MoveSpeed", moveSpeedValue);
		}

		JumpingAndLanding();

		wasGrounded = isGrounded;
		doJump = false;
	}

	/// <summary>
	/// Updates parent's Animator states per the current member property values
	/// </summary>
	private void JumpingAndLanding()
	{
		if (isGrounded && doJump)
		{
			bool jumpCooldownOver = (Time.time - jumpTimeStamp) >= minJumpInterval;
			if(jumpCooldownOver)
			{
				jumpTimeStamp = Time.time;
				velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			}
		}

		if (!wasGrounded && isGrounded)
		{
			animator.SetTrigger("Land");
		}

		if (!isGrounded && wasGrounded)
		{
			animator.SetTrigger("Jump");
		}

		controller.Move(velocity * Time.deltaTime);
	}
}
