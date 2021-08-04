using UnityEngine;

namespace IVLab.Utilities
{
    /// <summary>
	/// Uses the CoordConversion class to provide routines for converting from XROMM coordinates
	/// to Unity coordinates.
	///
	/// XROMM coordinates are:
	///   Right-Handed
	///   Up = +Z
	///   Forward = +Y
	///   Right = +X
	///
	/// Unity coordinates are:
	///   Left-Handed
	///   Up = +Y
	///   Forward = +Z
	///   Right = +X
	/// </summary>
    public class XROMMCoordinates
    {
        static CoordConversion.CoordSystem xrommCS = new CoordConversion.CoordSystem(
            CoordConversion.CoordSystem.Handedness.RightHanded, 
            CoordConversion.CoordSystem.Axis.PosZ, 
            CoordConversion.CoordSystem.Axis.PosY
        );

        /// <summary>
		/// Converts a point or vector from XROMM coordinates to Unity.
		/// </summary>
		/// <param name="xrommVector">Point or vector in XROMM coordinates</param>
		/// <returns>Unity point or vector</returns>
        public static Vector3 ToUnity(Vector3 xrommVector)
        {
            return CoordConversion.ToUnity(xrommVector, xrommCS);
        }

        /// <summary>
		/// Converts a quaternion from XROMM coordinates to Unity.
		/// </summary>
		/// <param name="xrommQuat">Quaternion in XROMM coordinates</param>
		/// <returns>Unity quaternion</returns>
        public static Quaternion ToUnity(Quaternion xrommQuat)
        {
            return CoordConversion.ToUnity(xrommQuat, xrommCS);
        }

        /// <summary>
        /// Converts a 4x4 rigid body transformation matrix defined in XROMM coordinates to Unity.
        /// </summary>
        /// <param name="xrommMat">Rigid body transformation matrix in XROMM coordinates</param>
        /// <returns>Unity matrix</returns>
        public static Matrix4x4 ToUnity(Matrix4x4 xrommMat)
        {
            return CoordConversion.ToUnity(xrommMat, xrommCS);
        }



        /// <summary>
		/// Converts a point or vector in Unity coordinates to XROMM coordinates.
		/// </summary>
		/// <param name="unityVector">Point or vector in Unity coordinates</param>
		/// <returns>XROMM version of the point or vector</returns>
        public static Vector3 FromUnity(Vector3 unityVector)
        {
            return CoordConversion.FromUnity(unityVector, xrommCS);
        }

        /// <summary>
		/// Converts a quaternion in Unity coordinates to XROMM coordinates.
		/// </summary>
		/// <param name="unityQuat">Quaternion in Unity coordinates</param>
		/// <returns>XROMM version of the quaternion</returns>
        public static Quaternion FromUnity(Quaternion unityQuat)
        {
            return CoordConversion.FromUnity(unityQuat, xrommCS);
        }

        /// <summary>
        /// Converts a rigid body transformation matrix in Unity coordinates to XROMM coordinates.
        /// </summary>
        /// <param name="unityMat">Matrix in Unity coordinates</param>
        /// <returns>XROMM version of the matrix</returns>
        public static Matrix4x4 FromUnity(Matrix4x4 unityMat)
        {
            return CoordConversion.FromUnity(unityMat, xrommCS);
        }
    }
}
