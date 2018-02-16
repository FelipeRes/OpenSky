using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenSky;

public class Ambient : MonoBehaviour {

	public Biome biome1;
	public Biome biome2;
	public BiomeManager manager;
	public GameObject player;

	// Use this for initialization
	void Start () {
		BaseTime.velocity = 1000;
		manager = new BiomeManager ();
		biome1 = new Biome ();
		biome2 = new Biome ();
		biome1.SetPosition (50, 1, 0);
		biome2.SetPosition (-50, 1, 0);
		biome1.fog = t => 30;
		biome2.fog = t => 20;
		manager.AddBiome ("Bioma 1", biome1);
		manager.AddBiome ("Bioma 2", biome2);
	}
	
	// Update is called once per frame
	void Update () {
		manager.SetPosition (player.transform.position.x, player.transform.position.y, player.transform.position.z);
		Debug.Log (manager.LocalFog(Time.time));
		//Debug.Log (manager.DistanceFromBiome (biome1));
		//Debug.Log (manager.localTemperature (Time.time));
	}
}
