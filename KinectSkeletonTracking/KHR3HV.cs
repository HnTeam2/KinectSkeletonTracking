using System;
using System.Runtime.Remoting.Channels;
using Microsoft.Kinect;
using static KinectSkeletonTracking.CalcRotate;
using static Microsoft.Kinect.JointType;
using System.Diagnostics;

namespace KinectSkeletonTracking
{
    public class KHR3HV
    {
        byte[] KHRElbowR = {0, 5, 0, 0};
        byte[] KHRShoulderRRoll = {0, 6, 0, 0};
        byte[] KHRShoulderRPitch = {0, 7, 0, 0};
        byte[] KHRShoulderLPitch = {0, 9, 0, 0};
        byte[] KHRShoulderLRoll = {0, 10, 0, 0};
        byte[] KHRElbowL = {0, 11, 0, 0};
        byte[] KHRSpineM = {0, 8, 0, 0};
        byte[] KHRFootR = {0, 0, 0, 0};
        byte[] KHRAnkleR = {0, 1, 0, 0};
        byte[] KHRKneeR = {0, 2, 0, 0};
        byte[] KHRHipRPitch = {0, 3, 0, 0};
        byte[] KHRHipRRoll = {0, 4, 0, 0};
        byte[] KHRHipLRoll = {0, 12, 0, 0};
        byte[] KHRHipLPitch = {0, 13, 0, 0};
        byte[] KHRKneeL = {0, 14, 0, 0};
        byte[] KHRAnkleLPitch = {0, 15, 0, 0};
        byte[] KHRFootL = {0, 16, 0, 0};
        //前進の時、送信するデータ
        byte[] KHRGo = { 1, 0, 0, 0 };
        //後退の時、送信するデータ
        byte[] KHRBack = { 2, 0, 0, 0 };
        //しゃがむの時、送信するデータ
        byte[] KHRSquat = { 3, 0, 0, 0 };
        //立つの時、送信するデータ
        byte[] KHRStand = { 4, 0, 0, 0 };
        private float SpainBaseZ = 0, SpainBaseY = 0;
        private Connection server;

        public KHR3HV(Connection server)
        {
            this.server = server;
        }

        //KHRのbyte型送信
        public void KHRByte(Body body)
        {
            // --------------------------------- 下半身 ---------------------------------

            //前後
            //TODO :この条件判定どうなっているの？要調査。
            //Z軸の座標によって前進後退判断する
            if (SpainBaseZ == 0 || SpainBaseZ - body.Joints[SpineBase].Position.Z > 0.5 ||
                SpainBaseZ - body.Joints[SpineBase].Position.Z < -0.5)
            {
                //差がプラスならば、前進信号を送信
                if (SpainBaseZ - body.Joints[SpineBase].Position.Z > 0.5)
                {
                    //server.sendBytes(KHRGo);
                }
                //差がマイナスならば、後退信号
                else if (SpainBaseZ - body.Joints[SpineBase].Position.Z < -0.5)
                {
                    //server.sendBytes(KHRBack);
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
                    //server.sendBytes(KHRSquat);
                }
                //差がマイナスならば、立つ信号
                else if (SpainBaseY - body.Joints[SpineBase].Position.Y < -0.5)
                {
                    //sendBytes(KHRStand);
                }
                SpainBaseY = body.Joints[SpineBase].Position.Y;
            }

            // --------------------------------- 上半身 ---------------------------------

            if (SpainBaseZ - body.Joints[SpineBase].Position.Z < 0.5 &&
                SpainBaseZ - body.Joints[SpineBase].Position.Z > -0.5)
            {
                byte[] SplitRotate;
                var rotateShoulderRightR = (int) CalcXYZ(body.Joints[ShoulderRight],
                    body.Joints[SpineMid],
                    body.Joints[ElbowRight]);
                rotateShoulderRightR -= 133;
                SplitRotate = BitConverter.GetBytes((Int16) rotateShoulderRightR);
                KHRShoulderRRoll[2] = SplitRotate[0];
                KHRShoulderRRoll[3] = SplitRotate[1];
                server.send("6:"+ rotateShoulderRightR.ToString());
                //Debug.WriteLine("K1" + rotateShoulderRightR);
                var rotateShoulderRightP = (int) CalcYZ(body.Joints[ShoulderRight],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowRight]);
                rotateShoulderRightP -= 128;
                if (rotateShoulderRightP > 0) rotateShoulderRightP *= 2;
                SplitRotate = BitConverter.GetBytes((Int16) rotateShoulderRightP);
                KHRShoulderRPitch[2] = SplitRotate[0];
                KHRShoulderRPitch[3] = SplitRotate[1];
                server.send("7:"+ rotateShoulderRightP.ToString());
                //Debug.WriteLine("K2" + rotateShoulderRightP);
                var rotateElbowRight = (int) CalcXYZ(body.Joints[ElbowRight], body.Joints[HandRight],
                    body.Joints[ShoulderRight]);
                rotateElbowRight -= 120;
                SplitRotate = BitConverter.GetBytes((Int16) rotateElbowRight);
                KHRElbowR[2] = SplitRotate[0];
                KHRElbowR[3] = SplitRotate[1];
                server.send("5:"+ rotateElbowRight.ToString());
                //Debug.WriteLine("K3" + rotateElbowRight);

                var rotateShoulderLeftP = (int) CalcYZ(body.Joints[ShoulderLeft], body.Joints[SpineShoulder],
                    body.Joints[ElbowLeft]);
                rotateShoulderLeftP -= 173;
                if (rotateShoulderLeftP > 0) rotateShoulderLeftP *= 2;
                SplitRotate = BitConverter.GetBytes((Int16) rotateShoulderLeftP);
                KHRShoulderLPitch[2] = SplitRotate[0];
                KHRShoulderLPitch[3] = SplitRotate[1];
                server.send("9:"+ rotateShoulderLeftP.ToString());
               // Debug.WriteLine("K4" + rotateShoulderLeftP);
                var rotateShoulderLeftR = (int) CalcXYZ(body.Joints[ShoulderLeft],
                    body.Joints[SpineShoulder],
                    body.Joints[ElbowLeft]);
                rotateShoulderLeftR = 205 - rotateShoulderLeftR; // TODO : 正しい？
                SplitRotate = BitConverter.GetBytes((Int16) rotateShoulderLeftR);
                KHRShoulderLRoll[2] = SplitRotate[0];
                KHRShoulderLRoll[3] = SplitRotate[1];
                server.send("10:"+ rotateShoulderLeftR.ToString());
               // Debug.WriteLine("K5" + rotateShoulderLeftR);
                var rotateElbowLeft = (int) CalcXYZ(body.Joints[ElbowLeft], body.Joints[HandLeft],
                    body.Joints[ShoulderLeft]);
                rotateElbowLeft = 120 - rotateElbowLeft; // TODO : 正しい？
                SplitRotate = BitConverter.GetBytes((Int16) rotateElbowLeft);
                KHRElbowL[2] = SplitRotate[0];
                KHRElbowL[3] = SplitRotate[1];
                server.send("11:"+ rotateElbowLeft.ToString());
                //Debug.WriteLine("K6" + rotateElbowLeft);
                var rotateSpainMidY = (int) CalcXz(body.Joints[SpineMid], body.Joints[ShoulderLeft],
                    body.Joints[ShoulderRight]);
                rotateSpainMidY -= 178;
                if (body.Joints[ShoulderRight].Position.Z > body.Joints[ShoulderLeft].Position.Z)
                {
                    rotateSpainMidY = 0 - rotateSpainMidY;
                }

                SplitRotate = BitConverter.GetBytes((Int16) rotateSpainMidY);
                KHRSpineM[2] = SplitRotate[0];
                KHRSpineM[3] = SplitRotate[1];
                server.send("8:"+ rotateSpainMidY.ToString());
               // Debug.WriteLine("K7" + rotateSpainMidY);
            }
        }
    }
}