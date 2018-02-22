﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public void socket(string sendMsg)
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
        int flag = 0,n=0;
        int ER1, ER2, SR3, SR4, SL5, SL6, EL7, EL8,SM10, SM11;
        public const int Port = 55555;
        public const int Port2 = 9999;
        BodyFrameReader bodyFrameReader; //
        Body[] bodies; // Bodyを保持する配列；Kinectは最大6人トラッキングできる
       Conection server1 = new Conection(Port);
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

        public static double XY(Joint cen , Joint first, Joint second)
        {
            const double PI = 3.1415926535897;

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
            const double PI = 3.1415926535897;

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
            const double PI = 3.1415926535897;

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
            const double PI = 3.1415926535897;

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
                                
                                
                                
                                flag = n % 3;
                                if (flag == 1)
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

                                    /*switch (joint.Key)          //ロボゼロ対応
                                     {
                                        // case JointType.SpineMid:
                                              //SM11 = (int)YZ(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], body.Joints[JointType.SpineShoulder]);
                                             //Debug.WriteLine("SM11:" + SM11);
                                             //server1.socket("10:" + YowRotate);
                                             //server1.socket("11:" + PitchRotate);
                                             //break;
                                         case JointType.ElbowLeft:
                                              EL8 = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                                    EL8 = 120 - EL8;
                                    string el8 = EL8.ToString();
                                    server1.socket("8:" + el8);
                                             break;
                                         case JointType.ShoulderLeft:
                                            SL6 = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                                    SL6 = 160 - SL6;
                                    string sl6 = SL6.ToString();
                                    server1.socket("6:" + sl6);
                                             break;
                                         case JointType.ShoulderRight:
                                              SR3 = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                                             SR4 = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                                             SR3 = SR3 - 160;
                                             // SR4 = (SR4 - 157) * 2;
                                             string sr3 = SR3.ToString();
                                            server1.socket("3:" + sr3);
                                            //string sr4 = SR4.ToString();
                                            //Debug.WriteLine("SR3:" + SR3);
                                            // Debug.WriteLine("SR4:" + SR4);
                                            //SR3 = SR3 - 75;
                                            // SR4 = 107 - SR4;
                                            //if (SR4 > 10) SR4 = SR4 * 3;
                                            //server1.socket("3:" + SR3);
                                            // server1.socket("4:" + SR4);
                                            break;
                                             case JointType.ElbowRight:
                                               ER1 = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                                            ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                                            ER1 = ER1 - 120;
                                            ER2 = ER2 - 130;
                                            string er1 = ER1.ToString();
                                            //string er2 = ER2.ToString();
                                            //Debug.WriteLine("ER1:" + ER1);
                                            //Debug.WriteLine("ER2:" + ER2);
                                            //server1.socket("2:" + er2);
                                            server1.socket("1:" + er1);
                                             break;  

                                     }*/



                                            SR3 = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                                            SR4 = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowRight]);
                                            SR3 = SR3 - 160;
                                            SR4 = SR4-158;
                                            if (SR4 > 0) SR4 = SR4 * 2;
                                            string sr3 = SR3.ToString();
                                            string sr4 = SR4.ToString();
                                            ER1 = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderRight]);
                                            //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                                            ER1 = ER1 - 120;
                                           // ER2 = ER2 - 130;
                                            string er1 = ER1.ToString();
                                           // string er2 = ER2.ToString();
                                            SL5 = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                                            SL5 = 159 - SL5;
                                            if (SL5 < 0) SL5 = SL5 * 2;
                                            string sl5 = SL5.ToString();
                                            SL6 = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder], body.Joints[JointType.ElbowLeft]);
                                            SL6 = 160 - SL6;
                                            string sl6 = SL6.ToString();
                                             EL8 = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderLeft]);
                                            EL8 = 120 - EL8;
                                            string el8 = EL8.ToString();
                                            SM11 = (int)XYZ(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], body.Joints[JointType.SpineShoulder]);
                                            SM10 = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ShoulderRight]);
                                            SM11 = SM11 - 225;
                                            SM10 = SM10 - 175;
                                            string sm11 = SM11.ToString();
                                            string sm10 = SM10.ToString();
                                            
                            var task1 = new Task(() =>
                                    {
                                        server1.socket("1:" + er1);
                                        Thread.Sleep(100);
                                    });
                                    var task2 = new Task(() =>
                                    {
                                        server1.socket("3:" + sr3);
                                        Thread.Sleep(100);
                                    });
                                    var task3 = new Task(() =>
                                    {
                                        server1.socket("4:" + sr4);
                                        Thread.Sleep(100);
                                    });
                                    var task4 = new Task(() =>
                                    {
                                        server1.socket("5:" + sl5);
                                        Thread.Sleep(100);
                                    });
                                    var task5 = new Task(() =>
                                    {
                                        server1.socket("6:" + sl6);
                                        Thread.Sleep(100);
                                    });
                                    var task6 = new Task(() =>
                                    {
                                        server1.socket("8:" + el8);
                                        Thread.Sleep(100);
                                    });
                                    var task7 = new Task(() =>
                                    {
                                        server1.socket("10:" + sm10);
                                        Thread.Sleep(100);
                                    });
                                    var task8 = new Task(() =>
                                    {
                                        server1.socket("11:" + sm11);
                                        Thread.Sleep(100);
                                    });
                                    task1.Start();
                                    task2.Start();
                                    task3.Start();
                                    task4.Start();
                                    task5.Start();
                                    task6.Start();
                                    task7.Start();
                                    task8.Start();
                                    
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