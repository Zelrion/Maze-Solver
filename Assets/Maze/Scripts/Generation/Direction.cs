using UnityEngine;
public enum Direction { North, East, South, West } //Direction Enumerations for Cell Navigation
public static class Directions { //Order Corresponds with the Enum Direction's Order
	public const int Count = 4; //Number of Directions
	private static IntVector2[] vectors = { new IntVector2(0,1),new IntVector2(1,0),new IntVector2(0,-1),new IntVector2(-1,0) }; //Directional Vectors
	private static Direction[] opposites = { Direction.South,Direction.West,Direction.North,Direction.East }; //Opposing Directions
	private static Quaternion[] rotations = { Quaternion.identity,Quaternion.Euler(0f,90f,0f),Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f) }; //Rotations
	public static IntVector2 ToIntVector2(this Direction direction) { return vectors[(int)direction]; } //Return Corresponding Unit Vector
	public static Quaternion ToRotation(this Direction direction) { return rotations[(int)direction]; } //Return Corresponding Rotation
	public static Direction GetOpposite(this Direction direction) { return opposites[(int)direction]; } //Return Opposite Direction
	public static Direction RandomValue { get { return (Direction)Random.Range(0,Count); } } //Return a Random Direction
}
