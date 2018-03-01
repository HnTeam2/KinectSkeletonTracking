using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace KinectSkeletonTracking
{
   class Connection
    {
        public const string IpString = "192.168.43.181"; //PCのIPアドレスにする
        private IPAddress ipAdd;
        private TcpListener listener;
        private NetworkStream ns;
        private MemoryStream ms;
        private TcpClient client;
        private Encoding enc;
        private TcpClient tcp;

        //-----//サーバ側の処理//-----//
        public Connection(int port)
        {
            ipAdd = IPAddress.Parse(IpString);
            listener = new TcpListener(ipAdd, port);
            //受信開始
            listener.Start();
            Console.WriteLine("Listenを開始しました({0}:{1})。",
                ((IPEndPoint) listener.LocalEndpoint).Address,
                ((IPEndPoint) listener.LocalEndpoint).Port);
            //接続植え付け
            client = listener.AcceptTcpClient();
            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                ((IPEndPoint) client.Client.RemoteEndPoint).Address,
                ((IPEndPoint) client.Client.RemoteEndPoint).Port);
            //NetworkStream（ネットワークを読み書きの対象とする時のストリーム）うまくかけん)を取得
            ns = client.GetStream();
        }


        //-----//接続相手にデータを送信する//-----//
        public void sendBytes(byte[] sendMsg)
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
}