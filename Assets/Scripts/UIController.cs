using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using System;

public class UIController : MonoBehaviour {

	public RocketController rocket;
	public Text mass;
	public Text fuelValue;
	public Text fuelRate;
	public Text rocketState;
	public Text altitude;
	public Text attitudex;
	public Text attitudey;
	public Text attitudez;
	public Camera camera;

	public void Update(){
		mass.text = ""+rocket.Mass;
		fuelValue.text = ""+rocket.fueltanks [0].FuelMass;
		fuelRate.text = ""+(rocket.staticEngines[0].FuelFlowRate + 
							rocket.staticEngines[1].FuelFlowRate +
							rocket.staticEngines[2].FuelFlowRate +
							rocket.staticEngines[3].FuelFlowRate +
							rocket.staticEngines[4].FuelFlowRate +
							rocket.staticEngines[5].FuelFlowRate +
							rocket.gimballedEngine.FuelFlowRate);
		rocketState.text = ""+rocket.CurrentState;
		altitude.text = ""+rocket.aocs.Altitude;
		attitudex.text = ""+rocket.aocs.Pitch;
		attitudey.text = ""+rocket.aocs.Yaw;
		attitudez.text = ""+rocket.aocs.Roll;

		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll != 0.0f) {
			float fov = camera.fieldOfView;
			fov += scroll * 10f;
			fov = Mathf.Clamp (fov, 5f, 90f);
			camera.fieldOfView = fov;
		}

	}
}
