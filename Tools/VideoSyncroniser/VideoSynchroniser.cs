using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using UnityEngine.Video;

public sealed class VideoSynchroniser
{
    private int Port = -1;
    private IPAddress[] NodeList = null;
    private int TickDelay { get; set; }
    private int TickTillConverge { get; set; }
    public int CurrentTick { get; set; }
    private static VideoSynchroniser instance;
    private TCPServer Server;
    public int AdjustedFrame { get; set; }

    private void ReadConfiguration (string ConfigFile)
    {
        try
        {
            string[] configRawLines = File.ReadAllLines(ConfigFile);

            Port = int.Parse(configRawLines[0]);
            NodeList = new IPAddress[configRawLines.Length-1];
            for (int i = 1; i < configRawLines.Length; i ++)
            {
                NodeList[i - 1] = IPAddress.Parse (configRawLines[i]);
            }

        }
        catch (IOException e)
        {
            Debug.Log(e.Message);
        }
    }

    public static void InitializeSingleton (string ConfigFile, int TickDelay, int TicksTillConverge, VideoPlayer VPlayer, string ServerIPAddress)
    {
        if (instance == null)
        {
            VideoSynchroniser.instance = new VideoSynchroniser(ConfigFile, TickDelay, TicksTillConverge, VPlayer, ServerIPAddress);
        }
    }

    public static VideoSynchroniser Instance ()
    {
        return instance;
    }

    private VideoSynchroniser (string ConfigFile, int TickDelay, int TicksTillConverge, VideoPlayer VPlayer, string ServerIPAddress)
    {
        this.TickDelay = TickDelay;
        this.TickTillConverge = TicksTillConverge;
        this.CurrentTick = 0;
        this.AdjustedFrame = -1;
        ReadConfiguration(ConfigFile);
        this.Server = new TCPServer(IPAddress.Parse(ServerIPAddress), Port, NodeList);
    }

    public int IncreaseCurrentTick(int curFrame)
    {
        CurrentTick++;
        Thread thread;
        if (CurrentTick % TickTillConverge == 0)
        {
            Debug.Log("Adjust");
            thread = new Thread(Server.SendAdjustFrame);
        } else
        {
            Debug.Log("KeepAlive");
            thread = new Thread(Server.SendKeepAlive);
        }
        thread.Start();
        int rtn = AdjustedFrame;
        AdjustedFrame = -1;
        return rtn;
    }

}
