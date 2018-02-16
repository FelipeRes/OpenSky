using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenSky;

public class Implementation : MonoBehaviour {

	public Biome biome;
	double[] diary = { 18, 19, 20, 21, 25, 28, 25, 24, 24, 23, 22, 21 };
	double[] diary2 = { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24 };

	// Use this for initialization
	void Start () {
		biome = new Biome ("basic");
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log (biome.temperature(Time.time/1000));
	}
}
