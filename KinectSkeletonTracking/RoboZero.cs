using System;
using Microsoft.Kinect;
using static KinectSkeletonTracking.CalcRotate;
using static Microsoft.Kinect.JointType;
using System.Diagnostics;

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
        //前進の時、送信するデータ
        private byte[] RzGo = { 1, 0, 0, 0 };
        //後退の時、送信するデータ
        private byte[] RzBack = { 2, 0, 0, 0 };
        //しゃがむの時、送信するデータ
        private byte[] RzSquat = { 3, 0, 0, 0 };
        //立つの時、送信するデータ
        private byte[] RzStand = { 4, 0, 0, 0 };
        private float SpainBaseZ = 0, SpainBaseY = 0;
        private Connection server;

        public RoboZero(Connection server)
        {
            this.server = server;
        }

        //ロボゼロのbyte型送信
        public void robozeroByte(Body body)
        {
            // --------------------------------- 下半身 ---------------------------------

            //前後
            //TODO :この条件判定どうなっているの？要調査。
            //Z軸の座標によって前進後退判断する
            if (SpainBaseZ == 0 || SpainBaseZ - body.Joints[SpineBase].Position.Z > 0.5 ||
                SpainBaseZ - body.Joints[SpineBase].Position.Z < -0.5)
            {
                //差がプラスならば、前進信号を送信
                if(SpainBaseZ - body.Joints[SpineBase].Position.Z > 0.5)
                {
                    //server.sendBytes(RzGo);
                }
                //差がマイナスならば、後退信号
                else if(SpainBaseZ - body.Joints[SpineBase].Position.Z < -0.5)
                {
                    //server.sendBytes(RzBack);
                }
                SpainBaseZ = body.Joints[SpineBase].Position.Z;
                
            }

            //しゃがむ　立つ
            //Y軸の座標の差によってしゃがむ立つ判断
            if (SpainBaseY == 0 || SpainBaseY - body.Joints[SpineBase].Position.Y > 0.5 ||
                SpainBaseY - body.Joints[SpineBase].Position.Y < -0.5)
            {
                //差がプラスならば、しゃがむ信号を送信
                if (SpainBaseY - body.Joints[SpineBase].Position.Y > 0.5)
                {
                   // server.sendBytes(RzSquat);
                }
                //差がマイナスならば、立つ信号
                else if (SpainBaseY - body.Joints[SpineBase].Position.Y < -0.5)
                {
                    //server.sendBytes(RzStand);
                }
                SpainBaseY = body.Joints[SpineBase].Position.Y;
            }

            // --------------------------------- 上半身 ---------------------------------
            if (SpainBaseZ - body.Joints[SpineBase].Position.Z < 0.5 &&
                SpainBaseZ - body.Joints[SpineBase].Position.Z > -0.5)
            {
                byte[] splitRotate;
                //角道計算
                var rotateShoulderRightR = (int)CalcXYZ(
                     body.Joints[ShoulderRight],
                     body.Joints[SpineMid],
                     body.Joints[ElbowRight]);
                rotateShoulderRightR -= 98;
                splitRotate = BitConverter.GetBytes((Int16)rotateShoulderRightR);
                RzShoulderRRoll[2] = splitRotate[0];
                RzShoulderRRoll[3] = splitRotate[1];
                if (rotateShoulderRightR > -200 && rotateShoulderRightR < 200)
                {
                    //server.sendBytes(RzShoulderRRoll);
                    //Debug.WriteLine("R1"+rotateShoulderRightR);
                }

                var rotateShoulderRightP = (int)CalcYZ(
                    body.Joints[ShoulderRight],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowRight]);
                rotateShoulderRightP -= 128;
                if (rotateShoulderRightP > 0) rotateShoulderRightP *= 2;
                splitRotate = BitConverter.GetBytes((Int16)rotateShoulderRightP);
                RzShoulderRPitch[2] = splitRotate[0];
                RzShoulderRPitch[3] = splitRotate[1];
                if (rotateShoulderRightP > -200 && rotateShoulderRightP < 200)
                {
                    //server.sendBytes(RzShoulderRPitch);
                    //Debug.WriteLine("R2" + rotateShoulderRightP);
                }
                var rotateElbowRight = (int)CalcXYZ(body.Joints[ElbowRight], body.Joints[HandRight],
                body.Joints[ShoulderRight]);
                rotateElbowRight -= 120;
                splitRotate = BitConverter.GetBytes((Int16)rotateElbowRight);
                RzElbowR[2] = splitRotate[0];
                RzElbowR[3] = splitRotate[1];
                if (rotateElbowRight > -200 && rotateElbowRight < 200)
                {
                    //server.sendBytes(RzElbowR);
                    Debug.WriteLine("R3" + rotateElbowRight);
                }
                var rotateShoulderLightP = (int)CalcYZ(body.Joints[ShoulderLeft],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowLeft]);
                rotateShoulderLightP -= 173;
                if (rotateShoulderLightP > 0) rotateShoulderLightP = rotateShoulderLightP * 2;
                splitRotate = BitConverter.GetBytes((Int16)rotateShoulderLightP);
                RzShoulderLPitch[2] = splitRotate[0];
                RzShoulderLPitch[3] = splitRotate[1];
                if (rotateShoulderLightP > -200 && rotateShoulderLightP < 200)
                {
                    //server.sendBytes(RzShoulderLPitch);
                    //Debug.WriteLine("R4" + rotateShoulderLightP);
                }
                var rotateShoulderLightR = (int)CalcXYZ(body.Joints[ShoulderLeft],
                body.Joints[SpineShoulder],
                body.Joints[ElbowLeft]);
                rotateShoulderLightR -= 160;
                splitRotate = BitConverter.GetBytes((Int16)rotateShoulderLightR);
                RzShoulderLRoll[2] = splitRotate[0];
                RzShoulderLRoll[3] = splitRotate[1];
                if (rotateShoulderLightR > -200 && rotateShoulderLightR < 200)
                {
                    //server.sendBytes(RzShoulderLPitch);
                    //Debug.WriteLine("R5" + rotateShoulderLightR);
                }
                var rotateElbowLight = (int)CalcXYZ(body.Joints[ElbowLeft], body.Joints[HandLeft],
                body.Joints[ShoulderLeft]);
                rotateElbowLight -= 120;
                splitRotate = BitConverter.GetBytes((Int16)rotateElbowLight);
                RzElbowL[2] = splitRotate[0];
                RzElbowL[3] = splitRotate[1];
                if (rotateElbowLight > -200 && rotateElbowLight < 200)
                {
                    //server.sendBytes(RzElbowL);
                    // Debug.WriteLine("R6" + rotateElbowLight);
                }
                var rotateSpainMidP = (int)CalcXz(body.Joints[SpineMid], body.Joints[ShoulderLeft],
                body.Joints[ShoulderRight]);
                rotateSpainMidP -= 225;
                splitRotate = BitConverter.GetBytes((Int16)rotateSpainMidP);
                RzSpineMPitch[2] = splitRotate[0];
                RzSpineMPitch[3] = splitRotate[1];
                if (rotateSpainMidP > -200 && rotateSpainMidP < 200)
                {
                    //server.sendBytes(RzSpineMPitch);
                    // Debug.WriteLine("R7" + rotateSpainMidP);
                }
                var rotateSpainMidY = (int)CalcYZ(body.Joints[SpineMid], body.Joints[SpineBase],
                body.Joints[SpineShoulder]);
                rotateSpainMidY -= 178;
                if (body.Joints[ShoulderRight].Position.Z > body.Joints[ShoulderLeft].Position.Z)
                {
                    rotateSpainMidY = 0 - rotateSpainMidY;
                }
                splitRotate = BitConverter.GetBytes((Int16)rotateSpainMidY);
                RzSpineMYow[2] = splitRotate[0];
                RzSpineMYow[3] = splitRotate[1];
                if (rotateSpainMidY > -200 && rotateSpainMidY < 200)
                {
                    // server.sendBytes(RzSpineMYow);
                    //Debug.WriteLine("R8" + rotateSpainMidY);
                }
            }
        }
    }
}