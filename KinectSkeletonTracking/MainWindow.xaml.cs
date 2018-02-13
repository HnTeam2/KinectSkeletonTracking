using System;
using System.Linq;
using System.Windows;
using Microsoft.Kinect;
using System.Diagnostics;

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

        private  void socket()
        {
            //サーバーに送信するデータを入力してもらう→何もなければ終了。
            Console.WriteLine("データを入力後Enterを押してください。");
            string sendMsg = Console.ReadLine();
            if (sendMsg == null || sendMsg.Length == 0)
            {
                return;
            }

            //サーバーのIPアドレスとポート番号？
            string ipOrHost = "172.20.10.5";//←ここ ローカルは”127.0.0.1”
            int port = 9999;

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
            // ns.ReadTimeout = 30000;
            //ns.WriteTimeout = 30000;

            //サーバに　データを送信する,バイト型？
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');

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
            ns.Close();
            tcp.Close();
            Console.WriteLine("切断しました。");

            Console.ReadLine();
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

        private void SendRotate()
        {
            // 追跡しているBodyのみループする
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                // Bodyから取得した全関節でループする。
                foreach (var joint in body.Joints)
                {
                    // 追跡可能な状態か？
                    if (joint.Value.TrackingState == TrackingState.Tracked)
                    {
                        // デバックのため右肘のピッチ軸を出力してみる
                        if (joint.Key == JointType.ElbowRight)
                        {
                            //関節の向きを取得する（Vector4型）。関節の指定にはJoyntType(enum)を使用する。
                            var orientation = body.JointOrientations[joint.Key].Orientation;

                            //関節のそれぞれの軸に対応する角度を取得する
                            //var pitchRotate = CalcRotate.Pitch(orientation);
                            //var yowRotate = CalcRotate.Yaw(orientation);
                            var rollRotate = CalcRotate.Roll(orientation);
                        
                            // TODO:↑の角度の値から必要なものをソケット通信で送信する
                            Debug.WriteLine(((int)rollRotate).ToString());
                        }
                    }
                }
            }
        }
    }
}