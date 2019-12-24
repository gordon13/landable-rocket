using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PID {

	public float P;
	public float I;
	public float D;

	float outMax;
	float outMin;

	float prevInput;

	public float error;
	float prevError;

	float differentiator;
	float integrator;

	float target;
	float output;

	public PID(float p, float i, float d, float outMax = 50, float outMin = -50, float initialError = 0f, float target = 0f) {
		this.outMax = outMax;
		this.outMin = outMin;
		this.P = p;
		this.I = i;
		this.D = d;
		this.prevError = initialError;
		this.target = target;
	}

	public void SetPID(float p, float i, float d) {
		this.P = p;
		this.I = i;
		this.D = d;
	}

	public void SetLimits(float min, float max) {
		this.outMax = max;
		this.outMin = min;
	}

	public void SetTarget(float _target) {
		this.target = _target;
	}

	public float Calculate (float dt, float input) {
		// proportional
		this.error = target - input;

		// integrator
		this.integrator += this.error / dt;
		if (this.integrator > this.outMax)
			this.integrator = this.outMax;
		else if (this.integrator < outMin)
			this.integrator = this.outMin;

		// differentiator
		this.differentiator = (input - prevInput) / dt;



		this.output = P * this.error + I * this.integrator - D * this.differentiator;
		if (this.output > this.outMax)
			this.output = this.outMax;
		else if (this.output < outMin)
			this.output = this.outMin;

		// lagging variables
		this.prevInput = input;

		return this.output;
	}

	void Cycle () {

	}
}






/*
double error = Setpoint - Input;

ITerm+= (ki * error);
if(ITerm> outMax) ITerm= outMax;
else if(ITerm< outMin) ITerm= outMin;

double dInput = (Input - lastInput);

Output = kp * error + ITerm- kd * dInput;
if(Output > outMax) Output = outMax;
else if(Output < outMin) Output = outMin;

lastInput = Input;
lastTime = now;


void SetTunings(double Kp, double Ki, double Kd)
{
	double SampleTimeInSec = ((double)SampleTime)/1000;
	kp = Kp;
	ki = Ki * SampleTimeInSec;
	kd = Kd / SampleTimeInSec;
}*/