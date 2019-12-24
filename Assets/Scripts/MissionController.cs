using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;

public class MissionController : MonoBehaviour {

	RocketController rocket;

	// Use this for initialization
	void Start () {
		rocket = FindObjectOfType<RocketController> ();
		StartCoroutine (startMission());
		StartCoroutine (addPitch());
		StartCoroutine (initLanding());
	}
	
	IEnumerator startMission(){
		print ("Wait 3 seconds...");
		yield return new WaitForSeconds (3);
		print ("Start mission!");
		rocket.goToState (RocketController.State.LAUNCH);
	}

	IEnumerator addPitch(){
		print ("Wait 6 seconds...");
		yield return new WaitForSeconds (6);
		print ("Add pitch!");
		rocket.aocs.PitchTarget = 45;
	}

	IEnumerator initLanding(){
		print ("Wait 10 seconds...");
		yield return new WaitForSeconds (10);
		print ("Enough fun. Now land!");
		rocket.goToState (RocketController.State.BALLISTIC);
	}

}

