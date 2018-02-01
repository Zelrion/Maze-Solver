[System.Serializable] //Indicates a Class that cannot be Inherited and can be Serialized; Can be used in Editor
public struct IntVector2 { //Simplified Vector, Useful in Grid based mapping
	private static IntVector2 identity = new IntVector2(1,1);
	public int x, z;
	public IntVector2(int x,int z) { this.x = x; this.z = z; }
	public static IntVector2 Identity { get { return identity; } }
	public static IntVector2 operator +(IntVector2 v1,IntVector2 v2) { v1.x += v2.x; v1.z += v2.z; return v1; } //Basic Operator Logic Specific to IntVector2
	public static IntVector2 operator -(IntVector2 v1,IntVector2 v2) { v1.x -= v2.x; v1.z -= v2.z; return v1; }
	public static IntVector2 operator *(IntVector2 v1,IntVector2 v2) { v1.x *= v2.x; v1.z *= v2.z; return v1; }
	public static IntVector2 operator /(IntVector2 v1,IntVector2 v2) { v1.x /= v2.x; v1.z /= v2.z; return v1; }
}
