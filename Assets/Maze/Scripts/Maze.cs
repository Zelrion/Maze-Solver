using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Maze:MonoBehaviour {
	private const int MAX_TRIS = 65000; //Max Number of Triangles in a Mesh

	#region Inspector Set Variables (Variables that are set in the Unity Editor)
	public IntVector2 size; //(X,Z) Size of the Maze, measured in Cells
	public bool perfectMaze; //Require the result be Perfect; One Solution
	public Material //Material to use for the Maze
		wallMaterial,
		floorMaterial;
	public Mesh floorMesh; //Mesh to use for the Floor
	[Range(60,70)]
	[Tooltip("How many Cells will be processed before the Delay is imposed.\n(Exponential Growth (N^N); 70 = 4,900 Cells)")] //4,900*TrianglesPerMesh(12) = 58,800/65,000 Max TrianglesPerMesh
	public int cellProcessRate; //Exponential Growth (N^N)
	[Range(0.02f,1f)]
	[Tooltip("Timed Delay between cycles.")] //Limit to inclusive range of 0.02 to 2.0
	public float cycleDelay; //Time before another Set of Generations can Occur
	public Cell cellPrefab; //Premade Object to use as the Cell
	public CellPassage passagePrefab; //Premade Object to use as the Passage
	public CellWall wallPrefab; //Premade Object to use as the Wall
	#endregion

	#region MazeData (Variables and Properties that are used with Maze Generation)
	public bool IsGenerated { get { return isGenerated; } }
	public IntVector2 RandomCoordinates { get { return new IntVector2(Random.Range(0,size.x),Random.Range(0,size.z)); } } //Random coordinate within Bounds
	private Cell[,] cells; //2D array to store the Cells
	private GameObject //Container Objects for Hierarchy
		cellMaster,
		meshMaster;
	private bool isGenerated = false;
	#endregion

	public IEnumerator Generate() {
		cells = new Cell[size.x,size.z]; //Initialize with Size as Capacity
		cellMaster = new GameObject("Cells"); //Create Cell Container
		cellMaster.transform.parent = transform; //Make a Child of Maze
		meshMaster = new GameObject("Meshes"); //Create Mesh Container
		meshMaster.transform.parent = transform; //Make a Child of Maze
		List<Cell> activeCells = new List<Cell>() { CreateCell(RandomCoordinates) }; //Create Initial Cell in new Active Cell List
		CreateFloor(); //Generate a floor for the Maze based on Size
		while(activeCells.Count > 0) { //While there are active cells
			for(int cycles = 0;cycles < (cellProcessRate * cellProcessRate) && activeCells.Count > 0;cycles++) NextCycle(activeCells); //Execute NextCycle based on cellProcessRate
			yield return new WaitForSeconds(cycleDelay); //Pause to prevent locking up resources; Minimum 2ms Delay
			MergeMeshes(); //Create a Single Mesh from the Objects Generate this Iteration
		}
		isGenerated = true;
	}
	public bool ContainsCoordinates(IntVector2 coords) { return (coords.x >= 0 && coords.x < size.x && coords.z >= 0 && coords.z < size.z); } //Check if inside Bounds
	public Cell GetCell(IntVector2 coords) { return cells[coords.x,coords.z]; } //Return Cell based on Coordinates, 0:0 - X:Z
	public Cell GetCell(int x,int z) { return cells[x,z]; }
	private Cell CreateCell(IntVector2 coordinates) { //Instantiate a new Cell at Coordinates
		Cell newCell = Instantiate(cellPrefab) as Cell; //Instantiate the Prefab as a Cell Object
		cells[coordinates.x,coordinates.z] = newCell; //Location in Array matches Location in World
		newCell.coordinates = coordinates; //Set the Cell Object's Coordinates (Variable used primarily for Identification)
		newCell.name = "[" + coordinates.x + "," + coordinates.z + "]"; //Q.O.L. Easier to read in Hierarchy in Editor
		newCell.transform.parent = cellMaster.transform; //Make the Cell a child of the Cell Master
		newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f,0f,coordinates.z - size.z * 0.5f + 0.5f); //Move Cell to location
		return newCell; //Return to caller with the newly created Cell
	}
	private void NextCycle(List<Cell> activeCells) { //Generate a new Cell, or Initialize an Existing Cell
		int curInd = activeCells.Count - 1; //Start at the End of the List
		Cell curCell = activeCells[curInd]; //Identify current Cell
		if(curCell.IsFullyInitialized) { activeCells.RemoveAt(curInd); return; } //Cell is already Fully Initialized, Remove from Active Cell List
		Direction direction = curCell.RandomUninitializedDirection; //Choose a Random Direction without an Edge
		IntVector2 coordinates = curCell.coordinates + direction.ToIntVector2(); //Add direction to the Current Cell's Location to find Neighbor
		if(ContainsCoordinates(coordinates)) { //If within Size Boundaries
			Cell neighbor = GetCell(coordinates); //Find Neighbor
			if(neighbor == null) { //If Neighbor Doesn't Exist, Make One
				neighbor = CreateCell(coordinates); //Create a blank Cell at location
				CreatePassage(curCell,neighbor,direction); //Set Edge between the two cells as Passage
				activeCells.Add(neighbor); //Add the Neighbor to Active List for use in same Process
			} else { //Neighbor Exists
				if(!perfectMaze && curCell.WallCount == 2) CreatePassage(curCell,neighbor,direction); //If not Perfect and Current Cell has 2 Walls; Create Passage
				else CreateWall(curCell,neighbor,direction); //Create Wall
			}
		} else { CreateWall(curCell,null,direction); } //Else, Current Cell is a Boundary, Create wall in that Direction
	}
	private void CreatePassage(Cell cell,Cell otherCell,Direction direction) { //Generate an Open Passage between two given cells
		CellPassage passage = Instantiate(passagePrefab) as CellPassage; //Instantiate the Prefab as a CellPassage Object
		passage.Initialize(cell,otherCell,direction); //Call for the Initialization of the Edge on current Cell
		passage = Instantiate(passagePrefab) as CellPassage; //Instantiate the Prefab as a CellPassage Object
		passage.Initialize(otherCell,cell,direction.GetOpposite()); //Call for the Initialization of the Opposite Edge on other Cell (The Edge Between)
	}
	private void CreateWall(Cell cell,Cell otherCell,Direction direction) { //Generate a Wall Edge 
		CellWall wall = Instantiate(wallPrefab) as CellWall; //Instantiate the Prefab as a CellWall Object
		wall.Initialize(cell,otherCell,direction); //Call for the Initialization of the Edge on current Cell 
		if(otherCell != null) { //If a connection exists between the Cell and another, Initialize that Cell's corresponding edge as the same Wall
			wall = Instantiate(wallPrefab) as CellWall; //Instantiate the Prefab as a CellWall Object
			wall.gameObject.SetActive(false);
			wall.Initialize(otherCell,cell,direction.GetOpposite()); //Call for the Initialization of the Opposite Edge on other Cell (The Edge Between)
		}
	}
	private void CreateFloor() {
		GameObject floor = new GameObject("Floor"); //Create the Object to be Made and Name it Floor
		floor.AddComponent<BoxCollider>(); //Allows Floor to Collide with RayCasts from Camera
		floor.AddComponent<MeshFilter>().mesh = floorMesh; //Add the MeshFilter and Set Mesh (Shape)
		floor.AddComponent<MeshRenderer>().material = floorMaterial; //Add the MeshRenderer and Set Material (Color/Texture/Shader/Etc)
		floor.transform.parent = transform; //Make the Object a child of the Maze
		floor.transform.localScale = new Vector3(size.x,size.z,1); //Set scale to the Size of the Maze
		floor.transform.localEulerAngles = new Vector3(90,0,0); //Rotate X by 90 to account for the Mesh being Upright (Quad)
	}
	private GameObject CreateMeshObject(string name,Mesh mesh) {
		GameObject meshObj = new GameObject(name); //Create the Object to be Made
		meshObj.AddComponent<MeshFilter>().mesh = mesh; //Add the MeshFilter and Set Mesh (Shape)
		meshObj.AddComponent<MeshRenderer>().material = wallMaterial; //Add the MeshRenderer and Set Material (Color/Texture/Shader/Etc)
		meshObj.transform.parent = meshMaster.transform; //Make the Object a child of the Mesh Master
		return meshObj;
	}
	private void MergeMeshes() {
		MeshFilter[] meshFilters = cellMaster.GetComponentsInChildren<MeshFilter>(); //Every MeshFilter in CellMaster
		List<Mesh> meshes = new List<Mesh> { new Mesh() }; //Initialize with first Mesh 
		int filterIndex = 0; //Current Index in MeshFilter Array
		for(int mesh = 0, trisCount = 0;mesh < meshes.Count;trisCount = 0, mesh++) { //For every Mesh (Meshes can be added if current exceeds 65k Tris, causing an extra iteration)
			CombineInstance[] combine = new CombineInstance[meshFilters.Length - filterIndex]; //Total combinations are the remaining MeshFilters that have yet to be processed
			for(int i = 0;filterIndex < meshFilters.Length;i++) { //Iterate the MeshFilters from Cells
				MeshFilter curMeshFilter = meshFilters[filterIndex++]; //Set Current MeshFilter
				if(trisCount + curMeshFilter.mesh.triangles.Length < MAX_TRIS) { //Check if the Sum of the current count of Triangles and the Next Mesh is less than the Max Number of Triangles
					curMeshFilter.gameObject.SetActive(false); //Set the Cell to InActive so it won't be found in the next call for children (Cell Master Children)
					combine[i].mesh = curMeshFilter.sharedMesh; //Set the Combine Instance Mesh to the Current MeshFilter's (MF)
					combine[i].transform = curMeshFilter.transform.localToWorldMatrix; //Transform from local to world space
					trisCount += curMeshFilter.mesh.triangles.Length; //Add the Triangle count
				} else { //If sum exceeded 65k Triangles
					meshes.Add(new Mesh()); //Add another Mesh to force another Iteration
					break; //Break current loop
				}
			}
			meshes[mesh].CombineMeshes(combine,true); //Combine the CombineInstance[] into the Mesh
			meshes[mesh].UploadMeshData(true); //Upload Mesh to GPU and tell the Engine it is Final
			CreateMeshObject("Tris:" + trisCount,meshes[mesh]); //Create Mesh Object with Name
		}
	}
}
