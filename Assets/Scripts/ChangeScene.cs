using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene: MonoBehaviour {
	public Text noticeText;

	// Use this for initialization
	void Start () {
		StartCoroutine (MVC ());

	}

	IEnumerator MVC (){
		noticeText.text = "Using maximum strength for UT";
		yield return new WaitForSeconds (10);
		noticeText.text = "Now measursing maximum strenth for the other muscle";
		yield return new WaitForSeconds (10);
		noticeText.text = "Now the game will start in 3 seconds";
		yield return new WaitForSeconds (3);
		changeToScene (1);
	}

	void changeToScene(int changetheScene){
		SceneManager.LoadScene (changetheScene);		
	}

	// Update is called once per frame
	void Update () {
		
	}
}
