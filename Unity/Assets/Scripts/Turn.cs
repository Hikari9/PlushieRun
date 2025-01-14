﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turn : MonoBehaviour {

	Vector3 gyroangle = Vector3.zero;

	public Quaternion orientation {
		get {
			Quaternion gyro = Quaternion.Euler (gyroangle);
			return gyro;
		}
	}

	public float turnAngle = 50f;
	public float dampness = 10f;
	public bool allowArrowKeys = true;

	public bool LeftTurn() {
		bool flag = (gyroangle.y < -turnAngle);
		if (allowArrowKeys)
			flag |= Input.GetKeyDown (KeyCode.LeftArrow);
		return flag;
	}

	public bool RightTurn() {
		bool flag = (gyroangle.y > turnAngle);
		if (allowArrowKeys)
			flag |= Input.GetKeyDown (KeyCode.RightArrow);
		return flag;
	}

	Quaternion target = Quaternion.identity;
	bool turning = false;

	void Start() {
		StartCoroutine (Repositioner ());
	}

	IEnumerator Repositioner() {
		while (true) {
			if (!turning) {
				gyroangle = Vector3.zero;
				yield return new WaitForSeconds(5f);
			}
			else {
				yield return new WaitForEndOfFrame();
			}
		}
	}

	// Update is called once per frame
	void Update () {

		gyroangle +=  Arduino.gyroscope * Time.deltaTime;

		if (LeftTurn () && !turning) {
			target *= Quaternion.FromToRotation (Vector3.forward, Vector3.left);
		}
		if (RightTurn () && !turning) {
			target *= Quaternion.FromToRotation (Vector3.forward, Vector3.right);
		}

		UpdateRotation ();

	}
	

	void UpdateRotation() {

		transform.parent.localRotation = Quaternion.Slerp (transform.parent.localRotation, target, Time.smoothDeltaTime * dampness);
		turning = Mathf.Abs(Quaternion.Angle (transform.parent.localRotation, target)) > 1;

	}

}
