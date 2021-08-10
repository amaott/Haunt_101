using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
	[SerializeField] Transform playerTransform;
	[SerializeField] PlayerController playerScript;

	public bool canBeGrabbed;
	GameObject handGameObject;
	[SerializeField] float delay = .4f;

	private void Awake()
	{
		if (!playerScript) playerScript = GameObject.FindGameObjectWithTag("TestPlayer").GetComponent<PlayerController>();
	}

	private void OnMouseEnter()
	{
		InvokeRepeating("CycleThroughGrasp", 0, delay);
	}

	private void OnMouseExit()
	{
		CancelInvoke("CycleThroughGrasp");
		Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
	}

	void CycleThroughGrasp() => CursorExtensions.CycleThroughGrasp();

	private void OnMouseOver()
	{
		if (Input.GetKey(KeyCode.E))
		{
			playerScript.PickUpObject(gameObject);
		}
	}
}
