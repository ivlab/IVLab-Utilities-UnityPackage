// Author: Volcano Kim <kimx3133@umn.edu>

using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    public static class LHtoRHConversion
    {
        public static List<Vector3> ConvertLeftHandToRightHand(Vector3 position, Quaternion rotation)
        {
            // something is slightly wrong with this part
            List<Vector3> converted = new List<Vector3>(); // empty now
            // http://www.geometrictools.com/Documentation/LeftHandedToRightHanded.pdf

            Matrix4x4 QuatToMat3x3 = Matrix4x4.identity;
            //trackerToDevice.translation[2] = -trackerToDevice.translation[2];
            QuatToMat3x3[0, 3] = position.x;
            QuatToMat3x3[1, 3] = position.y;
            QuatToMat3x3[2, 3] = -position.z;

            // Convert quaternion -> 3x3 rotation matrix
            // see: http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/
            QuatToMat3x3[0, 0] = 1f - 2 * rotation.y * rotation.y - 2 * rotation.z * rotation.z;
            QuatToMat3x3[0, 1] = 2 * rotation.x * rotation.y - 2 * rotation.z * rotation.w;
            QuatToMat3x3[0, 2] = 2 * rotation.x * rotation.z + 2 * rotation.y * rotation.w;
            QuatToMat3x3[1, 0] = 2 * rotation.x * rotation.y + 2 * rotation.z * rotation.w;
            QuatToMat3x3[1, 1] = 1f - 2 * rotation.x * rotation.x - 2 * rotation.z * rotation.z;
            QuatToMat3x3[1, 2] = 2 * rotation.y * rotation.z - 2 * rotation.x * rotation.w;
            QuatToMat3x3[2, 0] = 2 * rotation.x * rotation.z - 2 * rotation.y * rotation.w;
            QuatToMat3x3[2, 1] = 2 * rotation.y * rotation.z + 2 * rotation.x * rotation.w;
            QuatToMat3x3[2, 2] = 1f - 2 * rotation.x * rotation.x - 2 * rotation.y * rotation.y;

            // Convert Left to Right hand
            Matrix4x4 ConvertLeftToRight = QuatToMat3x3; // copy
            ConvertLeftToRight[0, 2] = -QuatToMat3x3[0, 2];
            ConvertLeftToRight[1, 2] = -QuatToMat3x3[1, 2];
            ConvertLeftToRight[2, 0] = -QuatToMat3x3[2, 0];
            ConvertLeftToRight[2, 1] = -QuatToMat3x3[2, 1];

            // create 90 degrees about x axis rotation matrix
            // 90 degrees = - pi / 2
            // http://inside.mines.edu/fs_home/gmurray/ArbitraryAxisRotation/
            Matrix4x4 nintyXRotMat = Matrix4x4.identity;
            nintyXRotMat[1, 1] = Mathf.Cos(Mathf.PI / 2);
            nintyXRotMat[1, 2] = -Mathf.Sin(Mathf.PI / 2);
            nintyXRotMat[2, 1] = Mathf.Sin(Mathf.PI / 2);
            nintyXRotMat[2, 2] = Mathf.Cos(Mathf.PI / 2);

            // rotate 90 degrees about x
            ConvertLeftToRight = nintyXRotMat * ConvertLeftToRight;

            // save position
            converted.Add(new Vector3(ConvertLeftToRight[0, 3], ConvertLeftToRight[1, 3], ConvertLeftToRight[2, 3]));

            // convert Mat3x3 to eulerAngles
            // See: http://nghiaho.com/?page_id=846
            Vector3 rotMatToEuler = Vector3.zero;
            rotMatToEuler.x = Mathf.Atan2(ConvertLeftToRight[2, 1], ConvertLeftToRight[2, 2]);
            rotMatToEuler.y = Mathf.Atan2(-ConvertLeftToRight[2, 0], Mathf.Sqrt(ConvertLeftToRight[2, 1] * ConvertLeftToRight[2, 1] + ConvertLeftToRight[2, 2] * ConvertLeftToRight[2, 2]));
            rotMatToEuler.z = Mathf.Atan2(ConvertLeftToRight[1, 0], ConvertLeftToRight[0, 0]);

            // radian to degree
            rotMatToEuler.x = rotMatToEuler.x * Mathf.Rad2Deg - 90;
            rotMatToEuler.y = -rotMatToEuler.y * Mathf.Rad2Deg;
            rotMatToEuler.z = rotMatToEuler.z * Mathf.Rad2Deg + 180;

            //save
            converted.Add(rotMatToEuler);

            //Vector3 converted = XYZtoZXY(rotation.eulerAngles);
            return converted;
        }

        public static Matrix4x4 getRHTrans(GameObject gb)
        {
            Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, gb.transform.localRotation, Vector3.one);
            return m;
        }

        public static Matrix4x4 UnityToJointTrack(GameObject gb)
        {
            List<Vector3> converted = new List<Vector3>(); // empty now

            // Step 1: Create a 4x4 matrix from the bone's pos and rotation
            Matrix4x4 m = Matrix4x4.TRS(gb.transform.localPosition, gb.transform.localRotation, Vector3.one);


            // Step 2: Convert this 4x4 matrix from left-handed coordinates to right-handed
            // This code is based on the article "Conversion of Left-Handed
            // Coordinates to Right-Handed Coordinates" by David Eberly,
            // available online:
            // http://www.geometrictools.com/Documentation/LeftHandedToRightHanded.pdf

            m[2, 3] = -m[2, 3];

            m[0, 2] = -m[0, 2];
            m[1, 2] = -m[1, 2];
            m[2, 0] = -m[2, 0];
            m[2, 1] = -m[2, 1];

            return m;
        }

        public static List<Vector3> ConvertLeftToRighGraphically(Vector3 position, Quaternion rotation)
        {
            List<Vector3> converted = new List<Vector3>(); // empty now
            converted.Add(new Vector3(position.x, position.z, position.y)); // swap y and z

            Vector3 currentEulers = rotation.eulerAngles;
            currentEulers = new Vector3(-currentEulers.x, -currentEulers.z, -currentEulers.y); // {x,y,z} -> {-x,-z,-y}
            converted.Add(currentEulers);
            return converted;
        }

        public static Vector3 XYZtoZXY(Vector3 eularAnglesIn)
        {
            // euler angles -> euler angles
            Quaternion rotation;
            rotation = Quaternion.AngleAxis(eularAnglesIn.x, Vector3.right);
            rotation = Quaternion.AngleAxis(eularAnglesIn.y, Vector3.up);
            rotation = Quaternion.AngleAxis(eularAnglesIn.z, Vector3.forward);
            rotation.w = rotation.z * rotation.x * rotation.y;
            Vector3 eularAnglesOut = rotation.eulerAngles;

            return eularAnglesOut;
        }
    }
}