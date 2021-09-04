using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
	[SerializeField] Transform playerTransform;
	[SerializeField] PlayerController playerScript;

	public bool canBeGrabbed;
	public float canBeGrabbed_Range = 2f;
	GameObject handGameObject;
	[SerializeField] float delay = .4f;

	void CycleThroughGrasp() => CursorExtensions.CycleThroughGrasp();

	bool PlayerCanReach => GetDistanceFromPlayer < canBeGrabbed_Range;

	private float GetDistanceFromPlayer => Vector3.Distance(
		transform.position,
		playerTransform.position
	);

	private void Awake()
	{
		if (!playerTransform) playerTransform = GameObject.FindGameObjectWithTag("TestPlayer").GetComponent<Transform>();
		if (!playerScript) playerScript = GameObject.FindGameObjectWithTag("TestPlayer").GetComponent<PlayerController>();
	}

	private void OnMouseEnter()
	{
		if(PlayerCanReach)
			InvokeRepeating("CycleThroughGrasp", 0, delay);
	}

	private void OnMouseExit()
	{
		CancelInvoke("CycleThroughGrasp");
		Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
	}

	private void OnMouseOver()
	{
		if (Input.GetKey(KeyCode.E) && PlayerCanReach)
		{
			playerScript.PickUpObject(gameObject);
		}

		if(IsInvoking("CycleThroughGrasp"))
		{
			if (!PlayerCanReach)
				Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
		}
		else
		{
			if (PlayerCanReach)
				InvokeRepeating("CycleThroughGrasp", 0, delay);
		}
	}
}
