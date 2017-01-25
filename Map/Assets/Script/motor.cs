using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.IO.Ports;
using System.Threading;

public class motor : MonoBehaviour {

	public static motor Instance;
	private int lastacc = 0, acc, mgnt;
	public int minStep = 20;
	private int angleR;
	private int mtA = 0;
	private int mtB = 0;
	private int r = 0;
	private int l = 0;
	private Calcul cal;
	private Rigidbody rb;
	private TestMotor tmotor;
	private Pot pot;
	private bool finR = true;
	private bool finL = true;
	#region Properties

	// The serial port
	public SerialPort SerialPort;

	// The script update can run as either a seperate thread
	// or as a standard coroutine. This can be selected via 
	// the unity editor.

	public enum LoopUpdateMethod
	{Threading}

	// This is the public property made visible in the editor.
	public LoopUpdateMethod UpdateMethod = 
		LoopUpdateMethod.Threading;

	// Thread used to recieve and send serial data
	private Thread serialThread;

	// List of all baudrates available to the arduino platform
	private ArrayList baudRates =
		new ArrayList() { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

	// List of all com ports available on the system
	private ArrayList comPorts =
		new ArrayList();

	// If set to true then open the port when the start
	// event is called.
	public bool OpenPortOnStart = false;

	// Holder for status report information
	private string portStatus = "";
	public string PortStatus
	{
		get { return portStatus; }
		set { portStatus = value; }
	}

	// Current com port and set of default
	public string ComPort = "COM5";

	// Current baud rate and set of default
	public int BaudRate = 38400;

	public int ReadTimeout = 10;

	public int WriteTimeout = 10;

	// Property used to run/keep alive the serial thread loop
	private bool isRunning = false;
	public bool IsRunning
	{
		get { return isRunning; }
		set { isRunning = value; }
	}

	// Set the gui to show ready
	private string rawData = "Ready";
	public string RawData
	{
		get { return rawData; }
		set { rawData = value; }
	}

	// Storage for parsed incoming data
	private string[] chunkData;
	public string[] ChunkData
	{
		get { return chunkData; }
		set { chunkData = value; }
	}

	// Refs populated by the editor inspector for default gui
	// functionality if script is to be used in a non-static
	// context.
	public GameObject ComStatusText;

	public GameObject RawDataText;

	#endregion Properties

	#region Unity Frame Events

	/// <summary>
	/// The awake call is used to populate refs to the gui elements used in this 
	/// example. These can be removed or replaced if needed with bespoke elements.
	/// This will not affect the functionality of the system. If we are using awake
	/// then the script is being run non staticaly ie. its initiated and run by 
	/// being dropped onto a gameObject, thus enabling the game loop events to be 
	/// called e.g. start, update etc.
	/// </summary>
	void Awake()
	{
		// Define the script Instance
		Instance = this;
	}

	void GameObjectSerialPort_DataRecievedEvent(string[] Data, string RawData)
	{
		print("Data Recieved: " + RawData);
	}

	/// <summary>
	/// The start call is used to populate a list of available com ports on the
	/// system. The correct port can then be selected via the respective guitext
	/// or a call to UpdateComPort();
	/// </summary>
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		tmotor = TestMotor.Instance;
		pot = Pot.Instance;
		cal = Calcul.Instance;
		// Population of comport list via system.io.ports
		PopulateComPorts();

		// If set to true then open the port. You must 
		// ensure that the port is valid etc. for this! 
		if (OpenPortOnStart) { OpenSerialPort(); }
	}

	/// <summary>
	/// The update frame call is used to provide caps for sending data to the arduino
	/// triggered via keypress. This can be replaced via use of the static functions
	/// SendSerialData() & SendSerialDataAsLine(). Additionaly this update uses the
	/// RawData property to update the gui. Again this can be removed etc.
	/// </summary>
	void Update()
	{

		//Debug.Log (mtB);
		mgnt = (int)System.Math.Round(rb.velocity.magnitude, 0);
		//angleR = (int)((rb.rotation.x*100+180)/360*100);
		// Check if the serial port exists and is open
		if (SerialPort == null || SerialPort.IsOpen == false) { return; }

		// Example calls from system to the arduino. For more detail on the
		// structure of the calls see: http://www.dyadica.co.uk/journal/simple-serial-string-parsing/
	}

	/// <summary>
	/// Clean up the thread and close the port on application close event.
	/// </summary>
	void OnApplicationQuit()
	{
		// Call to cloase the serial port

		CloseSerialPort();

		Thread.Sleep(500);

		if (UpdateMethod == LoopUpdateMethod.Threading)
		{
			// Call to end and cleanup thread
			StopSerialThread();
		}
		Thread.Sleep(500);
	}

	#endregion Unity Frame Events

	#region Object Serial Port

	/// <summary>
	/// Opens the defined serial port and starts the serial thread used
	/// to catch and deal with serial events.
	/// </summary>
	public void OpenSerialPort()
	{
		try
		{
			// Initialise the serial port
			SerialPort = new SerialPort(ComPort, BaudRate);
			SerialPort.ReadTimeout = ReadTimeout;
			SerialPort.WriteTimeout = WriteTimeout;
			// Open the serial port
			SerialPort.Open();
			if (UpdateMethod == LoopUpdateMethod.Threading)
			{
				// If the thread does not exist then start it
				if (serialThread == null)
				{ StartSerialThread(); }
			}
			print("SerialPort successfully opened!");
		}
		catch (Exception ex)
		{
			// Failed to open com port or start serial thread
			Debug.Log("Error 1: " + ex.Message.ToString());
		}
	}

	/// <summary>
	/// Cloases the serial port so that changes can be made or communication
	/// ended.
	/// </summary>
	public void CloseSerialPort()
	{
		try
		{
			// Close the serial port
			SerialPort.Close();

			// Update the gui if applicable
			if (Instance.ComStatusText != null)
			{ Instance.ComStatusText.GetComponent<GUIText>().text = "ComStatus: Closed"; }
		}
		catch (Exception ex)
		{
			if (SerialPort == null || SerialPort.IsOpen == false)
			{
				// Failed to close the serial port. Uncomment if
				// you wish but this is triggered as the port is
				// already closed and or null.

				// Debug.Log("Error 2A: " + "Port already closed!");
			}
			else
			{
				// Failed to close the serial port
				Debug.Log("Error 2B: " + ex.Message.ToString());
			}
		}

		print("Serial port closed!");
	}

	#endregion Object Serial Port

	#region Serial Thread

	/// <summary>
	/// Function used to start seperate thread for reading serial 
	/// data.
	/// </summary>
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

	/// <summary>
	/// The serial thread loop. A Seperate thread used to recieve
	/// serial data thus not affecting generic unity playback etc.
	/// </summary>
	private void SerialThreadLoop()
	{
		while (isRunning)
		{ GenericSerialLoop(); }

		print("Ending Thread!");
	}

	/// <summary>
	/// Function used to stop the serial thread and kill
	/// off any instance
	/// </summary>
	public void StopSerialThread()
	{
		// Set isRunning to false to let the while loop
		// complete and drop out on next pass
		isRunning = false;

		// Pause a little to let this happen
		Thread.Sleep(100);

		// If the thread still exists kill it
		// A bit of a hack using Abort :p
		if (serialThread != null)
		{
			serialThread.Abort();
			// serialThread.Join();
			Thread.Sleep(100);
			serialThread = null;
		}

		// Reset the serial port to null
		if (SerialPort != null)
		{ SerialPort = null; }

		// Update the port status... just in case :)
		portStatus = "Ended Serial Loop Thread";

		print("Ended Serial Loop Thread!");
	}

	#endregion Serial Thread 

	#region Static Functions

	/// <summary>
	/// Function used to send string data over serial with
	/// an included line return
	/// </summary>
	/// <param name="data">string</param>
	public void SendSerialDataAsLine(string data)
	{
		if (SerialPort != null)
		{ SerialPort.WriteLine(data); }

		print("Sent data: " + data);
	}

	/// <summary>
	/// Function used to send string data over serial without
	/// a line return included.
	/// </summary>
	/// <param name="data"></param>
	public void SendSerialData(string data)
	{
		if (SerialPort != null)
		{ SerialPort.Write(data); }

		print("Sent data: " + data);
	}

	#endregion Static Functions

	/// <summary>
	/// The serial thread loop & the coroutine loop both utilise
	/// the same code with the exception of the null return on 
	/// the coroutine, so we share it here.
	/// </summary>
	int DifferenceAngle(int AP,int AD)
	{
		return AP - AD;
	}
	private void GenericSerialLoop()
	{
		
		try{
			r = int.Parse(pot.RiData);
			//moitieR(r);

			//Debug.Log("R : " + r.ToString());
			if(finR)
			{
				int envoi = 0;
				try{
					mtA = cal.motorA;
					if (mtA< r + 5 && mtA > r - 5) 
						envoi = 0;
					else if(r>=0) 
						envoi = -400;
					else if (r<=100)
						envoi = 400;
					else 
						envoi = (int)(mtA*15f);
					//Debug.Log("Envoi : " + DifferenceAngle(r,mtA));
			//	Debug.Log("Envoi : " + envoi);
					SerialPort.WriteLine("M2:"+ envoi.ToString());

				}catch{Debug.Log("cal");}
			}
			else
				SerialPort.WriteLine ("M2:0");
		}
		catch{
			Debug.Log ("erreur parse");
		}
		try{
			//moitieL();
			//erialPort.WriteLine("M1:0");
			l = int.Parse(pot.LeData);
			//Debug.Log("L : " + l.ToString());
			if(finL)
			{
				int envoi = 0;
				try{
					mtB = cal.motorB;
					if (mtB< l + 5 && mtB > l - 5) 
						envoi = 0;
					else if(l==0) 
						envoi = -400;
					else if (l==100)
						envoi = 400;
					else 
						envoi = (int)(mtB*15f);
					//Debug.Log("Diff : " + DifferenceAngle(l,mtB));
					//Debug.Log("Diff : " + envoi);
					SerialPort.WriteLine("M1:"+ envoi.ToString());
				}catch{Debug.Log("cal");}
			}

			else
				SerialPort.WriteLine ("M1:0");

		}
		catch{
			Debug.Log ("erreur parse");
		}
	}
	/// <summary>
	/// Function used to filter and act upon the data recieved. You can add
	/// bespoke functionality here.
	/// </summary>
	/// <param name="data">string[] of raw data seperated into chunks via ','</param>
	/// <param name="rawData">string of raw data</param>
	private void ParseSerialData(string[] data, string rawData)
	{
		// Examples of reading a value from the recieved data
		// for use if required - remove or replase with bespoke
		// functionality etc

		if (data.Length == 2)
		{ int ReceviedValue = int.Parse(data[1]); }
		else { print(rawData); }

		if (data == null || data.Length != 2)
		{ print(rawData); }

		// The following can be run if the code is run via the coroutine method.

		//if (RawDataText != null)
		//    RawDataText.guiText.text = RawData;
	}

	/// <summary>
	/// Function that utilises system.io.ports.getportnames() to populate
	/// a list of com ports available on the system.
	/// </summary>
	public void PopulateComPorts()
	{
		// Loop through all available ports and add them to the list
		foreach (string cPort in System.IO.Ports.SerialPort.GetPortNames())
		{
			comPorts.Add(cPort); // Debug.Log(cPort.ToString());
		}

		// Update the port status just in case :)
		portStatus = "ComPort list population complete";
	}

	/// <summary>
	/// Function used to update the current selected com port
	/// </summary>
	public string UpdateComPort()
	{
		// If open close the existing port
		if (SerialPort != null && SerialPort.IsOpen)
		{ CloseSerialPort(); }

		// Find the current id of the existing port within the
		// list of available ports
		int currentComPort = comPorts.IndexOf(ComPort);

		// check against the list of ports and get the next one.
		// If we have reached the end of the list then reset to zero.
		if (currentComPort + 1 <= comPorts.Count - 1)
		{
			// Inc the port by 1 to get the next port
			ComPort = (string)comPorts[currentComPort + 1];
		}
		else
		{
			// We have reached the end of the list reset to the
			// first available port.
			ComPort = (string)comPorts[0];
		}

		// Update the port status just in case :)
		portStatus = "ComPort set to: " + ComPort.ToString();

		// Return the new ComPort just in case
		return ComPort;
	}

	/// <summary>
	/// Function used to update the current baudrate
	/// </summary>
	public int UpdateBaudRate()
	{
		// If open close the existing port
		if (SerialPort != null && SerialPort.IsOpen)
		{ CloseSerialPort(); }

		// Find the current id of the existing rate within the
		// list of defined baudrates
		int currentBaudRate = baudRates.IndexOf(BaudRate);

		// check against the list of rates and get the next one.
		// If we have reached the end of the list then reset to zero.
		if (currentBaudRate + 1 <= baudRates.Count - 1)
		{
			// Inc the rate by 1 to get the next rate
			BaudRate = (int)baudRates[currentBaudRate + 1];
		}
		else
		{
			// We have reached the end of the list reset to the
			// first available rate.
			BaudRate = (int)baudRates[0];
		}
		// Update the port status just in case :)
		portStatus = "BaudRate set to: " + BaudRate.ToString();
		// Return the new BaudRate just in case
		return BaudRate;
	}
}