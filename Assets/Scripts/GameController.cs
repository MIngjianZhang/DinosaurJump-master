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
	
    public static bool Jump
    {
		get { return Input.GetMouseButtonDown(0)|| Input.touchCount > 0; }
    }

	public static bool stop{
		get { return Input.GetMouseButtonDown (1); }
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
	int counter = 0;

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
        if(Die && GameInput.Jump)
            Restart();
    }

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
		PubEMG1 = EMG1;
	}

	internal void UpdateEMG2(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedEMG2", data);
		double ReturnedEMG2 = (double)data;
		double EMG2 = (double)data;
		if (EMG2Text != null)
			EMG2Text.text = EMG2.ToString("0.00000000");
		PubEMG2 = EMG2;
		}


	internal void UpdateHandPos(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedHandPos", data);
		Quaternion qt = (Quaternion)data;
		}
}


