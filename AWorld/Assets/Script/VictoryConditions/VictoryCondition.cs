/// <summary>
///  Abstact victory condition contains a method to check if it's there
/// </summary>
public abstract class VictoryCondition{
	public bool isCompleted;
	public int value;
	public TeamInfo completingTeam;
	public string victoryMessage;
	
	public VictoryCondition(int valueIn){
		isCompleted = false;
		this.value = value;
		completingTeam = null;
	}
	
	public abstract void CheckState(GameManager gm);
	
	public void SetVictory(TeamInfo T){
		isCompleted = true;
		completingTeam = T;
	}
	
	public string getVictorySting(){
		if(completingTeam != null){
			return string.Format(victoryMessage, completingTeam.teamNumber);
		}
		else{
			return null;
		}
	}
}
