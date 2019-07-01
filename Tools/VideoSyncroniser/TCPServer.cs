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

    private const int BUFFER_LEN = 256;

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

    private static byte[] GenerateByteMessage(string data)
    {
        byte[] tempByteArray = System.Text.Encoding.ASCII.GetBytes(data);
        byte[] rtn = new byte[BUFFER_LEN];
        tempByteArray.CopyTo(rtn, 0);

        return rtn;
    }

    private static string GetMessageFromNetworkStream(NetworkStream stream)
    {
        string rtn = "";
        int total_len = 0;

        while (total_len < BUFFER_LEN)
        {
            byte[] curBytes = new byte[BUFFER_LEN];
            total_len += (int)stream.Length;
            rtn += System.Text.Encoding.ASCII.GetString(curBytes);
            stream.Read(curBytes, 0, (int)stream.Length);
        }

        return rtn;
    }


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
        KeyValuePair<TCPServer, TcpClient> pair = (KeyValuePair<TCPServer, TcpClient>)res.AsyncState;
        TCPServer serv = pair.Key;
        TcpClient client = pair.Value;

        IPAddress address = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
        NetworkStream stream = client.GetStream();
        Debug.Log("Received Hello from " + address.ToString());

        byte[] bytes = GenerateByteMessage ("");
        stream.Write(bytes, 0, bytes.Length);

        string data = GetMessageFromNetworkStream(stream);
        string[] chunks = data.Split('|');
        VideoSynchroniser.Instance().CurrentTick = int.Parse(chunks[1]);

        client.Close();
    }

    private static void ReceiveKeepAliveMessage (IAsyncResult res)
    {
        KeyValuePair<TCPServer, TcpClient> pair = (KeyValuePair<TCPServer, TcpClient>)res.AsyncState;
        TCPServer serv = pair.Key;
        TcpClient client = pair.Value;

        IPAddress clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
        Debug.Log("Received KeepAlive from " + clientAddress.ToString());
        Debug.Log("KeepAlive Received from " + clientAddress.ToString());
        if (serv.IsAdjusting)
        {
            if (!serv.SecondaryActiveAddresses.Contains(clientAddress))
            {
                Debug.Log("Adding in SecondaryActiveAddresses");
                serv.SecondaryActiveAddresses.Add(clientAddress);
            }
        }
        else
        {
            if (!serv.ActiveAddresses.Contains(clientAddress))
            {
                Debug.Log("Adding in ActiveAddresses");
                serv.ActiveAddresses.Add(clientAddress);
            }
        }
    }

    private void SendHelloMessage()
    {
        foreach (IPAddress address in AllowedAddresses)
        {
            try
            {
                Debug.Log("Sending hello to " + address.ToString());
                TcpClient client = new TcpClient();
                client.BeginConnect(address.ToString(), Port, new AsyncCallback(TCPServer.ReceiveHelloMessage), new KeyValuePair <TCPServer, TcpClient>(this, client));
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

            String data = GetMessageFromNetworkStream(stream);
            Debug.Log(data);
            CMD cmd = (CMD)(data[0] - '0');
            Debug.Log("dddddddddd " + cmd);
            switch (cmd)
            {
                case CMD.Hello:
                    Debug.Log("Received Hello message from " + clientAddress.ToString());
                    data = string.Format("{0}|{1}", (int)CMD.Hello, VideoSynchroniser.Instance().CurrentTick);
                    byte[] bytes = GenerateByteMessage(data);
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
                    Debug.Log("Received KeepAlive message from " + clientAddress.ToString());
                    if (IsAdjusting)
                    {
                        if (!SecondaryActiveAddresses.Contains(clientAddress))
                        {
                            Debug.Log("Adding in SecondaryActiveAddresses");
                            SecondaryActiveAddresses.Add(clientAddress);
                        }
                    }
                    else
                    {
                        if (!ActiveAddresses.Contains(clientAddress))
                        {
                            Debug.Log("Adding in ActiveAddresses");
                            ActiveAddresses.Add(clientAddress);
                        }
                    }
                    break;
                case CMD.AdjustFrame:
                    Debug.Log("Received AdjustFrame message from: " + clientAddress.ToString());

                    string[] chunks = data.Split('|');
                    Received.Add(clientAddress, int.Parse(chunks[1]));
                    while (MyFrame == -1)
                    {
                        continue;
                    }
                    data = MyFrame.ToString();
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
            String data = string.Format("{0}|{1}", (int)CMD.KeepAlive, VideoSynchroniser.Instance().CurrentTick);
            byte[] bytes = GenerateByteMessage (data);
            stream.Write(bytes, 0, bytes.Length);

            client.Close();
        }
    }

    public void SendAdjustFrame()
    {
        IsAdjusting = true;
        foreach (IPAddress address in ActiveAddresses)
        {
            Debug.Log("Adjusting with IP " + address.ToString());
            if (Received.ContainsKey(address))
            {
                continue;
            }
            TcpClient client = new TcpClient(address.ToString(), Port);
            IPAddress clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            NetworkStream stream = client.GetStream();

            String data = string.Format("{0}|{1}", (int)CMD.AdjustFrame, VideoSynchroniser.Instance().CurrentTick);
            byte[] bytes = GenerateByteMessage(data);
            stream.Write(bytes, 0, bytes.Length);
            data = GetMessageFromNetworkStream(stream);
            Received.Add(clientAddress, int.Parse(data));
            AdjustedCnt++;

        }
        Debug.Log("AdjustCnt " + AdjustedCnt + " AllowedCnt " + ActiveAddresses.Count);
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