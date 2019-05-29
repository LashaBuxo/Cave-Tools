using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawPathWires : MonoBehaviour
{

    public float FPS = 30;
    public float trayDuration = 5;

    public enum TrackingDevice
    {
        Kinect,
        HTC
    }

    public bool drawWires;

    private Vector3 Kinect;
    private Vector3 HTC;

    public LineRenderer KinectLineRenderer;
    public LineRenderer HTCLineRenderer;
    public LineRenderer CalibratedLineRenderer;

    private Vector3[] InitArray = new Vector3[1] { Vector3.zero };

    GameObject KinectWires;
    GameObject HTCWires;
    GameObject calibratedWires;

    public static drawPathWires instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //Create 2 object for wire renderers;
        if (!drawWires) return;
        KinectWires = new GameObject("KinectWires");
        HTCWires = new GameObject("HTCWires");
        calibratedWires = new GameObject("calibratedWires");

        KinectWires.AddComponent<LineRenderer>();
        LineRenderer KinectLineRenderer = ((LineRenderer)KinectWires.GetComponent("LineRenderer"));
        KinectLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        KinectLineRenderer.startColor = Color.blue;
        KinectLineRenderer.endColor = Color.blue;
        KinectLineRenderer.positionCount = 0;
        KinectLineRenderer.startWidth = 0.1f;
        KinectLineRenderer.endWidth = 0.1f;

        HTCWires.AddComponent<LineRenderer>();
        LineRenderer HTCLineRenderer = ((LineRenderer)HTCWires.GetComponent("LineRenderer"));
        HTCLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        HTCLineRenderer.startColor = Color.red;
        HTCLineRenderer.endColor = Color.red;
        HTCLineRenderer.positionCount = 0;
        HTCLineRenderer.startWidth = 0.1f;
        HTCLineRenderer.endWidth = 0.1f;

        calibratedWires.AddComponent<LineRenderer>();
        LineRenderer CalibratedLineRenderer = ((LineRenderer)calibratedWires.GetComponent("LineRenderer"));
        CalibratedLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        CalibratedLineRenderer.startColor = Color.green;
        CalibratedLineRenderer.endColor = Color.green;
        CalibratedLineRenderer.positionCount = 0;
        CalibratedLineRenderer.startWidth = 0.1f;
        CalibratedLineRenderer.endWidth = 0.1f;


        this.KinectLineRenderer = KinectLineRenderer;
        this.HTCLineRenderer = HTCLineRenderer;
        this.CalibratedLineRenderer = CalibratedLineRenderer;

        arr = new Vector3[(int)(FPS * trayDuration)];
    }


    float t;
    void Update()
    {
        if (!drawWires) return;
        t += Time.deltaTime;
        if (t * FPS < 1) return;
        t = 0;
         

        //Vector3 posi = TrackingManager.instance.getKinectPosition(0);
        //posi=KinectsKalibrator.instance.trackingData.getPredictedKinectPos(0,posi);
        //drawPoints(posi, CalibratedLineRenderer);
    }

    Vector3[] arr;
    public void drawPoints(Vector3 pos, LineRenderer renderer )
    { 
        if (renderer.positionCount == (int)(FPS * trayDuration))
        {
            renderer.GetPositions(arr);
            for (int i = 0; i < arr.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
            renderer.SetPositions(arr);

        }
        else
            renderer.positionCount += 1;

        renderer.SetPosition(renderer.positionCount - 1, pos);
    }


}




//using UnityEngine;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//public class KinectReceiver : MonoBehaviour
//{
//    //
//    // Summary:
//    //     The types of joints of a Body.
//    public enum JointType
//    {
//        //
//        // Summary:
//        //     Base of the spine.
//        SpineBase = 0,
//        //
//        // Summary:
//        //     Middle of the spine.
//        SpineMid = 1,
//        //
//        // Summary:
//        //     Neck.
//        Neck = 2,
//        //
//        // Summary:
//        //     Head.
//        Head = 3,
//        //
//        // Summary:
//        //     Left shoulder.
//        ShoulderLeft = 4,
//        //
//        // Summary:
//        //     Left elbow.
//        ElbowLeft = 5,
//        //
//        // Summary:
//        //     Left wrist.
//        WristLeft = 6,
//        //
//        // Summary:
//        //     Left hand.
//        HandLeft = 7,
//        //
//        // Summary:
//        //     Right shoulder.
//        ShoulderRight = 8,
//        //
//        // Summary:
//        //     Right elbow.
//        ElbowRight = 9,
//        //
//        // Summary:
//        //     Right wrist.
//        WristRight = 10,
//        //
//        // Summary:
//        //     Right hand.
//        HandRight = 11,
//        //
//        // Summary:
//        //     Left hip.
//        HipLeft = 12,
//        //
//        // Summary:
//        //     Left knee.
//        KneeLeft = 13,
//        //
//        // Summary:
//        //     Left ankle.
//        AnkleLeft = 14,
//        //
//        // Summary:
//        //     Left foot.
//        FootLeft = 15,
//        //
//        // Summary:
//        //     Right hip.
//        HipRight = 16,
//        //
//        // Summary:
//        //     Right knee.
//        KneeRight = 17,
//        //
//        // Summary:
//        //     Right ankle.
//        AnkleRight = 18,
//        //
//        // Summary:
//        //     Right foot.
//        FootRight = 19,
//        //
//        // Summary:
//        //     Between the shoulders on the spine.
//        SpineShoulder = 20,
//        //
//        // Summary:
//        //     Tip of the left hand.
//        HandTipLeft = 21,
//        //
//        // Summary:
//        //     Left thumb.
//        ThumbLeft = 22,
//        //
//        // Summary:
//        //     Tip of the right hand.
//        HandTipRight = 23,
//        //
//        // Summary:
//        //     Right thumb.
//        ThumbRight = 24
//    }

//    public class KinectData
//    {
//        public List<Vector3> joints;
//    }
//    KinectData kinectData;
//    public int kinectsNum;
//    // public GameObject[] targetObjs;
//    SocketServer socketServer;
//    // Use this for initialization
//    public string Name;
//    public int Port;
//    public int startKinectsInd;
//    void Start()
//    {

//        var mainThread = SynchronizationContext.Current;
//        socketServer = new SocketServer("0.0.0.0", Port);
//        socketServer.Listen((data) =>
//        {

//            mainThread.Post((s) =>
//            {
//                getData(data);
//            }, null);
//        });
//    }

//    void getData(string Message)
//    {
//        Debug.Log(Message);
//        kinectData = JsonUtility.FromJson<KinectData>(Message);

//        // Vector3 pos = new Vector3(float.Parse(splitArray[0]), float.Parse(splitArray[1]), float.Parse(splitArray[2]));
//        TrackingManager.instance.KinectSensors[startKinectsInd].GetUpdatedData(kinectData.joints[(int)JointType.Head]);
//        Debug.Log(kinectData.joints[(int)JointType.Head]);
//    }

//    void OnDestroy()
//    {
//        if (socketServer != null)
//        {
//            socketServer.Close();
//        }
//    }
//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
