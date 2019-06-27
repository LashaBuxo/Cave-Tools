using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.IO;

public class TrackingManager : MonoBehaviour
{
    public enum TrackingMethod
    {
        KinectCustom = 0,
        KinectCalibrated = 1,
        Htc = 2
    }

    [Serializable]
    public class KinectTracker
    { 
        public int port;
        public Vector3 position;
        public Vector3 rotation;
        public Color color;
        public bool drawTarget;
        public bool simulate;
    }

    [Serializable]
    public class HtcTracker
    {
        public int port;
    }

    [Serializable]
    public class TrackingParameters
    {
        public KinectTracker[] kinectTrackers;
        public HtcTracker htcTracker;

        public TrackingParameters()
        {
            kinectTrackers =new KinectTracker[0];
            htcTracker = new HtcTracker();
        }
    }

    public static TrackingManager instance;
    public TrackingMethod trackingMethod = TrackingMethod.KinectCustom;

    

     
    public TrackingParameters tracking;

    [Header("Details")]
    public GameObject targetObj;
    public bool targetHide=false;
    public bool CubeHide=true; 
    public Color CubeColor = Color.green; 

    [Header("Tracked Local Position from Origin")] 
    public Vector3 TrackedPosition = Vector3.one;

    private GameObject target;

    [NonSerialized]
    public KinectSensor[] KinectSensors; 
    private static float easeSpeed = 2f;

    private void Awake()
    { 
        instance = this;
        loadConfigs(instance);
    }

    public static void loadConfigs(TrackingManager myScript)
    {
        if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
            Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");

        string path = Application.dataPath + "/StreamingAssets/tracking_Params.json"; 
        try
        {
            StreamReader reader = new StreamReader(path, false);
            string json = reader.ReadToEnd();
            TrackingParameters obj = JsonUtility.FromJson<TrackingParameters>(json);
            reader.Close();
            myScript.tracking = obj;
        } catch
        {
            Debug.LogWarning("Config file was empty.");
            myScript.tracking = new TrackingParameters();
        }
    }
    

    private void OnDrawGizmos()
    {
        if (CubeHide) return;
        Gizmos.color = CubeColor;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, this.transform.localRotation, this.transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        
        for (int i = 0; i < tracking.kinectTrackers.Length; i++)
        {

        }

        Gizmos.DrawWireCube(Vector3.one * 5  , Vector3.one * 10); 
    }

    void Start()
    {
        KinectSensors = new KinectSensor[tracking.kinectTrackers.Length];
        for (int i = 0; i < KinectSensors.Length; i++)
        {
            GameObject kinectObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kinectObject.transform.parent = TrackingManager.instance.transform;

            KinectSensor sensor=kinectObject.AddComponent<KinectSensor>();
            sensor.declareVariables(i,          
                                                tracking.kinectTrackers[i].color,
                                                Vector3.one*5,
                                                tracking.kinectTrackers[i].position,
                                                tracking.kinectTrackers[i].rotation,
                                                tracking.kinectTrackers[i].port,
                                                tracking.kinectTrackers[i].drawTarget,
                                                 tracking.kinectTrackers[i].simulate);
            KinectSensors[i] = sensor;  
        }
    }

    private void OnDestroy()
    {
        
    }
      
    // Update is called once per frame
    private void Update()
    {
        for (int i = 0; i < KinectSensors.Length; i++)
        { 
            if (Time.time - KinectSensors[i].getTimeOfUpdate() > 0.05f)
            {
                KinectSensors[i].ease = Math.Max(KinectSensors[i].ease - easeSpeed * Time.deltaTime, 0);
            }
            else
            {
                KinectSensors[i].ease = Math.Min(KinectSensors[i].ease + easeSpeed * Time.deltaTime, 1);
            }
        }
        KinectCalibration(); 
        drawDebugTarget();
    }

    private void drawDebugTarget()
    {
        if (targetHide)
        {
           if (targetObj!=null && targetObj.GetComponent<MeshRenderer>()!=null)
                targetObj.GetComponent<MeshRenderer>().enabled = false; 
        }
        else
        {
            if (targetObj != null && targetObj.GetComponent<MeshRenderer>() != null)
                targetObj.GetComponent<MeshRenderer>().enabled = true;
        }

        targetObj.transform.localPosition = getPlayerLocation();
    }
     
    private void KinectCalibration()
    {
        Vector3 meanPositon = Vector3.zero;
        float totalWeight = 0;

        for (int i = 0; i < KinectSensors.Length; i++)
        {
            meanPositon += KinectSensors[i].PlayerPos_FromOrigin * KinectSensors[i].ease;
            totalWeight += KinectSensors[i].ease;
        }

        if (totalWeight == 0)
            return;

        meanPositon /= totalWeight;

        TrackedPosition = (meanPositon);
    }

    public Vector3 getKinectPosition(int id)
    {
        if (KinectSensors[id].ease != 1) return Vector3.one;
        return KinectSensors[id].origanlPosition;
    }
     
    public void setHtcTrackingLocation(Vector3 pos)
    {
        TrackedPosition = pos;
    }

    public Vector3 getHtcPosition()
    {
        return TrackedPosition;
    }

    public Vector3 getPlayerLocation()
    {
        return TrackedPosition;
    }
}
