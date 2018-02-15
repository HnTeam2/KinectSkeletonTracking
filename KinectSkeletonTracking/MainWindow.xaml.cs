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

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        int flag = 0;
        BodyFrameReader bodyFrameReader; //
        Body[] bodies; // Bodyを保持する配列；Kinectは最大6人トラッキングできる


        public MainWindow()
        {
            InitializeComponent();
        }

        static void socket_open() {

        }
        private void socket(string sendMsg)
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
                                    JointType.FootRight, JointType.FootLeft, JointType.SpineMid, JointType.AnkleRight, JointType.AnkleLeft,
                                    };

                        /* TextBox[] textBox_joint = new TextBox[13]{ ElbowRight, ElbowLeft, HipRight, HipLeft ,
                                    ShourderRight, ShourderLeft, KneeRight, KneeLeft,
                                    FootRight, FootLeft, SpinMid, AncleRight, AncleLeft,
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
                            {JointType.FootRight,FootRight},
                            {JointType.FootLeft,FootLeft},
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
                                if (flag == 1)
                                {
                                    /*switch (joint.Key)           //KHR対応
                                    {
                                        case JointType.FootRight:
                                            socket("0:" + RollRotate);
                                            break;
                                        case JointType.AnkleRight:
                                            socket("1:" + PitchRotate);
                                            break;
                                        case JointType.KneeRight:
                                            socket("2:" + PitchRotate);
                                            break;
                                        case JointType.HipRight:
                                            socket("3:" + PitchRotate);
                                            socket("4:" + RollRotate);
                                            break;
                                        case JointType.ElbowRight:
                                            socket("5:" + PitchRotate);
                                            break;
                                        case JointType.ShoulderRight:
                                            socket("6:" + RollRotate);
                                            socket("7:" + PitchRotate);
                                            break;
                                        case JointType.SpineMid:
                                            socket("8:" + YowRotate);
                                            break;
                                        case JointType.ShoulderLeft:
                                            socket("9:" + PitchRotate);
                                            socket("10:" + RollRotate);
                                            break;
                                        case JointType.ElbowLeft:
                                            socket("11;" + PitchRotate);
                                            break;
                                        case JointType.HipLeft:
                                            socket("12:" + RollRotate);
                                            socket("13:" + PitchRotate);
                                            break;
                                        case JointType.KneeLeft:
                                            socket("14:" + PitchRotate);
                                            break;
                                        case JointType.AnkleLeft:
                                            socket("15:" + PitchRotate);
                                            break;
                                        case JointType.FootLeft:
                                            socket("16:" + RollRotate);
                                            break;
                                    }*/
                                    
                                    switch (joint.Key)          //ロボゼロ対応
                                    {
                                        case JointType.SpineMid:
                                            socket("10:" + YowRotate);
                                            socket("11:" + PitchRotate);
                                            break;
                                        case JointType.ElbowLeft:
                                            socket("7:" + YowRotate);
                                            socket("8:" + PitchRotate);
                                            break;
                                        case JointType.ShoulderLeft:
                                            socket("6:" + RollRotate);
                                            socket("5:" + PitchRotate);
                                            break;
                                        case JointType.ShoulderRight:
                                            socket("3:" + RollRotate);
                                            socket("4:" + PitchRotate);
                                            break;
                                        case JointType.ElbowRight:
                                            socket("2:" + YowRotate);
                                            socket("1:" + PitchRotate);
                                            break;

                                    }

                                }

                                //DictionaryのKeyで値と一致
                                var Key = jointType;

                                //Keyから値を取得
                                TextBox textBox_num = textBox_joint[Key];

                                textBox_num.Text = "R" + " " + RollRotate + " " + "Y" + " " + YowRotate + " " + "P" + " " + PitchRotate;
                            }
                            flag = 1 - flag;
                            
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