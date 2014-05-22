using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static bool overrideTutorial = false;
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
	public GameObject home1;
	public GameObject home2;
	private GameObject prfbBeacon;
	private GameObject prfbStartUp;
	private GameObject prfbTutorial;
	public VictoryCondition vIsForVendetta;	
	public int currentMarquee;

	public AudioClip Victory_Gong;
	public AudioClip Game_Launch;

	bool isPlaying = false;
	public List<GameObject> beacons;

	private GameObject _prfbBar;
	public GameObject teamBar1;
	public GameObject teamBar2;
	
	public List<GameObject> ReadyUps;

	public Texture[] tutorials;
	public int tutorialIndex = 0;
	private GameObject qudTutorial1, qudTutorial2;

	private float tutorialPerc = 0;


	// Use this for initializatio
	void Start () {

		//Fuck
		GameObject menuMusic;
		menuMusic = GameObject.FindWithTag ("MenuMusic");
		if (menuMusic != null) GameObject.Destroy (menuMusic);

		_prfbBar = (GameObject)Resources.Load("Prefabs/ScoreBar");
		prfbStartUp = (GameObject)Resources.Load("Prefabs/ReadyBackGround");
		ReadyUps = new List<GameObject>();
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();

		prfbTutorial = (GameObject)Resources.Load("Prefabs/Tutorial");
		beacons = new List<GameObject>();
		prfbPlayer = (GameObject)Resources.Load("Prefabs/Player");
		prfbAltar = (GameObject)Resources.Load("Prefabs/Altar");
		prfbHome = (GameObject)Resources.Load ("Prefabs/Home");
		prfbBeacon = (GameObject)Resources.Load ("Prefabs/Beacon");
		altars= new List<GameObject>();
		victoryConditions = new List<VictoryCondition>();
		teams = new List<TeamInfo>();
		StartCoroutine(doMarquee());
		Camera.main.orthographicSize = sRef.cameraSize;
		Camera.main.transform.position = sRef.cameraPosition;

		audio.PlayOneShot(Game_Launch, sRef.startGameVolume);
		qudTutorial1 = (GameObject)GameObject.Instantiate(prfbTutorial, new Vector3(100f,100f,10f), Quaternion.identity);
		
		qudTutorial2 = (GameObject)GameObject.Instantiate(prfbTutorial, new Vector3(101f,101f,10f), Quaternion.identity);
		
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
//	Debug.Log(currentState);
	if(Input.GetKeyDown("r")) {
		Time.timeScale = 1.0f;
		Pause.paused = false;
		Application.LoadLevel("PierreOptions");
	}
	
		if (setup){
			GameObject team1Home, team2Home;
			team1Home  = null;
			team2Home = null;
			List<AltarType> altarTypes = System.Enum.GetValues(typeof(AltarType)).Cast<AltarType>().ToList();
			if (sRef.numAltars > altarTypes.Count) Debug.LogError ("Too many altars and not enough altar types!"); 
			
			switch (sRef.gameMode){
			case Mode.TwoVTwo:{
				GameObject Player1 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player2 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player3 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player4 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);

				Player p1 = Player1.GetComponentInChildren<Player>();
				Player p2 = Player2.GetComponentInChildren<Player>();
				Player p3 = Player3.GetComponentInChildren<Player>();
				Player p4 = Player4.GetComponentInChildren<Player>();

				p1.SetTeam(TeamInfo.GetTeamInfo(1));
				p1.SetColor (p1.team.teamColor);
				p2.SetTeam(p1.team);
				p2.SetColor(p1.team.teamColorAlt);
				
				p3.SetTeam(TeamInfo.GetTeamInfo(2));
				p3.SetColor(p3.team.teamColor);
				p4.SetTeam(p3.team);
				p4.SetColor(p3.team.teamColorAlt);
				
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
				team2Home = setUpTeamHome(p3);
				
				//victoryConditions.Add (new LockMajorityAltars(1) );
				victoryConditions.Add (new ControlViaTime(1));
				// victoryConditions.Add (new NetworkEnemyBase(1));
				
				
				GameObject Player1ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x/8f) -.5f, sRef.boardSize.y*6/8, -3f), Quaternion.identity);
				GameObject Player2ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x/8f) -.5f, sRef.boardSize.y*2/8, -3f), Quaternion.identity);
				GameObject Player3ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x*7f/8f) -.5f,  sRef.boardSize.y*6/8, -3f), Quaternion.identity);
				GameObject Player4ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x*7f/8f) -.5f,sRef.boardSize.y*2/8, -3), Quaternion.identity);
				
				Player1ReadyUp.GetComponent<ReadyUp>().setPlayer(p1);	
				Player2ReadyUp.GetComponent<ReadyUp>().setPlayer(p2);
				Player3ReadyUp.GetComponent<ReadyUp>().setPlayer(p3);
				Player4ReadyUp.GetComponent<ReadyUp>().setPlayer(p4);
				
				Camera cam = Camera.main;
				float height = 2f * cam.orthographicSize;
				float width = height * cam.aspect;
				
//				Vector3 size = new Vector3(width*.3f, height * .3f,1f);
//				
//				Player1ReadyUp.transform.localScale = size;
//				Player2ReadyUp.transform.localScale = size;
//				Player3ReadyUp.transform.localScale = size;
//				Player4ReadyUp.transform.localScale = size;
				
				
				ReadyUps.Add (Player1ReadyUp);
				ReadyUps.Add(Player2ReadyUp);
				ReadyUps.Add (Player3ReadyUp);
				ReadyUps.Add(Player4ReadyUp);
				break;
			}
			case Mode.OneVOne:{
				GameObject Player1 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				GameObject Player2 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
				Player p1 = Player1.GetComponentInChildren<Player>();
				Player p2 = Player2.GetComponentInChildren<Player>();
				p1.SetTeam(TeamInfo.GetTeamInfo(1));
				p1.SetColor (p1.team.teamColor);
				p2.SetTeam(TeamInfo.GetTeamInfo(2));
				p2.SetColor(p2.team.teamColor);
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
				GameObject Player1ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x/8f) -.5f	, sRef.boardSize.y/2, -3), Quaternion.identity);
				GameObject Player2ReadyUp = (GameObject)Instantiate(prfbStartUp, new Vector3((sRef.boardSize.x*7f/8f) -.5f, sRef.boardSize.y/2, -3), Quaternion.identity);
				
				Camera cam = Camera.main;
				float height = 2f * cam.orthographicSize;
				float width = height * cam.aspect;
				
				//Vector3 size = new Vector3(width*.4f, height * .45f,1f);
				
				
				
				//Player1ReadyUp.transform.localScale = size;
				
				//Player2ReadyUp.transform.localScale = size;
				
				Player1ReadyUp.GetComponent<ReadyUp>().setPlayer(p1);
				Player2ReadyUp.GetComponent<ReadyUp>().setPlayer(p2);
				ReadyUps.Add (Player1ReadyUp);
				ReadyUps.Add(Player2ReadyUp);
				//victoryConditions.Add (new LockMajorityAltars(1) );
				victoryConditions.Add (new ControlViaTime(1));
				//victoryConditions.Add (new NetworkEnemyBase(1));
								
				break;
			}
			}
			teamBar1 = (GameObject)GameObject.Instantiate(_prfbBar, new Vector3(sRef.scorePos1.x, sRef.scorePos1.y,0), Quaternion.identity);
			teamBar1.GetComponent<Bar>().team = teams[0];
			teamBar2 = (GameObject)GameObject.Instantiate(_prfbBar, new Vector3(sRef.scorePos2.x, sRef.scorePos2.y,0), Quaternion.identity);
			teamBar2.GetComponent<Bar>().team = teams[1];
			teams[0].ScoreBar = teamBar1;
			teams[1].ScoreBar = teamBar2;
			
			teams[0].finalTarget = teamBar1.transform.FindChild ("ScoreBitFinalTarget").gameObject;
			teams[1].finalTarget = teamBar2.transform.FindChild ("ScoreBitFinalTarget").gameObject;
			
			teams[0].sRef = sRef;
			teams[1].sRef = sRef;
			
			ReadyUps.ForEach(delegate(GameObject g){
				g.renderer.enabled = true;
			});
			
			
			
			//Check for any homebase islands, if so regenerate
			//Check for fairness?  
			//Remove water where it's on an altar or home base
			BaseTile team1Tile, team2Tile;
			team1Tile =tiles[(int)teams[0].startingLocation.x,(int)teams[0].startingLocation.y].GetComponent<BaseTile>();
			team2Tile = tiles[(int)teams[1].startingLocation.x,(int)teams[1].startingLocation.y].GetComponent<BaseTile>();
			int maxTries = 10;
			
			
			checkFlipWater(team1Tile.brdXPos, team1Tile.brdYPos);
			checkFlipWater(team2Tile.brdXPos, team2Tile.brdYPos);
			
			while(!team1Tile.findEdges() && !team2Tile.findEdges() && maxTries > 0){
					Debug.Log ("Attempt: " + maxTries);
					tileSendMessage("Reset");
					GameObject.Find("TileCreator").GetComponent<TileCreation>().perlinPass(TileTypeEnum.water, sRef.optPerlinLevel);
					maxTries--;
				
				checkFlipWater(team1Tile.brdXPos, team1Tile.brdYPos);
				checkFlipWater(team2Tile.brdXPos, team2Tile.brdYPos);
			}

			
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
				GameObject a = (GameObject)Instantiate(prfbAltar, Vector3.zero, Quaternion.identity);
				Altar aObj = a.GetComponent<Altar>();
				aObj.setControl(null);
				aObj.altarType = AltarType.MagicalMysteryScore;
				altars.Add(a);
			}
			int absoluteMagnitude;
			absoluteMagnitude = getRandomMagnitude(.2f,.35f);


			Vector2[] alterPositions  = new Vector2[4]{new Vector2(sRef.boardSize.x*3/8,sRef.boardSize.y*2/8), new Vector2(sRef.boardSize.x*3/8,sRef.boardSize.y*6/8), new Vector2(sRef.boardSize.x*5/8,sRef.boardSize.y*2/8),new Vector2( sRef.boardSize.x*5/8,sRef.boardSize.y*6/8)};
			for (int i = 0; i< altars.Count; i++){
				Altar thisAltar = altars[i].GetComponent<Altar>();
				if(i != altars.Count -1){

					thisAltar.brdX = (int)alterPositions[i].x;
					thisAltar.brdY = (int)alterPositions[i].y;
					
//
//					if(i % teams.Count == 1){
//
//					}
//
//					Vector2 validPos = generateValidAltarPosition(thisAltar, teams[ i % teams.Count].startingLocation , ( i % teams.Count == 1) ? true : false, absoluteMagnitude);
//					thisAltar.brdX = (int)validPos.x;
//					thisAltar.brdY = (int)validPos.y;
				}
				else{
					thisAltar.brdX = tiles.GetLength(0)/2;
					thisAltar.brdY = tiles.GetLength(1)/2;
				
				}
				
				switch (PlayerPrefs.GetInt (PreferencesOptions.gameSpeed.ToString())) {
				case 1: 
					thisAltar.scoreBitInterval = sRef.scoreBitIntervalSlow;
					break;
				case 2:
					thisAltar.scoreBitInterval = sRef.scoreBitIntervalNormal;
					break;
				case 3:
					thisAltar.scoreBitInterval = sRef.scoreBitIntervalFast;
					break;
				default:
					thisAltar.scoreBitInterval = sRef.scoreBitIntervalNormal;
					Debug.LogWarning ("Game speed was a weird value while setting altar score bit intervals");
					break;
				}
			}


			int randomXOffset = Random.Range(0,3);
			int randomYOffset = Random.Range(0,2);
			
			for(int i = 0; i < altars.Count; i++){
				if( (i % 2) == 1){
					altars[Random.Range(0,altars.Count)].GetComponent<Altar>().brdX+=randomXOffset;
					
					altars[Random.Range(0,altars.Count)].GetComponent<Altar>().brdY+=randomYOffset;
				}else{
					
					altars[Random.Range(0,altars.Count)].GetComponent<Altar>().brdX-=randomXOffset;
					
					altars[Random.Range(0,altars.Count)].GetComponent<Altar>().brdY-=randomYOffset;
				
				}
				
			}


			int AStarTotalTeam1 = 0;
			int AStarTotalTeam2 = 0;
			
			AStarTotalTeam1 = calculateDistanceToAltars(teams[0]);
			//Debug.Log ("A star total blue = " + AStarTotalTeam1);
			
			AStarTotalTeam2 = calculateDistanceToAltars(teams[1]);
			
			//Debug.Log ("A star total yellow = " + AStarTotalTeam2);
			maxTries= 20;
			while(Mathf.Abs(AStarTotalTeam1 - AStarTotalTeam2) >5 && maxTries > 0){
				//Grab a random altar
				BaseTile home;
				TeamInfo weakTeam;
				if(AStarTotalTeam1 > AStarTotalTeam2){
					home = teams[0].goGetHomeTile().GetComponent<BaseTile>();
					weakTeam = teams[0];
				}
				else{
					
					home = teams[1].goGetHomeTile().GetComponent<BaseTile>();
					weakTeam = teams[1];
				}
				
				
				bool stop = true;
				
				
				Altar A = altars[Random.Range(0, altars.Count)].GetComponent<Altar>();
				BaseTile AltarLoc = tiles[A.brdX, A.brdY].GetComponent<BaseTile>();
				List<AStarholder> toHome = BaseTile.aStarSearch(AltarLoc, home,int.MaxValue, BaseTile.getLocalTraversableTiles,weakTeam);
				try{
					BaseTile next = toHome[1].current;
					if(next.getLocalAltar() == null){
						A.brdX = next.brdXPos;
						A.brdY = next.brdYPos;
					}
				}catch{
				}
				
				//Debug.Log ("Shifting Altar");	
				AStarTotalTeam1 = calculateDistanceToAltars(teams[0]);
				//Debug.Log ("A star total blue = " + AStarTotalTeam1);
				
				AStarTotalTeam2 = calculateDistanceToAltars(teams[1]);
				
				//Debug.Log ("A star total yellow = " + AStarTotalTeam2);
				maxTries --;
				
			}

			altars.ForEach(delegate (GameObject altarGO){
				Altar A = altarGO.GetComponent<Altar>();
				
				
				A.transform.parent = tiles[A.brdX, A.brdY].transform;
				A.transform.localPosition = new Vector3(0,0,-1);
				
				checkFlipWater(A.brdX, A.brdY);
			});

			float xMin, yMin, xMax, yMax;
			
			//we are just going to assume a 4/3 x/y split here
			
			int beaconXcount = sRef.neutralBeaconCount / 3;
			int beaconYcount = sRef.neutralBeaconCount / 4;
			
			float areaX = ((float)tiles.GetLength(0)/(float)beaconXcount);
			
			float areaY = ((float)tiles.GetLength(1)/(float)beaconYcount);
			
			//littlex/ * littley
			//int width =  tiles.GetLength(1)/Mathf.Sqrt(sRef.neutralBeaconCount);
			//Add Neutral beacons
			
			for(int xBeacons = 0; xBeacons < Mathf.RoundToInt(beaconXcount); xBeacons ++){
				for(int yBeacons = 0; yBeacons < Mathf.RoundToInt(beaconYcount); yBeacons ++){
					xMin = xBeacons * areaX;
					xMax = (xBeacons * areaX) + areaX;
					if(xMax > tiles.GetLength(0)){
						xMax =  tiles.GetLength(0);
					}
					
					yMin = yBeacons * areaY;
					yMax = (yBeacons * areaY) + areaY;
				
					if(yMax > tiles.GetLength(1)){
						yMax =  tiles.GetLength(1);
					}
					int x= Mathf.FloorToInt(Random.Range(xMin, xMax));
					int y = Mathf.FloorToInt(Random.Range(yMin, yMax));
	
					int maxWhile  = 0;
					while(!addNeutralBeacon(x,y) && maxWhile < 5) {
						x= Mathf.FloorToInt(Random.Range(xMin, xMax));
	                    y = Mathf.FloorToInt(Random.Range(yMin, yMax));
						 maxWhile++;
						 //Debug.Log ("maxwhile: " + maxWhile);
					} ///Weird placeholder, just go till you find a decent spot
				}
			}
//			for(int beaconsBuilt = 0; beaconsBuilt < sRef.neutralBeaconCount; beaconsBuilt ++){
//				float xMin = 
//				float yMin = beaconsBuilt*tiles.GetLength(1)/sRef.neutralBeaconCount;
//				
//				
//				float xMax = (beaconsBuilt+1)*tiles.GetLength(0)/sRef.neutralBeaconCount;
//				float yMax = (beaconsBuilt+1)*tiles.GetLength(1)/sRef.neutralBeaconCount;
//				
//				
//				int x= Mathf.FloorToInt(Random.Range(xMin, xMax));
//				int y = Mathf.FloorToInt(Random.Range(yMin, yMax));
//
//				int maxWhile  = 0;
//				while(!addNeutralBeacon(x,y) && maxWhile < 5) {
//					 x= Random.Range(0, tiles.GetLength(0));
//					 y = Random.Range(0, tiles.GetLength(1));
//					 maxWhile++;
//				} ///Weird placeholder, just go till you find a decent spot
//			}
//			
			
			for(int x = 0; x<  tiles.GetLength(0); x++){
				for(int y =0 ; y< tiles.GetLength(1); y++){
					if(tiles[x,y].GetComponent<BaseTile>().currentType != TileTypeEnum.water){
						tiles[x,y].GetComponent<BaseTile>().tooCloseToBeacon();
					}
				}
			}
			foreach (GameObject o in players) {
			 o.GetComponentInChildren<Player>().RevealTiles (); 
			 }
						
			setup = false;
			//Debug.Log (GameManager.overrideTutorial);
			if(sRef.tutorial){
				qudTutorial1.renderer.enabled = true;
				qudTutorial1.transform.position = new Vector3((sRef.boardSize.x/2)-.5f, (sRef.boardSize.y/2)-.5f, -3);
				qudTutorial1.transform.localScale = new Vector3(sRef.boardSize.x, sRef.boardSize.x*(3f/4f), 1);
				qudTutorial1.renderer.material.mainTexture = tutorials[tutorialIndex];
				
				qudTutorial2.renderer.enabled = true;
				qudTutorial2.transform.position = new Vector3((sRef.boardSize.x/2)-.5f, -20, -3);
				qudTutorial2.transform.localScale = new Vector3(sRef.boardSize.x, sRef.boardSize.x*(3f/4f), 1);
				qudTutorial2.renderer.material.mainTexture = tutorials[tutorialIndex];
				_currentState = GameState.tutorial;
				ReadyUps.ForEach(delegate (GameObject g) {
					g.SetActive(false);	
				});
				//Debug.Log ("In tutorial");
				
			} else if(sRef.useReadyUp){
				_currentState = GameState.gameNotStarted;
				ReadyUps.ForEach(delegate (GameObject g) {
					g.SetActive(true);	
				});
			} else{
				_currentState = GameState.playing;
				ReadyUps.ForEach(delegate (GameObject g) {
					g.SetActive(false);	
				});
			}
			
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
//				Debug.Log (debugMouse.findEdges());
			}
		}
		if(Input.GetButtonDown("Fire2")){
			if(debugMouse!= null){
				debugMouse.owningTeam = players[0].GetComponentInChildren<Player>().team;
				
				debugMouse.controllingTeam = players[0].GetComponentInChildren<Player>().team;
				
				debugMouse.addInfluenceReturnOverflow(100f);
			}
		}

		if(_currentState == GameState.gameNotStarted){
			bool startMatch  = true;
			ReadyUps.ForEach(delegate(GameObject g){
				if(!g.GetComponent<ReadyUp>().ready){
					startMatch = false;
				}
			});
			
			if(startMatch){
				
				ReadyUps.ForEach(delegate(GameObject g){
					g.SetActive(false);
				});
				
				_currentState = GameState.playing;
			}
			
			
		}
		if(_currentState == GameState.tutorial){
			//Debug.Log ("Getting Here");
			qudTutorial1.transform.FindChild("TutorialClock").renderer.material.SetFloat("_Cutoff",1.01f-(tutorialPerc /100f));
			qudTutorial2.transform.FindChild("TutorialClock").renderer.material.SetFloat("_Cutoff",1.01f-(tutorialPerc /100f));
			
			if(Input.GetButton("BuildAllPlayers") ){
				tutorialPerc+= sRef.vpsBasePlayerInfluence*2f * Time.deltaTime;	
				//Debug.Log ("Here");
			}
			else{
				tutorialPerc = 0f;
			}
				
			if(tutorialPerc > 100){
				if(!swapTutorial()){
					//qudTutorial1.SetActive(false);
					//qudTutorial2.SetActive(false);
					PlayerPrefs.SetInt(PreferencesOptions.tutorial.ToString(), 0);
					if(sRef.useReadyUp){_currentState = GameState.gameNotStarted;
						ReadyUps.ForEach(delegate (GameObject g) {
							g.SetActive(true);	
						});
					}
					else{
						_currentState = GameState.playing;
						GameManager.overrideTutorial = true;
					}
					
				}
				tutorialPerc = 0f;
			}
			
		}
		
		if(_currentState == GameState.playing){
			_victoryString = "";
			foreach (VictoryCondition v in victoryConditions){
				v.CheckState(this);
				//Debug.Log("checstate done");

				if(v.isCompleted && !isPlaying){
					audio.PlayOneShot(Victory_Gong, sRef.victoryVolume);
					isPlaying = true;
					_currentState = GameState.gameWon;
					_victoryString += v.getVictorySting();
					vIsForVendetta = v;
					Invoke("setRestartable", sRef.secTillRestartable);
				}

			}
			if(Pause.paused){
				_currentState = GameState.paused;
				Time.timeScale = 0;
			} 
		}

		if(_currentState == GameState.gameWon){
			
		}

		if(_currentState == GameState.paused){
				if (Pause.paused == false){
				Time.timeScale = 1;
				_currentState = GameState.playing;

			}
		}
			 
		if(_currentState == GameState.gameRestartable){
			//if(Input.GetButtonDown("BuildPlayer1")){
			//	Application.LoadLevel(Application.loadedLevel);
			//}
		}
		

	}

	public void resetTutorialClocks(){
		tutorialPerc = 0;
	}

	public bool swapTutorial(){
		Hashtable qud1HT = new Hashtable();
		
		//Debug.Log("Tutorial Index" + tutorialIndex);
		Hashtable qud2HT = new Hashtable();
		
		if(tutorials.Length > tutorialIndex+1){
			qudTutorial1.renderer.material.mainTexture = tutorials[tutorialIndex+1];
		}
		else{
			qudTutorial1.SetActive(false);
			
			return false;
			
		}
		
		/*GameObject swapHolder;
		qudTutorial2.transform.position = new Vector3((sRef.boardSize.x/2)-.5f, -20, -3);
		
		qud1HT.Add("y", 40);
		qud1HT.Add("time", 3f);
		qud1HT.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.MoveTo(qudTutorial1, qud1HT);
		
		qud2HT.Add("y", (sRef.boardSize.y/2) -.5f);
		qud2HT.Add("time", .5f);
		qud2HT.Add("easetype", iTween.EaseType.easeInElastic);
		iTween.MoveTo(qudTutorial2, qud2HT);
		
		/*swapHolder = qudTutorial1;
		qudTutorial1 = qudTutorial2;
		qudTutorial2 = swapHolder;*/
		tutorialIndex++;
		return true;
		
	}

	public void setRestartable(){
		_currentState = GameState.gameRestartable;
	}
public Vector2 generateValidAltarPosition(Altar thisAltar, Vector2 startPos, bool flip, int absoluteMagnitude){

		int x, y;
		//x^2 + y^2 = absoluteMag^2
		
		x = Mathf.RoundToInt(Random.Range(0f, absoluteMagnitude));
		y = Mathf.RoundToInt(Mathf.Sqrt(absoluteMagnitude * absoluteMagnitude - x*x));

		if(!flip){
			x += (int)teams[0].startingLocation.x;
			y += (int)teams[0].startingLocation.y;
		}
		else{
			x = (int)teams[0].startingLocation.x - x;
			y = (int)teams[0].startingLocation.y -y;
		}
		
		int origX = x;
		int baseX = x;
		while(y >= tiles.GetLength(1) || getDistanceToNearestAltar(new Vector2(x,y)) < 4f || getDistanceToNearestStart(new Vector2(x,y)) < 5f || y < 0 || x <= 0){
		
			baseX++;
			x = baseX;
			y = Mathf.RoundToInt(Mathf.Sqrt(absoluteMagnitude * absoluteMagnitude - x*x));
			
			if(!flip){
				x += (int)teams[0].startingLocation.x;
				y += (int)teams[0].startingLocation.y;
			}
			else{
				x = (int)teams[0].startingLocation.x - x;
				y = (int)teams[0].startingLocation.y -y;
			}
		}



//		if(i % 2 == 1){
//			thisAltar.brdX = tiles.GetLength(0)-1-x;
//			thisAltar.brdY = tiles.GetLength(1)-1-y;
//			
//			absoluteMagnitude  = getRandomMagnitude(.3f,.4f);
//		}
//		else{
			//thisAltar.brdX = x;
			//thisAltar.brdY = y;
//		}
		return new Vector2(x,y);
	}

	public float getDistanceToNearestStart(Vector2 position){
		float minDistance = float.MaxValue;

		teams.ForEach(delegate(TeamInfo t){
			float distance = Vector2.Distance(position, t.startingLocation);
			if(distance < minDistance) {minDistance = distance;}
		});
		return minDistance;
	}

	
	public float getDistanceToNearestAltar(Vector2 position){
		float minDistance = float.MaxValue;

		
		altars.ForEach(delegate(GameObject t){

			Vector2 test = new Vector2(t.GetComponent<Altar>().brdX,t.GetComponent<Altar>().brdY);
			float distance = Vector2.Distance(position,test);
			if(distance < minDistance) {minDistance = distance;}
		});
		return minDistance;
	}


	public float getDistanceToNearestBeacon(Vector2 position){
		float minDistance = float.MaxValue;

		
		beacons.ForEach(delegate(GameObject t){
			float distance = Vector2.Distance(position, new Vector2(t.transform.parent.gameObject.GetComponent<BaseTile>().brdXPos,t.transform.parent.gameObject.GetComponent<BaseTile>().brdYPos));
			if(distance < minDistance) {minDistance = distance;}
		});
		return minDistance;
	}

	public int getRandomMagnitude(float lower, float higher){
		return Mathf.RoundToInt(Random.Range( (new Vector2(tiles.GetLength(0), tiles.GetLength(1)).magnitude)*lower, (new Vector2(tiles.GetLength(0), tiles.GetLength(1)).magnitude)*higher));

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
					
					GUI.Box (new Rect (10,100,400,300), _victoryString);
					
					break;
				}
			
			
			}
		}
	}

	public IEnumerator StopSFXCoroutine(){
		yield return new WaitForSeconds(0.8f);
		audio.Stop();
	}

	private GameObject setUpTeamHome(Player example){
		GameObject teamHome = (GameObject)Instantiate(prfbHome, Vector3.zero, Quaternion.identity);
		teamHome.transform.parent= tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].transform;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().controllingTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().owningTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().addInfluenceReturnOverflow(100f);
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].transform.FindChild("NoBuildLayer").GetComponent<MeshRenderer>().enabled = true;
		teamHome.transform.localPosition = new Vector3(0,0,-.5f);
		teamHome.GetComponent<Home>().team = example.team;
//		teamHome.GetComponentInChildren<ParticleSystem>().startColor = example.team.highlightColor;
		if (example.teamNumber == 1) {
			home1 = teamHome;
		}
		if (example.teamNumber == 2) {
			home2 = teamHome;
		}
		//Destroy(team1Home.transform.parent.FindChild("ScoreBitTarget").gameObject);
		
		//ToSet = tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>();
		
		return teamHome;
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
			beacons.Add(beacon);
			if (BT.GetComponent<BaseTile>().IsRevealed && BT.GetComponent<BaseTile>().beacon != null){
				BT.GetComponent<BaseTile>().beacon.transform.FindChild ("Arrow").GetComponent<MeshRenderer>().enabled = true;	
				BT.GetComponent<BaseTile>().beacon.transform.FindChild ("ArrowShot").GetComponent<MeshRenderer>().enabled = true;	
			}
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
 

			
	private int calculateDistanceToAltars(TeamInfo team){			
		int returnable = 0;
		foreach(GameObject altarGO in altars){
			Altar a = altarGO.GetComponent<Altar>();
			BaseTile tile = tiles[a.brdX, a.brdY].GetComponent<BaseTile>();
			List<AStarholder>  pathToHome;
			pathToHome = BaseTile.aStarSearch(tile, team.goGetHomeTile().GetComponent<BaseTile>(),int.MaxValue, BaseTile.getLocalTraversableTiles,team);
			returnable += pathToHome.Count;
			//A.transform.parent = tiles[A.brdX, A.brdY].transform;
			//A.transform.localPosition = new Vector3(0,0,-1);
			
			//checkFlipWater(A.brdX, A.brdY);
		}
		return returnable;
	}
	
	
}


