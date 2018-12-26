using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Random=UnityEngine.Random;

public class MainCode : MonoBehaviour 
{
	public Text EMG1Text;
	public Text EMG2Text;
	public Text HandPosText;
	public Text result;
	public GameObject hand;
	public GameObject[] spawnpoints;
	public GameObject currentpoint;
	public bool ballalive = true;
	public bool movehand = false;
	public Image img1,img2;
	bool EMG1BiggerthanEMG2 = false;
	bool keeponworkingguys = true;
	double PubEMG1,PubEMG2;
	double EMG1Total =0;
	double EMG2Total =0;
	Quaternion targetpositive = Quaternion.Euler (0.0f, 0.0f, 35.0f);
	Quaternion targetnegative = Quaternion.Euler (0.0f, 0.0f, -35.0f);
	Quaternion targetneutral = Quaternion.Euler (0.0f, 0.0f, 0.0f);

   static MainCode pThis;
   public static MainCode Instance { get { return pThis; } }

   internal UDPDiscovery udp = new UDPDiscovery();
   internal DataReaderForUnity.DataReader readme = new DataReaderForUnity.DataReader();
   internal DataReaderForUnity.DataValues dataValues = new DataReaderForUnity.DataValues();

   internal MMServer mmServer = new MMServer();
   internal Dictionary<string, GameObject> mmObjects = new Dictionary<string, GameObject>();

   //public GameObject cube;

   public MainCode()
   {
      pThis = this;
   }
   

   // Use this for initialization
   void Start ()
   {
		StartCoroutine (Spawn());
      // Just a trivial notifier, does nothing
      mmServer.ConnectionOpened += MMGotNewClient;

      // The MMServer class runs in a seperate thread, which prevents Unity from handling object creation and manipulation from there.
      // Each AddVariableChangedNotification callback is instead scheduled to run on the main thread, which is handled by the Update
      // and FixedUpdate calls to mmServer.Pulse()

      // Some test variables for the treadmill and scalar data
      mmServer.AddVariableChangedNotification("EMG1", UpdateEMG1);

      // These are from the demoDataExchangeWithTMM.py file. It only does the CubePosition value which sets the cube pos and returns the position back
      mmServer.AddVariableChangedNotification("EMG2", UpdateEMG2);

	  // Additional data types for the Cube to demonstrate Scalar Orientation data types
	  mmServer.AddVariableChangedNotification("HandPos", UpdateHandPos);

      
		mmServer.Start();
   }

	IEnumerator Spawn(){
		while(keeponworkingguys){
		//spawnpoints = GameObject.FindGameObjectsWithTag ("Sphere");
		int index = Random.Range (0, spawnpoints.Length);
		currentpoint = spawnpoints [index];
		Instantiate (currentpoint);
		// start the timer of EMG1 and EMG2
			if (index == 0) {
				img1.enabled = true;
				img2.enabled = false;
			} else if (index == 1) {
				img1.enabled = false;
				img2.enabled = true;
			}
		double tempEMG1 = EMG1Total;
		double tempEMG2 = EMG2Total;
			HandPosText.text = "Hit the ball";
			HandPosText.color = Color.red;
		yield return new WaitForSeconds(3); // Compare the after result of the 2 EMG total
		double temp1EMG1 = EMG1Total;
		double temp1EMG2 = EMG2Total;
		callyourmom (tempEMG1, tempEMG2, temp1EMG1, temp1EMG2, index);
			HandPosText.text = "RELAX";
			HandPosText.color = Color.white;
		yield return new WaitForSeconds(4);

		}
	}
   void OnApplicationQuit()
   {
      Debug.Log("OnApplicationQuit");
      udp.ServerDiscovered -= DataStreamServerDiscovered;
      udp.StopDiscovery();
      readme.StopReader();
      mmServer.Stop();
   }


   // Update is called once per frame and is tied to the framerate and the complexity of the scene
   void Update ()
   {
      mmServer.Pulse();
	EMG1Total = Math.Abs (PubEMG1) + EMG1Total;
	EMG2Total = Math.Abs (PubEMG2) + EMG2Total;	  

   }

   // FixedUpdate is called multiple times per frame, but can be affected by the physics engine.
   void FixedUpdate()
   {
      mmServer.Pulse();
   }

	void callyourmom (double tempEMG1, double tempEMG2, double temp1EMG1, double temp1EMG2, int index){
		if ((temp1EMG1 - tempEMG1) > (temp1EMG2 - tempEMG2)) {
			EMG1BiggerthanEMG2 = true;
		} else if ((temp1EMG1 - tempEMG1) == (temp1EMG2 - tempEMG2)) {
			EMG1Text.text = "EMG is not connected";
		} else if ((temp1EMG1 - tempEMG1) < (temp1EMG2 - tempEMG2)) {
			EMG1BiggerthanEMG2 = false;
		}


		if((EMG1BiggerthanEMG2) && (index == 0)){//the EMG direction and ball direction the same){
			hand.transform.rotation = Quaternion.Slerp (transform.rotation, targetpositive, 1.5f);	//move hand to the ball direction
			result.text = ("Yes you did it!");
		}else if((EMG1BiggerthanEMG2) && (index == 1)){	
			hand.transform.rotation = Quaternion.Slerp (transform.rotation, targetpositive, 1.5f);
			result.text = ("Try again");
		}else if((!EMG1BiggerthanEMG2) && (index == 0)){	
			hand.transform.rotation = Quaternion.Slerp (transform.rotation, targetnegative, 1.5f);
			result.text = ("Try again");
		}else if((!EMG1BiggerthanEMG2) && (index == 1)){	
			hand.transform.rotation = Quaternion.Slerp (transform.rotation, targetnegative, 1.5f);
			result.text = ("Yes you did it!");
		}
	}
   internal void DataStreamServerDiscovered(string serverIP, string serverName, string webDataUrl, int webServerPort, int dataStreamServerPort, string dataStreamCmd)
   {
      string s = "Discovered server named " + serverName + " on ip address " + serverIP + " with server ports " + webServerPort + " for web and " + dataStreamServerPort + " for data";

      readme.SetServerAndPorts(serverIP, webDataUrl, dataStreamServerPort, dataStreamCmd);

//    StartCoroutine(readme.startViaWebDisco(webDataUrl));
      StartCoroutine(readme.StartViaDirect(serverIP, dataStreamServerPort, dataStreamCmd));
   }

   internal void MMGotNewClient(int connectionID,string ipadddress)
   {
      Debug.Log("Got a new client connection! " + ipadddress);
   }
		

	internal void UpdateEMG1(string varname, object data)
	{
		mmServer.SetFrameVariable("ReturnedEMG1", data);
		double ReturnedEMG1 = (double)data;
		/// Debug.Log("Scalar updated to " + Scalar);

		double EMG1 = (double)data;
		if (EMG1Text != null)
			EMG1Text.text = EMG1.ToString("0.00000000");

		PubEMG1 = EMG1;
		/*Quaternion originalROT = hand.transform.rotation;
		if (movehand) {
			if ((hand.transform.localEulerAngles.z <= 40 && hand.transform.localEulerAngles.z >= 0) || (hand.transform.localEulerAngles.z < 360 && hand.transform.localEulerAngles.z >= 320)) {
				hand.transform.rotation = originalROT * Quaternion.AngleAxis ((float)EMG1 * 1000, Vector3.forward);
			} else if (hand.transform.localEulerAngles.z > 40 && hand.transform.localEulerAngles.z < 180) {
				hand.transform.Rotate (0.0f, 0.0f, 39.0f);
			} else if (hand.transform.localEulerAngles.z < 320 && hand.transform.localEulerAngles.z > 180) {
				hand.transform.Rotate (0.0f, 0.0f, 321.0f);
			}
		}*/
		//HandPosText.text = (String)hand.transform.localEulerAngles.z;

	}

   internal void UpdateEMG2(string varname,object data)
   {
      mmServer.SetFrameVariable("ReturnedEMG2", data);
		double ReturnedEMG2 = (double)data;
		/// Debug.Log("Scalar updated to " + Scalar);

		double EMG2 = (double)data;
		if (EMG2Text != null)
			EMG2Text.text = EMG2.ToString("0.00000000");
		PubEMG2 = EMG2;
		/*Quaternion originalROT = hand.transform.rotation;
		if (movehand) {
			if ((hand.transform.localEulerAngles.z <= 40 && hand.transform.localEulerAngles.z >= 0) || (hand.transform.localEulerAngles.z <= 360 && hand.transform.localEulerAngles.z >= 320)) {
				hand.transform.rotation = originalROT * Quaternion.AngleAxis ((float)EMG2 * 1000, Vector3.back);
			} else if (hand.transform.localEulerAngles.z > 40 && hand.transform.localEulerAngles.z < 180) {
				hand.transform.Rotate (0.0f, 0.0f, 39.0f);
			} else if (hand.transform.localEulerAngles.z < 320 && hand.transform.localEulerAngles.z > 180) {
				hand.transform.Rotate (0.0f, 0.0f, 321.0f);
			}
		}*/
		//HandPosText.text = (String)hand.transform.localEulerAngles.z;
   }


	internal void UpdateHandPos(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedHandPos", data);
			Quaternion qt = (Quaternion)data;
			//if (hand != null)
			//	hand.transform.localRotation = qt;
	}

}
