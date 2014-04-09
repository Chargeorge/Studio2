using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBit : MonoBehaviour {
	List<GameObject> targets;
	// Use this for initialization
	void Start () {
		targets = new List<GameObject>();
		targets.Add (GameObject.Find ( "ScoreBitTarget"));
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 NewPos  =  Vector2.MoveTowards( (Vector2)transform.position, (Vector2)(targets[0].transform.position), .4f);
		Vector3 NewPos3 = new Vector3(NewPos.x, NewPos.y, transform.position.z);
		
		
		transform.position = NewPos3;
	}
}
