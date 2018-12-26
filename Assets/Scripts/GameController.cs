using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

public static class GameInput
{
	//public string name { get; set;}
    public static bool Jump
    {
		get { return Input.GetMouseButtonDown(0);}
    }

	public static bool stop{
		get { return Input.GetMouseButtonDown(1); }
	}
}

public class GameController : MonoBehaviour
{

	private PlayerController playerController;
	private SceneController sceneController;
	private CactusCreator cactusCreator;
	private GameObject gameoverUI;
	private ScoreManager scoreManager;
    private bool Die = false;
	//MainCode file
	static GameController pThis;
	public static GameController Instance{ get { return pThis;}}
	public Text EMG1Text;
	public Text EMG2Text;
	public Text Jumptext;
	public Text StopText;
	internal UDPDiscovery udp = new UDPDiscovery ();
	internal DataReaderForUnity.DataReader readme = new DataReaderForUnity.DataReader ();
	internal DataReaderForUnity.DataValues datavalues = new DataReaderForUnity.DataValues ();
	internal MMServer mmServer = new MMServer();
	internal Dictionary<String, GameObject> mmObjects = new Dictionary<string, GameObject>();
	static double PubEMG1,PubEMG2;
	static double EMG1Total =0;
	static double EMG2Total =0;
	static double EMG1Avg = 0;
	static double EMG2Avg = 0;
	public double EMG1MVC = 0.0002275;
	public double EMG2MVC = 0.0055815;
	int counter = 0;
	int i =1;
	int j =1;

	public static bool Jump;
	//{get { return Input.GetMouseButtonDown(0);}}
	public static bool stop;
	//{get { return Input.GetMouseButtonDown(1);}}

	public GameController(){
		pThis = this;
	}

	void Start ()
	{
		mmServer.ConnectionOpened += MMGotNewClient;
		mmServer.AddVariableChangedNotification("EMG1", UpdateEMG1);
		mmServer.AddVariableChangedNotification("EMG2", UpdateEMG2);
		mmServer.AddVariableChangedNotification("HandPos", UpdateHandPos);
		mmServer.Start ();

		StartCoroutine (Wait1sec1 ());
		StartCoroutine (Wait1sec2 ());

		playerController = FindObjectOfType<PlayerController>();
		sceneController = FindObjectOfType<SceneController>();
		cactusCreator = FindObjectOfType<CactusCreator>();
		scoreManager = FindObjectOfType<ScoreManager>();
		scoreManager.StartScoring();
		gameoverUI = GameObject.Find("GameoverUI");
		gameoverUI.SetActive(false);

		Physics2D.gravity = Vector2.down * 50;
	}

    private void Update()
    {
		mmServer.Pulse();
		EMG1Total = Math.Abs (PubEMG1) + EMG1Total;
		counter++;
		EMG1Avg = EMG1Total/(double)counter;
		EMG2Total = Math.Abs (PubEMG2) + EMG2Total;
		EMG2Avg = EMG2Total/(double)counter;
    }
		
	IEnumerator Wait1sec1(){
		while (true) {
			Debug.Log ("In the wait1sec2 loop");
			//Jump = false;
			yield return new WaitForSeconds (1);
			if (PubEMG1 > (0.5*EMG1MVC)) {
				stop = true;
				Debug.Log ("I have been here");
				StopText.text = ("Stopped ") + j;
			}
			j++;
			yield return new WaitForSeconds (0.1f);
			stop = false;
		}
	}

	IEnumerator Wait1sec2(){
		while (true) {
			Debug.Log ("In the wait1sec2 loop");
			//Jump = false;
			yield return new WaitForSeconds (1);
			if (PubEMG2 > (0.5*EMG2MVC)) {
				Jump = true;
				Debug.Log ("I have been here");
				Jumptext.text = ("Jumped ") + i;
			}
			i++;
			yield return new WaitForSeconds (0.1f);
			Jump = false;
		}
	}

	/*IEnumerator goback(){
		yield return new WaitForSeconds (0.5);
	}*/

    public void PlayerDie()
    {
        this.Die = true;
		playerController.Die();
		sceneController.StopRolling();
		cactusCreator.Stop();
		scoreManager.StopScoring();
		gameoverUI.SetActive(true);
	}

	public void Restart()
	{
		SceneManager.LoadScene(0);
	}

	internal void MMGotNewClient(int connectionID,string ipadddress)
	{
		Debug.Log("Got a new client connection! " + ipadddress);
	}
	internal void UpdateEMG1(string varname, object data)
	{
		mmServer.SetFrameVariable("ReturnedEMG1", data);
		double ReturnedEMG1 = (double)data;
		double EMG1 = (double)data;
		if (EMG1Text != null)
			EMG1Text.text = EMG1.ToString("0.00000000");
		PubEMG1 = Math.Abs(EMG1);
	}

	internal void UpdateEMG2(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedEMG2", data);
		double ReturnedEMG2 = (double)data;
		double EMG2 = (double)data;
		if (EMG2Text != null)
			EMG2Text.text = EMG2.ToString("0.00000000");
		PubEMG2 = Math.Abs(EMG2);
		}


	internal void UpdateHandPos(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedHandPos", data);
		Quaternion qt = (Quaternion)data;
		}
}


