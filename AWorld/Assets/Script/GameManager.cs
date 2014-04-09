using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {
	
	public Settings sRef;
	#region Statics
	/// <summary>
	/// Converts the grd position into the absolute Unity world position
	/// </summary>
	/// <returns>The world position from grid position.</returns>
	/// <param name="grdPos">Grd position.</param>
	public static Vector3 wrldPositionFromGrdPosition(Vector2 grdPos){
		//This method is probably incomplete, needs to handle tile sizing intelligently
		return new Vector3 (grdPos.x, grdPos.y, -1);
	}
	#endregion
	public GameObject[,] tiles;
	private bool setup = true;
	public static Mode gameMode = Mode.OneVOne;
	public List<GameObject> players = new List<GameObject>();
	public GameObject prfbPlayer;
	public BaseTile debugMouse;
	public Beacon debugBeacon;
	public GameObject prfbAltar, prfbHome;
	public List<GameObject> altars;
	public string debugString;
	public List<VictoryCondition> victoryConditions;
	private GameState _currentState;
	public List<TeamInfo> teams;
	public GameState currentState {
		get {
			return _currentState;
		}
	}
	private string _victoryString;
	//UIstuff
	public bool debugGUI;
	public Texture scoreBgTexture;
	public Texture scoreTexture1;
	public Texture scoreTexture2;
	public Texture winTexture1;
	public Texture winTexture2;
	private GameObject prfbBeacon;
	public VictoryCondition vIsForVendetta;	
	public int currentMarquee;
	public AudioClip Victory_Gong;

	// Use this for initializatio
	void Start () {
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
		prfbPlayer = (GameObject)Resources.Load("Prefabs/Player");
		prfbAltar = (GameObject)Resources.Load("Prefabs/Altar");
		prfbHome = (GameObject)Resources.Load ("Prefabs/Home");
		prfbBeacon = (GameObject)Resources.Load ("Prefabs/Beacon");
		altars= new List<GameObject>();
		victoryConditions = new List<VictoryCondition>();
		teams = new List<TeamInfo>();
		StartCoroutine(doMarquee());
		
	}
	/// <summary>
	/// For the length of the script, every number of frames, u
	/// </summary>
	/// <returns>The marquee.</returns>
	public IEnumerator doMarquee(){
		while(true){
			yield return new WaitForSeconds(sRef.secMarqueeUpgradeTime);
			currentMarquee = (currentMarquee +1) % sRef.marqueeCount;
		}
		
		
	}
	// Update is called once per frame
	void Update () {
		if (setup){
			GameObject team1Home, team2Home;
			team1Home  = null;
			team2Home = null;
			List<AltarType> altarTypes = System.Enum.GetValues(typeof(AltarType)).Cast<AltarType>().ToList();
			if (sRef.numAltars > altarTypes.Count) Debug.LogError ("Too many altars and not enough altar types!"); 
			
			for (int i=0; i<sRef.numAltars; i++){
				Debug.Log ("in Altar");		
				GameObject a = (GameObject)Instantiate(prfbAltar, Vector3.zero, Quaternion.identity);
				Altar aObj = a.GetComponent<Altar>();
				aObj.setControl(null);
//				
				int index = Random.Range (0, altarTypes.Count);
				aObj.altarType = altarTypes[index];
				altarTypes.Remove(AltarType.MagicalMysteryScore);
				altarTypes.RemoveAt (index);
				altars.Add (aObj.gameObject);
				//				altars.Add (aObj.gameObject);
//				//		aObj.brdX = Random.Range(0, tiles.GetLength(0));
//				//		aObj.brdY = Random.Range(0, tiles.GetLength(1));
//				aObj.brdX = (tiles.GetLength(0) - 1 - i*7)-8;	//Temp
//				aObj.brdY = i*3+3;	//Temp

//				aObj.setControl(null);
//				aObj.transform.parent = tiles[aObj.brdX, aObj.brdY].transform;
//				aObj.transform.localPosition = new Vector3(0,0,-1);
//				altars.Add (aObj.gameObject);
			}

			for (int i=0; i<sRef.numScoringAltars; i++){
				Debug.Log ("in Altar");		
				GameObject a = (GameObject)Instantiate(prfbAltar, Vector3.zero, Quaternion.identity);
				Altar aObj = a.GetComponent<Altar>();
				aObj.setControl(null);
				aObj.altarType = AltarType.MagicalMysteryScore;
				altars.Add(a);
			}
			int absoluteMagnitude;
			absoluteMagnitude = Mathf.RoundToInt(Random.Range(10f, (new Vector2(tiles.GetLength(0), tiles.GetLength(1)).magnitude)*.75f));

			for (int i = 0; i< altars.Count; i++){
				Altar thisAltar = altars[i].GetComponent<Altar>();
				if(i != altars.Count -1){

					int x, y;
					//x^2 + y^2 = absoluteMag^2
					
					x = Mathf.RoundToInt(Random.Range(0f, absoluteMagnitude));
					y = Mathf.RoundToInt(Mathf.Sqrt(absoluteMagnitude * absoluteMagnitude - x*x));

					while(y >= tiles.GetLength(1)){
						x = Mathf.RoundToInt(Random.Range(0f, absoluteMagnitude));
						y = Mathf.RoundToInt(Mathf.Sqrt(absoluteMagnitude * absoluteMagnitude - x*x));

					}

					if(i % 2 == 1){
						thisAltar.brdX = tiles.GetLength(0)-1-x;
						thisAltar.brdY = tiles.GetLength(1)-1-y;

						absoluteMagnitude = Mathf.RoundToInt(Random.Range(15f, (new Vector2(tiles.GetLength(0), tiles.GetLength(1)).magnitude)*.75f));
					}
					else{
						thisAltar.brdX = x;
						thisAltar.brdY = y;
					}
					Debug.Log (string.Format("Altar created at({0}, {1})", thisAltar.brdX, thisAltar.brdY));
					thisAltar.transform.parent = tiles[thisAltar.brdX, thisAltar.brdY].transform;
					thisAltar.transform.localPosition = new Vector3(0,0,-1);
				
				}
				else{
					thisAltar.brdX = tiles.GetLength(0)/2;
					thisAltar.brdY = tiles.GetLength(1)/2;

					thisAltar.transform.parent = tiles[thisAltar.brdX, thisAltar.brdY].transform;
					thisAltar.transform.localPosition = new Vector3(0,0,-1);
				}


			}
			switch (gameMode){
			case Mode.TwoVTwo:{
				_currentState = GameState.playing;
				GameObject Player1 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player2 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player3 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player4 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);

				Player p1 = Player1.GetComponent<Player>();
				Player p2 = Player2.GetComponent<Player>();
				Player p3 = Player3.GetComponent<Player>();
				Player p4 = Player4.GetComponent<Player>();

				p1.SetTeam(TeamInfo.GetTeamInfo(1));
				p2.SetTeam(p1.team);

				p3.SetTeam(TeamInfo.GetTeamInfo(2));
				p4.SetTeam(p3.team);
				teams.Add(p1.team);
				teams.Add(p3.team);
				p1.PlayerNumber = 1;
				p2.PlayerNumber = 2;

				p3.PlayerNumber = 3;
				p4.PlayerNumber = 4;
				players.Add(Player1);
				players.Add(Player2);
				
				players.Add(Player3);
				players.Add(Player4);
				
					//steps to ensure validity
				team1Home = setUpTeamHome(p1);
				team2Home = setUpTeamHome(p2);
				
				//victoryConditions.Add (new LockMajorityAltars(1) );
				victoryConditions.Add (new ControlViaTime(1));
				victoryConditions.Add (new NetworkEnemyBase(1));
				break;
			}
			case Mode.OneVOne:{
				_currentState = GameState.playing;
				GameObject Player1 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player2 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				Player p1 = Player1.GetComponent<Player>();
				Player p2 = Player2.GetComponent<Player>();
				p1.SetTeam(TeamInfo.GetTeamInfo(1));
				p2.SetTeam(TeamInfo.GetTeamInfo(2));
				teams.Add(p1.team);
				teams.Add(p2.team);
				p1.PlayerNumber = 1;
				p2.PlayerNumber = 2;
				players.Add(Player1);
				players.Add(Player2);
				
				
				
				//steps to ensure validity
				team1Home = setUpTeamHome(p1);
				team2Home = setUpTeamHome(p2);
				
				//
				
				
				//victoryConditions.Add (new LockMajorityAltars(1) );
				victoryConditions.Add (new ControlViaTime(1));
				victoryConditions.Add (new NetworkEnemyBase(1));				
				break;
			}
			}
			//Check for any homebase islands, if so regenerate
			//Check for fairness?  
			//Remove water where it's on an altar or home base
			BaseTile team1Tile, team2Tile;
			team1Tile =tiles[(int)teams[0].startingLocation.x,(int)teams[0].startingLocation.y].GetComponent<BaseTile>();
			team2Tile = tiles[(int)teams[1].startingLocation.x,(int)teams[1].startingLocation.y].GetComponent<BaseTile>();
			while(!team1Tile.findEdges() && !team2Tile.findEdges()){
				GameObject.Find("TileCreator").GetComponent<TileCreation>().perlinPass(TileTypeEnum.water, sRef.optPerlinLevel);
			}

			checkFlipWater(team1Tile.brdXPos, team1Tile.brdYPos);
			checkFlipWater(team2Tile.brdXPos, team2Tile.brdYPos);

			altars.ForEach(delegate (GameObject altarGO){
				Altar A = altarGO.GetComponent<Altar>();
				checkFlipWater(A.brdX, A.brdY);
			});

			for(int beaconsBuilt = 0; beaconsBuilt < sRef.neutralBeaconCount; beaconsBuilt ++){
				int x= Random.Range(0, tiles.GetLength(0));
				int y = Random.Range(0, tiles.GetLength(1));

				while(!addNeutralBeacon(x,y)) {
					 x= Random.Range(0, tiles.GetLength(0));
					 y = Random.Range(0, tiles.GetLength(1));
				} ///Weird placeholder, just go till you find a decent spot
			}
			for(int x = 0; x<  tiles.GetLength(0); x++){
				for(int y =0 ; y< tiles.GetLength(1); y++){
					if(tiles[x,y].GetComponent<BaseTile>().currentType != TileTypeEnum.water){
						tiles[x,y].GetComponent<BaseTile>().tooCloseToBeacon();
					}
				}
			}
			foreach (GameObject o in players) {
			 o.GetComponent<Player>().RevealTiles (); 
			 }
			setup = false;
			
		}
		GameObject hoveredTile = getHoveredTile();
		if(hoveredTile!= null){
			debugMouse = getHoveredTile().GetComponent<BaseTile>();
		}
		if(Input.GetButtonDown("Fire1")){
			//A* test
//			if(debugMouse.owningTeam != null){
//				BaseTile finalDestination = tiles[(int)debugMouse.owningTeam.startingLocation.x,(int)debugMouse.owningTeam.startingLocation.y].GetComponent<BaseTile>();
//				if(debugMouse.owningTeam != null){
//					debug
//					debugString = string.Format("A star len: {0}", As.Count);
//				}
//			}
			if(debugMouse != null){
				Debug.Log (debugMouse.findEdges());
			}
		}
		if(Input.GetButtonDown("Fire2")){
			debugMouse.owningTeam = players[0].GetComponent<Player>().team;
			
			debugMouse.controllingTeam = players[0].GetComponent<Player>().team;
			
			debugMouse.percControlled =100f;
		}
		_victoryString = "";
		foreach (VictoryCondition v in victoryConditions){
			v.CheckState(this);
			//Debug.Log("checstate done");
			if(v.isCompleted){
				_currentState = GameState.gameWon;
				_victoryString += v.getVictorySting();
				vIsForVendetta = v;
			}
			
		}	
	}
	
	public GameObject getHoveredTile(){
		Vector3 MousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Debug.Log("Mouse" + MousePos.x + ", " + MousePos.y);
		try{
			return tiles[Mathf.RoundToInt(MousePos.x), Mathf.RoundToInt(MousePos.y)];
		}
		catch{
			return null;
		}
	}
	
	void OnGUI(){

		if(debugGUI == true){
			switch(_currentState){
			
				case GameState.playing:{
					if(sRef.debugMode){
						if(debugMouse!=null){
							GUI.Box (new Rect (10,100,200,90), string.Format("Mouse Over x:{0} y:{1}\r\nState: {2}\r\nPercentControlled: not yet ", debugMouse.brdXPos, debugMouse.brdYPos, debugMouse.currentState));
							
						}
						if(debugBeacon !=null){
							GUI.Box (new Rect (10,200,200,90), string.Format(" team {0} controlling\r\nstate: {1}", debugBeacon.controllingTeam.teamNumber, debugBeacon.currentState));
						}
						if(debugString != ""){
							GUI.Box (new Rect (210,100,200,90), debugString);
							
						}
					}
					break;
				}
				case GameState.gameWon:{
					audio.PlayOneShot(Victory_Gong, 0.7f);
					GUI.Box (new Rect (10,100,400,300), _victoryString);
					
					break;
				}
			
			
			}
		}
	}

	public void PlaySFX(AudioClip clip, float volume){
		audio.PlayOneShot(clip);

		if(audio.volume <= volume){
			audio.volume += 0.3f;
		}
		if(audio.volume >= volume){
		audio.volume = volume;
		}		
	}

	public void StopSFX(){
		audio.Stop();
	/*
		audio.volume -= 0.4f;
		StartCoroutine(StopSFXCoroutine ());*/
	}

	public IEnumerator StopSFXCoroutine(){
		yield return new WaitForSeconds(0.8f);
		audio.Stop();
	}

	private GameObject setUpTeamHome(Player example){
		GameObject team1Home = (GameObject)Instantiate(prfbHome, Vector3.zero, Quaternion.identity);
		team1Home.transform.parent= tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].transform;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().controllingTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().owningTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().percControlled = 100f;
		team1Home.transform.localPosition = new Vector3(0,0,-.5f);
		team1Home.GetComponent<Home>().team = example.team;
		
		
		//ToSet = tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>();
		
		return team1Home;
	}
	
	public BaseTile getTeamBase(TeamInfo T){
		BaseTile finalDestination = tiles[(int)T.startingLocation.x,(int)T.startingLocation.y].GetComponent<BaseTile>();
		return finalDestination;
	}
	
	public static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		int debugVal  = UnityEngine.Random.Range(0,A.Length);
		
		T V = (T)A.GetValue(debugVal);
		
		return V;
	}
	
	public List<AltarType> getCapturedAltars(TeamInfo t){
		List<AltarType> returnable = new List<AltarType>();
		altars.ForEach(delegate (GameObject ToCheckGO) {
			Altar ToCheck = ToCheckGO.GetComponent<Altar>();
			if(ToCheck.currentControllingTeam != null){
				if( (ToCheck.currentControllingTeam.teamNumber == t.teamNumber && ToCheck.isLocked) || (ToCheck.currentControllingTeam.teamNumber == t.teamNumber && ToCheck.networked && !sRef.optLockTile)  ) {
					returnable.Add(ToCheck.altarType);
				}
			}
			
		});		
		return returnable;
	}

	public void checkFlipWater(int x, int y){
		BaseTile bt = tiles[x,y].GetComponent<BaseTile>();
		if(bt.currentType == TileTypeEnum.water){
			bt.setTileType(TileTypeEnum.regular);
		}
	}

	private static GameManager _instance;
	
	//This is the public reference that other classes will use
	public static GameManager GameManagerInstance
	{
		get
		{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GameManager>();
			return _instance;
		}
	}
	/// <summary>
	/// Adds the neutral beacon.
	/// </summary>
	/// <returns>If the beacon was succsefully added, if the location is invalid returns false</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public bool addNeutralBeacon(int x, int y){
		GameObject BT = tiles[x,y];
		if(BT.GetComponent<BaseTile>().buildable()){
			GameObject beacon = (GameObject)Instantiate(prfbBeacon, Vector3.zero, Quaternion.identity);
			beacon.GetComponent<Beacon>().buildNeutral(BT);
			return true;
		}
		else{
			return false;
		}
	}

	public void tileSendMessage(string message){
		for(int x = 0; x<  tiles.GetLength(0); x++){
			for(int y =0 ; y< tiles.GetLength(1); y++){
				tiles[x,y].SendMessage(message);
			}
		}
	}
/**	
	public List<AltarType> getNetworkedAltars(TeamInfo t){
		List<AltarType> returnable = new List<AltarType>();
		altars.ForEach(delegate (GameObject ToCheckGO) {
			Altar ToCheck = ToCheckGO.GetComponent<Altar>();
			if(ToCheck.currentControllingTeam != null){
				if(ToCheck.currentControllingTeam.teamNumber == t.teamNumber && ToCheck.networked) {
					
					returnable.Add(ToCheck.altarType);
				}
			}
			
		});		
		return returnable;
	}
*/
			

}


