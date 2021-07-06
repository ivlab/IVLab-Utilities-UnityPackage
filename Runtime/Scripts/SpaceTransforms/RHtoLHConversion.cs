// Author: Volcano Kim <kimx3133@umn.edu>

using UnityEngine;

namespace IVLab.Utilities
{
    public static class RHtoLHConversion 
    {
        public static Vector3 RHzUpVector3ToLHyUpVector3(Vector3 rhZupVector)
        {
            // This code converts a Vector3 in the Right Hand z Up coordinate system to
            // the Left Hand y Up coordinate system
            // example: (v 1 3 0.5) -> (v -1 0.5 -3)
            // negate X
            Vector3 LHConversion_Step1 = negateX(rhZupVector);

            // rotate 90 X axis 
            Matrix4x4 rotationMat = Matrix4x4.identity;
            rotationMat[0, 0] = 1.0f;
            rotationMat[1, 2] = -1.0f; // -sin(90)
            rotationMat[2, 1] = 1.0f; // sin(90)

            Vector3 LHConverted = rotationMat.MultiplyVector(LHConversion_Step1);

            return LHConverted;
        }

        // This is easy: negate x 
        public static Vector3 negateX(Vector3 rhVector)
        {
            return new Vector3(-rhVector.x, rhVector.y, rhVector.z);
        }
    
        public static Quaternion ToLeftHanded(Quaternion rhQuat)
        {
            // reference: https://gamedev.stackexchange.com/questions/129204/switch-axes-and-handedness-of-a-quaternion

            // first extract the axis (imaginary part) of the quaternion and convert
            // it to left handed as we would any other vector.
            Vector3 imaginaryPart = new Vector3(rhQuat.x, rhQuat.y, rhQuat.z);
            Vector3 imaginaryPartLH = RHtoLHConversion.RHzUpVector3ToLHyUpVector3(imaginaryPart);

            // in the new coordinate system the angle of rotation will take the opposite
            // sign.  This leaves the real part (w) unchanged since cos(theta) = cos(-theta)
            // and negates the imaginary part, since sin(theta) = -sin(-theta)
            return new Quaternion(-imaginaryPartLH.x, -imaginaryPartLH.y, -imaginaryPartLH.z, rhQuat.w);
        }
    }
}