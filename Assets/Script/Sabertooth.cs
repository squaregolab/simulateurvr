using System.Collections;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using Assets;

public class Sabertooth : MonoBehaviour
{
    // Serial Port Variables

	private Calcul cal;
    private Rigidbody rb;
	private ARDPIDTest ard;
	public static Sabertooth Instance;
    private int deadZone = 9; //Augmenter la valeur réduit la vibration des moteurs
    private int readAnalog = 8;

    private int currentAnalog0 = 0;
    private int currentAnalog1 = 0;
    private int target0 = 512;
    private int target1 = 512;

    //Retour Potentio limite
    private int feedBackMin0 = 450;
    private int feedBackMin1 = 200;
	private int feedBackMax0 = 850;
    private int feedBackMax1 = 600;
    private int feedBackPotDeadZone0, feedBackPotDeadZone1 = 0;

    //Variables PID
    private int motorDirection0, motorDirection1 = 0; // 0 = Stop || 1 = Avancer || 2 = Reculer
    private int oldMotorDirection0, oldMotorDirection1 = 0;

    private double Kmotor0 = 1;
    private double Kmotor1 = 1;
    private double proportional0 = 4.200; //Kp
    private double proportional1 = 4.200; 
    private double integral0 = 0.400; //Ki
    private double integral1 = 0.400;
    private double derivative0 = 0.400; //Kd
    private double derivative1 = 0.400;

    private int outputM0, outputM1 = 0;

    private double integratedM0Error, integratedM1Error = 0;
    private float lastMotor0Error, lastMotor1Error = 0;

    public int powerM0, powerM1 = 0; //M0 = gauche M1 = droite

    void Start() //Initialiation
    {
		Instance = this;
		ard = ARDPIDTest.Instance;;
		cal = Calcul.Instance;
        rb = GetComponent<Rigidbody>();
    }

    void Update() //Loop
    {
        FeedbackPotWorker();
        GetGameData();
        CalculPID();
        CalculMotorDirection();
        SetPWM();
    }

    void FeedbackPotWorker()
    {
        currentAnalog0 = 0; currentAnalog1 = 0;
        for (int i = 0; i < readAnalog; i++)
        {
			currentAnalog0 += int.Parse(ard.LeData); //Si deuxième méthode ne fonctionne pas rajouter 2000 en paramètre.
			currentAnalog1 += int.Parse(ard.RiData);
        }
        currentAnalog0 /= readAnalog;
        currentAnalog1 /= readAnalog;
    }

    void GetGameData()
    {
		/*
        target0 = Utils.Map((int)rb.angularVelocity.x, -180, 180, 0, 1024);
        target0 = (int)Utils.Clamp(target0, feedBackMin0, feedBackMax0);
        target1 = Utils.Map((int)rb.angularVelocity.y, -180, 180, 0, 1024);
        target1 = (int)Utils.Clamp(target1, feedBackMin1, feedBackMax1);
        */
		target0 = Utils.Map(cal.motorB, -180, 180, 0, 1024);
		target0 = (int)Utils.Clamp(target0, feedBackMin0, feedBackMax0);
		target1 = Utils.Map(cal.motorA, -180, 180, 0, 1024);
		target1 = (int)Utils.Clamp(target1, feedBackMin1, feedBackMax1);
    }

    void CalculPID()
    {
        outputM0 = UpdateMotor0PID(target0, currentAnalog0);
        outputM1 = UpdateMotor1PID(target1, currentAnalog1);
    }

    int UpdateMotor1PID(int targetPosition, int currentPosition)
    {
        float error = targetPosition - currentPosition;
        float pTermMotorR = (float)proportional1 * error;
        integratedM1Error = Utils.Clamp(integral1 * error + integratedM1Error, -100, 100);
        double iTermMotorR = integratedM1Error;
        double dTermMotorR = derivative1 * (error - lastMotor1Error);
        lastMotor0Error = error;
        return (int)Utils.Clamp(Kmotor1 * (pTermMotorR + iTermMotorR + dTermMotorR), -2046, 2046);
    }

    int UpdateMotor0PID(int targetPosition, int currentPosition)
    {
        float error = targetPosition - currentPosition;
        float pTermMotorL = (float)proportional0 * error;
        integratedM0Error = Utils.Clamp(integral0 * error + integratedM0Error, -100, 100);
        double iTermMotorL = integratedM0Error;
        double dTermMotorL = derivative0 * (error - lastMotor0Error);
        lastMotor0Error = error;
        return (int)Utils.Clamp((Kmotor0 * (pTermMotorL + iTermMotorL + dTermMotorL)), -2046, 2046);
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

        outputM0 = Mathf.Clamp(outputM0, -2047, 2047);
        outputM1 = Mathf.Clamp(outputM1, -2047, 2047);
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
        }
        else
			powerM0 = 0;

		if (motorDirection1 != 0) {
			if (motorDirection1 == 1)
				powerM1 = (int)(outputM1 / 2);
			else
				powerM1 = (int)-(outputM1 / 2);
			powerM1 *= 2;
		} else
			powerM1 = 0;
    }
}
