using UnityEngine;
using System.Collections;

using System.IO;

using System;
using System.Threading;

public class threadmotor : MonoBehaviour 
{
	// Init a static reference if script is to be accessed by others when used in a 
	// none static nature eg. its dropped onto a gameObject. The use of "Instance"
	// allows access to public vars as such as those available to the unity editor.
	public static threadmotor Instance;
    private Pot motor;
    Rigidbody rb;
	private int Langle;
	private int Rangle;
    public int directionL, directionR;
	private int ilm;
	private int irm;
	// The serial port
	
	public enum LoopUpdateMethod
	{Threading}

	// This is the public property made visible in the editor.
	public LoopUpdateMethod UpdateMethod = 
		LoopUpdateMethod.Threading;

	// Thread used to recieve and send serial data
	private Thread serialThread;
	// Property used to run/keep alive the serial thread loop
	private bool isRunning = false;
	public bool IsRunning
	{
		get { return isRunning; }
		set { isRunning = value; }
	}
	void Awake()
	{
		// Define the script Instance
		Instance = this;
	}
	void Start()
	{
        rb = GetComponent<Rigidbody>();
        motor = Pot.Instance;
        try
        {
            if (UpdateMethod == LoopUpdateMethod.Threading)
            {
                // If the thread does not exist then start it
                if (serialThread == null)
                { StartSerialThread();

				}

            }
            print("SerialPort successfully opened!");
        }
        catch (Exception ex)
        {
            // Failed to open com port or start serial thread
            Debug.Log("Error 1: " + ex.Message.ToString());
        }
	}
    void Update()
    {
		Langle = (int)System.Math.Round(rb.rotation.x, 0);
		Rangle = (int)System.Math.Round(rb.rotation.x, 0);

    }
    void OnApplicationQuit()
	{

		Thread.Sleep(500);

		if (UpdateMethod == LoopUpdateMethod.Threading)
		{
			// Call to end and cleanup thread
			StopSerialThread();
		}
		Thread.Sleep(500);
	}
    public void StartSerialThread()
	{
		try
		{
			// define the thread and assign function for thread loop
			serialThread = new Thread(new ThreadStart(SerialThreadLoop));
			// Boolean used to determine the thread is running
			isRunning = true;
			// Start the thread
			serialThread.Start();
			print("Serial thread started!");
		}
		catch (Exception ex)
		{
			// Failed to start thread
			Debug.Log("Error 3: " + ex.Message.ToString());
		}
	}
	private void SerialThreadLoop()
	{
		
		while (isRunning)
		{GenericSerialLoop();}

		print("Ending Thread!");
	}
	public void StopSerialThread()
	{
		isRunning = false;
        Thread.Sleep(100);
        if (serialThread != null)
		{
			serialThread.Abort();
			Thread.Sleep(100);
			serialThread = null;
		}
		print("Ended Serial Loop Thread!");
	}
    private void GenericSerialLoop()
    {
        string lm = motor.LeData; 
		//Debug.Log (lm);
        //Direction L
		try {
			//Debug.Log(lm.Substring(0, lm.Length));
			ilm = int.Parse(lm);
		}
		catch{
			Debug.Log ("erreur parse");
		}
		//Debug.Log (lm);
		//Debug.Log (ilm);

		//Debug.Log(int.Parse(lm.Substring(1, lm.Length)));
		if (ilm == Langle) {
			directionL = 0;
		} else if (ilm < Langle) {
			directionL = 2;
		} else if (ilm > Langle) {
			directionL = 1;
		}
		else {directionL = 0;
			}

        /////////////////////////
        string Rm = motor.RiData;
		Debug.Log(Rm);
		//Debug.Log (Rm);
		//Debug.Log (Rm);
        //DirectionR
		try {
			//Debug.Log(lm.Substring(0, lm.Length));
			irm =  System.Convert.ToInt32(Rm);
			//Debug.Log(irm);
		}
		catch{
			Debug.Log("erreur parse1");
		}
       
        if (irm == Rangle) directionR = 0;
        else if (irm < Rangle) directionR = 2;
        else if (irm > Rangle) directionR = 1;
        else directionR = 0;
    }
}