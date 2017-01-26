using UnityEngine;
using System.Collections;

using System.IO;

using System;
using System.IO.Ports;
using System.Threading;

public class TestMotor : MonoBehaviour {
		public static TestMotor Instance;
		public int minStep = 20;
		private Rigidbody rb;
		private Pot pot;
		private int anglePot;
		private int AccM;
		private int lastMgnt = 0;
		private int AccAM;
		private int AccAX;
		private	int AccAY;
		private	int angleDesireeX;
		private	int angleDesireeY;
		public int tempsL;
		public int vitesseL;
		public int distance;
		public int direction;
	public int tempsR;
	public int vitesseR;


		public SerialPort SerialPort;

		void Awake()
		{
			Instance = this;
		}

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			pot = Pot.Instance;
		}

		void Update()
		{
			int mgnt = (int)System.Math.Round(rb.velocity.magnitude, 0);
		anglePot = int.Parse(pot.LeData);
			AccM = mgnt - lastMgnt;
			lastMgnt = mgnt;
			//AccAM = (int)System.Math.Round(rb.angularVelocity.magnitude,0);
		AccAX = (int)System.Math.Round(rb.rotation.x*100,0);
		AccAY = (int)System.Math.Round(rb.rotation.y*100,0);

			angleDesireeX = (int)System.Math.Round(rb.rotation.x);
			angleDesireeY = (int)System.Math.Round(rb.rotation.y);
		tempsL = (int)(((AccAX + AccAY)/2));
		Debug.Log (tempsL);
		vitesseL = (dir(anglePot,angleDesireeX) * (DifferenceAngle(anglePot, angleDesireeX) / tempsL));
		//Debug.Log (tempsL);
		tempsR = (int)((( AccAX + AccAY)/2));
		//Debug.Log ((((AccAX + AccAY)/2)*100));

		vitesseR = (dir(anglePot,angleDesireeY) * DifferenceAngle(anglePot, angleDesireeY) / tempsR);
		//Debug.Log (vitesseR);
		}
		int dir(int AP,int AD)
		{
			if (AD > AP + 50 && AD < AP - 50) {
				direction = 0;
				return direction;
			}
			else if (AD < AP) {
				direction = -1;
				return direction;
			}	
			else if (AD > AP) {
				direction = 1;
				return direction;
			}
		return 0;
		}

		int DifferenceAngle(int AP,int AD)
		{
			distance = AD - AP;
			return distance;
		}
}