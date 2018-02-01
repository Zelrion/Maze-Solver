using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Maze))] //Requires a Maze Component to Exist on Same GameObject
public class MazeSearch:MonoBehaviour {
	public delegate void SearchStatisticsEventHandler(string stats); //Event Delegate
	public static event SearchStatisticsEventHandler UpdateStatistics; //Invokable Event for Updating Statistics

	#region Inspector Set Variables (Variables that are set in the Unity Editor)
	public GameObject marker;
	public Material
		m_Processing,
		m_Seen,
		m_Visited,
		m_Path;
	public bool useDelay = true;
	[Range(0.02f,1f)]
	[Tooltip("Timed Delay between steps.")] //Limit to inclusive range of 0.02 to 2.0
	public float stepDelay; //Time before another step can occur.
	#endregion

	#region SearchData (Variables and Properties that are used with most searches)
	public bool IsSearching { get { return isSearching; } }
	public Cell Origin { get; set; } //Search Origin
	public Cell Target { get; set; } //Search Target
	private Maze mazeInst;
	private Dictionary<Cell,GameObject> markers;
	private int
		sizeX, sizeZ,
		pathSize = 0,
		totalSteps = 0,
		totalSeen = 0,
		totalBacktracks = 0;
	private bool isSearching = false;
	#endregion

	private void Start() {
		mazeInst = gameObject.GetComponent<Maze>(); //Get Maze Instance
		markers = new Dictionary<Cell,GameObject>(sizeX * sizeZ); //Possible for a Search to have a Marker in every Cell; 1:1 Total Cells
	}
	private void LateUpdate() {
		if(UpdateStatistics != null) { //If any Subscribers/Listeners Exist for Invokable Event
			UpdateStatistics("" //Invoke Event
				+ string.Format("{0,-17}{1}\n","Searching:",isSearching)
				+ string.Format("{0,-17}{1}\n","Maze:","[" + sizeX + ":" + sizeZ + "]")
				+ string.Format("{0,-17}{1}\n","Total Steps:","[" + totalSteps + "]")
				+ string.Format("\t{0,-13}{1}\n","Cells Seen:","[" + totalSeen + "]")
				+ string.Format("\t{0,-13}{1}\n","Path Size:","[" + pathSize + "]")
				+ string.Format("\t{0,-13}{1}\n","Backtracks:","[" + totalBacktracks + "]")
			);
		}
	}
	private GameObject AddMarker(Cell curCell,Material mat) {
		GameObject mark = Instantiate(marker,curCell.transform); //Create Marker as a Child of the Cell
		SetMarkerMaterial(mark,mat); //Set Marker Material to Given Material
		markers.Add(curCell,mark); //Add Marker to Dictionary with Cell as the Key
		return mark;
	}
	private void SetMarkerMaterial(GameObject mark,Material mat) {
		mark.GetComponent<MeshRenderer>().material = mat; //Set Visual Material
		if(mat == m_Path) {
			mark.transform.localScale = new Vector3(0.65f,0.65f,1); //If Material is the Path Visual; Different Scale and Rotation
			mark.transform.localEulerAngles = new Vector3(90,0,0);
		} else if(mat == m_Visited) {
			mark.transform.localScale = new Vector3(0.35f,0.35f,1); //If Material is the Visited Visual; Different Scale and Rotation
			mark.transform.localEulerAngles = new Vector3(90,45,0);
		} else {
			mark.transform.localScale = new Vector3(0.5f,0.5f,1); //Default Scale and Rotation
			mark.transform.localEulerAngles = new Vector3(90,45,0);
		}
	}
	private GameObject GetMarker(Cell curCell) {
		if(markers.ContainsKey(curCell)) return markers[curCell]; //Check if Key Exists in Dictionary and return the Marker
		return null; //Key doesn't Exist; No Marker
	}
	private void RemoveMarker(Cell curCell) { //Removal of Singular Marker
		if(markers.ContainsKey(curCell)) { //Check if Key Exists in Dictionary
			GameObject mark = markers[curCell]; //Temp Store GameObject
			markers.Remove(curCell); //Remove from Dictionary
			Destroy(mark); //Destory the GameObject
		}
	}
	private void RemoveAllMarkers() { //Removal of all Markers
		Dictionary<Cell,GameObject>.ValueCollection values = markers.Values; //Temp Store Values
		foreach(GameObject mark in values) Destroy(mark); //Destroy the GameObject
		markers.Clear(); //Clear Dictionary
	}
	private bool Initialize() { //Initial Setup for a Search
		if(mazeInst.IsGenerated) { //If Maze is Generated
			if(markers.Count > 0) RemoveAllMarkers(); //Prior Search Markers Exist, Remove All
			pathSize = 0; totalSteps = 0; totalSeen = 0; totalBacktracks = 0; //Reset Statistics
			sizeX = mazeInst.size.x;
			sizeZ = mazeInst.size.z;
			AddMarker(Origin,m_Processing); //Create Origin Marker
			AddMarker(Target,m_Path); //Create Target Marker
			return true; //Maze is Ready
		}
		return false; //Maze is not Ready
	}
	internal IEnumerator DepthFirst() {
		isSearching = true;
		if(Initialize()) { //Only attempt if requirments are met
			HashSet<Cell> visited = new HashSet<Cell>(); //Create a HashSet to Contain Visited Cells
			Stack<Cell> stack = new Stack<Cell>(sizeX + sizeZ); //Create Stack with an Initial Size
			stack.Push(Origin); //Push Origin onto Stack

			bool targetFound = false; //Set Initial Value
			while(stack.Count > 0 && !targetFound) { //While Stack is not Empty and Incomplete
				totalSteps++;
				Cell curCell = stack.Peek(); //Get Current Cell
				GameObject mark = GetMarker(curCell); //Get the Cell's Marker; Should always Exist
				if(useDelay) {
					yield return new WaitForSeconds(stepDelay);
					SetMarkerMaterial(mark,m_Processing); //Visualize that it is being Processed
					yield return new WaitForSeconds(stepDelay);  //Pause to prevent locking up resources; Minimum 2ms Delay
				}
				if(visited.Contains(curCell)) { //Dead End or Already Visited; Backtrack
					stack.Pop(); //Remove from Stack
					SetMarkerMaterial(mark,m_Visited);
					pathSize--; //Decrement PathSize Counter
					totalBacktracks++; //Increment Backtrack Counter
					continue; //Continue to next iteration
				} else visited.Add(curCell); //Add to Visited 
				foreach(CellEdge edge in curCell.Edges) { //Each Edge of the Cell
					if(edge is CellWall) continue; //Skip Walls
					GameObject otherMark = GetMarker(edge.otherCell);
					if(!visited.Contains(edge.otherCell)) { //Ignore already Visited Cells
						if(edge.otherCell == Target) targetFound = true; //Found Target, DONE
						else if(otherMark == null) { //Possible to run into a Seen Cell that has not been Visited yet; Ignore if a Marker Exists
							AddMarker(edge.otherCell,m_Seen); //Add Marker to Cell and Visualize that it has been Seen
							stack.Push(edge.otherCell); //Push onto Stack
						} 
					}
				}
				SetMarkerMaterial(mark,m_Path); //Visualize current marker as the Path
				totalSeen = stack.Count - pathSize - 1; //Stack also contains entire Path
				pathSize++; //Increment PathSize Counter
			}
		}
		isSearching = false;
	}
	internal IEnumerator BreadthFirst() {
		isSearching = true;
		if(Initialize()) { //Only attempt if requirments are met
			Dictionary<Cell,CellMetadata> visited = new Dictionary<Cell,CellMetadata>();
			Queue<Cell> queue = new Queue<Cell>(sizeX + sizeZ); //Create Queue with an Initial Size
			queue.Enqueue(Origin); //Enqueue Origin
			visited.Add(Origin,new CellMetadata(null,0));

			int distance = 0;
			bool targetFound = false;
			while(queue.Count > 0 && !targetFound) {
				if(useDelay) {
					yield return new WaitForSeconds(stepDelay);
					foreach(Cell cell in queue.ToArray()) SetMarkerMaterial(GetMarker(cell),m_Processing);
					yield return new WaitForSeconds(stepDelay);  //Pause to prevent locking up resources; Minimum 2ms Delay
				}
				for(int steps = queue.Count;steps > 0;steps--) {
					Cell curCell = queue.Dequeue(); //Get Current Cell
					GameObject mark = GetMarker(curCell); //Get the Cell's Marker; Should always Exist
					SetMarkerMaterial(mark,m_Visited);
					foreach(CellEdge edge in curCell.Edges) { //Each Edge of the Cell
						if(edge is CellWall) continue; //Skip Walls
						if(!visited.ContainsKey(edge.otherCell)) { //Ignore already Visited Cells
							totalSeen++; //Increment Seen Counter
							visited.Add(edge.otherCell,new CellMetadata(curCell,distance + 1));
							if(edge.otherCell == Target) targetFound = true; //Found Target, DONE
							else {
								AddMarker(edge.otherCell,m_Seen); //Add Marker to Cell and Visualize that it has been Seen
								queue.Enqueue(edge.otherCell); //Push onto Stack
							} 
						}
					}
					totalSteps++;
					totalSeen = queue.Count;
				}
				distance++;
				pathSize++;
			}
			if(visited.ContainsKey(Target)) {
				for(CellMetadata curMeta = visited[Target];curMeta.previous != null;curMeta = visited[curMeta.previous]) {
					if(useDelay) yield return new WaitForSeconds(stepDelay);  //Pause to prevent locking up resources; Minimum 2ms Delay
					GameObject mark = GetMarker(curMeta.previous);
					SetMarkerMaterial(mark,m_Path);
					totalSteps++;
				}
			}
		}
		isSearching = false;
	}
}