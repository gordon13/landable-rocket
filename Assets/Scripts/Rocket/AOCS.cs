using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOCS : MonoBehaviour {

	[Header("Hardware")]
	public GridFin[] GridFins;

	[Header("Attitude")]
	public float Velocity;
	public float Altitude;
	public float Pitch;
	public float Roll;
	public float Yaw;
	public float AltitudeRate;
	public float PitchRate;
	public float RollRate;
	public float YawRate;
	public float PitchForce;
	public float RollForce;
	public float YawForce;

	[Header("Attitude control")]
	public bool enablePitch;
	public float pitchP = 0f;
	public float pitchI = 0f;
	public float pitchD = 0f;
	public float pitchError;
	[Space(10)]
	public bool enableYaw;
	public float yawP = 0f;
	public float yawI = 0f;
	public float yawD = 0f;
	public float yawError;
	[Space(10)]
	public bool enableRoll;
	public float rollP = 0f;
	public float rollI = 0f;
	public float rollD = 0f;
	public float rollError;


	[Header("Flight")]
	public FlightMode flightMode;
	public enum FlightMode {
		Manual,
		StraightUp,
		BallisticTrajectory
	};


	[Header("Manual Controls")]
	public float PitchTarget;
	public float RollTarget;
	public float YawTarget;
	public float PitchRateTarget;
	public float RollRateTarget;
	public float YawRateTarget;


	[Header("Thrusters")]
	public ThrusterCluster ThrusterCluster1;
	public ThrusterCluster ThrusterCluster2;
	public float MaxThrust;

	Vector3 _thrusterCluster1WorldPosition;
	Vector3 _thrusterCluster2WorldPosition;

	Vector3 _thrusterCluster1ForceVector;
	Vector3 _thrusterCluster2ForceVector;

	Vector3 origin;

	Rigidbody _rigidBody;
	PID pitchPid;
	PID yawPid;
	PID rollPid;

	bool _finsDeployed = false;

	// Use this for initialization
	void Start () {
		
		_rigidBody = GetComponentInParent<Rigidbody> ();
		// pitch
		pitchPid = new PID (pitchP, pitchI, pitchD, MaxThrust, -MaxThrust);
		// yaw
		yawPid = new PID (yawP, yawI, yawD, MaxThrust, -MaxThrust);
		// roll
		rollPid = new PID (rollP, rollI, rollD, MaxThrust, -MaxThrust);
	}

	void Update () {
		
		// initial stuff
		origin = transform.position;
		Altitude = transform.position.y;
		AltitudeRate = _rigidBody.velocity.y;
		Velocity = _rigidBody.velocity.magnitude;
		/*rotation = Quaternion.FromToRotation(Vector3.up, transform.up);
		rotation.w = transform.rotation.w;*/

		// forces positions
		_thrusterCluster1WorldPosition = ThrusterCluster1.transform.position;
		_thrusterCluster2WorldPosition = ThrusterCluster2.transform.position;
		/*_thrusterCluster1WorldPosition = origin + transform.right * thrusterPos.x + transform.up * thrusterPos.y + transform.forward * thrusterPos.z;
		_thrusterCluster2WorldPosition = origin + transform.right * thrusterPos2.x + transform.up * thrusterPos2.y + transform.forward * thrusterPos2.z;*/

		// update axes
		if (enableYaw)
			yawUpdate();
		if (enablePitch)
			pitchUpdate();
		if (enableRoll)
			rollUpdate();

		// draw thrusters
		Debug.DrawLine (origin, _thrusterCluster1WorldPosition, Color.white);
		Debug.DrawLine (origin, _thrusterCluster2WorldPosition, Color.blue);


		/*_thrusterCluster1WorldPosition = ThrusterCluster1.transform.position;
		_thrusterCluster2WorldPosition = ThrusterCluster2.transform.position;
		Debug.DrawLine (_thrusterCluster1WorldPosition, _thrusterCluster1WorldPosition - (0.1f *_thrusterCluster1ForceVector));
		Debug.DrawLine (_thrusterCluster2WorldPosition, _thrusterCluster2WorldPosition - (0.1f * _thrusterCluster2ForceVector));
		_rigidBody.AddForceAtPosition(_thrusterCluster1ForceVector, _thrusterCluster1WorldPosition);
		_rigidBody.AddForceAtPosition(_thrusterCluster2ForceVector, _thrusterCluster2WorldPosition);*/
		Pitch = transform.rotation.eulerAngles.x;
		Yaw = transform.rotation.eulerAngles.y;
		Roll = transform.rotation.eulerAngles.z;

		/* Update pid targets */
		pitchPid.SetTarget (PitchTarget);
		yawPid.SetTarget (YawTarget);
		rollPid.SetTarget (RollTarget);
	}

	void rollUpdate() {
		// input
		float roll = (transform.parent.localEulerAngles.y < 180f) ? transform.parent.localEulerAngles.y : transform.parent.localEulerAngles.y - 360;

		// roll PID
		Vector3 force = transform.forward * rollPid.Calculate (Time.deltaTime, roll);
		rollError = rollPid.error;
		RollForce = force.magnitude;

		// add force
		_rigidBody.AddForceAtPosition (-force, _thrusterCluster1WorldPosition);
		_rigidBody.AddForceAtPosition (force, _thrusterCluster2WorldPosition);

		// draw debug
		Debug.DrawLine (_thrusterCluster1WorldPosition, _thrusterCluster1WorldPosition + (-1) * force, Color.yellow);
		Debug.DrawLine (_thrusterCluster2WorldPosition, _thrusterCluster2WorldPosition + (-1) * force, Color.yellow);
	}

	void pitchUpdate() {
		// input
		float pitch = (transform.parent.localEulerAngles.x < 180f) ? transform.parent.localEulerAngles.x : transform.parent.localEulerAngles.x - 360;

		// picth PID
		Vector3 force = transform.forward * pitchPid.Calculate (Time.deltaTime, pitch);
		pitchError = pitchPid.error;
		PitchForce = force.magnitude;

		// add force
		_rigidBody.AddForceAtPosition (force, _thrusterCluster1WorldPosition);
		_rigidBody.AddForceAtPosition (force, _thrusterCluster2WorldPosition);

		// draw debug
		Debug.DrawLine (_thrusterCluster1WorldPosition, _thrusterCluster1WorldPosition + (-1) * force, Color.yellow);
		Debug.DrawLine (_thrusterCluster2WorldPosition, _thrusterCluster2WorldPosition + (-1) * force, Color.yellow);
	}

	void yawUpdate() {
		// input
		float yaw = (transform.parent.localEulerAngles.z < 180f) ? transform.parent.localEulerAngles.z : transform.parent.localEulerAngles.z - 360;

		// yaw PID
		Vector3 force = -1 * transform.right * yawPid.Calculate (Time.deltaTime, yaw);
		yawError = yawPid.error;
		YawForce = force.magnitude;

		// add force
		_rigidBody.AddForceAtPosition (force, _thrusterCluster1WorldPosition);
		_rigidBody.AddForceAtPosition (force, _thrusterCluster2WorldPosition);

		// draw debug
		Debug.DrawLine (_thrusterCluster1WorldPosition, _thrusterCluster1WorldPosition + (-1) * force, Color.yellow);
		Debug.DrawLine (_thrusterCluster2WorldPosition, _thrusterCluster2WorldPosition + (-1) * force, Color.yellow);
	}











	public void StopThrusters() {
		RollRateTarget = 0;
		YawRateTarget = 0;
		PitchRateTarget = 0;
	}

	public void DeployGridFins() {
		if (!_finsDeployed) {
			MoveFins (90);
			_finsDeployed = true;
		}
	}

	public void StowGridFins() {
		if (_finsDeployed) {
			MoveFins (0);
			_finsDeployed = false;
		}
	}

	public void MoveFins(float angle){
		foreach (GridFin fin in GridFins) {
			Quaternion temp = fin.transform.rotation;
			temp.eulerAngles = new Vector3(temp.eulerAngles.x + angle, temp.eulerAngles.y, temp.eulerAngles.z);
			fin.transform.localRotation = temp;
		}
	}
}
