using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System;
public class HTCReceiver : MonoBehaviour {

    public class HTCData
    {
        public Vector3 trackedObjPos;
        public Vector3 controllerPos;
        public Quaternion controllerRot;
        public float triggerState;
        public float trackpadState;
         
        public long t;

        public HTCData()
        {

        }

        public HTCData(Vector3 trackedObjPos, Vector3 controllerPos, Quaternion controllerRot, float triggerState, float trackpadState, long t)
        {
            this.trackedObjPos = trackedObjPos;
            this.controllerPos = controllerPos;
            this.controllerRot = controllerRot;
            this.triggerState = triggerState;
            this.trackpadState = trackpadState;
            this.t = t;
        }
    }

    SocketServer socketServer; 

    public int Port;
    
    Vector3 playerPos=new Vector3(-1,-1,-1);
    void Start () {
      
        var mainThread = SynchronizationContext.Current;
        socketServer = new SocketServer("0.0.0.0", Port);
        socketServer.Listen((data) =>
        {
            mainThread.Post((s) =>
            {
                getData(data);
            }, null);
        });
    }
    
    public string Message;
    public float scale = 10;

    private float lastTrackPadState;
    void getData(string message)
    {
        Message = message; 
        HTCData data = JsonUtility.FromJson<HTCData>(message);
        
       if (data.trackedObjPos==-Vector3.one)
            TrackingManager.instance.setHtcTrackingLocation( data.trackedObjPos);
       else
            TrackingManager.instance.setHtcTrackingLocation(scale*  data.trackedObjPos);
       
        if (data. trackpadState > 0 && lastTrackPadState == 0)  HtcController.instance.isDrawingActivated = !HtcController.instance.isDrawingActivated;
        lastTrackPadState = data.trackpadState;

        HtcController
            .instance.
            getUpdatedData(
            data.controllerPos * scale, 
            data.controllerRot, 
            data.triggerState);
    } 

    void OnDestroy()
    {
        if (socketServer != null)
        {
            socketServer.Close();
        }
    }

    private void OnApplicationQuit()
    {
        if (socketServer != null)
        {
            socketServer.Close();
        }
    }

    public float smooth = 5;
  
    void Update () {  
    }
}
