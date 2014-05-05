using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBit : MonoBehaviour {
	List<GameObject> targets;
	private TeamInfo team;
	private FinalScoreTarget finalTarget;
	public Settings sRef;
	public float speed;
	public float rotateSpeed;
	public float scoreAmt;
	
	public bool bigBit;

	// Use this for initialization
	void Start () {
		if(targets == null){
			targets = new List<GameObject>();
		}
		
		switch (PlayerPrefs.GetInt (PreferencesOptions.gameSpeed.ToString())) {
		case 1: 
			speed = 0.1f;
			break;
		case 2:
			speed = 0.2f;
			break;
		case 3:
			speed = 0.3f;
			break;
		default:
			speed = 0.2f;
			Debug.LogWarning ("Game speed was a weird value while setting score bit speed");
			break;
		}
		if (bigBit) { 
			speed = 0.05f;
			rotateSpeed = 100.0f;
		}
		else {
			rotateSpeed = 250.0f;
		}
			
	}

	public void setTeam(TeamInfo T){


		team = T;
		Color32 scoreBitColor = team.beaconColor;
		scoreBitColor.a = 170;


	//	renderer.material.color = scoreBitColor;
		renderer.material.color = new Color32(237, 20, 90, 180);
		
		finalTarget = T.ScoreBar.transform.FindChild ("ScoreBitFinalTarget").gameObject.GetComponent<FinalScoreTarget>();
		

	}
	
	// Update is called once per frame
	void Update () {
		
		if (!Pause.paused) {	
		
			if (bigBit) { //It's not loading in Start correctly sometimes, so just setting it every update... not optimized but should be okay
				speed = 0.05f;
				rotateSpeed = 100.0f;
			}
		
			transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * Time.deltaTime);
			
			//	transform.RotateAround (transform.position, Vector3.forward, 0.2f * Time.deltaTime);
	
			if(targets.Count > 0){
				Vector2 NewPos  =  Vector2.MoveTowards( (Vector2)transform.position, (Vector2)(targets[0].transform.position), speed);
				Vector3 NewPos3 = new Vector3(NewPos.x, NewPos.y, transform.position.z);
				
				//I'm doing this twice just in case something sneaks inside the collider
				if(transform.position == NewPos3){
					
					if(targets.Count > 0){
						targets.RemoveAt(0);
						
						setTarget(targets[0]);
						 NewPos  =  Vector2.MoveTowards( (Vector2)transform.position, (Vector2)(targets[0].transform.position), speed);
						 NewPos3 = new Vector3(NewPos.x, NewPos.y, transform.position.z);
						
						}
					}
					if (targets.Count == 1) {
						//Moving to final target
						transform.renderer.material.color = team.teamColor;
						if (team.teamNumber == 1) GameObject.Find ("GameManager").GetComponent<GameManager>().home1.GetComponent<Home>().Jiggle (0.1f, 0.05f);
						else GameObject.Find ("GameManager").GetComponent<GameManager>().home2.GetComponent<Home>().Jiggle (0.1f, 0.05f);
					}
	
				transform.position = NewPos3;
				
			}	
	
			if (finalTarget != null && closeEnoughToTarget (transform.position, finalTarget.transform.position, sRef.closeEnoughDistanceScoreBit)) {
	//			Debug.Log ("Collision detected");
				float percToWin = team.score / sRef.valPointsToWin;
				if (percToWin > 1f) percToWin = 1f;
				finalTarget.GetComponent<ParticleSystem>().startSize = sRef.scoreBitExplosionStartSize + ((sRef.scoreBitExplosionFinishSize-sRef.scoreBitExplosionStartSize)*percToWin);
				finalTarget.GetComponent<ParticleSystem>().startSpeed = sRef.scoreBitExplosionStartSpeed + ((sRef.scoreBitExplosionFinishSpeed-sRef.scoreBitExplosionStartSpeed)*percToWin);
				finalTarget.GetComponent<ParticleSystem>().startColor = team.teamColor;
				
				if (team.score >= sRef.valPointsToWin) {
					finalTarget.PlayScoreAnimation (100);
				} else {
					finalTarget.PlayScoreAnimation (10);
				}
				
				BulletPool.instance.PoolObject(gameObject);
				team.addScore(scoreAmt);
				//CancelInvoke();
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D collided){
		if(targets.Count>0){
			//int target0Ident = targets[0].gameObject.GetComponent<BaseTile>().Ident;
			//int collidedTarget = collided.gameObject.GetComponent<BaseTile>().Ident;
			if(collided.gameObject == targets[0]){
//				Debug.Log ("Collided");
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

//		float angleTo =   Vector2.Angle(transform.up , newTrans.normalized);
//		Debug.Log(angleTo);
//		Debug.Log (newTrans);
		//transform.eulerAngles = new Vector3(0,0,0);

//		transform.Rotate(new Vector3(0,0,angleTo));
	}

	public void start(List<AStarholder> tiles){
		targets = new List<GameObject>();

		tiles.ForEach(delegate (AStarholder tile){
			targets.Add(tile.current.scoreBitTarget);
		});
		targets.Add (team.ScoreBar.transform.Find ("ScoreBitFinalTarget").gameObject);
		setTarget(targets[0]);
		//Invoke ("remove", 5f);
	}

	public void remove(){
		BulletPool.instance.PoolObject(gameObject);
		team.addScore(Settings.SettingsInstance.vpsScorePerBit);
	}
	
	public bool closeEnoughToTarget (Vector3 newPos, Vector3 target, float closeEnoughDistance) {
		//Hoo boy
		return Mathf.Abs (newPos.x - target.x) + Mathf.Abs (newPos.y - target.y) <= closeEnoughDistance; 
	
	
	
}
}
