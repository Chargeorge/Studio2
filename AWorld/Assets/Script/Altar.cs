using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Altar : MonoBehaviour {

	private TeamInfo _currentControllingTeam;
	public bool networked;
	public Settings sRef;
	public AltarType altarType;
	private TeamInfo _firstControllingTeam;
	public TeamInfo touchControl;
	public int brdX;
	public int brdY;
	public List<AStarholder> networkToBase;
	public GameManager gm;
	private GameObject _lockedLayer;
	private GameObject _scoreBar;
	public float baseScoreScale = .74f;
	public float scoreShotInterval = 1f;
	private float timeToNextScoreShot;
	public GameObject prfbScoreBit;
	public Transform symbol;

	public GameObject scoreBar {
		get {
			if(_scoreBar == null){
				_scoreBar = gameObject.transform.FindChild("ScoreThingy").gameObject;
			
			}
			return _scoreBar;
		}
	}	
	
	
	private bool _isLocked;
	
	public float scoreLeft;
	

	public AudioClip Praying;

	public bool isLocked {
		get {
			return _isLocked;
		}
		set {
			_lockedLayer.renderer.enabled = value;
			_isLocked = value;
		}
	}	

	#region accessors
	public TeamInfo currentControllingTeam {
		get {
			return _currentControllingTeam;
		}
		set{
			if(_firstControllingTeam == null) _firstControllingTeam = value;
			_currentControllingTeam = value;
		}
	}
	
	
	public TeamInfo firstControllingTeam {
		get {
			return _firstControllingTeam;
		}
	}
	#endregion
	
	// Use this for initialization
	void Start () {
		_lockedLayer = transform.FindChild("LockedLayer").gameObject;
		isLocked = false;
		sRef = Settings.SettingsInstance;
		//TODO OHH GOD THIS IS BAD I SHOULDN'T DO THIS
//		altarType = GameManager.GetRandomEnum<AltarType>();
		Debug.Log("altar: " +altarType.ToString());
		Material loaded =  (Material)Resources.Load(string.Format("Sprites/Materials/{0}", altarType.ToString()));
		gm = GameManager.GameManagerInstance;
		transform.FindChild("Quad").renderer.material = loaded;
		symbol = transform.FindChild("Quad");
		scoreLeft = sRef.valScorePerMine;
	}
	
	public void setScoreBarTeam(TeamInfo t ){
		if(t != null){
			scoreBar.renderer.material.color = t.teamColor;
		}
		else{
			scoreBar.renderer.material.color =  new Color32(220, 30, 47, 255);
		}
		
	}
	
	public void setScoreBarLen(){
		Vector3 newScale = scoreBar.transform.localScale;
		newScale.x =  baseScoreScale * scoreLeft  / sRef.valScorePerMine;
		scoreBar.transform.localScale = newScale;

		Vector3 new2scale = symbol.localScale;
		new2scale.x = baseScoreScale * scoreLeft  / sRef.valScorePerMine;
		new2scale.y = baseScoreScale * scoreLeft  / sRef.valScorePerMine;
		symbol.localScale = new2scale;
		
	}
	
	// Update is called once per frame
	void Update () {
		//Am I controlled? 

		if(altarType != AltarType.MagicalMysteryScore){
			scoreBar.renderer.enabled = false;
			transform.FindChild("ScoreThingyBG").renderer.enabled = false;
		}
		
		if(_currentControllingTeam != null){
			//check to see if I'm networked
			networked= checkNetwork();
			if(networked){
				if(altarType !=  AltarType.MagicalMysteryScore){
					List <AltarType> a = gm.getCapturedAltars(_currentControllingTeam);
	 
					if(a.Contains(AltarType.Khepru)){
						_currentControllingTeam.score += sRef.vpsScorePerAltarPerSecond * sRef.coefKhepru * Time.deltaTime;
					}else{
						_currentControllingTeam.score += sRef.vpsScorePerAltarPerSecond * Time.deltaTime;
					}
				}else{
					List <AltarType> a = gm.getCapturedAltars(_currentControllingTeam);
					if(scoreLeft >0){
						timeToNextScoreShot -= Time.deltaTime;
						if(timeToNextScoreShot < 0){
							float scoreToAdd = sRef.vpsScorePerMinePerSecond * ((a.Contains(AltarType.Khepru)) ? sRef.coefKhepru : 1 )* Time.deltaTime;
							if(scoreLeft - scoreToAdd < 0){
								scoreToAdd = scoreLeft;
							}
							scoreLeft -= scoreToAdd;
							setScoreBarLen();
							Vector3 scoreBitStartPos = transform.position;

							scoreBitStartPos.z = -1.2f;
							GameObject scoreBit = BulletPool.instance.GetObjectForType("ScoreBit", false);
							scoreBit.transform.position = scoreBitStartPos;
							scoreBit.GetComponent<ScoreBit>().setTeam(currentControllingTeam);
							scoreBit.GetComponent<ScoreBit>().start(networkToBase);
							timeToNextScoreShot = scoreShotInterval;
							//_currentControllingTeam.score += scoreToAdd;
						}

					}
				}
				
				audio.PlayOneShot(Praying, 1.0f); //this is not the right place to put it, it apparently fucks up the other sounds ?
			}
			gm.debugString = string.Format(" Number: {0},\r\n Networked: {1}", _currentControllingTeam.teamNumber, networked);
		}
		
		//transform.GetComponent<MeshRenderer>().enabled = gm.tiles[brdX, brdY].GetComponent<BaseTile>().IsRevealed;
		//transform.FindChild ("Quad").renderer.enabled = gm.tiles[brdX, brdY].GetComponent<BaseTile>().IsRevealed;
		
	}
	
	public void setControl(TeamInfo team){
		if(!isLocked){
			if(team!=null) {
				Color32 copy = team.beaconColor;
				renderer.material.color = copy;
				//renderer.material.color = team.teamColor;
				_currentControllingTeam = team;
				checkNetwork();
//				StartCoroutine(AnimateTiles());
				
				//TODO - fix - not sure why this doesn't work...
				if (!sRef.optLockTile) {
					//Update all existing beacons to match new altar effects
					GameObject[] beacons = GameObject.FindGameObjectsWithTag("Beacon");
					foreach (GameObject go in beacons) {
						go.GetComponent<Beacon>().UpdateInfluencePatterns();
					}
				}
				
			}else{
				renderer.material.color = new Color32(237, 20, 90, 255);
				//pink (237, 20, 90, 255)
			}
		}
			
	}
	

	
	public bool checkNetwork(){
		if(_currentControllingTeam != null){
			List<AStarholder> As = 	BaseTile.aStarSearch(gameObject.transform.parent.gameObject.GetComponent<BaseTile>(),gm.getTeamBase(_currentControllingTeam),int.MaxValue, BaseTile.getLocalSameTeamTiles, _currentControllingTeam);
			if(As.Count> 0){
				networkToBase = As;
				audio.Stop();
				audio.PlayOneShot(Praying, 0.8f);
				return true;
				
			}
			else{
				return false;
			}
		}
		return false;
	}
	
	public bool doCapture(TeamInfo T){
		if(sRef.optLockTile){
			if(!isLocked){
				if(currentControllingTeam.teamNumber == T.teamNumber){
					if(networked){
						isLocked = true;
						Hashtable ht = new Hashtable();
						ht.Add("x",.5f);
						ht.Add("y",.5f);
						ht.Add("time",.50f);
						iTween.ShakePosition(gameObject, ht);
						
						//Update all existing beacons to match new altar effects
						GameObject[] beacons = GameObject.FindGameObjectsWithTag("Beacon");
						foreach (GameObject go in beacons) {
							go.GetComponent<Beacon>().UpdateInfluencePatterns();
						}
						audio.PlayOneShot(Praying, 1.0f);
						return true;
					}
				}
			}
		}
		return false;
	}
	
	
/*	public IEnumerator AnimateTiles(){
		Debug.Log("In Animate Co Routine");		
		foreach (AStarholder tileHolder in networkToBase) {
			Debug.Log("Starting Pulse");	
			tileHolder.current.GetComponent<Animator>().SetTrigger("Pulse");
			yield return new WaitForSeconds(.01f);
		}
		
		yield  break;
	}*/
	
	public int? owningTeamNetworkedAndLocked(){
		if(isLocked && networked || !sRef.optLockTile && networked){
			audio.PlayOneShot(Praying, 1.0f);
			return currentControllingTeam.teamNumber;

		}
		else{
			return null;
		}
	}
	
	
}
