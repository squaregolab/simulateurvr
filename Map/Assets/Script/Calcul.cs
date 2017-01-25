using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calcul : MonoBehaviour {
	private Rigidbody rb;
	public static Calcul Instance;
	//Postion moteur
	public int motorA, motorB;
	public int motorXA, motorXB;
	public int motorYA, motorYB;
	public int motorZA, motorZB;
	//Vitesse
	private float rotZ, rotX, rotY;
	private float lastrotZ, lastrotX, lastrotY;
	private int acc;
	// Use this for initialization
	void Start () {
		Instance = this;
		rb = GetComponent<Rigidbody> ();
		motorA = 0;
		motorB = 0;
		motorXA = 0;
		motorXB = 0;
		motorXB = 0;
		motorYA = 0;
		motorYB = 0;
		rotX = 0;
	}

	// Update is called once per frame
	void Update () {

		//Obtention valeurs
		//bool stickMove = Mathf.Abs(Input.GetAxis("Vertical")) > 0.2F || Mathf.Abs(Input.GetAxis("Vertical")) > 0.2F;
		if (Input.GetAxisRaw ("Vertical") > 0.2f && Input.GetAxisRaw ("Vertical") != 0f)
			rotX = 100 * Input.GetAxisRaw ("Vertical");
		else if (Input.GetAxisRaw ("Vertical") < 0.2f && Input.GetAxisRaw ("Vertical") != 0f)
			rotX = 100 * Input.GetAxisRaw ("Vertical");
		else
			rotX = 0;
		if (Input.GetAxisRaw ("Horizontal") > 0.2f && Input.GetAxisRaw ("Horizontal") != 0f)
			rotY = 100 * Input.GetAxisRaw ("Horizontal");
		else if (Input.GetAxisRaw ("Horizontal") < 0.2f && Input.GetAxisRaw ("Horizontal") != 0f)
			rotY = 100 * Input.GetAxisRaw ("Horizontal");
		else
			rotY = 0;
		////////////////////////////////////////////////////
		//Debug.Log ("MtA : " + rotY.ToString());
		//rotY = (int)((lastrotY - (float)(rotY))/Time.deltaTime);
	//	rotZ = (int)((lastrotZ - (float)(rotZ))/Time.deltaTime);
		///////////////////////////////
		//CalculZ (rotZ);
		CalculX (rotX);
		CalculY (rotY);
		CalculFinal (motorXA, motorXB, motorYA, motorYB);
		Debug.Log ("MtA : " + motorA.ToString() + " MtB : " + motorB.ToString());

	}
/*	void CalculZ(int rz)
	{
		motorZA = rz;
		motorZB = rz;
	}*/
	void CalculX(float rx)
	{
		motorXA = (int)rx;
		motorXB = (int)rx;
	}
	void CalculY(float ry)
	{
		motorYA = -(int)ry;
		motorYB = (int)ry;
	}
	void CalculFinal(int mtxa, int mtxb,int mtya,int mtyb)
	{
		motorA = (mtxa + mtya)/2;
		motorB = (mtxb + mtyb)/2;
	}
}