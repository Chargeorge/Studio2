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
	private bool _isLocked;

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
	}
	
	// Update is called once per frame
	void Update () {
		//Am I controlled? 
		
		if(_currentControllingTeam != null){
			//check to see if I'm networked
			networked= checkNetwork();
			if(networked){
				_currentControllingTeam.score += sRef.vpsScorePerAltarPerSecond * Time.deltaTime;
			}
			gm.debugString = string.Format(" Number: {0},\r\n Networked: {1}", _currentControllingTeam.teamNumber, networked);
		}
		
		//transform.GetComponent<MeshRenderer>().enabled = gm.tiles[brdX, brdY].GetComponent<BaseTile>().IsRevealed;
		//transform.FindChild ("Quad").renderer.enabled = gm.tiles[brdX, brdY].GetComponent<BaseTile>().IsRevealed;
		
	}
	
	public void setControl(TeamInfo team){
		if(!isLocked){
			if(team!=null) {
				Color32 copy = new Color32((byte)(team.teamColor.r +30), (byte)(team.teamColor.g-30), (byte)(team.teamColor.b+30), (byte)255);
				renderer.material.color = copy;
				//renderer.material.color = team.teamColor;
				_currentControllingTeam = team;
				checkNetwork();
				StartCoroutine(AnimateTiles());
				
				//TODO - fix - not sure why this doesn't work...
				if (!sRef.optLockTile) {
					//Update all existing beacons to match new altar effects
					GameObject[] beacons = GameObject.FindGameObjectsWithTag("Beacon");
					foreach (GameObject go in beacons) {
						go.GetComponent<Beacon>().UpdateInfluencePatterns();
					}
				}
				
			}else{
				renderer.material.color = Color.gray;
			}
		}
			
	}
	

	
	public bool checkNetwork(){
		if(_currentControllingTeam != null){
			List<AStarholder> As = 	BaseTile.aStarSearch(gameObject.transform.parent.gameObject.GetComponent<BaseTile>(),gm.getTeamBase(_currentControllingTeam),int.MaxValue, BaseTile.getLocalSameTeamTiles, _currentControllingTeam);
			if(As.Count> 0){
				networkToBase = As;
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
						
						return true;
					}
					
					
				}
			}
		}
		return false;
	
	}
	
	
	public IEnumerator AnimateTiles(){
		Debug.Log("In Animate Co Routine");		
		foreach (AStarholder tileHolder in networkToBase) {
			Debug.Log("Starting Pulse");	
			tileHolder.current.GetComponent<Animator>().SetTrigger("Pulse");
			yield return new WaitForSeconds(.01f);
		}
		
		yield  break;
	}
	
	public int? owningTeamNetworkedAndLocked(){
		if(isLocked && networked || !sRef.optLockTile && networked){
			return currentControllingTeam.teamNumber;
		}
		else{
			return null;
		}
	}
	
	
}
