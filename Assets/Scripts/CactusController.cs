using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusController : MonoBehaviour
{
	static bool collided = false;

	private void OnTriggerEnter2D(Collider2D other)
	{
		collided = true;

		if (other.CompareTag("Player"))
		{
			FindObjectOfType<SceneController>().StopRolling();
			if (collided && GameController.Jump) {
				Debug.Log ("This should be the place");
				FindObjectOfType<SceneController>().resumeRolling();
			}
		}
	}

	private void update(){
		if (collided && GameController.Jump) {
			Debug.Log (" Or This should be the place");
			FindObjectOfType<SceneController>().resumeRolling();
			collided = false;
		}
	}
}
