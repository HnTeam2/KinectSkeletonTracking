using System;
using Microsoft.Kinect;
using static KinectSkeletonTracking.CalcRotate;
using static Microsoft.Kinect.JointType;

namespace KinectSkeletonTracking
{
    public class RoboZero : sendRobotics
    {
        private byte[] RzElbowR = {0, 1, 0, 0};
        private byte[] RzShoulderRRoll = {0, 3, 0, 0};
        private byte[] RzShoulderRPitch = {0, 4, 0, 0};
        private byte[] RzShoulderLPitch = {0, 5, 0, 0};
        private byte[] RzShoulderLRoll = {0, 6, 0, 0};
        private byte[] RzElbowL = {0, 8, 0, 0};
        private byte[] RzSpineMYow = {0, 10, 0, 0};
        private byte[] RzSpineMPitch = {0, 11, 0, 0};
        private byte[] RzFootR = {5, 0, 0, 0};
        private byte[] RzAnkleR = {5, 1, 0, 0};
        private byte[] RzKneeR = {5, 2, 0, 0};
        private byte[] RzHipRPitch = {5, 4, 0, 0};
        private byte[] RzHipRRoll = {5, 5, 0, 0};
        private byte[] RzHipLRoll = {5, 6, 0, 0};
        private byte[] RzHipLPitch = {5, 7, 0, 0};
        private byte[] RzKneeL = {5, 9, 0, 0};
        private byte[] RzAnkleLPitch = {5, 10, 0, 0};
        private byte[] RzFootL = {5, 11, 0, 0};

        private float SpainBaseZ = 0, SpainBaseY = 0;

        //ロボゼロのbyte型送信
        private void robozeroByte(Body body, Connection server)
        {
            // --------------------------------- 下半身 ---------------------------------

            //前後
            //TODO :この条件判定どうなっているの？要調査。
            if (SpainBaseZ == 0 || SpainBaseZ - body.Joints[SpineBase].Position.Z > 0.5 ||
                SpainBaseZ - body.Joints[SpineBase].Position.Z < -0.5)
            {
                SpainBaseZ = body.Joints[SpineBase].Position.Z;
            }

            //しゃがむ　立つ
            if (SpainBaseY == 0 || SpainBaseY - body.Joints[SpineBase].Position.Y > 0.5 ||
                SpainBaseY - body.Joints[SpineBase].Position.Y < -0.5)
            {
                SpainBaseY = body.Joints[SpineBase].Position.Y;
            }

            // --------------------------------- 上半身 ---------------------------------
            if (SpainBaseZ - body.Joints[SpineBase].Position.Z < 0.5 &&
                SpainBaseZ - body.Joints[SpineBase].Position.Z > -0.5)
            {
                byte[] splitRotate;
                //角道計算
                var rotateShoulderRightR = (int) CalcXYZ(
                    body.Joints[ShoulderRight],
                    body.Joints[SpineMid],
                    body.Joints[ElbowRight]);
                rotateShoulderRightR -= 98;
                splitRotate = BitConverter.GetBytes((Int16) rotateShoulderRightR);
                RzShoulderRRoll[2] = splitRotate[0];
                RzShoulderRRoll[3] = splitRotate[1];
                server.sendBytes(RzShoulderRRoll);

                
                var rotateShoulderRightP = (int) CalcYZ(
                    body.Joints[ShoulderRight],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowRight]);
                rotateShoulderRightP -= 128;
                if (rotateShoulderRightP > 0) rotateShoulderRightP *= 2;
                splitRotate = BitConverter.GetBytes((Int16) rotateShoulderRightP);
                RzShoulderRPitch[2] = splitRotate[0];
                RzShoulderRPitch[3] = splitRotate[1];
                server.sendBytes(RzShoulderRPitch);

                
                var rotateElbowRight = (int) CalcXYZ(body.Joints[ElbowRight], body.Joints[HandRight],
                    body.Joints[ShoulderRight]);
                rotateElbowRight -= 120;
                splitRotate = BitConverter.GetBytes((Int16) rotateElbowRight);
                RzElbowR[2] = splitRotate[0];
                RzElbowR[3] = splitRotate[1];
                server.sendBytes(RzElbowR);

                
                var rotateShoulderLightP = (int) CalcYZ(body.Joints[ShoulderLeft],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowLeft]);
                rotateShoulderLightP -= 173;
                if (rotateShoulderLightP > 0) rotateShoulderLightP = rotateShoulderLightP * 2;
                splitRotate = BitConverter.GetBytes((Int16) rotateShoulderLightP);
                RzShoulderLPitch[2] = splitRotate[0];
                RzShoulderLPitch[3] = splitRotate[1];
                server.sendBytes(RzShoulderLPitch);


                var rotateShoulderLightR = (int) CalcXYZ(body.Joints[ShoulderLeft],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowLeft]);
                rotateShoulderLightR -= 160;
                splitRotate = BitConverter.GetBytes((Int16) rotateShoulderLightR);
                RzShoulderLRoll[2] = splitRotate[0];
                RzShoulderLRoll[3] = splitRotate[1];
                server.sendBytes(RzShoulderLPitch);

                
                var rotateElbowLight = (int) CalcXYZ(body.Joints[ElbowLeft], body.Joints[HandLeft],
                    body.Joints[ShoulderLeft]);
                rotateElbowLight -= 120;
                splitRotate = BitConverter.GetBytes((Int16) rotateElbowLight);
                RzElbowL[2] = splitRotate[0];
                RzElbowL[3] = splitRotate[1];
                server.sendBytes(RzElbowL);

                
                var rotateSpainMidP = (int) CalcXz(body.Joints[SpineMid], body.Joints[ShoulderLeft],
                    body.Joints[ShoulderRight]);
                rotateSpainMidP -= 225;
                splitRotate = BitConverter.GetBytes((Int16) rotateSpainMidP);
                RzSpineMPitch[2] = splitRotate[0];
                RzSpineMPitch[3] = splitRotate[1];
                server.sendBytes(RzSpineMPitch);

                
                var rotateSpainMidY = (int) CalcYZ(body.Joints[SpineMid], body.Joints[SpineBase],
                    body.Joints[SpineShoulder]);
                rotateSpainMidY -= 169;
                if (body.Joints[ShoulderRight].Position.Z > body.Joints[ShoulderLeft].Position.Z)
                {
                    rotateSpainMidY = 0 - rotateSpainMidY;
                }
                splitRotate = BitConverter.GetBytes((Int16) rotateSpainMidY);
                RzSpineMYow[2] = splitRotate[0];
                RzSpineMYow[3] = splitRotate[1];
                server.sendBytes(RzSpineMYow);

            }
        }
    }
}