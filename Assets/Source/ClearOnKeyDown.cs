using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearOnKeyDown : MonoBehaviour
{

	// Update is called once per frame
	void Update ()
	{
		if (Input.anyKeyDown)
		{
			foreach (var r in gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				r.enabled = false;
			}
		}		
	}
}
