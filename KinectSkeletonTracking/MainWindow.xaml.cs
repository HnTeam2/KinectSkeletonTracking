using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        //int sendTime = 0, n = 0;
        private int sendTime = 0;

        /*static int nan = 0;

        enum rzeroJointId
        {
            ElbowRightJiku = 1,
            ShoulderRightR = 3,
            ShoulderRightP = 4,
            ShoulderLeftP = 5,
            ShoulderLeftR = 6,
            ElbowLeftJiku = 8,
            SpineMidY = 10,
            SpineMidP = 11
        }

        enum khrJointId
        {
            ElbowRightJiku = 5,
            ShoulderRightR = 6,
            ShoulderRightP = 7,
            ShoulderLeftP = 9,
            ShoulderLeftR = 10,
            ElbowLeftJiku = 11,
            SpineMidY = 8
        }*/
        /*private Dictionary<string, int> robozeroID = new Dictionary<string, int>
        {
            {"ElbowRight", 1},
            {"ElbowLeft", 8},
            {"ElbowLeft", nan},
            {"HipRight", nan},
            {"HipLeft", nan},
            {"ShoulderRightR", 3},
            {"ShoulderRightP", 4},
            {"ShoulderLeftP", 5},
            {"ShoulderLeftR", 6},
            {"KneeRight", nan},
            {"KneeLeft", nan},
            {"HandRight", nan},
            {"HandLeft", nan},
            {"SpineMidY", 10},
            {"SpineMidY", 11},
            {"AnkleRight", nan},
            {"AnkleLeft", nan}
        };
    */
        /*private Dictionary<string, int> khrID = new Dictionary<string, int>
        {
            {"ElbowRight", 5},
            {"ElbowLeft", 11},
            {"ElbowLeft", nan},
            {"HipRight", nan},
            {"HipLeft", nan},
            {"ShoulderRightR", 6},
            {"ShoulderRightP", 7},
            {"ShoulderLeftP", 9},
            {"ShoulderLeftR", 10},
            {"KneeRight", nan},
            {"KneeLeft", nan},
            {"HandRight", nan},
            {"HandLeft", nan},
            {"SpineMid", 8},
            {"AnkleRight", nan},
            {"AnkleLeft", nan}
        };*/


        //ポート番号
        public const int Port1 = 55555;
        public const int Port2 = 66666;
        Connection server1;
        Connection server2;

        private KHR3HV khr3Hv;
        private RoboZero roboZero;
        BodyFrameReader bodyFrameReader; //
        Body[] bodies=null; // Bodyを保持する配列；Kinectは最大6人トラッキングできる

        public MainWindow()
        {
            InitializeComponent();

            server1 = new Connection(Port1);
            //server2 = new Connection(Port2);

            khr3Hv = new KHR3HV(server1);
            //roboZero = new RoboZero(server2);
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
                Fill = brush
            };

            // カメラからDepth座標へ変換
            var point = kinect.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            // 関節の位置を点で描画
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
                if (sendTime % 3 == 1)
                {
                    khr3Hv.KHRByte(bodies[0]);
                    
                    if (bodies[1] != null)
                    {
                        roboZero.robozeroByte(bodies[1]);
                    }
                }

                // Bodyから取得した全関節でループする。
                foreach (var joint in body.Joints)
                {
                    // 追跡可能な状態か？
                    if (joint.Value.TrackingState == TrackingState.Tracked)
                    {
                        DrawEllipse(joint.Value, 10, Brushes.Blue);

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
                        foreach (var jointType in textBox_joint.Keys)
                        {
                            if (joint.Key == jointType)
                            {
                                //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                                var orientation = body.JointOrientations[joint.Key].Orientation;

                                //関節のそれぞれの軸に対応する角度を取得する

                                var pitchRotate = (int) CalcRotate.Pitch(orientation);
                                var yowRotate = (int) CalcRotate.Yaw(orientation);
                                var rollRotate = (int) CalcRotate.Roll(orientation);

                                int R = (int) rollRotate;
                                int Y = (int) yowRotate;
                                int P = (int) pitchRotate;

                                //Textで表示させるためにstring型へ変換
                                string RollRotate = R.ToString();
                                string YowRotate = Y.ToString();
                                string PitchRotate = P.ToString();

                                
                                
                                //DictionaryのKeyで値と一致
                                var Key = jointType;

                                //Keyから値を取得
                                TextBox textBox_num = textBox_joint[Key];
                                textBox_num.Text = joint.Key + "R" + " " + RollRotate + " " + "Y" + " " + YowRotate +
                                                   " " + "P" + " " + PitchRotate;
                            }

                            sendTime++;
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