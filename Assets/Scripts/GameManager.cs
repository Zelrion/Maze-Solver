using UnityEngine;
public class GameManager:MonoBehaviour {
	#region Inspector Set Variables (Variables that are set in the Unity Editor)
	public Maze mazePrefab; //Premade Object to use as the Maze
	#endregion

	private Maze mazeInst;
	private DepthFirstSearch searchInst;
	void Start() { Begin(); } //Once manager loads, call Begin
	void Update() {
		if(Input.GetKeyDown(KeyCode.Space)) Restart(); //Every update check Spacebar; Call Restart if Pressed
		else {
			if(Input.GetKeyDown(KeyCode.Return)) SearchMaze();
		}
	}
	private void Begin() {
		mazeInst = Instantiate(mazePrefab) as Maze; //Instantiate the Prefab as a Maze Object
		StartCoroutine(mazeInst.Generate()); //Start Maze Generation
	}
	private void SearchMaze() {
		searchInst = mazeInst.GetComponent<DepthFirstSearch>();
		searchInst.Origin = mazeInst.GetCell(0,0);
		searchInst.Target = mazeInst.GetCell(mazeInst.size-IntVector2.Identity);
		StartCoroutine(searchInst.Solve()); //Start Maze Generation
	}
	private void Restart() {
		StopAllCoroutines(); //Stops all Coroutines running on MonoBehavior
		Destroy(mazeInst.gameObject); //Delete the Maze Instance
		Begin(); //Start Over
	}
}
