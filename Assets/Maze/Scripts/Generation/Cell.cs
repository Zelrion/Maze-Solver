using UnityEngine;
public class Cell:MonoBehaviour {
	public IntVector2 coordinates; //Used primarily in Identification and Locating the Cell
	private int
		passageEdgeCount,
		wallEdgeCount,
		initializedEdgeCount; //Used in property to check if Fully Initialized
	private CellEdge[] edges = new CellEdge[Directions.Count]; //Create Array with initial capacity based on total Directions; 4 Default
	public CellEdge GetEdge(Direction direction) { return edges[(int)direction]; } //Return the Edge in given Direction
	public void SetEdge(Direction direction,CellEdge edge) { //Set Edge and Increment
		edges[(int)direction] = edge;
		initializedEdgeCount++;
		if(edge is CellPassage) passageEdgeCount++;
		else wallEdgeCount++;
	}
	public Direction RandomUninitializedDirection { //Return a random Direction(Edge) that has yet to be Initialized(Created)
		get {
			for(int i = 0, s = Random.Range(0,Directions.Count - initializedEdgeCount); //Iterate through Edge array, and return when S is 0; Dice Roll
				i < Directions.Count;i++) {
				if(edges[i] == null) { //If edge doesn't Exist
					if(s == 0) return (Direction)i; //If s is equal to 0, Return the Direction based on i
					else s--; //Else decrement s
				}
			}
			throw new System.InvalidOperationException("Error: " + name + ": has no unitialized directions left."); //If thrown check logic in this method or caller
		}
	}
	public CellEdge[] Edges { get { return edges; } }
	public int PassageCount { get { return passageEdgeCount; } }
	public int WallCount { get { return wallEdgeCount; } }
	public bool IsFullyInitialized { get { return initializedEdgeCount == Directions.Count; } } //Return true if an Edge exists for each Direction
}
