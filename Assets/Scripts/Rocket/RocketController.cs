using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.CodeDom.Compiler;
using System;
using System.Security.Cryptography;
using System.Diagnostics;
using NUnit.Framework.Constraints;

public class RocketController : MonoBehaviour {

	public float Throttle;
	public Engine gimballedEngine;
	public Engine[] staticEngines;
	public LandingLeg[] landingLegs;
	public bool LegsDeployed;
	public float InertMassFraction;
	public FuelTank[] fueltanks;
	public float Mass {
		get{
			return _rigidBody.mass;
		}
	}

	public AOCS aocs;
	private Rigidbody _rigidBody;

	// Use this for initialization
	void Start () {
		aocs = GetComponentInChildren<AOCS> ();
		fueltanks = GetComponentsInChildren<FuelTank> ();
		_rigidBody = GetComponent<Rigidbody> ();
		_rigidBody.mass = InertMassFraction * fueltanks [0].FuelMass;
		initialiseMotors ();
		initStateMachine (State.IDLE);
	}

	// Update is called once per frame
	void Update () {
		updateStateMachine ();
		refreshRocketMass ();
	}

	void initialiseMotors(){
		foreach (Engine engine in staticEngines){
			engine.fueltank = fueltanks[0];
		}
		gimballedEngine.fueltank = fueltanks [0];
	}

	public void setAllStaticEnginesThrottle(float throttle){
		foreach (Engine engine in staticEngines){
			engine.Throttle = throttle;
		}
	}


	// ========   state machine
	public State CurrentState {
		get{
			return _currentState;
		}
		private set {
			_currentState = value;
		}
	}

	State _currentState;
	public enum State {
		IDLE,
		LAUNCH,
		BALLISTIC,
		EDL,
		LANDED

	}

	public void initStateMachine(State defaultState){
		_currentState = defaultState;
		switch (_currentState) {
		case State.IDLE:
			aocs.StowGridFins ();
			DeployLegs ();
			break;
		case State.LAUNCH:
			aocs.StowGridFins ();
			StowLegs ();
			break;
		case State.BALLISTIC:
			aocs.DeployGridFins ();
			StowLegs ();
			break;
		case State.EDL:
			aocs.DeployGridFins ();
			StowLegs ();
			break;
		case State.LANDED:
			aocs.StowGridFins ();
			DeployLegs ();
			break;
		default:
			print ("State not supported: " + _currentState);
			break;
		}
		//gimballedEngine.setGimbalAngle(0, 0);
	}

	// ======== this is where we define actions to perform during transitions
	public void goToState(State nextState) {
		switch (_currentState) {
		case State.IDLE:
			switch (nextState) {
			case State.LAUNCH: // LAUNCH -> EDL
				print ("State change: IDLE -> LAUNCH");
				StowLegs ();
				_currentState = nextState;
				break;
			default:
				break;
			}
			break;

		case State.LAUNCH:
			switch (nextState) {
			case State.BALLISTIC: // LAUNCH -> EDL
				print ("State change: LAUNCH -> BALLISTIC");
				setAllStaticEnginesThrottle (0);
				//gimballedEngine.setGimbalAngle (0, 0);
				gimballedEngine.Throttle = 0;
				aocs.DeployGridFins ();

				_currentState = nextState;
				break;
			default:
				break;
			}
			break;
		
		case State.BALLISTIC:
			switch (nextState) {
			case State.EDL: // ENTRY_DECENT -> LANDING
				print ("State change: BALLISTIC -> EDL");
				_currentState = nextState;
				break;
			default:
				break;
			}
			break;

		case State.EDL:
			switch (nextState) {
			case State.LANDED: // ENTRY_DECENT -> LANDING
				print ("State change: EDL -> LANDED");
				setAllStaticEnginesThrottle (0);
				gimballedEngine.Throttle = 0;
				DeployLegs ();
				_currentState = nextState;
				break;
			default:
				break;
			}
			break;

		case State.LANDED:
			break;

		default:
			break;
		}
	}

	// ======== this is where we define what happens in each state
	public void updateStateMachine(){
		switch (_currentState) {
		case State.LAUNCH:
			setAllStaticEnginesThrottle (Throttle);
			gimballedEngine.Throttle = Throttle;

			//if (aocs.Altitude > 20) {
			//	setAllStaticEnginesThrottle (0);
			//	gimballedEngine.Throttle = 0;
			//}
			
			//if (aocs.Altitude > 20 && aocs.AltitudeRate < 0.3)
			//	goToState (State.EDL);
			break;

		case State.BALLISTIC:
			aocs.PitchTarget = 0;
			aocs.YawTarget = 0;
			aocs.RollTarget = 0;

			if (aocs.AltitudeRate < 3) // stay in ballistic mode untli we start to fall
				goToState (State.EDL);
			break;

		case State.EDL:
			LandingEngineControl ();

			// set target orientation to upward
			aocs.PitchTarget = 0;
			aocs.YawTarget = 0;
			aocs.RollTarget = 0;

			if (aocs.Altitude < 6)
				DeployLegs ();
			else
				StowLegs ();

			if (aocs.Altitude < 3)
				goToState (State.LANDED);
			break;
		
		case State.LANDED:
			break;
		
		default:
			break;
		}
		//gimballedEngine.setGimbalAngle(0, 0);
	}





	// ============== control algos
	Vector3 _prevVelocity = Vector3.zero;
	Vector3 Acceleration = Vector3.zero;

	float correctedThrottle = 0;

	float distanceError = 0;
	float distanceErrorRate = 0;
	float _prevDistanceError = 0;

	float velocityError = 0;
	float velocityErrorRate = 0;
	float _prevVelocityError = 0;

	//float altitudeVelocityCurve = x => { return -9.81 * x;};
	float VelocityAltitudeCurve {
		get {
			return 0.1f * 9.81f * aocs.Altitude;
		}
	}
		
//	public void LandingEngineControl(){
//		Acceleration = (_rigidBody.velocity - _prevVelocity) / Time.fixedDeltaTime;
//
//		velocityError = VelocityAltitudeCurve - _rigidBody.velocity.magnitude;
//		velocityErrorRate = velocityError - _prevVelocityError;
//
//		//correctedThrottle = (velocityError * 0.1f);
//		//print (VelocityAltitudeCurve + ", " + correctedThrottle);
//		//gimballedEngine.Throttle = correctedThrottle;
//		//========
//		//gimballedEngine.Throttle = 0.02f;
//		//========
//		float requiredAccelerationToStop = Mathf.Pow(aocs.Altitude, 2.0f) / (0.5f * _rigidBody.velocity.magnitude);
//		gimballedEngine.Throttle = (Mass * requiredAccelerationToStop) / gimballedEngine.MaxThrust;
//		print ("speed:"+_rigidBody.velocity.magnitude+", acceleration to stop:" + requiredAccelerationToStop + ", throttle:" + gimballedEngine.Throttle);
//
//		_prevVelocity = _rigidBody.velocity;
//
//		_prevVelocityError = velocityError;
//	}

	public void LandingEngineControl(){
		// in other words, suicide burn!
		float totalthrust = gimballedEngine.MaxThrust;
		foreach (Engine e in staticEngines) {
			totalthrust += e.MaxThrust;
		}

		// initiate suicide burn!
		if (aocs.AltitudeRate < 1) {// start controling engine only if altitude rate is negative
			float radarOffset = 2.24f;	 				// The value of alt:radar when landed (on gear)
			float trueRadar = aocs.Altitude - radarOffset;			// Offset radar to get distance from gear to ground
			float g = 9.81f;
			float maxDecel = g - (totalthrust/Mass);	// Maximum deceleration possible (m/s^2)
			float impactTime = -Math.Abs(aocs.AltitudeRate) / maxDecel;		// Time until impact, used for landing gear
			float stopDist = Math.Abs(aocs.AltitudeRate)*impactTime + 0.5f*maxDecel*impactTime*impactTime;		// The distance the burn will require
			float idealThrottle = stopDist / trueRadar;			// Throttle required for perfect hoverslam

			print ("max decel:" + maxDecel);
			print ("impact time:" + impactTime);
			print ("stop distance:" + stopDist);
			print ("total thrust:" + totalthrust);
			print ("ideal throt:" + idealThrottle);
			print ("altitude:" + aocs.Altitude + ", stop distance:" + stopDist + ", time:" + impactTime);
			print ("total thrust:" + (totalthrust) + ", mass:" + Mass + ", velocity:" + aocs.Velocity + ", alt rate:" + aocs.AltitudeRate);


			if (aocs.Altitude < stopDist) {
				//setAllStaticEnginesThrottle (1);
				//gimballedEngine.Throttle = 1;
				//===
				setAllStaticEnginesThrottle (idealThrottle);
				gimballedEngine.Throttle = idealThrottle;
			} else {
				setAllStaticEnginesThrottle (0);
				gimballedEngine.Throttle = 0;
			}
		} else {
			setAllStaticEnginesThrottle (0);
			gimballedEngine.Throttle = 0;
		}
	}



	// =============== utility functions
	public void refreshRocketMass(){
		_rigidBody.mass = InertMassFraction * ((fueltanks [0].IsEmpty) ? 0f : fueltanks[0].FuelMass) + ((fueltanks [0].IsEmpty) ? 0f : fueltanks[0].FuelMass);
	}

	public void DeployLegs() {
		MoveLegsTo (130);
		LegsDeployed = true;
	}

	public void StowLegs() {
		MoveLegsTo (0);
		LegsDeployed = false;
	}

	public void MoveLegsTo(float angle) {
		foreach (LandingLeg leg in landingLegs) {
			Quaternion temp = leg.transform.localRotation;
			temp.eulerAngles = new Vector3(temp.eulerAngles.x+angle, temp.eulerAngles.y, temp.eulerAngles.z);
			leg.transform.localRotation = temp;
		}
	}

}
