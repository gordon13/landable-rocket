using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class Engine : MonoBehaviour {

	public float MaxThrust;
	public bool Gimbable;
	public float pitch;
	public float yaw;
	public float Isp;
	public FuelTank fueltank;

	public float _throttle;
	public float Throttle {
		get {
			return _throttle;
		}
		set {
			_throttle = Mathf.Clamp (value, 0f, 1f);
		}
	}

	public float Thrust {
		get {
			return Throttle * MaxThrust;
		}
	}

	public float FuelFlowRate {
		get {
			return Thrust / Isp;
		}
	}

	private Rigidbody _mainRigidBody;
	private Vector3 forceVector;
	private Quaternion gimbalQuat;

	void Start () {
		_mainRigidBody = GetComponentInParent<Rigidbody> ();
	}

	//findNextRigidBody
	void setThrust(float forceMag) {
		if (_mainRigidBody) {
			if (fueltank && !fueltank.IsEmpty) {
				forceVector = gameObject.transform.up * forceMag;
				_mainRigidBody.AddForceAtPosition (forceVector, gameObject.transform.position);
				fueltank.FuelMass -= FuelFlowRate * Time.deltaTime;
			} else {
				forceVector = Vector3.zero;
				print ("No fuel (" + gameObject.name + ")");
			}
		}
	}

	private void setGimbalAngle(float pitch, float yaw) {
		if (Gimbable) {
			gimbalQuat = Quaternion.Euler (pitch, 0, yaw);
			gameObject.transform.rotation = gimbalQuat;
		}
	}

	void Update () {
		setThrust (Thrust);
		if (Gimbable)
			setGimbalAngle (pitch, yaw);
		//Debug.DrawLine (gameObject.transform.position, gameObject.transform.position - new Vector3(0, 1, 0));
		Debug.DrawLine (gameObject.transform.position, gameObject.transform.position - forceVector);
	}
}
