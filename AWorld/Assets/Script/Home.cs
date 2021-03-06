﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Home : MonoBehaviour {

	private TeamInfo _team;
	private bool finalChitLaunched = false;
	private bool _jiggling;
	private float _jiggleRange;			//Max distance from center of grid the player will jiggle
	private float endJiggleTime;
	private Vector3 _homePosition;
	
	public TeamInfo team{
		get{
			return _team;
		}
		set{
			_team = value;
			Color32 copy = team.beaconColor;
			renderer.material.color = copy;
			
		}
	}
	//public TeamInfo  team;
	public BaseTile HomeTile{
		get{
			return GameManager.GameManagerInstance.tiles[(int)team.startingLocation.x, (int)team.startingLocation.y].GetComponent<BaseTile>();
		}
	}
	// Use this for initialization
	void Start () {
		_homePosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
//		Color32 copy = new Color32((byte)(team.teamColor.r +30), (byte)(team.teamColor.g-30), (byte)(team.teamColor.b+30), (byte)255);
		TeamInfo otherTeam = (team.teamNumber == 1) ? GameManager.GameManagerInstance.teams[1] : GameManager.GameManagerInstance.teams[0];
		
		if(HomeTile.owningTeam == otherTeam){
			if(HomeTile.checkNetworkToHomeBase() && !finalChitLaunched){
				Vector3 scoreBitStartPos = transform.position;
				
				scoreBitStartPos.z = -1.2f;
				GameObject BigScoreBit = BulletPool.instance.GetObjectForType("ScoreBit", false);
				BigScoreBit.transform.localScale = new Vector3(2f,2f,1f);
				BigScoreBit.transform.position = scoreBitStartPos;
				BigScoreBit.GetComponent<ScoreBit>().bigBit = true;
				BigScoreBit.GetComponent<ScoreBit>().setTeam(HomeTile.owningTeam);
				BigScoreBit.GetComponent<ScoreBit>().start(checkNetwork());
				BigScoreBit.GetComponent<ScoreBit>().sRef = Settings.SettingsInstance;
				BigScoreBit.GetComponent<ScoreBit>().scoreAmt= Settings.SettingsInstance.valScoreBaseCapture;
				finalChitLaunched  = true;
			}
		}
		
		if (!Pause.paused) {
			if (Time.time < endJiggleTime) {
				Vector3 _positionOffset = new Vector3 (Random.Range (-1 * _jiggleRange, _jiggleRange), Random.Range (-1 * _jiggleRange, _jiggleRange), 0);
				Vector3 jigglePos = _homePosition + _positionOffset;
				transform.position = new Vector3 (jigglePos.x, jigglePos.y, transform.position.z);
			}
			else {
				transform.position = _homePosition;
			}
		}
	}
	
	public List<AStarholder> checkNetwork(){
	
		List<AStarholder> As = 	BaseTile.aStarSearch(HomeTile.GetComponent<BaseTile>(),HomeTile.owningTeam.goGetHomeTile().GetComponent<BaseTile>(),int.MaxValue, BaseTile.getLocalSameTeamTiles, HomeTile.owningTeam);
		As.RemoveAt(0);
		return As;

	}
	
	public void Jiggle (float duration, float range) {
		_jiggling = true;
		_jiggleRange = range;
		endJiggleTime = Time.time + duration;
	}	
}
