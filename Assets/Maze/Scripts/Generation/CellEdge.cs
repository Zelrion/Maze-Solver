using UnityEngine;
public abstract class CellEdge:MonoBehaviour { //Not to be used Directly, Inherited
	public Cell cell, otherCell; //Cell the Edge belongs to, and Cell the Edge provides connection with
	public Direction direction; //Direction of Edge in relation to it's Cell
	public void Initialize(Cell cell,Cell otherCell,Direction direction) {
		this.cell = cell; this.otherCell = otherCell; this.direction = direction;
		cell.SetEdge(direction,this); //Set the Edge in given Direction to the Object that Inherited this Class
		transform.parent = cell.transform; //Make the Edge a child of the Cell
		transform.localPosition = Vector3.zero; //Zero out it's Position, Centered on the Cell(Parent)
		transform.localRotation = direction.ToRotation(); //Rotate to direction it Belongs
	}
}
