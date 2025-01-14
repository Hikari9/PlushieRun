﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class Arduino : MonoBehaviour {

	// static string CommPort = SerialPort.GetPortNames()[0];
	static int BaudRate = 9600;
	static Arduino ard = null;

	public static Vector3 accelerometer
	{

		get { return new Vector3(-a.x, a.z, -a.y); }
	}


	public static float gravity
	{

		get { return accelerometer.magnitude; }
	}


	public static Quaternion accelQuaternion
	{

		get { return Quaternion.FromToRotation(Vector3.up, accelerometer); }
	}


	public static Vector3 accelEuler
	{

		get { return accelQuaternion.eulerAngles; }
	}


	public static Vector3 gyroscope // degrees per second
	{

		get { return new Vector3(g.x, g.z, g.y); }
	}


	public static Vector3 filteredAccelerometer // degrees
	{

		get {
			return look * accelerometer.magnitude;
		}
	}


	public static Quaternion filteredAccelQuaternion
	{

		get {
			return Quaternion.FromToRotation (Vector3.up, filteredAccelerometer);
		}
	}


	public static Vector3 filteredAccelEuler
	{

		get { return filteredAccelQuaternion.eulerAngles; }
	}

	static Vector3 a, g;
	static Vector3 look;
	static SerialPort Serial;
	const int Bytes = 22;
	static byte[] buffer = new byte[Bytes];
	static int ptr = 0;

	// Use this for initialization
	void Start () {
		ard = this;
		// Serial = new SerialPort ();
		if (SerialPort.GetPortNames ().Length == 0)
			return;
		Serial = new SerialPort (SerialPort.GetPortNames()[0], BaudRate);
		Serial.ReadTimeout = BaudRate;
		Serial.Open ();
		Read ();
		Parse ();
		look = accelerometer.normalized;
		ComplementaryFilter ();
	}

	// Update is called once per frame
	void Update () {
		Read ();
		Parse ();
		ComplementaryFilter ();
	}

	static void Read() {
		if (Serial == null || !Serial.IsOpen) {
			ard.Start ();
			return;
		}
		int Threshold = Bytes;
		while (Threshold-- > 0) {
			buffer[ptr++] = (byte) Serial.ReadByte();
			if (ptr == buffer.Length) ptr = 0;
		}
	}

	static void Parse() {
		if (Serial == null || !Serial.IsOpen) {
			ard.Start ();
			return;
		}
		try {
			MemoryStream ms = new MemoryStream(buffer);
			BinaryReader br = new BinaryReader(ms);
			a = new Vector3(br.ReadInt16 (), br.ReadInt16 (), br.ReadInt16 ());
			g = new Vector3(br.ReadSingle (), br.ReadSingle (), br.ReadSingle());
		}
		catch {
			Debug.Log ("Error in parsing");
		}
	}

	
	static float trustGyro = 5f;

	static void ComplementaryFilter() {
		if (Serial == null || !Serial.IsOpen) {
			ard.Start ();
			return;
		}
		Quaternion delta = Quaternion.Euler (gyroscope * Time.deltaTime);
		Vector3 gLook = delta * look;
		look = (accelerometer.normalized + gLook * trustGyro) / (1 + trustGyro);
	}

	static float idleMag = 10; // degrees per second
	static float idleTimeAllowance = 0.17f; // seconds
	static float firstIdleTime = 0;
	public static bool isIdle = false;

	static bool IsIdle() {
		if (gyroscope.x < idleMag && gyroscope.y < idleMag && gyroscope.z < idleMag) {
			// trigger idle
			firstIdleTime = Time.time;
			isIdle = true;
		} else if (Time.time - firstIdleTime >= idleTimeAllowance) {
			// not idle anymore
			isIdle = false;
		}
		return isIdle;
	}


	void OnDestroy() {
		if (Serial != null)
			Serial.Close ();
	}

	void OnApplicationQuit() {
		if (Serial != null)
			Serial.Close ();
	}

}
