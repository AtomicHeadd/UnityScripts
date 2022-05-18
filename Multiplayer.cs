using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Linq;
//using ParrelSync;

public class Multiplayer : MonoBehaviour
{
    [SerializeField] int myPort;
    [SerializeField] int opponentPort;
    [SerializeField] bool enableSmoothing = false;
    [SerializeField] Transform player;
    [SerializeField] List<GameObject> otherPlayerObjects;

    //UDP通信系
    int sendPerSecond = 20;
    UdpClient client;
    Thread receiveThread;
    Thread sendThread;
    bool isSendTiming = false;
    List<IPEndPoint> ackWaiting = new List<IPEndPoint>(4);
    List<IPEndPoint> connectedPlayerEPs = new List<IPEndPoint>(4);
    List<ReceivedUnit> messageStack = new List<ReceivedUnit>(15);
    //ゲーム情報
    List<PositionAndRotation> otherPlayerInfo = new List<PositionAndRotation>(3);

    void Start()
    {
        client = new UdpClient(new IPEndPoint(IPAddress.Any, myPort));
        receiveThread = new Thread(new ThreadStart(ThreadReceive));
        receiveThread.Start();
    }

    public void RegisterOpponentPort(string IP = "127.0.0.1", int port = 0)
    {
        port = opponentPort;
        byte[] message = UDPMessage.Ack.ToByte();
        IPEndPoint opponentEP = new IPEndPoint(IPAddress.Parse(IP), port);
        client.Send(message, message.Length, opponentEP);
        ackWaiting.Add(opponentEP);
        print($"IP: {IP}:{port} に接続要求");
    }
    [ContextMenu("Register")]
    public void OnClickRegister()
    {
        RegisterOpponentPort(port: opponentPort);
    }

    /// <summary>
    /// 受信用のスレッド。受信した際に情報をスタックに保存しておく。
    /// </summary>
    void ThreadReceive()
    {
        while (true)
        {
            IPEndPoint senderEP = null;
            byte[] receivedBytes = client.Receive(ref senderEP);

            Debug.Log($"受け取ったメッセージ長: {receivedBytes.Length}");
            messageStack.Add(new ReceivedUnit(senderEP, receivedBytes));
        }
    }
    /// <summary>
    /// 送信用のスレッド。送信タイミングでisSendTimingをtrueにする。
    /// </summary>
    void ThreadSend()
    {
        while (true)
        {
            //OnUpdateSend();
            isSendTiming = true;
            Thread.Sleep(1000 / sendPerSecond);
        }
    }
    /// <summary>
    /// 受信したメッセージの内容によって処理を行う
    /// </summary>
    /// <param name="unit">受信情報</param>
    void Parse(ReceivedUnit unit)
    {
        //なるべくメッセージごと１関数で実装する
        UDPMessage type = unit.message.ToUDPMessage();
        int ackRegisteredIndex = ackWaiting.IndexOfPort(unit.senderEP.Port);
        int connectedIndex = connectedPlayerEPs.IndexOfPort(unit.senderEP.Port);
        print("メッセージを受信");
        switch (type)
        {
            case UDPMessage.Ack:
                {
                    print(ackRegisteredIndex);
                    if (ackRegisteredIndex == -1) break;
                    connectedPlayerEPs.Add(unit.senderEP);
                    ackWaiting.RemoveAt(ackRegisteredIndex);
                    if (connectedPlayerEPs.Count == 1)
                    {
                        sendThread = new Thread(new ThreadStart(ThreadSend));
                        sendThread.Start();
                    }
                    print("他の人から接続がありました");
                    byte[] message = UDPMessage.AckComplete.ToByte();
                    client.SendAsync(message, message.Length, unit.senderEP);
                    otherPlayerObjects[connectedPlayerEPs.Count - 1].SetActive(true);
                    break;
                }
            case UDPMessage.AckComplete:
                {
                    print(ackRegisteredIndex);
                    if (ackRegisteredIndex == -1) break;
                    connectedPlayerEPs.Add(unit.senderEP);
                    ackWaiting.RemoveAt(ackRegisteredIndex);
                    if (connectedPlayerEPs.Count == 1)
                    {
                        sendThread = new Thread(new ThreadStart(ThreadSend));
                        sendThread.Start();
                    }
                    print("他の人から接続がありました");
                    otherPlayerObjects[connectedPlayerEPs.Count - 1].SetActive(true);
                    break;
                }
            case UDPMessage.PosUpdate:
                {
                    Vector3 pos = unit.message.ToVector3(4);
                    float Yrot = BitConverter.ToSingle(unit.message, 16);
                    if (otherPlayerInfo.Count <= connectedIndex)
                    {
                        //プレイヤーのSmoothedMovementがない場合
                        PositionAndRotation newPlayer = new PositionAndRotation(sendPerSecond);
                        newPlayer.UpdateInformation(pos, Yrot);
                        otherPlayerInfo.Add(newPlayer);
                    }
                    else
                    {
                        otherPlayerInfo[connectedIndex].UpdateInformation(pos, Yrot);
                    }
                    break;
                }
            default:
                {
                    print("malformed packet!!!!!!!!!");
                    break;
                }
        }
    }
    /// <summary>
    /// 通信相手全員に自分の状態を送る
    /// </summary>
    void BroadcastStatus()
    {
        //変化がない場合送らない
        Vector3 posThisFlame = player.position;
        float YthisFlame = player.eulerAngles.y;

        //データを作る
        byte[] message = UDPMessage.PosUpdate.ToByte();
        message = message.Concat(posThisFlame.ToByte()).Concat(BitConverter.GetBytes(YthisFlame)).ToArray();
        for (int i = 0; i < connectedPlayerEPs.Count; i++)
        {
            client.SendAsync(message, message.Length, connectedPlayerEPs[i]);
        }
    }

    private void Update()
    {
        //送信タイミング
        if (isSendTiming)
        {
            BroadcastStatus();
            isSendTiming = false;
        }

        //受信メッセージがある場合
        for (int i = 0; i < messageStack.Count; i++)
        {
            Parse(messageStack[i]);
            messageStack.RemoveAt(i);
            i--;
        }

        //各プレイヤーの位置アップデート
        for (int i = 0; i < connectedPlayerEPs.Count; i++)
        {
            if (i >= otherPlayerInfo.Count) return;
            if (enableSmoothing)
            {
                otherPlayerInfo[i].UpdateTime();
                otherPlayerObjects[i].transform.position = otherPlayerInfo[i].GetLerpPosition();
                otherPlayerObjects[i].transform.eulerAngles = new Vector3(0, otherPlayerInfo[i].GetLerpRotation(), 0);
            }
            else
            {
                otherPlayerObjects[i].transform.position = otherPlayerInfo[i].GetPosition();
                otherPlayerObjects[i].transform.eulerAngles = new Vector3(0, otherPlayerInfo[i].GetRotation(), 0);
            }
        }
    }

    /// <summary>
    /// 受信メッセージの情報を保存
    /// </summary>
    class ReceivedUnit
    {
        public IPEndPoint senderEP;
        public byte[] message;

        public ReceivedUnit(IPEndPoint Ep, byte[] Message)
        {
            senderEP = Ep;
            message = Message;
        }
    }

    /// <summary>
    /// スレッド〇す用
    /// これがないと連続してプレイ時にポートのバインドが解除されていない
    /// </summary>
    private void OnApplicationQuit()
    {
        if (sendThread != null) sendThread.Abort();
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
    /// <summary>
    /// オンラインでGameObjectを特定する一つの方法
    /// タグで候補を特定し、posでオブジェクトを一つに特定する
    /// </summary>
    /// <param name="tag">特定するオブジェクトのタグ</param>
    /// <param name="pos">特定するオブジェクトの位置</param>
    /// <param name="threshold">同じだとする位置の閾値、よほどのことがない限り0だと思うが</param>
    /// <returns></returns>
    public GameObject GetGameObjectWithTagAndPostion(string tag, Vector3 pos, float threshold = .5f)
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject i in candidates)
        {
            if ((i.transform.position - pos).magnitude < threshold) return (i);
        }
        return null;
    }
    /// <summary>
    /// 受信する間の位置・回転を補完するためのクラス
    /// 例えば同期レートが1秒に15回の場合60fpsでは4フレームの間止まって瞬間移動を繰り返すように見える。
    /// したがって同期レートの2つの値を4分割すると4フレームの間も移動しているように見せることができる。
    /// </summary>
    class PositionAndRotation
    {
        public List<Vector3> position;
        public List<float> rot;
        public float timeFromLastInformation;
        float timeToNextInformation;

        public PositionAndRotation(int tickPerSecond)
        {
            position = new List<Vector3>();
            rot = new List<float>();
            timeFromLastInformation = 0;
            timeToNextInformation = 1f / tickPerSecond;
        }
        /// <summary>
        /// 補完する情報を更新する
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="Yrot"></param>
        public void UpdateInformation(Vector3 pos, float Yrot)
        {
            timeFromLastInformation = 0;
            if (position.Count == 2) position.RemoveAt(0);
            position.Add(pos);
            if (rot.Count == 0) rot.Add(Yrot);
            else
            {
                //1,2 の場合は閾値を超えていたら更新、それ以外は前のパラメーター複製
                float lastRot = rot[rot.Count - 1];
                if (Mathf.Abs(Yrot - lastRot) < 45) rot.Add(lastRot);
                else rot.Add(Yrot);
            }
            if (rot.Count == 3) rot.RemoveAt(0);
        }
        /// <summary>
        /// 時間を更新する、これにより補完のタイミングが進む
        /// </summary>
        public void UpdateTime()
        {
            timeFromLastInformation += Time.deltaTime;
        }
        /// <summary>
        /// 補完された位置を変えす
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLerpPosition()
        {
            if (position.Count == 0) return Vector3.zero;
            else if (position.Count == 1) return position[0];
            return LerpPos(position[0], position[1], timeFromLastInformation / timeToNextInformation);
        }
        public Vector3 GetPosition()
        {
            if (position.Count == 0) return Vector3.zero;
            else if (position.Count == 1) return position[0];
            return position[1];
        }
        /// <summary>
        /// 補完された回転を返す
        /// </summary>
        /// <returns></returns>
        public float GetLerpRotation()
        {
            if (rot.Count == 0) return 0;
            else if (rot.Count == 1) return rot[0];
            return Mathf.LerpAngle(rot[0], rot[1], timeFromLastInformation / timeToNextInformation);
        }
        public float GetRotation()
        {
            if (rot.Count == 0) return 0;
            else if (rot.Count == 1) return rot[0];
            return rot[1];
        }
        /// <summary>
        /// Vector3を線形補完する
        /// </summary>
        /// <param name="from"></param>
        /// <param name="end"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        Vector3 LerpPos(Vector3 from, Vector3 end, float a)
        {
            float x = Mathf.Lerp(from.x, end.x, a);
            float z = Mathf.Lerp(from.z, end.z, a);
            float y = Mathf.Lerp(from.y, end.y, a);
            return new Vector3(x, y, z);
        }
    }

    
}


enum UDPMessage
{
    Ack = 100001,
    AckComplete,
    PosUpdate,
    AnimUpdate,
    RightDoorOpen,
    LeftDoorOpen,
}

static class MultiPlayerExt
{
    public static byte[] ToByte(this UDPMessage udpm)
    {
        return BitConverter.GetBytes((int)udpm);
    }

    public static UDPMessage ToUDPMessage(this byte[] b, int startIndex = 0)
    {
        int number = BitConverter.ToInt32(b, startIndex);
        return (UDPMessage)Enum.ToObject(typeof(UDPMessage), number);
    }
    public static byte[] ToByte(this Vector3 v)
    {
        byte[] x = BitConverter.GetBytes(v.x);
        byte[] y = BitConverter.GetBytes(v.y);
        byte[] z = BitConverter.GetBytes(v.z);
        return x.Concat(y).Concat(z).ToArray();
    }

    public static Vector3 ToVector3(this byte[] b, int startIndex)
    {
        float x = BitConverter.ToSingle(b, startIndex);
        float y = BitConverter.ToSingle(b, startIndex + 4);
        float z = BitConverter.ToSingle(b, startIndex + 8);
        return new Vector3(x, y, z);
    }
    public static int IndexOfPort(this List<IPEndPoint> eps, int targetPort)
    {
        int index = -1;
        for (int i = 0; i < eps.Count; i++)
        {
            if (eps[i].Port == targetPort) index = i;
        }
        return index;
    }
    
}
