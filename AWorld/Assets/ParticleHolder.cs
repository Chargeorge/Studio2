using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
public class ParticleHolder : MonoBehaviour {
	
	private Dictionary<DirectionEnum, GameObject> directionalSystems;
	// Use this for initialization
	void Start () {
		directionalSystems = new Dictionary<DirectionEnum, GameObject>();
		var values = (DirectionEnum[])Enum.GetValues(typeof(DirectionEnum));
		foreach (DirectionEnum d in values ){
			directionalSystems.Add(d, transform.Find(d.ToString()).gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	public void blowInfluence(TeamInfo T, float influenceThisFrame){
		float totalInfluence = influenceThisFrame / Time.deltaTime; 
		
		int influenceToBlow = Mathf.RoundToInt(totalInfluence/25f);
		
	}
}
