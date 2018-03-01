using System;
using Microsoft.Kinect;

namespace KinectSkeletonTracking
{
    public class CalcRotate
    {
        const double PI = 3.1415926535897;
         //XY XZ YZ XYZ 面の角道計算
        public static double CalcXY(Joint cen, Joint first, Joint second)
        {
            double maX = first.Position.X - cen.Position.X;
            double maY = first.Position.Y - cen.Position.Y;
            double mbX = second.Position.X - cen.Position.X;
            double mbY = second.Position.Y - cen.Position.Y;
            double v1 = (maX * mbX) + (maY * mbY);
            double maVal = Math.Sqrt(maX * maX + maY * maY);
            double mbVal = Math.Sqrt(mbX * mbX + mbY * mbY);
            double cosM = v1 / (maVal * mbVal);
            double angleAMB = Math.Acos(cosM) * 180 / PI;

            return angleAMB;
        }

        public static double CalcXz(Joint cen, Joint first, Joint second)
        {
            double maZ = first.Position.Z - cen.Position.Z;
            double maX = first.Position.X - cen.Position.X;
            double mbZ = second.Position.Z - cen.Position.Z;
            double mbX = second.Position.X - cen.Position.X;
            double v1 = (maZ * mbZ) + (maX * mbX);
            double maVal = Math.Sqrt(maZ * maZ + maX * maX);
            double mbVal = Math.Sqrt(mbZ * mbZ + mbX * mbX);
            double cosM = v1 / (maVal * mbVal);
            double angleAMB = Math.Acos(cosM) * 180 / PI;

            return angleAMB;
        }

        public static double CalcYZ(Joint cen, Joint first, Joint second)
        {
            double maZ = first.Position.Z - cen.Position.Z;
            double maY = first.Position.Y - cen.Position.Y;
            double mbZ = second.Position.Z - cen.Position.Z;
            double mbY = second.Position.Y - cen.Position.Y;
            double v1 = (maZ * mbZ) + (maY * mbY);
            double maVal = Math.Sqrt(maZ * maZ + maY * maY);
            double mbVal = Math.Sqrt(mbZ * mbZ + mbY * mbY);
            double cosM = v1 / (maVal * mbVal);
            double angleAMB = Math.Acos(cosM) * 180 / PI;

            return angleAMB;
        }

        public static double CalcXYZ(Joint cen, Joint first, Joint second)
        {
            double maX = first.Position.X - cen.Position.X;
            double maY = first.Position.Y - cen.Position.Y;
            double mbX = second.Position.X - cen.Position.X;
            double mbY = second.Position.Y - cen.Position.Y;
            double maZ = first.Position.Z - cen.Position.Z;
            double mbZ = second.Position.Z - cen.Position.Z;
            double v1 = (maX * mbX) + (maY * mbY) + (maZ * mbZ);
            double maVal = Math.Sqrt(maX * maX + maY * maY + maZ * maZ);
            double mbVal = Math.Sqrt(mbX * mbX + mbY * mbY + mbZ * mbZ);
            double cosM = v1 / (maVal * mbVal);
            double angleAMB = Math.Acos(cosM) * 180 / PI;

            return angleAMB;
        }
        
        /// <summary>
        /// Rotates the specified quaternion around the X axis.
        /// </summary>
        /// <param name="quaternion">The orientation quaternion.</param>
        /// <returns>The rotation in degrees.</returns>
        public static double Pitch(Vector4 quaternion)
        {
            double value1 = 2.0 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            double value2 = 1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);

            double roll = Math.Atan2(value1, value2);

            return roll * (180.0 / Math.PI);
        }

        /// <summary>
        /// Rotates the specified quaternion around the Y axis.
        /// </summary>
        /// <param name="quaternion">The orientation quaternion.</param>
        /// <returns>The rotation in degrees.</returns>
        public static double Yaw(Vector4 quaternion)
        {
            double value = 2.0 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            value = value > 1.0 ? 1.0 : value;
            value = value < -1.0 ? -1.0 : value;

            double pitch = Math.Asin(value);

            return pitch * (180.0 / Math.PI);
        }

        /// <summary>
        /// Rotates the specified quaternion around the Z axis.
        /// </summary>
        /// <param name="quaternion">The orientation quaternion.</param>
        /// <returns>The rotation in degrees.</returns>
        public static double Roll(Vector4 quaternion)
        {
            double value1 = 2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double value2 = 1.0 - 2.0 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);

            double yaw = Math.Atan2(value1, value2);

            return yaw * (180.0 / Math.PI);

            //nasudayo
        }
    }
}