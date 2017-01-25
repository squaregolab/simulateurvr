using System.Collections;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using Assets;

public class Sabertooth : MonoBehaviour
{
	//Recuperation des valeur
	private ARDTest ARDTest;
	private float pot;
	//Acceleration
	private float accX;
	private float lastaccX;
	private float accY;
	private float lastaccY;
    // Serial Port Variables
	public int ReadTimeout = 10;
    static SerialPort sabertoothPort;
    static SerialPort arduinoPort;

    private Rigidbody rb;

    private int deadZone = 9; //Augmenter la valeur réduit la vibration des moteurs
    private int readAnalog = 8;

    private int currentAnalog0, currentAnalog1 = 0;
    private int target0 = 512;
    private int target1 = 512;

    //Retour Potentio limite
    private int feedBackMin0 = 62; 
    private int feedBackMin1 = 62;
    private int feedBackMax0 = 962;
    private int feedBackMax1 = 962;
    private int feedBackPotDeadZone0, feedBackPotDeadZone1 = 0;

    //Variables PID
    private int motorDirection0, motorDirection1 = 0; // 0 = Stop || 1 = Avancer || 2 = Reculer
    private int oldMotorDirection0, oldMotorDirection1 = 0;

    private double Kmotor0 = 1;
    private double Kmotor1 = 1;
    private double proportional0 = 4.200;
    private double proportional1 = 4.200;
    private double integral0 = 0.400;
    private double integral1 = 0.400;
    private double derivative0 = 0.400;
    private double derivative1 = 0.400;

    private int outputM0, outputM1 = 0;

    private double integratedM0Error, integratedM1Error = 0;
    private float lastMotor0Error, lastMotor1Error = 0;

    private int powerM0, powerM1 = 0; //M0 = gauche M1 = droite

    void Start() //Initialiation
    {
        sabertoothPort = InitSerialPort(true);
        arduinoPort = InitSerialPort(false);
		ARDTest = ARDTest.Instance;
        rb = GetComponent<Rigidbody>();
		sabertoothPortOpen (0, true);
    }

    void Update() //Loop
    {
		
		if (ARDTest.RawData != "Ready")
			pot = float.Parse(ARDTest.RawData);
		//Debug.Log (pot);
        FeedbackPotWorker();
		SetZero();
		Debug.Log (currentAnalog0 + " " + currentAnalog1);
		//Debug.Log (currentAnalog0 + " " + currentAnalog1);
      //  GetGameData();
		//Debug.Log (target0 + " " + currentAnalog1);
     //   CalculPID();
     //   CalculMotorDirection();
      //  SetPWM();
    }
	void SetZero ()
	{
		
		/*
		if (currentAnalog0 < 200 && currentAnalog0 > 200)
			return;
		else if (currentAnalog0 < 200)
			sabertoothPortOpen (100, true);
		else if(currentAnalog0 > 200)
			sabertoothPortOpen(-100,true);*/
	}
    void FeedbackPotWorker()
    {
        currentAnalog0 = 0; currentAnalog1 = 0;
        for (int i = 0; i < readAnalog; i++)
        {
			currentAnalog0 += (int)pot; //Si deuxième méthode ne fonctionne pas rajouter 2000 en paramètre.
			currentAnalog1 += (int)pot;
        }
        currentAnalog0 /= readAnalog;
        currentAnalog1 /= readAnalog;
    }

    void GetGameData()
    {
		accX = (rb.velocity.magnitude - lastaccX)/ Time.fixedDeltaTime;
		lastaccX = rb.velocity.magnitude;
		/*accY = (rb.velocity.y - lastaccY)/ Time.fixedDeltaTime;
		lastaccY = rb.velocity.y;*/
        target0 = Mathf.Clamp((int)accX , feedBackMin0, feedBackMax0);
        target1 = Mathf.Clamp((int)accX , feedBackMin1, feedBackMax1);
		//Debug.Log (accX + " " + lastaccX + " " + accY + " " + lastaccY);
    }

    void CalculPID()
    {
        outputM0 = UpdateMotor0PID(target0, currentAnalog0);
        outputM1 = UpdateMotor1PID(target1, currentAnalog1);
    }

    int UpdateMotor1PID(int targetPosition, int currentPosition)
    {
        float error = (float)targetPosition - (float)currentPosition;
        float pTermMotorR = (float)proportional1 * error;
        integratedM0Error += error;
        float iTermMotorR = (float)integral1 * Mathf.Clamp((float)integratedM1Error, -100, 100);
        float dTermMotorR = (float)derivative1 * (error - lastMotor1Error);
        lastMotor0Error = error;
        return (int)Utils.Clamp(Kmotor1 * (pTermMotorR + iTermMotorR + dTermMotorR), -255, 255);
    }

    int UpdateMotor0PID(int targetPosition, int currentPosition)
    {
        float error = (float)targetPosition - (float)currentPosition;
        float pTermMotorL = (float)proportional0 * error;
        integratedM0Error += error;
        float iTermMotorL = (float)integral0 * Mathf.Clamp((float)integratedM0Error, -100, 100);
        float dTermMotorL = (float)derivative0 * (error - lastMotor0Error);
        lastMotor0Error = error;
        return (int)Utils.Clamp(Kmotor0 * (pTermMotorL + iTermMotorL + dTermMotorL), -255, 255);
    }

    void CalculMotorDirection()
    {
        if (target0 > (currentAnalog0 + (deadZone + feedBackPotDeadZone0)) || target0 < (currentAnalog0 - (deadZone + feedBackPotDeadZone0)))
        {
            if (outputM0 >= 0)
                motorDirection0 = 1;
            else
            {
                motorDirection0 = 2;
                outputM0 = Mathf.Abs(outputM0);
            }
        }
        else
            motorDirection0 = 0;


        if (target1 > (currentAnalog1 + (deadZone + feedBackPotDeadZone1)) || target1 < (currentAnalog1 - (deadZone + feedBackPotDeadZone1)))
        {
            if (outputM1 >= 0)
                motorDirection1 = 1;
            else
            {
                motorDirection1 = 2;
                outputM1 = Mathf.Abs(outputM1);
            }
        }
        else
            motorDirection1 = 0;

        outputM0 = Mathf.Clamp(outputM0, -255, 255);
        outputM1 = Mathf.Clamp(outputM1, -255, 255);
    }

    void SetPWM()
    {
        if (motorDirection0 != 0)
        {
            if (motorDirection0 == 1)
                powerM0 = (int)(outputM0 / 2);
               
            else
                powerM0 = (int)-(outputM0 / 2);
            powerM0 *= 2;
            sabertoothPortOpen(powerM0, true);
        }
        else
            sabertoothPortOpen(0, true);

        if (motorDirection1 != 0)
        {
            if (motorDirection1 == 1)
                powerM1 = (int)(outputM1 / 2);
            else
                powerM1 = (int)-(outputM1 / 2);
            powerM1 *= 2;
            sabertoothPortOpen(powerM1, false);
        }
        else
            sabertoothPortOpen(0, false);
       /* Debug.Log("test = " + feedBackMin0 + " y = " + target1);
        Debug.Log("m0 = " + powerM0 + " m1 = " + powerM1);*/
        }

/*    int arduinoPortOpen(int timeout = 0)
    {
        int value;
        arduinoPort.ReadTimeout = ReadTimeout;
        try
        {
            arduinoPort.Open();
           // Thread.Sleep(10);
            try { value = int.Parse(arduinoPort.ReadLine()); }
            catch (Exception ex) { Debug.Log(ex); value = 0; }
            arduinoPort.BaseStream.Flush();
            arduinoPort.Dispose();
        }
        catch (TimeoutException)
        {
            return 0;
        }
        
        return value;
    }*/


    void sabertoothPortOpen(int power, bool isRight)
    {
        sabertoothPort.Open();
        if (isRight)
            sabertoothPort.WriteLine("M1:" + power.ToString());
        else
            sabertoothPort.WriteLine("M2:" + power.ToString());
        sabertoothPort.BaseStream.Flush();
        sabertoothPort.Dispose();
    }

    SerialPort InitSerialPort(bool isFirst)
    {
		return new SerialPort(isFirst ? "COM7" : "COM8", isFirst ? 115200 : 9600, Parity.None, 8, StopBits.None);
    }
}
