﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{

	public float rollSpeed;
	public GameObject floorPrefab;
	public GameObject cloudPrefab;

	private GameObject scene;
	private float floorWaitTime;
	private float cloudWaitTime = 3f;
	private bool rolling = true;

	void Start ()
	{
		scene = gameObject;

		var sprite = floorPrefab.GetComponent<SpriteRenderer>().sprite;
		var floorLength = (sprite.rect.width - 20) / sprite.pixelsPerUnit;
		floorWaitTime = floorLength / rollSpeed;

		StartCoroutine("CreateFloor");
		StartCoroutine("CreateCloud");
	}

	IEnumerator CreateFloor()
	{
		while (true)
		{
			var floor = Instantiate(floorPrefab, scene.transform);
			floor.transform.position = floorPrefab.transform.position;
			yield return new WaitForSeconds(floorWaitTime);
		}
	}

	IEnumerator CreateCloud()
	{
		while (true)
		{
			var cloud = Instantiate(cloudPrefab, scene.transform);
			cloudPrefab.transform.position = new Vector2(4, Random.Range(0f, 2f));
			yield return new WaitForSeconds(cloudWaitTime);
		}
	}

	public void StopRolling()
	{
		rolling = false;
		StopCoroutine("CreateCloud");
		StopCoroutine("CreateFloor");
	}

	public void resumeRolling(){
		rolling = true;
		StartCoroutine("CreateFloor");
		StartCoroutine("CreateCloud");
	}

	void FixedUpdate ()
	{
		if(rolling)
			transform.position += Vector3.left * rollSpeed * Time.deltaTime;
		
		if (GameController.Jump) {
			resumeRolling ();
			Time.timeScale = 1;
			Debug.Log ("I pressed butsss.");
		}
		if (GameController.stop) {
			Time.timeScale = 0.5f;
			Debug.Log ("I pressed but.");
		}
	}
}
