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

        BodyFrameReader bodyFrameReader; //
        Body[] bodies; // Bodyを保持する配列；Kinectは最大6人トラッキングできる

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
                        // デバックのため右肘のピッチ軸を出力してみる
                        if (joint.Key == JointType.ElbowRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str = rollRotate.ToString();
                            //表示
                            CanvasElbowright.Text = str;
                            
                        
                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.ElbowLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str1 = rollRotate.ToString();
                            //表示
                            CanvasElbowleft.Text = str1;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.HipRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str2 = rollRotate.ToString();
                            //表示
                            CanvasHipright.Text = str2;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }
                        if (joint.Key == JointType.HipLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str3 = rollRotate.ToString();
                            //表示
                            CanvasHipleft.Text = str3;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.ShoulderRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str4 = rollRotate.ToString();
                            //表示
                            CanvasShourderright.Text = str4;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.ShoulderLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str5 = rollRotate.ToString();
                            //表示
                            CanvasShourderleft.Text = str5;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.KneeRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str6 = rollRotate.ToString();
                            //表示
                            CanvasKneeright.Text = str6;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.KneeLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str7 = rollRotate.ToString();
                            //表示
                            CanvasKneeleft.Text = str7;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.FootRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str8 = rollRotate.ToString();
                            //表示
                            CanvasFootright.Text = str8;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.FootLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str9 = rollRotate.ToString();
                            //表示
                            CanvasFootleft.Text = str9;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.SpineMid)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str10 = rollRotate.ToString();
                            //表示
                            CanvasSpinbase.Text = str10;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.AnkleRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str11 = rollRotate.ToString();
                            //表示
                            CanvasAncleright.Text = str11;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }

                        if (joint.Key == JointType.AnkleLeft)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            var pitchRotate = CalcRotate.Pitch(orientation);
                            var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = (int)CalcRotate.Roll(orientation);


                            //Textで表示させるためにstring型へ変換
                            string str12 = rollRotate.ToString();
                            //表示
                            CanvasAncleleft.Text = str12;


                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }
                    }


                    else if (joint.Value.TrackingState == TrackingState.Inferred)
                    {
                        DrawEllipse(joint.Value, 10, Brushes.Yellow);
                    }
                }
            }
        }
    }
}