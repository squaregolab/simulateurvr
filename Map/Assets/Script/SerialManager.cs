// <copyright file="GUIManager.cs" company="dyadica.co.uk">
// Copyright (c) 2010, 2014 All Right Reserved, http://www.dyadica.co.uk

// This source is subject to the dyadica.co.uk Permissive License.
// Please see the http://www.dyadica.co.uk/permissive-license file for more information.
// All other rights reserved.

// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>

// <author>SJB</author>
// <email>SJB@dyadica.co.uk</email>
// <date>04.09.2013</date>
// <summary>A MonoBehaviour type class containing an example gui that can be used to 
// communicate with the UnitySerialPort.cs script</summary>

using UnityEngine;
using System.Collections;

public class SerialManager : MonoBehaviour 
{
	private ARDTest ARDTest;
	public static SerialManager Instance;

	void Awake ()
	{
		Instance = this;
	}

	void Start () 
	{
		ARDTest = ARDTest.Instance;
	}

	void Update () 
	{
		Debug.Log (ARDTest.RawData);
	}

/*	void OnGUI ()
	{

		Debug.Log(unitySerialPort.RawData);

	}*/
}
