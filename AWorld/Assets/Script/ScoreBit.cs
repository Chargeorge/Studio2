using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBit : MonoBehaviour {
	List<GameObject> targets;
	private TeamInfo team;
	private FinalScoreTarget finalTarget;
	public Settings sRef;

	// Use this for initialization
	void Start () {
		if(targets == null){
			targets = new List<GameObject>();
		}
	}

	public void setTeam(TeamInfo T){
		team = T;
	//	renderer.material.color = team.getHighLightColor();
		switch (T.teamNumber) {
			case 1: 
				finalTarget = GameObject.Find ("GameManager").GetComponent<GameManager>().home1.transform.FindChild ("ScoreBitFinalTarget").gameObject.GetComponent<FinalScoreTarget>();
				break;
			case 2: 
				finalTarget = GameObject.Find ("GameManager").GetComponent<GameManager>().home2.transform.FindChild ("ScoreBitFinalTarget").gameObject.GetComponent<FinalScoreTarget>();
				break;
		}

	}
	
	// Update is called once per frame
	void Update () {
		if(targets.Count > 0){
			Vector2 NewPos  =  Vector2.MoveTowards( (Vector2)transform.position, (Vector2)(targets[0].transform.position), .2f);
			Vector3 NewPos3 = new Vector3(NewPos.x, NewPos.y, transform.position.z);
			transform.position = NewPos3;
		}
		if (finalTarget != null && closeEnoughToTarget (transform.position, finalTarget.transform.position, sRef.closeEnoughDistanceScoreBit)) {
//			Debug.Log ("Collision detected");
			finalTarget.PlayScoreAnimation ();
			BulletPool.instance.PoolObject(gameObject);
			team.addScore(Settings.SettingsInstance.vpsScorePerMinePerSecond);
		}
	}
	
	void OnTriggerEnter2D(Collider2D collided){
		if(targets.Count>0){
			if(collided.gameObject == targets[0]){

				if(collided.gameObject.tag == "ScoreBitTarget"){
					if(targets.Count > 0){
						targets.RemoveAt(0);
					
						setTarget(targets[0]);

					}
				}

			}
		}
		if(collided.gameObject.tag == "ScoreBitFinalTarget"){
//			Debug.Log ("Collision detected");
//			collided.gameObject.SendMessage("PlayScoreAnimation");
//			BulletPool.instance.PoolObject(gameObject);
//			team.addScore(Settings.SettingsInstance.vpsScorePerMinePerSecond);
		}
	}

	void setTarget(GameObject target){
		Vector2 newTrans =  target.transform.position - transform.position;

		float angleTo =   Vector2.Angle(transform.up , newTrans.normalized);
//		Debug.Log(angleTo);
//		Debug.Log (newTrans);
		transform.eulerAngles = new Vector3(0,0,0);

		transform.Rotate(new Vector3(0,0,angleTo));
	}

	public void start(List<AStarholder> tiles){
		targets = new List<GameObject>();

		tiles.ForEach(delegate (AStarholder tile){
			targets.Add(tile.current.scoreBitTarget);
		});
		targets.Add (team.goGetHomeTile());
		setTarget(targets[0]);
		Invoke ("remove", 3f);
	}

	public void remove(){
		BulletPool.instance.PoolObject(gameObject);
	}
	
	public bool closeEnoughToTarget (Vector3 newPos, Vector3 target, float closeEnoughDistance) {
		//Hoo boy
		return Mathf.Abs (newPos.x - target.x) + Mathf.Abs (newPos.y - target.y) <= closeEnoughDistance; 
	}
	
	
}
