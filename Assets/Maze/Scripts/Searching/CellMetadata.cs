public struct CellMetadata {
	public Cell previous;
	public int distance;
	public CellMetadata(Cell previousCell,int distance) {
		this.previous = previousCell;
		this.distance = distance;
	}
}
