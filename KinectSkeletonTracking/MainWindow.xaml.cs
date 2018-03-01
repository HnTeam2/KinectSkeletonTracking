using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public void socket(byte[] sendMsg)
        {
            enc = System.Text.Encoding.UTF8;
            //クライアントに送る文字列を作成してデータを送信する

            //文字列をバイト型配列に変換
            //byte[] sendBytes = enc.GetBytes(sendMsg);
            //データを送信する
            ns.Write(sendMsg, 0, sendMsg.Length);
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
        int flag = 0, n = 0;
        byte[] Er1 = new byte[4] { 0, 1, 0, 0 };
        byte[] Sr3 = new byte[4] { 0, 3, 0, 0 };
        byte[] Sr4 = new byte[4] { 0, 4, 0, 0 };
        byte[] Sl5 = new byte[4] { 0, 5, 0, 0 };
        byte[] Sl6 = new byte[4] { 0, 6, 0, 0 };
        byte[] El8 = new byte[4] { 0, 8, 0, 0 };
        byte[] Sm10 = new byte[4] { 0, 10, 0, 0 };
        byte[] Sm11 = new byte[4] { 0, 11, 0, 0 };
        int ER1 = 163, ER2 = 130, SR3 = 125, SR4 = 162, SL5 = 168, SL6 = 119, EL7 = 139, EL8 = 168, SM11 = 177, SM10;
        int cnt = 0, cut = 0;
        float z, y;
        public const int Port = 55555;
        public const int Port2 = 9999;
        BodyFrameReader bodyFrameReader; //

        Body[] bodies; // Bodyを保持する配列；Kinectは最大6人トラッキングできる

        //     Conection server1 = new Conection(Port);
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
            SendRotate(); // 角度を取得して送信する
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

        private static double XY(Joint cen, Joint first, Joint second)
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

        private static double XZ(Joint cen, Joint first, Joint second)
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

        private static double YZ(Joint cen, Joint first, Joint second)
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

        private static double XYZ(Joint cen, Joint first, Joint second)
        {
            const double PI = 3.1415926535897;

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

        private static int CopZ(Joint first, Joint second)
        {
            int flag = 0;
            if (first.Position.Z > second.Position.Z)
            {
                flag = 1;
            }

            return flag;
        }

        private static double CopY(Joint first, Joint second)
        {
            int flag = 0;
            if (first.Position.Z > second.Position.Z)
            {
                flag = 1;
            }

            return flag;
        }

        private async Task Robozero(Body body)
        {
            if (cnt == 0)
            {
                z = body.Joints[JointType.SpineBase].Position.Z;
                cnt++;
            }
            else
            {
                if (z - body.Joints[JointType.SpineBase].Position.Z > 0.5)
                {
                    z = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] sya = new byte[] { 1, 0, 0, 0 };
                    Debug.WriteLine(BitConverter.ToString(sya));
                    //server1.socket(sya);
                }

                if (z - body.Joints[JointType.SpineBase].Position.Z < -0.5)
                {
                    z = body.Joints[JointType.SpineBase].Position.Z;
                    byte[] ta = new byte[] { 2, 0, 0, 0 };
                    //server1.socket(ta);
                    Debug.WriteLine(BitConverter.ToString(ta));
                }
            }

            if (cut == 0)
            {
                y = body.Joints[JointType.SpineBase].Position.Y;
                cut++;
            }
            else
            {
                if (y - body.Joints[JointType.SpineBase].Position.Y > 0.5)
                {
                    z = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] go = new byte[] { 3, 0, 0, 0 };
                    Debug.WriteLine(BitConverter.ToString(go));
                    //server1.socket(go);
                }

                if (z - body.Joints[JointType.SpineBase].Position.Y < -0.5)
                {
                    z = body.Joints[JointType.SpineBase].Position.Y;
                    byte[] back = new byte[] { 4, 0, 0, 0 };
                    Debug.WriteLine(BitConverter.ToString(back));
                    //server1.socket(back);
                }
            }

            if (z - body.Joints[JointType.SpineBase].Position.Z < 20 &&
                z - body.Joints[JointType.SpineBase].Position.Z > -20)
            {
                var taskSr3 = Task.Run(() =>
                {
                    SR3 = (int)XYZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineMid],
                        body.Joints[JointType.ElbowRight]);
                    SR3 = (SR3 - 98) / 6;
                    SR3 = SR3 * 6;
                    Int16 sr3 = (Int16)SR3;
                    byte[] d = BitConverter.GetBytes(sr3);
                    Sr3[2] = d[0];
                    Sr3[3] = d[1];
                });

                var taskSr4 = Task.Run(() =>
                {
                    SR4 = (int)YZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineShoulder],
                        body.Joints[JointType.ElbowRight]);
                    SR4 = (SR4 - 128) / 6;
                    SR4 = SR4 * 6;
                    if (SR4 > 0) SR4 = SR4 * 2;
                    Int16 sr4 = (Int16)SR4;
                    byte[] e = BitConverter.GetBytes(sr4);
                    Sr4[2] = e[0];
                    Sr4[3] = e[1];
                });

                var taskEr1 = Task.Run(() =>
                {
                    ER1 = (int)XYZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandRight],
                        body.Joints[JointType.ShoulderRight]);
                    //ER2 = (int)XZ(body.Joints[JointType.ElbowRight], body.Joints[JointType.HandTipRight], body.Joints[JointType.ShoulderRight]);
                    ER1 = (ER1 - 120) / 6;
                    ER1 = ER1 * 6;
                    // ER2 = ER2 - 130;
                    Int16 er1 = (Int16)ER1;
                    byte[] c = BitConverter.GetBytes(er1);
                    Er1[2] = c[0];
                    Er1[3] = c[1];
                });

                var taskSl5 = Task.Run(() =>
                {
                    // string er2 = ER2.ToString();
                    SL5 = (int)YZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder],
                        body.Joints[JointType.ElbowLeft]);
                    SL5 = (SL5 - 173) / 6;
                    SL5 = SL5 * 6;
                    if (SL5 > 0) SL5 = SL5 * 2;
                    Int16 sl5 = (Int16)SL5;
                    byte[] f = BitConverter.GetBytes(sl5);
                    Sl5[2] = f[0];
                    Sl5[3] = f[1];
                });

                var taskSl6 = Task.Run(() =>
                {
                    SL6 = (int)XYZ(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.SpineShoulder],
                        body.Joints[JointType.ElbowLeft]);
                    SL6 = 160 - SL6;
                    SL6 = SL6 / 6;
                    SL6 = SL6 * 6;
                    Int16 sl6 = (Int16)SL6;
                    byte[] g = BitConverter.GetBytes(sl6);
                    Sl6[2] = g[0];
                    Sl6[3] = g[1];
                });

                var taskEl8 = Task.Run(() =>
                {
                    EL8 = (int)XYZ(body.Joints[JointType.ElbowLeft], body.Joints[JointType.HandLeft],
                        body.Joints[JointType.ShoulderLeft]);
                    EL8 = (120 - EL8) / 6;
                    EL8 = EL8 * 6;
                    Int16 el8 = (Int16)EL8;
                    byte[] h = BitConverter.GetBytes(el8);
                    El8[2] = h[0];
                    El8[3] = h[1];
                });

                var taskSm10 = Task.Run(() =>
                {
                    SM10 = (int)XZ(body.Joints[JointType.SpineMid], body.Joints[JointType.ShoulderLeft],
                        body.Joints[JointType.ShoulderRight]);
                    int flag = CopZ(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ShoulderLeft]);
                    SM10 = (SM10 - 169) / 6;
                    SM10 = SM10 * 6;
                    if (flag == 0)
                    {
                        SM10 = 0 - SM10;
                    }

                    Int16 sm10 = (Int16)SM10;
                    byte[] i = BitConverter.GetBytes(sm10);
                    Sm10[2] = i[0];
                    Sm10[3] = i[1];
                });

                var taskSm11 = Task.Run(() =>
                {
                    SM11 = (int)YZ(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase],
                        body.Joints[JointType.SpineShoulder]);
                    SM11 = (SM11 - 225) / 6;
                    SM11 = SM11 * 6;
                    Int16 sm11 = (Int16)SM11;
                    byte[] j = BitConverter.GetBytes(sm11);
                    Sm11[2] = j[0];
                    Sm11[3] = j[1];
                });
                //同期待ち状態。動作が遅くなる場合は消す
                var combinedTask = Task.WhenAll(taskSr3, taskSr4, taskEr1, taskSl5, taskSl6, taskEl8, taskSm10,
                    taskSm11);
                await combinedTask;
            }
        }

        private async void SendRotate()
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
                        JointType[] useJointType =
                        {
                            JointType.ElbowRight, JointType.ElbowLeft, JointType.HipRight, JointType.HipLeft,
                            JointType.ShoulderRight, JointType.ShoulderLeft, JointType.KneeRight, JointType.KneeLeft,
                            JointType.HandRight, JointType.HandLeft, JointType.SpineMid, JointType.AnkleRight,
                            JointType.AnkleLeft,
                        };

                        /* TextBox[] textBox_joint = new TextBox[13]{ ElbowRight, ElbowLeft, HipRight, HipLeft ,
                                    ShourderRight, ShourderLeft, KneeRight, KneeLeft,
                                    HandRight, HandLeft, SpinMid, AncleRight, AncleLeft,
                                    };*/

                        //連想配列
                        Dictionary<JointType, TextBox> textBox_joint = new Dictionary<JointType, TextBox>
                        {
                            {JointType.ElbowRight, ElbowRight},
                            {JointType.ElbowLeft, ElbowLeft},
                            {JointType.HipRight, HipRight},
                            {JointType.HipLeft, HipLeft},
                            {JointType.ShoulderRight, ShourderRight},
                            {JointType.ShoulderLeft, ShourderLeft},
                            {JointType.KneeRight, KneeRight},
                            {JointType.KneeLeft, KneeLeft},
                            {JointType.HandRight, HandRight},
                            {JointType.HandLeft, HandLeft},
                            {JointType.SpineMid, SpinMid},
                            {JointType.AnkleRight, AncleRight},
                            {JointType.AnkleLeft, AncleLeft}
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
                                await Robozero(body);


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


                                    //転送
                                    //server1.socket(Er1);
                                    // server1.socket(Sr3);
                                    //server1.socket(Sr4);
                                    //server1.socket(Sl5);
                                    //server1.socket(Sl6);
                                    //server1.socket(El8);
                                    //server1.socket(Sm10);
                                    //server1.socket(Sm11);


                                    Debug.WriteLine("1" + BitConverter.ToString(Er1));
                                    Debug.WriteLine("3" + BitConverter.ToString(Sr3));
                                    Debug.WriteLine("4" + BitConverter.ToString(Sr4));
                                    Debug.WriteLine("5" + BitConverter.ToString(Sl5));
                                    Debug.WriteLine("6" + BitConverter.ToString(Sl6));
                                    Debug.WriteLine("8" + BitConverter.ToString(El8));
                                    Debug.WriteLine("10" + BitConverter.ToString(Sm10));
                                    Debug.WriteLine("11" + BitConverter.ToString(Sm11));

                                    //Debug.WriteLine("5:" + sl5);
                                }

                                //DictionaryのKeyで値と一致
                                var Key = jointType;

                                //Keyから値を取得
                                TextBox textBox_num = textBox_joint[Key];
                                textBox_num.Text = joint.Key + "R" + " " + RollRotate + " " + "Y" + " " + YowRotate +
                                                   " " + "P" + " " + PitchRotate;
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