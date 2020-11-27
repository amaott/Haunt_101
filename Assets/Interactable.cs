using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	private void OnMouseEnter()
	{
		Debug.Log("Hey is me!");
	}

	private void OnMouseExit()
	{
		Debug.Log("Bye bye");
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
