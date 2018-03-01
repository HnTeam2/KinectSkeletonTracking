using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectSkeletonTracking
{
    class Conection
    {
        public const string IpString = "192.168.43.181"; //PCのIPアドレスにする
        private System.Net.IPAddress ipAdd;
        private System.Net.Sockets.TcpListener listener;
        private System.Net.Sockets.NetworkStream ns;
        private System.IO.MemoryStream ms;
        private System.Net.Sockets.TcpClient client;
        private System.Text.Encoding enc;
        private System.Net.Sockets.TcpClient tcp;

        //-----//サーバ側の処理//-----//
        public Conection(int port)
        {
            ipAdd = System.Net.IPAddress.Parse(IpString);
            listener = new System.Net.Sockets.TcpListener(ipAdd, port);
            //受信開始
            listener.Start();
            Console.WriteLine("Listenを開始しました({0}:{1})。",
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
            //接続植え付け
            client = listener.AcceptTcpClient();
            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);
            //NetworkStream（ネットワークを読み書きの対象とする時のストリーム）うまくかけん)を取得
            ns = client.GetStream();
        }

      

       

        //-----//接続相手にデータを送信する//-----//
        public void sendByte(byte[] sendMsg)
        {
            enc = System.Text.Encoding.UTF8;
            ns.Write(sendMsg, 0, sendMsg.Length);
            Console.WriteLine(sendMsg);
        }
        public void send(string sendMsg)
        {
            enc = System.Text.Encoding.UTF8;
            //クライアントに送る文字列を作成してデータを送信する

            //文字列をバイト型配列に変換
            byte[] sendBytes = enc.GetBytes(sendMsg);
            //データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine(sendMsg);
        }

        //-----//サーバ側のソケットを閉じる//-----//
        public void serverClose()
        {
            ns.Close();
            client.Close();
            Console.WriteLine("クライアントとの接続終了");
            listener.Stop();
            Console.WriteLine("Listener終了");
            Console.ReadLine();
        }

      
    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        int sendTime = 0,n=0;
        //角道格納(バイト型)
        byte[] angle;
        
        //robozero
        byte[] RzElbowR = new byte[4] { 0, 1, 0, 0 };
        byte[] RzShoulderRRoll = new byte[4] { 0, 3, 0, 0 };
        byte[] RzShoulderRPitch = new byte[4] { 0, 4, 0, 0 };
        byte[] RzShoulderLPitch = new byte[4] { 0, 5, 0, 0 };
        byte[] RzShoulderLRoll = new byte[4] { 0, 6, 0, 0 };
        byte[] RzElbowL = new byte[4] { 0, 8, 0, 0 };
        byte[] RzSpineMYow = new byte[4] { 0, 10, 0, 0 };
        byte[] RzSpineMPitch = new byte[4] { 0, 11, 0, 0 };
        byte[] RzFootR = new byte[4] { 5, 0, 0, 0 };
        byte[] RzAnkleR = new byte[4] { 5, 1, 0, 0 };
        byte[] RzKneeR = new byte[4] { 5, 2, 0, 0 };
        byte[] RzHipRPitch = new byte[4] { 5, 4, 0, 0 };
        byte[] RzHipRRoll = new byte[4] { 5, 5, 0, 0 };
        byte[] RzHipLRoll = new byte[4] { 5, 6, 0, 0 };
        byte[] RzHipLPitch = new byte[4] { 5, 7, 0, 0 };
        byte[] RzKneeL = new byte[4] { 5, 9, 0, 0 };
        byte[] RzAnkleLPitch = new byte[4] { 5, 10, 0, 0 };
        byte[] RzFootL = new byte[4] { 5, 11, 0, 0 };
        //KHR
        byte[] KHRElbowR = new byte[4] { 0, 5, 0, 0 };
        byte[] KHRShoulderRRoll = new byte[4] { 0, 6, 0, 0 };
        byte[] KHRShoulderRPitch = new byte[4] { 0, 7, 0, 0 };
        byte[] KHRShoulderLPitch = new byte[4] { 0, 9, 0, 0 };
        byte[] KHRShoulderLRoll = new byte[4] { 0, 10, 0, 0 };
        byte[] KHRElbowL = new byte[4] { 0, 11, 0, 0 };
        byte[] KHRSpineM = new byte[4] { 0, 8, 0, 0 };
        byte[] KHRFootR = new byte[4] { 0, 0, 0, 0 };
        byte[] KHRAnkleR = new byte[4] { 0, 1, 0, 0 };
        byte[] KHRKneeR = new byte[4] { 0, 2, 0, 0 };
        byte[] KHRHipRPitch = new byte[4] { 0, 3, 0, 0 };
        byte[] KHRHipRRoll = new byte[4] { 0, 4, 0, 0 };
        byte[] KHRHipLRoll = new byte[4] { 0, 12, 0, 0 };
        byte[] KHRHipLPitch = new byte[4] { 0, 13, 0, 0 };
        byte[] KHRKneeL = new byte[4] { 0, 14, 0, 0 };
        byte[] KHRAnkleLPitch = new byte[4] { 0, 15, 0, 0 };
        byte[] KHRFootL = new byte[4] { 0, 16, 0, 0 };
        //関数角度
        int ElbowR = 163,ShoulderRRoll = 125, ShoulderRPitch = 162, ShoulderLRoll = 168, ShoulderLPitch = 119,  ElbowL = 168, SpainMPitch = 177,SpainMYow;
        //ロボットの移動が必要な関数
        int Zsflg, Ysflg;
        float SpainBaseZ, SpainBaseY;
        //ポート番号
        public const int Port1 = 55555;
        public const int Port2 = 9999;

        const double PI = 3.1415926535897;
        BodyFrameReader bodyFrameReader; //
        Body[] bodies=null; // Bodyを保持する配列；Kinectは最大6人トラッキングできる
        
        //Conection server1 = new Conection(Port1);
        //Conection server2 = new Conection(Port2);
        public MainWindow()
        {
            InitializeComponent();
        }  
    
    // Windowが表示されたときコールされる
    private void Window_Loaded(object sensor, RoutedEventArgs e)
        {
            try
            {
                kinect = KinectSensor.GetDefault();
                // TODO: Kinectが使用可能状態か調べてから次の処理に移りたい。
                kinect.Open();

                // Bodyを入れる配列を作る
                bodies = new Body[kinect.BodyFrameSource.BodyCount];

                // body用のフレームリーダーを取得する。30fps?
                bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                //ここでイベントハンドラを追加する。Kinectが撮影したBodyのフレームデータが到着したときに通知される。
                bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }


        // Windowが閉じたときにコールされる
        private void Window_Closing(object sensor, System.ComponentModel.CancelEventArgs e)
        {
            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        // イベント発生はこのメソッドに通知される？
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            
            UpdateBodyFrame(e); // ボディデータの更新をする
            // DrawBodyFrame(); // TODO:GUIに対する描写は後に実装する
            SendRotate();    // 角度を取得して送信する
        }



        // ボディの更新をする。イベントハンドラ（フレームが取得できた、イベントが発生した ときにコールされる）
        private void UpdateBodyFrame(BodyFrameArrivedEventArgs e)
        {

            // usingブロック内で宣言された変数はGCに任せずに確実に開放される
            // フレームはKinectから送られてくるデータの最小単位。e.FrameReference.AcquireFrame()で取得する。
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                // nullが送られてくる可能性があるので、その場合は破棄。
                if (bodyFrame == null)
                {
                    return;
                }

                // ボディデータをbodiesにコピーする
                bodyFrame.GetAndRefreshBodyData(bodies);
            }
        }

        private void DrawEllipse(Joint joint, int R, Brush brush)
        {
            var ellipse = new Ellipse()
            {
                // X,Y,Z軸（三次元）をX,Y軸（二次元）に変換
                Width = R,
                Height = R,
                Fill = brush,
            };

            // カメラからDepth座標へ変換
            var point = kinect.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            // 関節を円で描画
            Canvas.SetLeft(ellipse, point.X - (R / 2));
            Canvas.SetTop(ellipse, point.Y - (R / 2));

            CanvasBody.Children.Add(ellipse);
        }
        //XY XZ YZ XYZ 面の角道計算
        public static double XY(Joint cen , Joint first, Joint second)
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
        public static double XZ(Joint cen, Joint first, Joint second)
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
        public static double YZ(Joint cen, Joint first, Joint second)
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
        public static double XYZ(Joint cen , Joint first, Joint second)
        {
            

            double maX = first.Position.X - cen.Position.X;
            double maY = first.Position.Y - cen.Position.Y;
            double mbX = second.Position.X - cen.Position.X;
            double mbY = second.Position.Y - cen.Position.Y;
            double maZ= first.Position.Z - cen.Position.Z;
            double mbZ= second.Position.Z - cen.Position.Z;
            double v1 = (maX * mbX) + (maY * mbY)+(maZ*mbZ);
            double maVal = Math.Sqrt(maX * maX + maY * maY+maZ*maZ);
            double mbVal = Math.Sqrt(mbX * mbX + mbY * mbY+mbZ*mbZ);
            double cosM = v1 / (maVal * mbVal);
            double angleAMB = Math.Acos(cosM) * 180 / PI;

            return angleAMB;
        }
        //Z軸の比べで前進後退判定
        public static int CopZ(Joint first, Joint second)
        {
            int flag = 0;
            if (first.Position.Z > second.Position.Z) { flag = 1; }
            return flag;

        }
        //Y軸の比べでしゃがみ立つ判定
        public static double CopY(Joint first, Joint second)
        {
            int flag = 0;
            if (first.Position.Z > second.Position.Z) { flag = 1; }
            return flag;

        }
       //ロボゼロのstring型送信
        private void robozeroString(Body body)
        {
            //下半身
            //前後
            if (Zsflg == 0)
            {
                SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                Zsflg++;
            }
            else
            {
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > 0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    
                    //Debug.WriteLine(BitConverter.ToString(go));
                    //server1.send("go");
                }
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < -0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    
                    //server1.send("back");
                    // Debug.WriteLine(BitConverter.ToString(back));
                }
            }
            //しゃがむ　立つ
            if (Ysflg == 0)
            {
                SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                Ysflg++;
            }
            else
            {
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y > 0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    //Debug.WriteLine(BitConverter.ToString(squat));
                    //server1.send("squat");
                }
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y < -0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    // Debug.WriteLine(BitConverter.ToString(stand));
                    //server1.send("stand");
                }
            }
            //上半身
            if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < 0.5 && SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > -0.5)
            {
                //角道計算
                ShoulderRRoll = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineMid], body.Joints[JointType.ElbowRight]);
                ShoulderRPitch = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                ShoulderRRoll = (ShoulderRRoll - 98) / 6;
                ShoulderRPitch = (ShoulderRPitch - 128) / 6;
                ShoulderRRoll = ShoulderRRoll * 6;
                ShoulderRPitch = ShoulderRPitch * 6;
                if (ShoulderRPitch > 0) ShoulderRPitch = ShoulderRPitch * 2;
                ElbowR = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                ElbowR = (ElbowR - 120) / 6;
                ElbowR = ElbowR * 6;
                // ER2 = ER2 - 130;
                ShoulderLPitch = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLPitch = (ShoulderLPitch - 173) / 6;
                ShoulderLPitch = ShoulderLPitch * 6;
                if (ShoulderLPitch > 0) ShoulderLPitch = ShoulderLPitch * 2;
                ShoulderLRoll = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLRoll = 160 - ShoulderLRoll;
                ShoulderLRoll = ShoulderLRoll / 6;
                ShoulderLRoll = ShoulderLRoll * 6;
                ElbowL = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                ElbowL = (120 - ElbowL) / 6;
                ElbowL = ElbowL * 6;
                SpainMPitch = (int)YZ(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], body.Joints[JointType.SpineShoulder]);
                SpainMYow = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ShoulderRight]);
                SpainMPitch = (SpainMPitch - 225) / 6;
                SpainMPitch = SpainMPitch * 6;
                int flag = CopZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ShoulderLeft]);
                SpainMYow = (SpainMYow - 169) / 6;
                SpainMYow = SpainMYow * 6;
                if (flag == 0) { SpainMYow = 0 - SpainMYow; }

                //ロボゼロ転送<string>
                //server1.send("1:"+ ElbowR.ToString());
                // server1.send("3:"+ShoulderRRoll.ToString());
                //server1.send("4:"+ShoulderRPitch.ToString());
                //server1.send("5:"+ShoulderLPitch.ToString());
                //server1.send("6:"+ShoulderLRoll.ToString());
                //server1.send("8:"+ElbowL.ToString());
                //server1.send("10:"+SpainMYow.ToString());
                //server1.send("11:"+SpainMPitch.ToString());
                Debug.WriteLine("1:" + ElbowR.ToString());
                Debug.WriteLine("3:"+ShoulderRRoll.ToString());
                Debug.WriteLine("4:"+ShoulderRPitch.ToString());
                Debug.WriteLine("5:"+ShoulderLPitch.ToString());
                Debug.WriteLine("6:"+ShoulderLRoll.ToString());
                Debug.WriteLine("8:"+ElbowL.ToString());
                Debug.WriteLine("10:"+SpainMYow.ToString());
                Debug.WriteLine("11:"+SpainMPitch.ToString());
            }
        }
        //ロボゼロのbyte型送信
        private void robozeroByte(Body body)
        {
            //下半身
            //前後
            if (Zsflg == 0)
            {
                SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                Zsflg++;
            }
            else
            {
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > 0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] go = new byte[] { 1, 0, 0, 0 };
                    //Debug.WriteLine(BitConverter.ToString(go));
                    //server1.sendByte(go);
                }
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < -0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] back = new byte[] { 2, 0, 0, 0 };
                    //server1.sendByte(back);
                   // Debug.WriteLine(BitConverter.ToString(back));
                }
            }
            //しゃがむ　立つ
            if (Ysflg == 0)
            {
                SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                Ysflg++;
            }
            else
            {
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y > 0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] squat = new byte[] { 3, 0, 0, 0 };
                    //Debug.WriteLine(BitConverter.ToString(squat));
                    //server1.sendByte(squat);
                }
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y < -0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] stand = new byte[] { 4, 0, 0, 0 };
                   // Debug.WriteLine(BitConverter.ToString(stand));
                    //server1.sendByte(stand);
                }
            }
            //上半身
            if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < 0.5 && SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > -0.5)
            {
                //角道計算
                ShoulderRRoll = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineMid], body.Joints[JointType.ElbowRight]);
                ShoulderRPitch = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                ShoulderRRoll = (ShoulderRRoll - 98) / 6;
                ShoulderRPitch = (ShoulderRPitch - 128) / 6;
                ShoulderRRoll = ShoulderRRoll * 6;
                ShoulderRPitch = ShoulderRPitch * 6;
                if (ShoulderRPitch > 0) ShoulderRPitch = ShoulderRPitch * 2;
                ElbowR = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                ElbowR = (ElbowR - 120) / 6;
                ElbowR = ElbowR * 6;
                // ER2 = ER2 - 130;
                ShoulderLPitch = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLPitch = (ShoulderLPitch - 173) / 6;
                ShoulderLPitch = ShoulderLPitch * 6;
                if (ShoulderLPitch > 0) ShoulderLPitch = ShoulderLPitch * 2;
                ShoulderLRoll = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLRoll = 160 - ShoulderLRoll;
                ShoulderLRoll = ShoulderLRoll / 6;
                ShoulderLRoll = ShoulderLRoll * 6;
                ElbowL = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                ElbowL = (120 - ElbowL) / 6;
                ElbowL = ElbowL * 6;
                SpainMYow = (int)YZ(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], body.Joints[JointType.SpineShoulder]);
                SpainMYow = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ShoulderRight]);
                SpainMPitch = (SpainMPitch - 225) / 6;
                SpainMPitch = SpainMPitch * 6;
                int flag = CopZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ShoulderLeft]);
                SpainMYow = (SpainMYow - 169) / 6;
                SpainMYow = SpainMYow * 6;
                if (flag == 0) { SpainMYow = 0 - SpainMYow; }

                //バイト型変換
                angle = BitConverter.GetBytes((Int16)ElbowR);
                RzElbowR[2] = angle[0];
                RzElbowR[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderRRoll);
                RzShoulderRRoll[2] = angle[0];
                RzShoulderRRoll[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderRPitch);
                RzShoulderRPitch[2] = angle[0];
                RzShoulderRPitch[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderLPitch);
                RzShoulderLPitch[2] = angle[0];
                RzShoulderLPitch[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderLRoll);
                RzShoulderLRoll[2] = angle[0];
                RzShoulderLRoll[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ElbowL);
                RzElbowL[2] = angle[0];
                RzElbowL[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)SpainMYow);
                RzSpineMYow[2] = angle[0];
                RzSpineMYow[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)SpainMPitch);
                RzSpineMPitch[2] = angle[0];
                RzSpineMPitch[3] = angle[1];


                //ロボゼロの送信
                //server1.sendtbyte(RzElbowR);
                //server1.sendtbyte(RzShoulderRRoll);
                //server1.sendtbyte(RzShoulderRPitch);
                //server1.sendtbyte(RzShoulderLPitch);
                //server1.sendtbyte(RzShoulderLRoll);
                //server1.sendtbyte(RzElbowL);
                //server1.sendtbyte(RzSpineMYow);
                //server1.sendtbyte(RzSpineMPitch);
                Debug.WriteLine(BitConverter.ToString(RzElbowR));
            }
        }
        //KHRのbyte型送信
        private void KHRByte(Body body)
        {
            //下半身
            //前後
            if (Zsflg == 0)
            {
                SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                Zsflg++;
            }
            else
            {
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > 0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] go = new byte[] { 1, 0, 0, 0 };
                    //Debug.WriteLine(BitConverter.ToString(go));
                    //server1.sendByte(go);
                }
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < -0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] back = new byte[] { 2, 0, 0, 0 };
                    //server1.sendByte(back);
                    // Debug.WriteLine(BitConverter.ToString(back));
                }
            }
            //しゃがむ　立つ
            if (Ysflg == 0)
            {
                SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                Ysflg++;
            }
            else
            {
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y > 0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] squat = new byte[] { 3, 0, 0, 0 };
                    //Debug.WriteLine(BitConverter.ToString(squat));
                    //server1.sendByte(squat);
                }
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y < -0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] stand = new byte[] { 4, 0, 0, 0 };
                    // Debug.WriteLine(BitConverter.ToString(stand));
                    //server1.sendByte(stand);
                }
            }
            //上半身
            if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < 0.5 && SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > -0.5)
            {
                ShoulderRRoll = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineMid], body.Joints[JointType.ElbowRight]);
                ShoulderRPitch = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                ShoulderRRoll = (ShoulderRRoll - 133) / 6;
                ShoulderRPitch = (ShoulderRPitch - 128) / 6;
                ShoulderRRoll = ShoulderRRoll * 6;
                ShoulderRPitch = ShoulderRPitch * 6;
                if (ShoulderRPitch > 0) ShoulderRPitch = ShoulderRPitch * 2;
                ElbowR = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                ElbowR = (ElbowR - 120) / 6;
                ElbowR = ElbowR * 6;
                // ER2 = ER2 - 130;
                ShoulderLPitch = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLPitch = (ShoulderLPitch - 173) / 6;
                ShoulderLPitch = ShoulderLPitch * 6;
                if (ShoulderLPitch > 0) ShoulderLPitch = ShoulderLPitch * 2;
                ShoulderLRoll = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLRoll = 205 - ShoulderLRoll;
                ShoulderLRoll = ShoulderLRoll / 6;
                ShoulderLRoll = ShoulderLRoll * 6;
                ElbowL = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                ElbowL = (120 - ElbowL) / 6;
                ElbowL = ElbowL * 6;
                SpainMYow = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ShoulderRight]);
                int flag = CopZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ShoulderLeft]);
                SpainMYow = (SpainMYow - 169) / 6;
                SpainMYow = SpainMYow * 6;
                if (flag == 0) { SpainMYow = 0 - SpainMYow; }


                //バイト型変換

                angle = BitConverter.GetBytes((Int16)ElbowR);
                KHRElbowR[2] = angle[0];
                KHRElbowR[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderRRoll);
                KHRShoulderRRoll[2] = angle[0];
                KHRShoulderRRoll[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderRPitch);
                KHRShoulderRPitch[2] = angle[0];
                KHRShoulderRPitch[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderLPitch);
                KHRShoulderLPitch[2] = angle[0];
                KHRShoulderLPitch[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ShoulderLRoll);
                KHRShoulderLRoll[2] = angle[0];
                KHRShoulderLRoll[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)ElbowL);
                KHRElbowL[2] = angle[0];
                KHRElbowL[3] = angle[1];
                angle = BitConverter.GetBytes((Int16)SpainMYow);
                KHRSpineM[2] = angle[0];
                KHRSpineM[3] = angle[1];


                //KHR転送
                //server1.sendtbyte(KHRElbowR);
                //server1.sendtbyte(KHRShoulderRRoll);
                //server1.sendtbyte(KHRShoulderRPitch);
                //server1.sendtbyte(KHRShoulderLPitch);
                //server1.sendtbyte(KHRShoulderLRoll);
                //server1.sendtbyte(KHRElbowL);
                //server1.sendtbyte(KHRSpineM);
                Debug.WriteLine(BitConverter.ToString(KHRElbowR));
            }
        }
        //KHRのstring型送信
        private void KHRString(Body body)
        {
            //下半身
            //前後
            if (Zsflg == 0)
            {
                SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;
                Zsflg++;
            }
            else
            {
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > 0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;

                    //Debug.WriteLine(BitConverter.ToString(go));
                    //server1.send("go");
                }
                if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < -0.5)
                {
                    SpainBaseZ = body.Joints[JointType.SpineBase].Position.Z;

                    //server1.send("back");
                    // Debug.WriteLine(BitConverter.ToString(back));
                }
            }
            //しゃがむ　立つ
            if (Ysflg == 0)
            {
                SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                Ysflg++;
            }
            else
            {
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y > 0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    //Debug.WriteLine(BitConverter.ToString(squat));
                    //server1.send("squat");
                }
                if (SpainBaseY - body.Joints[JointType.SpineBase].Position.Y < -0.5)
                {
                    SpainBaseY = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] stand = new byte[] { 4, 0, 0, 0 };
                    // Debug.WriteLine(BitConverter.ToString(stand));
                    //server1.send("stand");
                }
            }
            //上半身
            if (SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z < 0.5 && SpainBaseZ - body.Joints[JointType.SpineBase].Position.Z > -0.5)
            {
                ShoulderRRoll = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineMid], body.Joints[JointType.ElbowRight]);
                ShoulderRPitch = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                ShoulderRRoll = (ShoulderRRoll - 133) / 6;
                ShoulderRPitch = (ShoulderRPitch - 128) / 6;
                ShoulderRRoll = ShoulderRRoll * 6;
                ShoulderRPitch = ShoulderRPitch * 6;
                if (ShoulderRPitch > 0) ShoulderRPitch = ShoulderRPitch * 2;

                ElbowR = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                ElbowR = (ElbowR - 120) / 6;
                ElbowR = ElbowR * 6;
                // ER2 = ER2 - 130;
                ShoulderLPitch = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLPitch = (ShoulderLPitch - 173) / 6;
                ShoulderLPitch = ShoulderLPitch * 6;
                if (ShoulderLPitch > 0) ShoulderLPitch = ShoulderLPitch * 2;

                ShoulderLRoll = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                ShoulderLRoll = 205 - ShoulderLRoll;
                ShoulderLRoll = ShoulderLRoll / 6;
                ShoulderLRoll = ShoulderLRoll * 6;

                ElbowL = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                ElbowL = (120 - ElbowL) / 6;
                ElbowL = ElbowL * 6;


                SpainMYow = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ShoulderRight]);

                int flag = CopZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ShoulderLeft]);
                SpainMYow = (SpainMYow - 169) / 6;
                SpainMYow = SpainMYow * 6;
                if (flag == 0) { SpainMYow = 0 - SpainMYow; }

                //KHR転送<string>
                //server1.send("5:"+ElbowR.ToString());
                // server1.send("6:"+ShoulderRRoll.ToString());
                //server1.send("7:"+ShoulderRPitch.ToString());
                //server1.send("9:"+ShoulderLPitch.ToString());
                //server1.send("10:"+ShoulderLRoll.ToString());
                //server1.send("11:"+ElbowL.ToString());
                //server1.send("8:"+SpainMYow.ToString());

                Debug.WriteLine("K5:"+ElbowR.ToString());
                Debug.WriteLine("K6:"+ShoulderRRoll.ToString());
                Debug.WriteLine("K7:"+ShoulderRPitch.ToString());
                Debug.WriteLine("K9:"+ShoulderLPitch.ToString());
                Debug.WriteLine("K10:"+ShoulderLRoll.ToString());
                Debug.WriteLine("K11:"+ElbowL.ToString());
                Debug.WriteLine("K8:"+SpainMYow.ToString());
            }
        }
        private void SendRotate()
        {
            
            CanvasBody.Children.Clear();

            // 追跡しているBodyのみループする
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                // Bodyから取得した全関節でループする。
                foreach (var joint in body.Joints)
                {
                    // 追跡可能な状態か？
                    if (joint.Value.TrackingState == TrackingState.Tracked)
                    {
                        DrawEllipse(joint.Value, 10, Brushes.Blue);

                        //JointTypeを配列を格納
                        JointType[] useJointType = { JointType.ElbowRight, JointType.ElbowLeft, JointType.HipRight,JointType. HipLeft ,
                                    JointType.ShoulderRight, JointType.ShoulderLeft, JointType.KneeRight, JointType.KneeLeft,
                                    JointType.HandRight, JointType.HandLeft, JointType.SpineMid, JointType.AnkleRight, JointType.AnkleLeft,
                                    };

                        /* TextBox[] textBox_joint = new TextBox[13]{ ElbowRight, ElbowLeft, HipRight, HipLeft ,
                                    ShourderRight, ShourderLeft, KneeRight, KneeLeft,
                                    HandRight, HandLeft, SpinMid, AncleRight, AncleLeft,
                                    };*/

                        //連想配列
                        Dictionary<JointType, TextBox> textBox_joint = new Dictionary<JointType, TextBox>
                        {
                            {JointType.ElbowRight,ElbowRight},
                            {JointType.ElbowLeft,ElbowLeft},
                            {JointType.HipRight,HipRight},
                            {JointType.HipLeft,HipLeft},
                            {JointType.ShoulderRight,ShourderRight},
                            {JointType.ShoulderLeft,ShourderLeft},
                            {JointType.KneeRight,KneeRight},
                            {JointType.KneeLeft,KneeLeft},
                            {JointType.HandRight,HandRight},
                            {JointType.HandLeft,HandLeft},
                            {JointType.SpineMid,SpinMid},
                            {JointType.AnkleRight,AncleRight},
                            {JointType.AnkleLeft,AncleLeft}
                        };

                       

                        //foreachでJointTypeを網羅
                        foreach (var jointType in useJointType)
                        {
                            if (joint.Key == jointType)
                            {
                                //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                                var orientation = body.JointOrientations[joint.Key].Orientation;

                                //関節のそれぞれの軸に対応する角度を取得する

                                var pitchRotate = (int)CalcRotate.Pitch(orientation);
                                var yowRotate = (int)CalcRotate.Yaw(orientation);
                                var rollRotate = (int)CalcRotate.Roll(orientation);

                                int R = (int)rollRotate;
                                int Y = (int)yowRotate;
                                int P = (int)pitchRotate;
                                
                                //Textで表示させるためにstring型へ変換
                                string RollRotate = R.ToString();
                                string YowRotate = Y.ToString();
                                string PitchRotate = P.ToString();


                                sendTime = n % 30;
                                    if (sendTime == 1)
                                    {


                                    /*switch (joint.Key)           //KHR対応
                                    {
                                        case JointType.FootRight:
                                            server2.socket("0:" + RollRotate);
                                            break;
                                        case JointType.AnkleRight:
                                            server2.socket("1:" + PitchRotate);
                                            break;
                                        case JointType.KneeRight:
                                            server2.socket("2:" + PitchRotate);
                                            break;
                                        case JointType.HipRight:
                                            server2.socket("3:" + PitchRotate);
                                            server2.socket("4:" + RollRotate);
                                            break;
                                        case JointType.ElbowRight:
                                            server2.socket("5:" + PitchRotate);
                                            break;
                                        case JointType.ShoulderRight:
                                            server2.socket("6:" + RollRotate);
                                            server2.socket("7:" + PitchRotate);
                                            break;
                                        case JointType.SpineMid:
                                            server2.socket("8:" + YowRotate);
                                            break;
                                        case JointType.ShoulderLeft:
                                            server2.socket("9:" + PitchRotate);
                                            server2.socket("10:" + RollRotate);
                                            break;
                                        case JointType.ElbowLeft:
                                            server.socket("11;" + PitchRotate);
                                            break;
                                        case JointType.HipLeft:
                                            server2.socket("12:" + RollRotate);
                                            server2.socket("13:" + PitchRotate);
                                            break;
                                        case JointType.KneeLeft:
                                            server2.socket("14:" + PitchRotate);
                                            break;
                                        case JointType.AnkleLeft:
                                            server2.socket("15:" + PitchRotate);
                                            break;
                                        case JointType.FootLeft:
                                            server2.socket("16:" + RollRotate);
                                            break;
                                    }*/

                                    robozeroString(body);
                                    //robozeroByte(body);

                                    //KHRByte(bodies[1]);
                                    if (bodies[1] != null)
                                    {
                                        KHRString(bodies[1]);
                                    }
                                   
                                }
                                //DictionaryのKeyで値と一致
                                var Key = jointType;

                                //Keyから値を取得
                                TextBox textBox_num = textBox_joint[Key];
                                textBox_num.Text = joint.Key+"R" + " " + RollRotate + " " + "Y" + " " + YowRotate + " " + "P" + " " + PitchRotate;
                            }
                            n++;
                        }



                        if (joint.Value.TrackingState == TrackingState.Inferred)
                        {
                            DrawEllipse(joint.Value, 10, Brushes.Yellow);
                        }
                    }
                }
            }
        }
    }
}