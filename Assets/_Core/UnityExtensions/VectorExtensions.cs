using UnityEngine;

public static class VectorExtensions {

	public static Vector2 ToVector2WithoutZ(this Vector3 vector3) => new(vector3.x, vector3.y);

	public static Vector3 ToVector3WithZ(this Vector2 vector2, float z) => new(vector2.x, vector2.y, z);

	public static Vector3 WithZ(this Vector3 vector3, float z) => new(vector3.x, vector3.y, z);

}