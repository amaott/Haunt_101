using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Transform groundCheck;
	public LayerMask groundMask;

	public float speed = 12f;
	public float gravity = -9.81f;
	public float pickupObjectScale = 0.2f;
	public float groundDistance = 0.4f;

	bool doJump = false;

	[SerializeField] float minJumpInterval = 0.25f;
	[SerializeField] float jumpHeight = 1f;
	[SerializeField] readonly float walkScale = 0.33f;

	Animator animator;
	CharacterController controller;
	Transform handTransform;
	Vector3 velocity;
	
	float jumpTimeStamp = 0;
	float currentX = 0;
	float currentZ = 0;
	float previousYPosition = 0;
	readonly float interpolation = 10;
	Vector3 currentDirection = Vector3.zero;
	bool pickedUpObjectThisFrame = false;
	List<Collider> m_collisions = new List<Collider>();

	/// <summary>
	/// Assigns the attached gameObject's Animator and Rigidbody compenent values when not provided.
	/// </summary>
	private void Awake()
	{
		if (!animator) animator = gameObject.GetComponent<Animator>();
		if (!controller) controller = gameObject.GetComponent<CharacterController>();

		if (!handTransform) handTransform = GameObject.FindGameObjectWithTag("GraspTag").transform;
	}

	public void PickUpObject(GameObject pickupObject)
	{
		// If no heldObject and canGrab, pick up object
		bool canGrab = pickupObject.GetComponent<Interactable>()?.canBeGrabbed ?? false;
		if (HeldObject == null && canGrab)
		{
			Vector3 scale = pickupObject.transform.localScale;
			scale.Set(pickupObjectScale, pickupObjectScale, pickupObjectScale);
			pickupObject.transform.localScale = scale;

			pickupObject.GetComponent<Collider>().enabled = false;
			HeldObject = pickupObject;

			pickedUpObjectThisFrame = true;
		}
	}

	public void DropObject()
	{
		if (HeldObject)
		{
			HeldObject.transform.position = transform.position;

			Vector3 scale = HeldObject.transform.localScale;
			scale.Set(1, 1, 1);
			HeldObject.transform.localScale = scale;

			HeldObject.GetComponent<Collider>().enabled = true;
			HeldObject = null;
		}
	}

	// Update is called once per frame
	void Update()
	{
		/* Update Animator States */
		animator.SetBool("Grounded", isGrounded);

		if (!doJump && Input.GetKeyDown(KeyCode.Space))
		{
			doJump = true;
		}

		// If holding an object, update position to the hand and disable its colliders
		if (HeldObject)
		{
			// Drop picked up item
			if (Input.GetKeyDown(KeyCode.E))
			{
				if(!pickedUpObjectThisFrame)
					DropObject();

				pickedUpObjectThisFrame = false;
			}
			else
				HeldObject.transform.position = handTransform.position;
		}
	}

	GameObject _heldObject;
	bool isMovingHeldToInventory = false;
	public GameObject HeldObject
	{
		get { return _heldObject; }
		set {
			if (value == null && !isMovingHeldToInventory)
			{
				HeldObject.transform.position = transform.position;
			}

			_heldObject = value;
		}
	}

	bool wasGrounded;
	bool isGrounded;
	private void FixedUpdate()
	{

		/* Update movement */
		isGrounded = Physics.CheckSphere(transform.position, groundDistance);

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
		previousYPosition = transform.position.y;
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

				isGrounded = true;
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



	private void OnCollisionEnter(Collision collision)
	{
		ContactPoint[] contactPoints = collision.contacts;
		for (int i = 0; i < contactPoints.Length; i++)
		{
			if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
			{
				if (!m_collisions.Contains(collision.collider))
				{
					m_collisions.Add(collision.collider);
				}
				isGrounded = true;
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		ContactPoint[] contactPoints = collision.contacts;
		bool validSurfaceNormal = false;
		for (int i = 0; i < contactPoints.Length; i++)
		{
			if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
			{
				validSurfaceNormal = true; break;
			}
		}

		if (validSurfaceNormal)
		{
			isGrounded = true;
			if (!m_collisions.Contains(collision.collider))
			{
				m_collisions.Add(collision.collider);
			}
		}
		else
		{
			if (m_collisions.Contains(collision.collider))
			{
				m_collisions.Remove(collision.collider);
			}
			if (m_collisions.Count == 0) { isGrounded = false; }
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (m_collisions.Contains(collision.collider))
		{
			m_collisions.Remove(collision.collider);
		}
		if (m_collisions.Count == 0) { isGrounded = false; }
	}
}
