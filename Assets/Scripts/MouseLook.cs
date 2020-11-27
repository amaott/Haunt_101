﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public float mouseSensitivity = 100f;
	float rotationX = 0f;

	public Transform playerBody;
	// Start is called before the first frame update
	void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		rotationX -= mouseY;
		rotationX = Mathf.Clamp(rotationX, -90, 90);

		transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		playerBody.Rotate(Vector3.up, mouseX);
	}
}
