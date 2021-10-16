using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

public class GameServerUdp
{
    public bool enableTimeOut = true;
    public int timeOutSecond = 30;
    public bool denyAck = false;

    public Action<byte[], int, IPEndPoint> OnReceiveMessage;
    public Action OnUpdate;

    UdpClient udpClient;
    List<IPEndPoint> users = new List<IPEndPoint>();//接続済み
    List<Vector3> pos = new List<Vector3>();
    int tickRate = 30;
    Thread receiveThread;
    Thread sendThread;
    List<bool> aliveSign = new List<bool>();
    int timeOutCounter = 0;
    float serverIdlingTime = 0;

    //UdpMessage.Ackを送信してきたクライアントを接続する
    
    //enableTimeOut：タイムアウトの有無
    //TimeOutSecond：タイムアウトする時間
    //denyAck：Ackを受信しても接続させない。ユーザー数を制限したい場合に利用
    
    //BroadCast(byte[] b) bを接続済みの全クライアントに送信
    //Send(byte[] b, int i) i番目のクライアントにbを送信
    //StartSever(IPEndPoint ep) epのポートでサーバーを起動
    //Reset() サーバー内部情報をリセット
    //ShutDownServer() サーバーをシャットダウンする。サーバー側のOnApplicationQuitで必ず呼び出す。

    public void Broadcast(byte[] message)
    {
        for (int i = 0; i < users.Count; i++)
        {
            udpClient.SendAsync(message, message.Length, users[i]);
        }
    }

    public void Send(byte[] message, int clientIndex)
    {
        udpClient.SendAsync(message, message.Length, users[clientIndex]);
    }


    public void StartServer(IPEndPoint receiveEP)
    {
        try
        {
            udpClient = new UdpClient(receiveEP);

            receiveThread = new Thread(new ThreadStart(ThreadReceive));
            receiveThread.Start();
            sendThread = new Thread(new ThreadStart(ThreadSend));
            sendThread.Start();
            Debug.Log("サーバー起動成功");
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message);
            Debug.LogError("サーバー起動失敗");

        }
        //StartCoroutine(BroadCastGameState());
    }

    public void Reset()
    {
        users = new List<IPEndPoint>();
        pos = new List<Vector3>();
        aliveSign = new List<bool>();
    }

    void ThreadReceive()
    {
        while (true && !forceAbort)
        {
            IPEndPoint senderEP = null;
            byte[] receivedBytes = udpClient.Receive(ref senderEP);
            Parse(senderEP, receivedBytes);
        }
    }

    void Parse(IPEndPoint senderEP, byte[] message)
    {
        int clientIndex = users.IndexOfEP(senderEP);
        UDPMessage type = message.ToMessageType();
        switch (type)
        {
            case UDPMessage.Ack:
                {
                    byte[] content;
                    if (denyAck)
                    {
                        content = UDPMessage.AckDenied.ToByte();
                    }
                    else
                    {
                        content = UDPMessage.Ack.ToByte();
                        content = content.Concat(BitConverter.GetBytes(users.Count)).ToArray();
                        users.Add(senderEP);
                        aliveSign.Add(true);
                    }
                    udpClient.SendAsync(content, content.Length, senderEP);
                    break;
                }
            case UDPMessage.Echo:
                {
                    byte[] ret_type = UDPMessage.Echo.ToByte();
                    udpClient.SendAsync(ret_type, ret_type.Length, senderEP);
                    return;
                }
        }

        OnReceiveMessage(message, clientIndex, senderEP);

        serverIdlingTime = 0;
        if (clientIndex != -1) aliveSign[clientIndex] = true;
    }

    async void ThreadSend()
    {
        while (true && !forceAbort)
        {
            await Task.Delay(1000 / tickRate);
            if (users.Count < 2)
            {
                continue;
            }
            OnUpdate();
            timeOutCounter++;

            if (enableTimeOut && timeOutCounter >= timeOutSecond * tickRate)
            {
                ClientTimingOut();
                timeOutCounter = 0;
            }
        }
    }
    
    void ClientTimingOut()
    {
        List<int> removedUsers = new List<int>();
        int j = 0;
        for (int i = 0; i < users.Count; i++)
        {
            if (aliveSign[i]) continue;
            byte[] type = UDPMessage.TimedOut.ToByte();
            udpClient.SendAsync(type, type.Length, users[i]);
            users.RemoveAt(i);
            aliveSign.RemoveAt(i);
            pos.RemoveAt(i);
            i--;
            removedUsers.Add(j);
            j++;
        }
        for (int i = 0; i < aliveSign.Count; i++)
        {
            aliveSign[i] = false;
        }
        for (int k = 0; k < removedUsers.Count; k++)
        {
            byte[] content = UDPMessage.UserRemove.ToByte();
            content.Concat(BitConverter.GetBytes(removedUsers[k])).ToArray();
            Broadcast(content);
        }
    }

    public void ShutDownServer()
    {
        sendThread.Abort();
        receiveThread.Abort();
        forceAbort = true;
        udpClient.Close();
    }
}

public enum UDPMessage
{
    Ack = 10001,
    AckDenied,
}


/* 0 ask (4) 4bytes
 * 
 * 0 ask (4) 
 * 4 number (4) 8bytes
 */
