using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketClient
{
    Thread receiveThread;
    Thread connectThread;
    UdpClient client;

    string localAddress = "0.0.0.0";
    string remoteAddress;
    int port = 5065;

    Action<string> onReceiveFunc;

    public SocketClient()
    {
        InitThread();
    }

    public SocketClient(string remote)
    {
        this.remoteAddress = remote;
        InitThread();
    }

    public SocketClient(string remoteAddress, int remotePort)
    {
        this.remoteAddress = remoteAddress;
        this.port = remotePort;
        InitThread();
    }

    private void InitThread()
    {
        receiveThread = new Thread(new ThreadStart(InitSocket));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void InitSocket()
    {
        client = new UdpClient(port);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

        if (remoteAddress != null)
        {
            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(remoteAddress), port);
            client.Connect(remoteIP);
        }

        while (client.Client != null)
        {
            try
            {
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                if (onReceiveFunc != null)
                {
                    onReceiveFunc(text);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    public void OnReceive(Action<string> func)
    {
        onReceiveFunc = func;
    }

    public void Send(string data)
    {
        if (client.Client != null)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                client.Send(bytes, bytes.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        else
        {
            Debug.Log("Socket not initialized yet");
        }
    }

    public void Close()
    {
        client.Close();
    }
}