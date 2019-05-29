using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
public class KinectReceiver : MonoBehaviour
{
    public enum JointType
    {
        //
        // Summary:
        //     Base of the spine.
        SpineBase = 0,
        //
        // Summary:
        //     Middle of the spine.
        SpineMid = 1,
        //
        // Summary:
        //     Neck.
        Neck = 2,
        //
        // Summary:
        //     Head.
        Head = 3,
        //
        // Summary:
        //     Left shoulder.
        ShoulderLeft = 4,
        //
        // Summary:
        //     Left elbow.
        ElbowLeft = 5,
        //
        // Summary:
        //     Left wrist.
        WristLeft = 6,
        //
        // Summary:
        //     Left hand.
        HandLeft = 7,
        //
        // Summary:
        //     Right shoulder.
        ShoulderRight = 8,
        //
        // Summary:
        //     Right elbow.
        ElbowRight = 9,
        //
        // Summary:
        //     Right wrist.
        WristRight = 10,
        //
        // Summary:
        //     Right hand.
        HandRight = 11,
        //
        // Summary:
        //     Left hip.
        HipLeft = 12,
        //
        // Summary:
        //     Left knee.
        KneeLeft = 13,
        //
        // Summary:
        //     Left ankle.
        AnkleLeft = 14,
        //
        // Summary:
        //     Left foot.
        FootLeft = 15,
        //
        // Summary:
        //     Right hip.
        HipRight = 16,
        //
        // Summary:
        //     Right knee.
        KneeRight = 17,
        //
        // Summary:
        //     Right ankle.
        AnkleRight = 18,
        //
        // Summary:
        //     Right foot.
        FootRight = 19,
        //
        // Summary:
        //     Between the shoulders on the spine.
        SpineShoulder = 20,
        //
        // Summary:
        //     Tip of the left hand.
        HandTipLeft = 21,
        //
        // Summary:
        //     Left thumb.
        ThumbLeft = 22,
        //
        // Summary:
        //     Tip of the right hand.
        HandTipRight = 23,
        //
        // Summary:
        //     Right thumb.
        ThumbRight = 24
    }
     
    public class KinectData
    {
        public bool isFloorInformation;
        public float KinectTiltAngle;
     
        public float KinectHeight;
        public List<Vector3> joints;
    }

    public KinectData kinectData; 
    SocketServer socketServer;
     
    public int Port;
    public Vector3 headPosition;
    
    public void startListening(int Port)
    {
        this.Port = Port;
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
    void getData(string Message)
    { 
        kinectData = JsonUtility.FromJson<KinectData>(Message); 
        if (!kinectData.isFloorInformation)
        {
           if ( TrackingManager.instance!=null)
              GetComponent<KinectSensor>().GetUpdatedData(kinectData.joints[(int)JointType.Head]);
           else
            {
                headPosition= kinectData.joints[(int)JointType.Head];
            }
        } else
        {
           //Floor Information
        } 
    }

    void OnDestroy()
    {
        if (socketServer != null)
        {
            socketServer.Close();
        }
    }

    void Update()
    {

    }
}
