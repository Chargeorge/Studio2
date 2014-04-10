﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBit : MonoBehaviour {
	List<GameObject> targets;
	// Use this for initialization
	void Start () {
		targets = new List<GameObject>();
		targets.Add (GameObject.Find ( "ScoreBitTarget"));
		targets.Add (GameObject.Find ( "ScoreBitFinalTarget"));
		
		setTarget(targets[0]);

	}
	
	// Update is called once per frame
	void Update () {
		Vector2 NewPos  =  Vector2.MoveTowards( (Vector2)transform.position, (Vector2)(targets[0].transform.position), .2f);
		Vector3 NewPos3 = new Vector3(NewPos.x, NewPos.y, transform.position.z);
		transform.position = NewPos3;
	}

	void OnTriggerEnter2D(Collider2D collided){
		if(collided.gameObject == targets[0]){

			if(collided.gameObject.tag == "ScoreBitTarget"){
				if(targets.Count > 0){
					targets.RemoveAt(0);
				
					setTarget(targets[0]);
					GetComponent<ParticleSystem>().Emit(20);
				}
			}
			if(collided.gameObject.tag == "ScoreBitFinalTarget"){
				collided.gameObject.SendMessage("PlayScoreAnimation");
			}
		}
	}

	void setTarget(GameObject target){
		Vector2 newTrans =  target.transform.position - transform.position;

		float angleTo =   Vector2.Angle(transform.up , newTrans.normalized);
		Debug.Log(angleTo);
		Debug.Log (newTrans);
		transform.eulerAngles = new Vector3(0,0,0);

		transform.Rotate(new Vector3(0,0,angleTo));
	}
}
