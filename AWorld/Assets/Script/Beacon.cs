using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Beacon : MonoBehaviour {

	private List<List<InfluencePatternHolder>> _patternList;
	public DirectionEnum? facing;
	public DirectionEnum? dirRotatingToward;	//Used to set visual direction while rotating
	public TeamInfo controllingTeam;
	private BeaconState _currentState;
	public int PlayerNumber;
	public float percBuildComplete = 0;
	public float percRotateComplete = 0;
	public float percUpgradeComplete = 0;
	public float percSmaller = 100;

	public float timeStoppedBuilding;
	public bool selfDestructing = false;
	public float timeStoppedUpgrading;
	public bool losingUpgradeProgress = false;
	public float timeStoppedRotating;
	public bool losingRotateProgress = false;

	public GameManager gm;
	public GameObject tileBeingConverted;
	private InfluencePatternHolder patternConverting;
	public Settings sRef;
	private GameObject _lastTileInfluenced;

	public bool newShot;
	public float speed = 1f;
	private float startTime;
	private float journeyLength;
	public Vector3 arrowStartPos;
	public Vector3 arrowPos;
	Vector3 tilePos;
	Vector2 destPos;

	public GameObject lastTileInfluenced {
		get {
			if(controllingTeam != null){
				return _lastTileInfluenced;
			}
			else{
				return null;
			}
		}
	}	
	
	public BeaconState currentState{
		get{
			return _currentState;
		}
	}

	bool buildButtonDown;

	public AudioClip beaconBuilding;
	public AudioClip beaconBuilt;
	public AudioClip beaconUpgrading;
	public AudioClip beaconUpgraded;
	public AudioClip beaconRotating;
	public AudioClip beaconRotated;
	public float volumeLerpRate = 0.2f;
	public float volumeLerpCloseEnough = 0.01f;
	
	public AudioSource audioSourceActionCompleted;
	public AudioSource audioSourceBuilding;
	public AudioSource audioSourceUpgrading;
	public AudioSource audioSourceRotating;
	public float buildingTargetVol;
	public float upgradingTargetVol;
	public float rotatingTargetVol;
	
	private Material matBasic;
	private Material matUpgraded;
	private Material arrowUpgraded;
	public Color32 neutralColor;
	public Color32 neutralColorB;


	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
		sRef= GameObject.Find ("Settings").GetComponent<Settings>();
		matBasic = (Material) Resources.Load ("Sprites/Materials/Base");
		matUpgraded = (Material) Resources.Load ("Sprites/Materials/BaseUpg");
		arrowUpgraded = (Material) Resources.Load ("Sprites/Materials/ArrowUpgraded");
		
		audioSourceBuilding.clip = beaconBuilding;
		audioSourceUpgrading.clip = beaconUpgrading;
		audioSourceRotating.clip = beaconRotating;
		
		newShot = false;
	}
	
	// Update is called once per frame
	void Update () {
	//	Debug.Log("startTime" + startTime);
		int brdX; int brdY;
		if (transform.parent != null) { //Hax
			brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
		 	brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
			
			setVisualDirection();	//Why is this happening every frame?

			buildButtonDown = getPlayerBuild();

			if (!buildButtonDown){
				buildingTargetVol = 0.0f;
				upgradingTargetVol = 0.0f;
				rotatingTargetVol = 0.0f;
			}
									
			if (audioSourceBuilding.volume != buildingTargetVol) {
				if (Mathf.Abs (audioSourceBuilding.volume - buildingTargetVol) < volumeLerpCloseEnough) audioSourceBuilding.volume = buildingTargetVol;
				else audioLerp(audioSourceBuilding, buildingTargetVol, volumeLerpRate); 
			}
			if (audioSourceBuilding.volume == 0.0f && buildingTargetVol == 0.0f) audioSourceBuilding.Stop ();
				
			if (audioSourceUpgrading.volume != upgradingTargetVol) {
				if (Mathf.Abs (audioSourceUpgrading.volume - upgradingTargetVol) < volumeLerpCloseEnough) audioSourceUpgrading.volume = upgradingTargetVol;
				else audioLerp(audioSourceUpgrading, upgradingTargetVol, volumeLerpRate); 
			}
			if (audioSourceUpgrading.volume == 0.0f && upgradingTargetVol == 0.0f) audioSourceUpgrading.Stop (); 
			
			if (audioSourceRotating.volume != rotatingTargetVol) {
				if (Mathf.Abs (audioSourceRotating.volume - rotatingTargetVol) < volumeLerpCloseEnough) audioSourceRotating.volume = rotatingTargetVol;
				else audioLerp(audioSourceRotating, rotatingTargetVol, volumeLerpRate); 
			}
			if (audioSourceRotating.volume == 0.0f && rotatingTargetVol == 0.0f) audioSourceRotating.Stop ();
			
			
			if((_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced || _currentState == BeaconState.Advanced) && controllingTeam != null){


				//find nearest convertable block
				//FIND The first convertable tile, list is ordered by distance
				//TODO setup for bases so multiple tiles can influence at once
				foreach (List<InfluencePatternHolder> list in _patternList) {
					float influenceThisFrame = sRef.vpsBeaconBaseInfluence * Time.deltaTime;				
					bool waterFound = false;	
					list.ForEach(delegate (InfluencePatternHolder p){
						if (!waterFound) {

							if(influenceThisFrame > 0f){
								int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
								int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
								GameObject tile;
								
								try { tile = gm.tiles[x, y]; }
									catch { return; }
								if (tile != null && 
									tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water && 
									!gm.getCapturedAltars (controllingTeam).Contains (AltarType.Thotzeti)) 
								{
									waterFound = true;
									return;
								}
								if(tile != null && tile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
									BaseTile Bt =  tile.GetComponent<BaseTile>();
									if(Bt.controllingTeam == null){
										Bt.startInfluence(influenceThisFrame, controllingTeam);
										Bt.tiltingFromBeacon = true;
										influenceThisFrame = 0;  //Assume a null controlled Tile will eat all influence.
										//Debug.Log("influencing Null tile");
									}
									else if(Bt.controllingTeam.teamNumber != controllingTeam.teamNumber){
										Bt.tiltingFromBeacon = true;
										influenceThisFrame = Bt.subTractInfluence(influenceThisFrame * p.coefInfluenceFraction, controllingTeam);
										//Debug.Log("Removing other influence");
										if(influenceThisFrame > 0){
											influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
											//Debug.Log("Adding next frames influence");
										}
									}
									else if(Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled < 100f){
										Bt.tiltingFromBeacon = true;
										influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
										//Debug.Log ("Adding my influence");
									}
									else if (Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled >= 100f) {
										Bt.tiltingFromBeacon = false;
									}
			//						|| Bt.percControlled < 100f
									_lastTileInfluenced = Bt.gameObject;
								}
								
							}//InfluenceTHis Frame
						
						} //waterfound
					
					
					});
				 }

	//			 
	//			 else{
	//				
	//			 //TODO: Handle situations where other tiles are influencing.  
	//				Debug.Log("Trying to influence at rate " + patternConverting.vpsInfluence );
	//				if(tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam)){
	//					tileBeingConverted = null;
	//					patternConverting= null;
	//				}
	//			 }
			}
		

		//With no powers this probably isn't necessary?

		//UpdateInfluencePatterns();	//Probably shouldn't call this every frame, but just doing this for now
		
		/**
		//Check self-destruction and losing upgrade progress - probably shouldn't call this every frame either, but I'm a rebel and I do what I want when I want 
		if (timeToSelfDestruct ()) { 
			subtractBuildingProgress (sRef.vpsBaseBuild);
		}
		
		if (timeToLoseUpgradeProgress ()) {
			subtractUpgradeProgress (sRef.vpsBaseUpgrade);
		}
		*/
		}
		
		if (lastTileInfluenced != null && !Pause.paused){
			if(newShot){
				shootSetup();
			}
			shootArrow();
		}
	}
	
	//START BUILDING:
	//SET the parent tile
	// Set the facing
	// ser State to building
	/// <summary>
	/// Puts it in the building state and starts the building.  
		/// </summary>
	/// <param name="tileLocation">Tile location.</param>
	/// <param name="player">Player.</param>
	/// <param name="valInit">Value init.</param>
	/// 
	private bool getPlayerBuild(){
		List<GameObject> p = GameManager.GameManagerInstance.players;
		BaseTile parent = transform.parent.gameObject.GetComponent<BaseTile>();
		GameObject playerOnTop = null;
		bool returnable = false;
		p.ForEach( delegate (GameObject go){
				Debug.Log (go.GetComponentInChildren<Player>());
			if(go.GetComponentInChildren<Player>().currentTile == parent){
				if(go.GetComponent<Player>().getPlayerBuild())
					returnable = true;
			}
		});
		
	
		return returnable;
	}
		
	public void startBuilding(GameObject tileLocation, GameObject player, float valInit){
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = player.GetComponentInChildren<Player>().facing;
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.BuildingBasic;
		if (controllingTeam == null) {
			controllingTeam = player.GetComponentInChildren<Player>().team;
			this.setTeam();
		}
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

		this.transform.FindChild("Base").localPosition = new Vector3(0f,0f,-.1f);
		//Debug.Log(this.transform.FindChild("Base").localPosition);
		
		buildingTargetVol = 0.5f;
		
		if (!audioSourceBuilding.isPlaying) {
			audioSourceBuilding.volume = 0.0f;
			audioSourceBuilding.Play ();	//Not working for some stupid reason
		}
	}
	

	public void buildNeutral(GameObject tileLocation){
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = (DirectionEnum)Random.Range(1,5);
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.Basic;
		this.setTeam(null);
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

		_currentState = BeaconState.Basic;

		_patternList = createBasicInfluenceList(getAngleForDir(facing));

		
	}

	public void startUpgrading(){
		this._currentState = BeaconState.BuildingAdvanced;
		
		upgradingTargetVol = sRef.beaconUpgradingVolume;
		
		if (!audioSourceUpgrading.isPlaying) {
			audioSourceUpgrading.volume = 0.0f;
			audioSourceUpgrading.Play ();	//Not working for some stupid reason
		}
	}
	
	public void startRotating (DirectionEnum? dir) {
		dirRotatingToward = dir;
		
		rotatingTargetVol = sRef.beaconRotatingVolume;
		
		if (!audioSourceRotating.isPlaying) {
			audioSourceRotating.volume = 0.0f;
			audioSourceRotating.Play ();	//Not working for some stupid reason
		}
	}
	
	public void setTeam(){
		Color32 controllingTeamColor = controllingTeam.beaconColor;	
		Color32 platformColor = controllingTeam.tileColor;
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = 0;
		
		if(controllingTeam.teamNumber == 1){
			beaconBuilt = Resources.Load("SFX/Beacon_Built_Lo") as AudioClip;
			beaconUpgraded = Resources.Load("SFX/Beacon_Upgraded_Lo_2") as AudioClip;
		}
		if(controllingTeam.teamNumber == 2) {
			beaconBuilt = Resources.Load("SFX/Beacon_Built_Hi") as AudioClip;
			beaconUpgraded = Resources.Load("SFX/Beacon_Upgraded_Hi_2") as AudioClip;
		}

		transform.FindChild("Arrow").renderer.material.color = controllingTeamColor;
		transform.FindChild("ArrowShot").renderer.material.color = controllingTeamColor;
		transform.FindChild("Base").renderer.material.color = controllingTeamColor;
		transform.FindChild("Anim").renderer.material.color = controllingTeamColor;
		transform.FindChild("Platform").renderer.material.color = platformColor;
		
		shootSetup ();
//		Invoke ("StartShooting", 0.1f);
	}

	public void setTeam(TeamInfo teamIn){
	if(teamIn != null) {
		PlayerNumber = teamIn.teamNumber;
		controllingTeam = teamIn;
		Color32 controllingTeamColor = controllingTeam.beaconColor;	
			Color32 platformColor = controllingTeam.tileColor;
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = (byte)(transform.FindChild("Arrow").renderer.material.color.a * 255);
		controllingTeamColor.a = (byte)(transform.FindChild("ArrowShot").renderer.material.color.a * 255);
		controllingTeamColor.a = (byte)(transform.FindChild("Base").renderer.material.color.a * 255);
			platformColor.a = (byte)(transform.FindChild("Platform").renderer.material.color.a * 255);

			transform.FindChild("Arrow").renderer.material.color = controllingTeamColor;	
			transform.FindChild("ArrowShot").renderer.material.color = controllingTeamColor;
			transform.FindChild("Base").renderer.material.color = controllingTeamColor;
			transform.FindChild("Anim").renderer.material.color = controllingTeamColor;
			transform.FindChild("Platform").renderer.material.color = platformColor;
		}
		else{

			neutralColor = new Color32 (100, 100, 100, 255);
			neutralColorB = new Color32 (200, 200, 200, 255);

			controllingTeam = null;
			transform.FindChild("Arrow").renderer.material.color = neutralColor;
			transform.FindChild("ArrowShot").renderer.material.color = neutralColor;
			transform.FindChild("Base").renderer.material.color = neutralColor;
			transform.FindChild("Platform").renderer.material.color = neutralColorB;
		}	}
	
	/// <summary>
	/// Adds progress to the current building action
	/// </summary>
	/// <param name="rate">Rate.</param>
	public void addBuildingProgress(float rate){
		if (!audioSourceBuilding.isPlaying) {
			audioSourceBuilding.volume = 0.0f;
			audioSourceBuilding.Play (); 	//Dunno why the fuck this is necessary but whatever
		}
		buildingTargetVol = sRef.beaconBuildingVolume;
	
		percBuildComplete += rate*Time.deltaTime;
						
		Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
		Color32 platformColor = controllingTeam.tileColor;
		float newColor =  (255f * (percBuildComplete/100f)) ;
		newColor = (newColor >= 255) ? 254 : newColor;		
		beaconColor.a = (byte)newColor;

		//successful attempt at rising platform
		Transform beaconbase = transform.FindChild ("Base");
		Transform platform = transform.FindChild ("Platform");
		Vector3 platformPos = platform.localPosition;
		Vector3 beaconPos = beaconbase.localPosition;
		float newPos = (0.09f * (percBuildComplete/100f));
		newPos = (newPos >= 0.09f) ? 0.089f : newPos;
		platformPos.y = newPos;
		beaconPos.y = newPos;
		platform.localPosition = platformPos;
		beaconbase.localPosition = beaconPos;

		updateBuildAnim ();

//		platform.Translate(Vector3.up * platformPos);

		transform.FindChild("Arrow").renderer.material.color = beaconColor;	
		
		transform.FindChild("ArrowShot").renderer.material.color = beaconColor;		
		transform.FindChild("Platform").renderer.material.color = platformColor;
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 0;
		transform.FindChild("Base").renderer.material.color = baseColor;	
	}
	
	public void subtractBuildingProgress(float rate) {
	
		percBuildComplete -= rate*Time.deltaTime;
		
		updateBuildAnim ();
		
		if (percBuildComplete <= 0f) {
			GameObject.Destroy (this.gameObject);
		}
		
		else {
			Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
			Color32 platformColor = controllingTeam.tileColor;
			float newColor =  (255f * (percBuildComplete/100f)) ;
			newColor = (newColor >= 255) ? 254 : newColor;		
			beaconColor.a = (byte)newColor;
			transform.FindChild("Arrow").renderer.material.color = beaconColor; 
			
			transform.FindChild("ArrowShot").renderer.material.color = beaconColor; 
			transform.FindChild("Platform").renderer.material.color = platformColor;

			Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
			baseColor.a = 0;
			transform.FindChild("Base").renderer.material.color = baseColor;		
		}
	}
		
	public void addRotateProgress (float rate) {
		if (!audioSourceRotating.isPlaying) {
			audioSourceRotating.volume = 0.0f;
			audioSourceRotating.Play ();
		}
		rotatingTargetVol = sRef.beaconRotatingVolume;
		
		percRotateComplete += rate*Time.deltaTime;
		losingRotateProgress = false;
	}
	
	public void addUpgradeProgress (float rate) {
		if (!audioSourceUpgrading.isPlaying) {
			audioSourceUpgrading.volume = 0.0f;
			audioSourceUpgrading.Play (); 	//Dunno why the fuck this is necessary but whatever
		}
		upgradingTargetVol = sRef.beaconUpgradingVolume;
		
		percUpgradeComplete += rate*Time.deltaTime;
		updateUpgradeAnim ();
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 255;
		transform.FindChild("Base").renderer.material.color = baseColor;	
		
//		percSmaller -= rate*Time.deltaTime;

//		Vector3 newScale = animTrans.localScale;
//		float newScale =  (0.5f * (percUpgradeComplete/100f)) ;
//		newScale = (newScale >= 0.5f) ? 0.49f : newScale;		

//		newScale.x = percSmaller  / 100f;
//		newScale.y = percSmaller  / 100f;
//		newScale.x = 100f/percSmaller;
//		newScale.y = 100f/percSmaller;
//		animTrans.localScale = newScale;

		//We need some visual representation for this	
	}
	
	
	public void subtractUpgradeProgress (float rate) {
		percUpgradeComplete -= rate*Time.deltaTime;
		updateUpgradeAnim ();		

//		percSmaller += rate*Time.deltaTime;		
//		Vector3 newScale = animTrans.localScale;
//		newScale.x = 100f/percSmaller;
//		newScale.y = 100f/percSmaller;
//		animTrans.localScale = newScale;

		if (percUpgradeComplete <= 0f) {
			percUpgradeComplete = 0f;
			_currentState = BeaconState.Basic;
			Color32 animColor = transform.FindChild("Anim").renderer.material.color;
			animColor.a = (byte)0f;
			transform.FindChild("Anim").renderer.material.color = animColor;
		}
		
		//We need some visual representation for this
	}
	#region creating_influence_lists
	//This is gonna be mad long so I added a region - minimize at your pleasure - should probably use for loops, but whatevs, it's all manual now 
	
	public List<List<InfluencePatternHolder>> createBasicInfluenceList(float degreeRotated){
		
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Not sure if sorting the lists is necessary?
		
		if (controllingTeam != null) {
		
			bool onixtal = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal);
			bool yaxchay = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay);
			bool tepwante = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante);
			int baseRange = sRef.beaconBasicRange;
			
			if (!yaxchay && !onixtal && !tepwante) {
				for (int i = 1; i <= sRef.beaconBasicRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}			
			}
			
			//Yaxchay: x2 range, x2 power
			else if (yaxchay) {
				
				//Deal with straight-forward pattern first since others don't affect this
				for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				list.Add (forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				
				//Yaxchay + Onixtal - build right, backward, and left patterns
				if (onixtal) {
					
					List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
						backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
						leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
					}
					
					list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					
				}
				
				//Yaxchay + Tepwante - build forward triple-beam patterns
				if (tepwante) {
					
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				}
				
				//Yaxchay + Tepwante + Onixtal - build triple-beam patterns for left, backward, and right
				if (onixtal && tepwante) {
					
					//Backward triple-beam patterns
					List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
						backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
					}
					
					list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					
					//Left and right patterns 
					//lololololololol					
					List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 2; i <= baseRange*sRef.coefYaxchay; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
						rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
						rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
						leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
						leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
					}
					
					list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				}
			}
			
			//No Yaxchay
			else {
				
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				
				//Onixtal: Influence in non-facing directions at 25% strength
				if (onixtal) {
					
					if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal) && !gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
						
						List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
							backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
							leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
						}
						
						list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						
					}
					
					//Onixtal + Tepwante - build triple-beam patterns for left, backward, and right 
					if (tepwante) {
						//Backward triple-beam patterns
						List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
							backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
						}
						
						list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						
						//Left and right patterns 
						List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 2; i <= sRef.beaconBasicRange; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
							rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
							rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
							leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
							leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
						}
						
						list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());					
					}
				}
				
				//Tepwante: Triple beam - assuming no Yaxchay and no Onixtal by now
				else if (tepwante) {
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());			
				}
			}
		}

		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		return list;
	}	
	
	public List<List<InfluencePatternHolder>> createAdvancedInfluenceList(float degreeRotated){
	
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Not sure if sorting the lists is necessary?
		
		if (controllingTeam != null) {
		
			bool onixtal = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal);
			bool yaxchay = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay);
			bool tepwante = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante);
			int baseRange = sRef.beaconAdvancedRange;
			
			if (!yaxchay && !onixtal && !tepwante) {
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}			
			}
			
			//Yaxchay: x2 range, x2 power
			else if (yaxchay) {
				
				//Deal with straight-forward pattern first since others don't affect this
				for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				list.Add (forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				
				//Yaxchay + Onixtal - build right, backward, and left patterns
				if (onixtal) {
					
					List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
						backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
						leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
					}
					
					list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					
				}
				
				//Yaxchay + Tepwante - build forward triple-beam patterns
				if (tepwante) {
					
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				}
				
				//Yaxchay + Tepwante + Onixtal - build triple-beam patterns for left, backward, and right
				if (onixtal && tepwante) {
					
					//Backward triple-beam patterns
					List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
						backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
					}
					
					list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					
					//Left and right patterns 
					//lololololololol					
					List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 2; i <= baseRange*sRef.coefYaxchay; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
						rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
						rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
						leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
						leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
					}
					
					list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				}
			}
			
			//No Yaxchay
			else {
			
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
		
				//Onixtal: Influence in non-facing directions at 25% strength
				if (onixtal) {
				
					if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal) && !gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
						
						List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
							backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
							leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
						}
						
						list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						
					}
					
					//Onixtal + Tepwante - build triple-beam patterns for left, backward, and right 
					if (tepwante) {
						//Backward triple-beam patterns
						List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
							backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
						}
						
						list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						
						//Left and right patterns 
						List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 2; i <= baseRange; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
							rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
							rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
							leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
							leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
						}
						
						list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());					
					}
				}
			
			//Tepwante: Triple beam - assuming no Yaxchay and no Onixtal by now
				else if (tepwante) {
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());			
				}
			}
		}
		
		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		return list;
	}
	#endregion
	
	void OnMouseOver() {
		GameObject.Find("GameManager").GetComponent<GameManager>().debugBeacon = this;
	}
	
	private float getAngleForDir(DirectionEnum? N){
		switch (N){
		case DirectionEnum.North:
			return 0f;
			
		case DirectionEnum.East:
			return 90f;
			
		case DirectionEnum.South:
			return 180f;
			
		case DirectionEnum.West:
			return 270f;
			
		}
		return 0;
	}
	
	public void setDirection(DirectionEnum? N){
		
		float rotAngle = getAngleForDir(N);
		float currentRotAngle = getAngleForDir(facing);
		
		facing = N;
		dirRotatingToward = N;
		transform.FindChild ("Arrow").RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.FindChild ("Arrow").RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
		transform.FindChild ("ArrowShot").RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.FindChild ("ArrowShot").RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
	}
	
	public void setVisualDirection(){
		Transform arrow = transform.FindChild ("Arrow");
		Transform arrowShot = transform.FindChild ("ArrowShot");

		arrow.localEulerAngles = new Vector3(0,0,-1*getAngleForDir(facing));
		arrowShot.localEulerAngles = new Vector3(0,0,-1*getAngleForDir(facing));


		if (dirRotatingToward != facing && percRotateComplete > 0f && percRotateComplete < 100f) { //Rotating
			
			Vector3 angleRotatingToward = Vector3.zero;	//The angle we're roughly rotating toward
			float percRotatingToward = 0f;				//How far toward the angle above we're going to get just before 100% rotation
						
			if (Mathf.Abs ((getAngleForDir (facing)-getAngleForDir(dirRotatingToward)) % 360f) == 180f) {	//Rotating 180 degrees
				if (facing == DirectionEnum.North || facing == DirectionEnum.South)  
					angleRotatingToward = new Vector3 (-90f, 0f, 0f);
				else
					angleRotatingToward = new Vector3 (0, -90f, 0f); 
					
				percRotatingToward = 90f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			//	arrowShot.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			//I know there's a mathy way to do this but holy motherfucking shit fuck I cannot figure it out so fuck it
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.South) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.North)) 
			{
				angleRotatingToward = new Vector3 (0,0,90f);
				percRotatingToward = 75f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			//	arrowShot.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.North) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.South)) 
			{
				angleRotatingToward = new Vector3 (0,0,-90f);
				percRotatingToward = 75f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			//	arrowShot.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			else{
				Debug.LogWarning ("Something's wrong in Beacon.setVisualDirection");
			}
		}
	}
	
	public void Rotate (DirectionEnum? N) {
		setDirection (N);
		setVisualDirection ();
		ClearJiggle ();
		UpdateInfluencePatterns();

		rotatingTargetVol = 0.0f;
		audioSourceActionCompleted.clip = beaconRotated;
		audioSourceActionCompleted.volume = sRef.beaconRotatedVolume;
		audioSourceActionCompleted.Play ();

		if(percRotateComplete >= 100f){
			//audioLerp(audioSourceBeacon, 0.01f, lerpRate);
			//audio.PlayOneShot(beaconRotated, 1.0f);	
			transform.FindChild("ArrowShot").renderer.enabled = false;
		}
		
		else {
			transform.FindChild("ArrowShot").renderer.enabled = !facingAdjacentWater();
		}
	}
	
	//Stops a-jiggling what ought not be a-jiggling - use before UpdateInfluencePatterns
	public void ClearJiggle () {
		
		int brdX; int brdY;
		if (transform.parent != null) { //Hax
			brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
			brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
			
			if((_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced || _currentState == BeaconState.Advanced) && controllingTeam != null){
				
				foreach (List<InfluencePatternHolder> list in _patternList) {
					bool waterFound = false;	
					list.ForEach(delegate (InfluencePatternHolder p){
						if (!waterFound) {
							
							if(true == true){
								int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
								int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
								GameObject tile;
								
								try { tile = gm.tiles[x, y]; }
								catch { return; }
								if (tile != null && 
								    tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water && 
								    !gm.getCapturedAltars (controllingTeam).Contains (AltarType.Thotzeti)) 
								{
									waterFound = true;
									return;
								}
								if(tile != null && tile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
									BaseTile Bt =  tile.GetComponent<BaseTile>();
									Bt.tiltingFromBeacon = false;
								}								
							}
						}
					});
				}
			}
		}
	}
	
	//Creates new influence patterns, for example when a new altar is captured or when the beacon is rotated.
	public void UpdateInfluencePatterns () {
		if (_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced) {
			_patternList = createBasicInfluenceList (getAngleForDir (facing));
		}
		if (_currentState == BeaconState.Advanced) {
			_patternList = createAdvancedInfluenceList (getAngleForDir(facing));
		}		
	}
	
	/// <summary>
	/// Finishes the current building action.  USE ONLY FOR BUILDING, INFLUENCE HANDLED ELSEWHERE
	/// </summary>
	public void Build (){
		
		//Used to be finishAction() - refactored upgrade stuff into Upgrade() - seems cleaner this way
		
		///TODO: add end semaphore stuff her
		selfDestructing = false;
		
		if(percBuildComplete >= 100f){
			percBuildComplete = 100f;
			buildingTargetVol = 0.0f;
			audioSourceActionCompleted.clip = beaconBuilt;
			audioSourceActionCompleted.volume = sRef.beaconBuiltVolume;
			audioSourceActionCompleted.Play ();
			
			_currentState = BeaconState.Basic;
			_patternList = createBasicInfluenceList(getAngleForDir(facing));
			
			for(int x = 0; x<  GameManager.GameManagerInstance.tiles.GetLength(0); x++){
				for(int y =0 ; y< GameManager.GameManagerInstance.tiles.GetLength(1); y++){
					if(GameManager.GameManagerInstance.tiles[x,y].GetComponent<BaseTile>().currentType != TileTypeEnum.water){
						GameManager.GameManagerInstance.tiles[x,y].GetComponent<BaseTile>().tooCloseToBeacon();
					}
				}
			}
		}

		transform.FindChild ("Arrow").GetComponent<MeshRenderer>().enabled = true;
		transform.FindChild ("ArrowShot").GetComponent<MeshRenderer>().enabled = true;
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 255;
		transform.FindChild("Base").renderer.material.color = baseColor;	
		
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = 0;
		transform.FindChild("Anim").renderer.material.color = animColor;
		
		transform.FindChild("ArrowShot").renderer.enabled = !facingAdjacentWater();
		
//		transform.FindChild("Point light").GetComponent<Light>().enabled = true;
	}
	
	
	public void AbortBuild () {
		selfDestructing = true;
		timeStoppedBuilding = Time.time;
		Invoke ("CheckSelfDestruct", sRef.selfDestructDelay);
	}
	
	public void CheckSelfDestruct () {
		if (timeToSelfDestruct ()) {
	//		GameObject.Destroy (this.gameObject);	//Destroys beacon immediately
			StartCoroutine (selfDestruct());
		}
	}
	
	private IEnumerator selfDestruct () {
		while (selfDestructing) {
			subtractBuildingProgress (sRef.vpsBaseBuild);
			yield return new WaitForEndOfFrame ();
			
		}
	}
	
	private bool timeToSelfDestruct () {
		return selfDestructing && timeStoppedBuilding <= Time.time - sRef.selfDestructDelay;
	}
	
	public void Upgrade () {


		losingUpgradeProgress = false;

		upgradingTargetVol = 0.0f;
		audioSourceActionCompleted.clip = beaconUpgraded;
		audioSourceActionCompleted.volume = sRef.beaconUpgradedVolume;
		audioSourceActionCompleted.Play ();

		_currentState = BeaconState.Advanced;
		_patternList = createAdvancedInfluenceList(getAngleForDir(facing));
		
		_currentState = BeaconState.Advanced;
		transform.FindChild("Base").renderer.material = matUpgraded;
		transform.FindChild("Arrow").renderer.material = arrowUpgraded;
		transform.FindChild("ArrowShot").renderer.material = arrowUpgraded;
		//transform.FindChild("Arrow").transform.localScale = new Vector3(17, 17, 0);
		
		//hax
		setTeam ();
		Color32 platformColor = transform.FindChild("Base").renderer.material.color;
		Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		platformColor.a = 255;
		beaconColor.a = 255;
		animColor.a = 0;
		transform.FindChild("Arrow").renderer.material.color = beaconColor;
		transform.FindChild("ArrowShot").renderer.material.color = beaconColor;
		transform.FindChild("Base").renderer.material.color = platformColor;
		transform.FindChild("Anim").renderer.material.color = animColor;
	}
	
	//Player stopped in the middle of an upgrade
	public void AbortUpgrade () {
		losingUpgradeProgress = true;
		timeStoppedUpgrading = Time.time;
		Invoke ("CheckLoseUpgradeProgress", sRef.loseUpgradeProgressDelay);		
	}
	
	public void CheckLoseUpgradeProgress () {
		if (timeToLoseUpgradeProgress ()) {
//			_currentState = BeaconState.Basic;
//			percUpgradeComplete = 0f;
			StartCoroutine(loseUpgradeProgress());
		}
	}
	
	private bool timeToLoseUpgradeProgress () {
		return losingUpgradeProgress && timeStoppedUpgrading <= Time.time - sRef.loseUpgradeProgressDelay;
	}
	
	private IEnumerator loseUpgradeProgress () {
		while (losingUpgradeProgress) {
			subtractUpgradeProgress (sRef.vpsBaseUpgrade);
			yield return new WaitForEndOfFrame ();
		}
	}
	
	public void AbortRotate () {
		losingRotateProgress = true;
		timeStoppedRotating = Time.time;
		Invoke ("CheckLoseRotateProgress", sRef.loseRotateProgressDelay);		
	}
	
	public void CheckLoseRotateProgress () {
		if (timeToLoseRotateProgress ()) {
			StartCoroutine(loseRotateProgress());
		}
	}
	
	private bool timeToLoseRotateProgress () {
		return losingRotateProgress && timeStoppedRotating <= Time.time - sRef.loseRotateProgressDelay;
	}
	
	private IEnumerator loseRotateProgress () {
		while (losingRotateProgress) {
			subtractRotateProgress (sRef.vpsBaseRotate);
			yield return new WaitForEndOfFrame ();
		}
	}
	
	public void subtractRotateProgress (float rate) {
		percRotateComplete -= rate*Time.deltaTime;

		if (percRotateComplete <= 0f) {
			losingRotateProgress = false;
			percRotateComplete = 0f;
		}
	}
	
	private void updateBuildAnim () {
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = (byte) (255f * ((sRef.buildCircleFinishAlpha - sRef.buildCircleStartAlpha) * percBuildComplete/100f + sRef.buildCircleStartAlpha));
		transform.FindChild("Anim").renderer.material.color = animColor;
		
		Transform animTrans = transform.FindChild("Anim");
		float newScale = sRef.buildCircleStartScale - (sRef.buildCircleStartScale - sRef.buildCircleFinishScale) * percBuildComplete/100f;
		animTrans.localScale = new Vector3 (newScale, newScale, animTrans.localScale.z);
	}
	
	private void updateUpgradeAnim () {
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = (byte) (255f * ((sRef.upgradeCircleFinishAlpha - sRef.upgradeCircleStartAlpha) * percUpgradeComplete/100f + sRef.upgradeCircleStartAlpha));
		transform.FindChild("Anim").renderer.material.color = animColor;
		
		Transform animTrans = transform.FindChild("Anim");
		float newScale = sRef.upgradeCircleStartScale - (sRef.upgradeCircleStartScale - sRef.upgradeCircleFinishScale) * percUpgradeComplete/100f;
		animTrans.localScale = new Vector3 (newScale, newScale, animTrans.localScale.z);
	}
	
	private float getBaseBeaconStrength (int distance) {

		float strength = -1;
		
		//Basic, no Yaxchay
		if (!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && (currentState == BeaconState.Basic || currentState == BeaconState.BuildingAdvanced)) { 
			if (distance == 1) strength = 1f;
			else if (distance == 2) strength = 0.5f;
			else if (distance == 3) strength = 0.33f;
			else if (distance == 4) strength = 0.25f;				
		}
		
		//Basic, Yaxchay
		else if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && (currentState == BeaconState.Basic || currentState == BeaconState.BuildingAdvanced)) { 
			if (distance >= 1 && distance <= 2) strength = 1f;
			if (distance >= 3 && distance <= 4) strength = 0.5f;
			if (distance >= 5 && distance <= 6) strength = 0.33f;
			if (distance >= 7 && distance <= 8) strength = 0.25f;
		}
		
		//Advanced, no Yaxchay - same as basic + Yaxchay given current bonuses, but that may change
		else if (!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && currentState == BeaconState.Advanced) { 
			if (distance >= 1 && distance <= 2) strength = 1f;
			if (distance >= 3 && distance <= 4) strength = 0.5f;
			if (distance >= 5 && distance <= 6) strength = 0.33f;
			if (distance >= 7 && distance <= 8) strength = 0.25f;
		}
		
		//Advanced, Yaxchay
		else if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && currentState == BeaconState.Advanced) { 
			if (distance >= 1 && distance <= 4) strength = 1f;
			if (distance >= 5 && distance <= 8) strength = 0.5f;
			if (distance >= 9 && distance <= 12) strength = 0.33f;
			if (distance >= 13 && distance <= 16) strength = 0.25f;
		}		
		
		if (strength == -1) {
			Debug.LogWarning ("Error when determining base beacon strength");
		}
		
		return strength;
	}

	public void audioLerp (AudioSource source, float target, float rate) {
		if (Mathf.Abs (source.volume - target) <= 0.01f) {
			source.volume = target;
		}
		else {
			source.volume = Mathf.Lerp (source.volume, target, rate);
		}
	}


	//code for arrowShooting


	public void shootSetup(){

		Transform arrow = this.transform.FindChild("ArrowShot");
		arrowStartPos = this.transform.FindChild("Base").position;
		transform.FindChild("ArrowShot").renderer.enabled = !facingAdjacentWater();
		arrow.position = arrowStartPos;
		if(lastTileInfluenced != null){
			tilePos = lastTileInfluenced.transform.position;
		}
		startTime = Time.time;
		journeyLength = Vector3.Distance(arrowStartPos, tilePos);
		newShot = false;
//		Debug.Log ("I happen");
	//	destPos = Vector2(tilePos.x, tilePos.y);

		arrow.renderer.material.color = controllingTeam.beaconColor;

//		GameObject oldLastTile = lastTileInfluenced;
	}
	
	public void shootArrow(){
	
		//Weird hack, I dunno. Fixes neutral beacons shooting first arrow to 0,0
		if (tilePos.x == 0 && tilePos.y == 0) tilePos = lastTileInfluenced.transform.position;	

		Transform arrow = this.transform.FindChild("ArrowShot");
//		Vector3 tilePos = lastTileInfluenced.transform.position;
	//	Vector3 arrowPos = this.transform.FindChild("Arros").position;

	//	float distCovered = (Time.time - startTime) * speed;
	//	float fracJourney = distCovered / journeyLength;
		//Debug.Log(fracJourney);
		arrowPos = Vector3.Lerp(arrow.position, tilePos, 0.05f);
		arrowPos.z = -1f;
		arrow.position = arrowPos;
		
		float distance = Vector2.Distance (new Vector2 (arrow.position.x, arrow.position.y), new Vector2 (tilePos.x, tilePos.y));

		if (distance < 1f && distance > 0.5f) {
			float percAlpha = (distance - 0.5f) / 0.5f; 
			float alpha = percAlpha * 255;
			Color32 arrowAlpha = controllingTeam.beaconColor;
			arrowAlpha.a = (byte)alpha;
			arrow.renderer.material.color = arrowAlpha;
		}

		if (distance < 0.5f) { 
			arrow.renderer.enabled = false;
		}
		
		if (distance < 0.01f) {
			newShot = true;
		}
	//	if (fracJourney > 1f){
	//			newShot = true;
	//	}
	}

//		if(oldLastTile != lastTileInfluenced){
//			newShot = true;
//		}

		//	Vector3 dir = arrowPos - tilePos;
		//	arrow.Translate(dir * Time.deltaTime);
		
	public void StartShooting () {
		newShot = true;
	}
	
	//Returns whether this beacon is facing and immediately adjacent to a water tile. 
	public bool facingAdjacentWater () {
		int brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
		int brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
		
		if (_patternList != null) {
			foreach (List<InfluencePatternHolder> list in _patternList) {
				foreach (InfluencePatternHolder p in list) {
					int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
					int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
					GameObject tile;
					if (x < 0 || y < 0 || x >= gm.boardX || y >= gm.boardY) return true;
					if (gm.tiles[x,y] == null) { return true; }
					tile = gm.tiles[x,y];
					return (tile != null && 
					    tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water && 
					    !gm.getCapturedAltars (controllingTeam).Contains (AltarType.Thotzeti));
				}
			}
		}
		return false;	
	}
}