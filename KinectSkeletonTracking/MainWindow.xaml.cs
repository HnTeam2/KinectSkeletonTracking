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

        /* private void socket(string sendMsg)
         {
             //string ipString = "172.20.10.5"; //ローカルは”127.0.0.1”
            // System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);

             //サーバーのIPアドレスとポート番号？
            string ipOrHost = "192.168.43.120";//←ここ ローカルは”127.0.0.1”
             int port = 55555;
             //サーバーと接続する（わからん）
               System.Net.Sockets.TcpClient tcp =
                 new System.Net.Sockets.TcpClient(ipOrHost, port);
             Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
               ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port,
                 ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address,
                 ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port);
             //NetworkStreamを取得する
             System.Net.Sockets.NetworkStream ns = tcp.GetStream();

             //読み取り、書き込みのタイムアウトを”　”秒にする。
             ns.ReadTimeout = 30000;
             ns.WriteTimeout = 30000;

             //サーバに　データを送信する,バイト型？
             System.Text.Encoding enc = System.Text.Encoding.UTF8;
             byte[] sendBytes = enc.GetBytes(sendMsg);

             //データを送信する
             ns.Write(sendBytes, 0, sendBytes.Length);
             Console.WriteLine(sendMsg);

             //サーバーから送られたデータを受信する
             System.IO.MemoryStream ms = new System.IO.MemoryStream();
             byte[] resBytes = new byte[256];
             int resSize = 0;
             do
             {
                 //データを受信する、0だと切断してる判断。
                 resSize = ns.Read(resBytes, 0, resBytes.Length);
                 if (resSize == 0)
                 {
                     Console.WriteLine("サーバーが切断しました。");
                     break;
                 }
                 //受信したデータを蓄積する
                 ms.Write(resBytes, 0, resSize);
             } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
             string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();

             //閉じる
            //ns.Close();
           //tcp.Close();
         }*/

     

       
    
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

        public static double Angle(Joint cen , Joint first, Joint second)
        {
            const double M_PI = 3.1415926535897;

            double ma_x = first.Position.X - cen.Position.X;
            double ma_y = first.Position.Y - cen.Position.Y;
            double mb_x = second.Position.X - cen.Position.X;
            double mb_y = second.Position.Y - cen.Position.Y;
            double v1 = (ma_x * mb_x) + (ma_y * mb_y);
            double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
            double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
            double cosM = v1 / (ma_val * mb_val);
            double angleAMB = Math.Acos(cosM) * 180 / M_PI;

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
                                switch (joint.Key)          //ロボゼロ対応
                                {
                                    case JointType.SpineMid:
                                        if (Y < -45) Y = -45;
                                        if (Y >45) Y = 45;
                                        if (P > 0) P = 0 - P;
                                        P = P + 130;
                                        if (P > 45) P = 45;
                                        if (P < -45) P = -45;
                                        break;
                                    case JointType.ElbowRight:
                                        if (P > 0) P = 0 - P;
                                        P = 180 + P;
                                        Y = Y + 45;
                                        if (Y < -45) Y = -45;
                                        if (Y > 45) Y = 45;
                                        break;
                                    

                                }
                                // if (R < 0) R = 0 - R;
                                // if (Y < 0) Y = 0 - Y;
                                // if (P < 0) P = 0 - P;
                                //Textで表示させるためにstring型へ変換
                                string RollRotate = R.ToString();
                                string YowRotate = Y.ToString();
                                string PitchRotate = P.ToString();
                                int X = (int)Angle(body.Joints[JointType.ShoulderRight], body.Joints[JointType.SpineBase], body.Joints[JointType.ElbowRight]);
                                X = X - 75;
                                string Xing = X.ToString();
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
                                    
                                   switch (joint.Key)          //ロボゼロ対応
                                    {
                                        //case JointType.SpineMid:
                                           //server1.socket("10:" + YowRotate);
                                           // server1.socket("11:" + PitchRotate);
                                          //  break;
                                        /*case JointType.ElbowLeft:
                                            server1.socket("7:" + YowRotate);
                                            server1.socket("8:" + PitchRotate);
                                            break;
                                        case JointType.ShoulderLeft:
                                            server1.socket("6:" + RollRotate);
                                            server1.socket("5:" + PitchRotate);
                                            break;*/
                                        case JointType.ShoulderRight:
                                          server1.socket("3:" + Xing);
                                            Debug.WriteLine(Xing);
                                           // server1.socket("4:" + PitchRotate);
                                            break;
                                        //case JointType.ElbowRight:
                                          //  server1.socket("2:" + YowRotate);
                                           // server1.socket("1:" + PitchRotate);
                                           // break;  

                                    }
                                    
                                }

                                //DictionaryのKeyで値と一致
                                var Key = jointType;

                                //Keyから値を取得
                                TextBox textBox_num = textBox_joint[Key];

                                textBox_num.Text = joint.Key+"R" + " " + RollRotate + " " + "Y" + " " + YowRotate + " " + "P" + " " + PitchRotate;
                            }
                            //flag = 1 - flag;
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