using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DepthFirstSearch:MonoBehaviour {
	#region Inspector Set Variables (Variables that are set in the Unity Editor)
	public GameObject marker;
	public Material
		m_Processing,
		m_Seen,
		m_Collision,
		m_Visited,
		m_Path;
	public bool useDelay = true;
	[Range(0.02f,1f)]
	[Tooltip("Timed Delay between steps.")] //Limit to inclusive range of 0.02 to 2.0
	public float stepDelay; //Time before another step can occur.
	#endregion

	[HideInInspector]
	public Cell Origin { get; set; } //Search Origin
	[HideInInspector]
	public Cell Target { get; set; } //Search Target
	private Maze mazeInst;

	internal IEnumerator Solve() {
		mazeInst = gameObject.GetComponent<Maze>(); //The Maze Instance
		if(mazeInst.IsGenerated) { //Only attempt if the Maze is Generated
			int
				sizeX = mazeInst.size.x,
				sizeZ = mazeInst.size.z;
			HashSet<Cell> visited = new HashSet<Cell>();
			Stack<Cell> stack = new Stack<Cell>(sizeX + sizeZ);
			stack.Push(Origin);

			Instantiate(marker,Origin.transform).GetComponent<MeshRenderer>().material = m_Processing; //Create Origin Marker
			Instantiate(marker,Target.transform).GetComponent<MeshRenderer>().material = m_Path; //Create Target Marker
			bool targetFound = false;
			while(stack.Count > 0 && !targetFound) {
				Cell curCell = stack.Peek(); //Get Current Cell
				GameObject mark = GetMarker(curCell); //Get the Cell's Marker; Should always Exist
				mark.GetComponent<MeshRenderer>().material = m_Processing; //Visualize that it is being Processed
				if(useDelay) yield return new WaitForSeconds(stepDelay);  //Pause to prevent locking up resources; Minimum 2ms Delay
				if(visited.Contains(curCell)) { //Dead End or Already Visited; Backtrack
					mark.GetComponent<MeshRenderer>().material = m_Visited; //Visualize that it has been Visited
					mark.transform.localScale = new Vector3(0.25f,0.25f,1);
					stack.Pop(); //Remove from Stack
					continue; //Continue to next iteration
				} else visited.Add(curCell); //Add to Visited
				foreach(CellEdge edge in curCell.Edges) { //Each Edge of the Cell
					if(edge is CellWall) continue; //Skip Walls
					else if(!visited.Contains(edge.otherCell)) { //Ignore already Visited Cells
						if(edge.otherCell == Target) targetFound = true; //Found Target, DONE
						else {
							GameObject otherMark = GetMarker(edge.otherCell);
							if(otherMark != null) otherMark.GetComponent<MeshRenderer>().material = m_Collision; //Ran into another Marker; Loop
							else {
								Instantiate(marker,edge.otherCell.transform).GetComponent<MeshRenderer>().material = m_Seen; //Visualize that it has been Seen
								stack.Push(edge.otherCell); //Push onto Stack
							}
						}
					}
				}
				mark.GetComponent<MeshRenderer>().material = m_Path; //Visualize current marker as the Path
			}
		}
	}
	private GameObject GetMarker(Cell curCell) {
		for(int i = 0;i < curCell.transform.childCount;i++) {
			GameObject obj = curCell.transform.GetChild(i).gameObject;
			if(obj.CompareTag("Marker")) return obj;
		}
		return null;
	}
}
