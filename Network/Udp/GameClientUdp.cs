using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using UniRx;

public class GameUdpClient
{
    public Action<byte[]> OnReceiveMessage;
    public Action OnUpdateSend;

    UdpClient client;
    int tickRate = 30;
    Thread receiveThread;
    Thread sendThread;
    bool forceAbort = false;

    //OnReceiveMessage：メッセージ受信時に呼び出すイベント
    //OnUpdateSend：一定時間ごとに呼び出すイベント
    //tickRate：一秒間に何回上のOnUpdateSendを実行するか

    /// <summary>
    /// クライアント起動時に必ず呼び出す。
    /// </summary>
    /// <param name="clientEP">クライアント側のポート</param>
    /// <param name="hostEP">サーバー側のIPとポート</param>
    public void StartClient(IPEndPoint clientEP, IPEndPoint hostEP)
    {
        ConnectToServer(clientEP, hostEP);
        return;
    }

    /// <summary>
    /// contentをサーバーに送る
    /// </summary>
    /// <param name="content">メッセージ内容</param>
    public void Send(byte[] content)
    {
        client.SendAsync(content, content.Length);
    }

    void ConnectToServer(IPEndPoint clientEP, IPEndPoint hostEP)
    {

        //クライアントポートから受信、このポートに接続する
        try
        {

            client = new UdpClient(clientEP);
            client.Connect(hostEP);
            //接続用ポートに送信
            client.Send(UDPMessage.Ack.ToByte(), 4);

            IPEndPoint serverEP = null;
            byte[] welcomeMessage = client.Receive(ref serverEP);
            Parse(welcomeMessage);
            //print("登録完了。サーバー情報: " serverEP.ToString());
            receiveThread = new Thread(new ThreadStart(ThreadReceive));
            receiveThread.Start();

            //print("同期を開始します");

            sendThread = new Thread(new ThreadStart(ThreadSend));
            sendThread.Start();
            //StartCoroutine(SendPlayerState());
        }
        catch (SocketException e)
        {
            Debug.LogWarning(e.Message);
            TitleManager.isError = true;
            TitleManager.errorMessage = "サーバーに接続できません";
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
            return;
        }
    }

    void Parse(byte[] msg)
    {
        OnReceiveMessage(msg);
    }

    void ThreadReceive()
    {
        while (true && !forceAbort)
        {
            IPEndPoint serverEP = null;
            byte[] receivedBytes = client.Receive(ref serverEP);
            Parse(receivedBytes);
        }
    }
    void ThreadSend()
    {
        while (true && !forceAbort)
        {
            OnUpdateSend();
            Thread.Sleep(1000 / tickRate);
        }
    }

    /// <summary>
    /// OnApplicationQuitで必ず呼び出す。
    /// </summary>
    public void ShutDownClient()
    {
        sendThread.Abort();
        receiveThread.Abort();
        forceAbort = true;
        client.Close();
    }

}

static class Ext
{
    public static UDPMessage ToMessageType(this byte[] bytes, int startIndex = 0)
    {
        return (UDPMessage)Enum.ToObject(typeof(UDPMessage), BitConverter.ToInt32(bytes, startIndex));
    }

    public static byte[] ToByte(this UDPMessage msg)
    {
        return BitConverter.GetBytes((int)msg);
    }

    public static byte[] ToByte(this Quaternion q)
    {
        return BitConverter.GetBytes(q.x).Concat(BitConverter.GetBytes(q.y)).Concat(BitConverter.GetBytes(q.z)).Concat(BitConverter.GetBytes(q.w)).ToArray();
    }
    public static Quaternion ToQuaternion(this byte[] bytes, int startIndex = 0)
    {
        float x = BitConverter.ToSingle(bytes, startIndex);
        float y = BitConverter.ToSingle(bytes, startIndex + 4);
        float z = BitConverter.ToSingle(bytes, startIndex + 8);
        float w = BitConverter.ToSingle(bytes, startIndex + 12);
        return new Quaternion(x, y, z, w);
    }

    public static void SetRotationY(this Transform t, float y)
    {
        Vector3 v = t.localEulerAngles;
        t.localEulerAngles = new Vector3(v.x, y, v.z);
        return;
    }

    public static byte[] ToByte(this Vector3 vec)
    {
        byte[] xbyte = BitConverter.GetBytes(vec.x);
        byte[] ybyte = BitConverter.GetBytes(vec.y);
        byte[] zbyte = BitConverter.GetBytes(vec.z);
        byte[] pos_byte = new byte[xbyte.Length + ybyte.Length + zbyte.Length];
        for (int i = 0; i < xbyte.Length; i++)
        {
            pos_byte[i] = xbyte[i];
            pos_byte[i + xbyte.Length] = ybyte[i];
            pos_byte[i + xbyte.Length * 2] = zbyte[i];
        }
        return pos_byte;
    }

    public static Vector3 ToVector3(this Byte[] bytes, int startIndex = 0)
    {
        float x = BitConverter.ToSingle(bytes, startIndex);
        float y = BitConverter.ToSingle(bytes, startIndex + 4);
        float z = BitConverter.ToSingle(bytes, startIndex + 8);
        return new Vector3(x, y, z);
    }

    public static int IndexOfEP(this List<IPEndPoint> eplist, IPEndPoint ep)
    {
        for (int i = 0; i < eplist.Count; i++)
        {
            if (eplist[i].ToString() == ep.ToString())
            {
                return i;
            }
        }
        return -1;
    }
}
