using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class TCPServer
{
    enum CMD
    {
        Hello = 0,
        KeepAlive = 1,
        AdjustFrame = 2
    }

    private int Port;
    private IPAddress Address;
    private TcpListener server;
    private HashSet<IPAddress> AllowedAddresses;
    private HashSet<IPAddress> ActiveAddresses;
    private HashSet<IPAddress> SecondaryActiveAddresses;
    private Dictionary<IPAddress, Int32> Sent;
    private Dictionary<IPAddress, Int32> Received;
    int AdjustedCnt;
    bool IsAdjusting;
    System.Object LockObj;
    int MyFrame;



    public TCPServer(IPAddress address, int port, IPAddress[] allowedAddresses)
    {
        this.Address = address;
        this.Port = port;
        this.AllowedAddresses = new HashSet<IPAddress>();
        this.ActiveAddresses = new HashSet<IPAddress>();
        this.SecondaryActiveAddresses = new HashSet<IPAddress>();
        this.Sent = new Dictionary<IPAddress, int>();
        this.Received = new Dictionary<IPAddress, int>();
        this.IsAdjusting = false;
        this.LockObj = new System.Object();
        this.MyFrame = -1;

        foreach (IPAddress curAddress in allowedAddresses)
        {
            if (curAddress.Equals(address))
            {
                continue;
            }
            AllowedAddresses.Add(curAddress);
        }
        SendHelloMessage();
        Thread serverListenThread = new Thread(Listen);
        serverListenThread.Start();
    }

    private static void ReceiveHelloMessage (IAsyncResult res)
    {
        TcpClient client = (TcpClient)res.AsyncState;
        IPAddress address = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
        Debug.Log("Received from " + address.ToString());
        NetworkStream stream = client.GetStream();
        Byte[] bytes = System.Text.Encoding.ASCII.GetBytes("0");
        stream.Write(bytes, 0, bytes.Length);
        stream.Read(bytes, 0, bytes.Length);
        string data = System.Text.Encoding.ASCII.GetString(bytes);
        string[] chunks = data.Split('|');
        VideoSynchroniser.Instance().CurrentTick = int.Parse(chunks[1]);
        client.Close();
    }

    private void SendHelloMessage()
    {
        foreach (IPAddress address in AllowedAddresses)
        {
            try
            {
                Debug.Log("Sending hello to " + address.ToString());
                TcpClient client = new TcpClient();
                client.BeginConnect(address.ToString(), Port, new AsyncCallback(TCPServer.ReceiveHelloMessage), client);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }
    }

    private void Listen()
    {
        this.server = new TcpListener(Address, Port);
        this.server.Start();
        Debug.Log("Server Started");
        while (true)
        {
            Debug.Log("Waiting for connection");
            TcpClient client = server.AcceptTcpClient();
            Debug.Log("Connected!");
            IPAddress clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            if (!AllowedAddresses.Contains(clientAddress))
            {
                continue;
            }
            NetworkStream stream = client.GetStream();
            Byte[] bytes = new Byte[256];
            int i = stream.Read(bytes, 0, bytes.Length);
            String data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            Debug.Log(("Got Data: {0}", data));
            CMD cmd = (CMD)(data[0] - '0');
            switch (cmd)
            {
                case CMD.Hello:
                    data = string.Format("{0}|{1}", CMD.Hello, VideoSynchroniser.Instance().CurrentTick);
                    bytes = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(bytes, 0, bytes.Length);
                    if (IsAdjusting)
                    {
                        SecondaryActiveAddresses.Add(clientAddress);
                    }
                    else
                    {
                        ActiveAddresses.Add(clientAddress);
                    }
                    break;
                case CMD.KeepAlive:
                    Debug.Log("KeepAlive Received from " + clientAddress.ToString());
                    if (IsAdjusting && !ActiveAddresses.Contains(clientAddress))
                    {
                        if (!SecondaryActiveAddresses.Contains(clientAddress))
                        {
                            SecondaryActiveAddresses.Add(clientAddress);
                        }
                    }
                    else
                    {
                        ActiveAddresses.Add(clientAddress);
                    }
                    break;
                case CMD.AdjustFrame:
                    string[] chunks = data.Split('|');
                    int curFrame = int.Parse(chunks[1]);
                    Received.Add(clientAddress, curFrame);
                    while (MyFrame == -1)
                    {
                        continue;
                    }
                    data = string.Format("{0}|{1}", CMD.AdjustFrame, MyFrame);
                    bytes = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(bytes, 0, bytes.Length);
                    client.Close();
                    AdjustedCnt++;
                    break;
            }
            client.Close();
        }
    }

    public void SendKeepAlive()
    {
        try
        {
            Monitor.PulseAll(LockObj);
            Monitor.Exit(LockObj);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        foreach (IPAddress address in ActiveAddresses)
        {
            Debug.Log("Sending Keepalive to IP " + address.ToString());
            TcpClient client = new TcpClient(address.ToString(), Port);
            NetworkStream stream = client.GetStream();
            String data = string.Format("{0}|{1}", CMD.KeepAlive, VideoSynchroniser.Instance().CurrentTick);
            Byte[] bytes = new Byte[256];
            bytes = System.Text.Encoding.ASCII.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            AdjustedCnt++;
            client.Close();
        }
    }

    public void SendAdjustFrame()
    {
        IsAdjusting = true;
        foreach (IPAddress address in ActiveAddresses)
        {
            if (Received.ContainsKey(address))
            {
                continue;
            }
            TcpClient client = new TcpClient(address.ToString(), Port);
            NetworkStream stream = client.GetStream();
            String data = string.Format("{0}|{1}", CMD.AdjustFrame, VideoSynchroniser.Instance().CurrentTick);
            Byte[] bytes = new Byte[256];
            bytes = System.Text.Encoding.ASCII.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            stream.Read(bytes, 0, bytes.Length);

        }
        Monitor.Enter(LockObj);
        Int32 minFrame = Int32.MaxValue;
        if (AdjustedCnt == ActiveAddresses.Count)
        {
            Debug.Log("Adjsting!!!");
            foreach (KeyValuePair<IPAddress, Int32> item in Sent)
            {
                minFrame = Math.Min(minFrame, item.Value);
            }
            foreach (KeyValuePair<IPAddress, Int32> item in Received)
            {
                minFrame = Math.Min(minFrame, item.Value);
            }
            if (minFrame != Int32.MaxValue)
            {
                VideoSynchroniser.Instance().AdjustedFrame = minFrame;
            }
        }
        Sent.Clear();
        Received.Clear();
        AdjustedCnt = 0;
        MyFrame = -1;
        IsAdjusting = false;
        ActiveAddresses.Clear();
        foreach (IPAddress el in SecondaryActiveAddresses)
        {
            ActiveAddresses.Add(el);
        }
    }
}