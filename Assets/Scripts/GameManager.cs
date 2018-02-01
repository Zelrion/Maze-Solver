using UnityEngine;
public class GameManager:MonoBehaviour {
	#region Inspector Set Variables (Variables that are set in the Unity Editor)
	public Maze mazePrefab; //Premade Object to use as the Maze
	#endregion

	private Maze mazeInst;
	private MazeSearch searchInst;
	private IntVector2[] mazeSizes = { new IntVector2(15,15),new IntVector2(50,50),new IntVector2(100,100) };
	public int SearchType { get; set; }
	public int MazeSize { get; set; }
	public float SearchDelay { get; set; }
	public bool UseDelay { get; set; }
	public bool MazePerfection { get; set; }
	void Start() {
		MazeSize = 0;
		UseDelay = true;
		Begin();
	}
	void Update() { //Every Update
		if(Input.GetKeyDown(KeyCode.Space)) Restart(); //Check Spacebar; Call Restart if Pressed
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SearchMaze(); //Check Return/Enter; Call SearchMaze if Pressed
	}
	private void FixedUpdate() {
		if(searchInst != null) { //Adjust Properties for Search
			searchInst.useDelay = UseDelay;
			searchInst.stepDelay = SearchDelay;
		}
	}
	private void Begin() {
		mazeInst = Instantiate(mazePrefab) as Maze; //Instantiate the Prefab as a Maze Object
		mazeInst.size = mazeSizes[MazeSize];
		mazeInst.perfectMaze = MazePerfection;
		searchInst = mazeInst.GetComponent<MazeSearch>();
		StartCoroutine(mazeInst.Generate()); //Start Maze Generation
	}
	public void SearchMaze() {
		if(!searchInst.IsSearching) {
			searchInst.Origin = mazeInst.GetCell(0,0);
			searchInst.Target = mazeInst.GetCell(mazeInst.size - IntVector2.Identity);
			switch(SearchType) { //Determine which Search to use; Replace with use of Delegates Later
				case 0: StartCoroutine(searchInst.DepthFirst()); break; //Start Maze Search
				case 1: StartCoroutine(searchInst.BreadthFirst()); break; //Start Maze Search
			}
		}
	}
	public void Restart() {
		StopAllCoroutines(); //Stops all Coroutines running on MonoBehavior
		Destroy(mazeInst.gameObject); //Delete the Maze Instance
		Begin(); //Start Over
	}
}
