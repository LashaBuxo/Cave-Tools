using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class KinectsCalibrator : MonoBehaviour
{
    public bool isCalibrationActive;
    public bool drawCalibratedData;
    public int recordRate = 30;
    public int recordsNeeded = 500;
      
    public TrackingPositionsData trackingData;

    public static KinectsCalibrator instance;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (isCalibrationActive)
            readTrackingData();
    }

    public void saveTrackingData()
    {

        if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
            Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");

        using (StreamWriter writer = new StreamWriter(Application.dataPath + "/StreamingAssets/trackingData.json", false))
        { 
            writer.WriteLine(JsonUtility.ToJson(trackingData, true));
        }

    }
    
    private void readTrackingData()
    {

        //  Debug.Log(Application.dataPath + "/trackingData.json");
        if (!File.Exists(Application.dataPath + "/StreamingAssets/trackingData.json"))
        {
            trackingData = new TrackingPositionsData(TrackingManager.instance.KinectSensors.Length, recordsNeeded);
        }
        else
        {
            using (StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/trackingData.json"))
            {
                string data = reader.ReadToEnd();
                trackingData = JsonUtility.FromJson<TrackingPositionsData>(data);
                recordsNeeded = trackingData.recordsNeeded;
                if (trackingData.getPositionsCount(0) == 0 && trackingData.getPositionsCount(1) == 0 && trackingData.getPositionsCount(2) == 0)
                    trackingData.resetData();
            }
        }
       
        loadTranslationsToSensors(trackingData);

        if (drawCalibratedData)
        {
            List<Vector3> correctPosi = new List<Vector3>();
            List<Vector3> kinectPosi = new List<Vector3>();
            List<Vector3> calibratedPos = new List<Vector3>();


            for (int i = 0; i < trackingData.kinectDevices; i++)
            {
                if (i != 0) continue;
                for (int j = 0; j < recordsNeeded; j++)
                {
                    Vector3 kinecti = trackingData.getKinectPos(i, j);
                    Vector3 correcti = trackingData.getCorrectPos(i, j);

                    correctPosi.Add(correcti);
                    kinectPosi.Add(kinecti);
                    calibratedPos.Add(trackingData.getPredictedKinectPos(i, kinecti));
                }
            }

            List<Vector3> newKinectPosi = new List<Vector3>();
            for (int i = 0; i < kinectPosi.Count; i++)
            {
                // newKinectPosi.Add(TrackingManager.instance.KinectSensors[0].localPosToGlobal(kinectPosi[i]));
            }
            drawPoints("kinectPointsUpdated", newKinectPosi, Color.yellow);
            //drawpoints("kinectpoints", kinectposi, color.blue);
            drawPoints("HTCPoints", correctPosi, Color.red);
            drawPoints("HTCPoints", calibratedPos, Color.green);
            float dist = 0;
            for (int i = 0; i < correctPosi.Count; i++)
            {
                dist += Vector3.Distance(correctPosi[i], calibratedPos[i]);
            }

            dist /= correctPosi.Count;
            Debug.Log("Aprox. diff: "+ Math.Round( dist*10,2)+" cm.");
        }
    } 
    public void loadTranslationsToSensors(TrackingPositionsData data)
    {
        for (int i = 0; i < TrackingManager.instance.KinectSensors.Length; i++)
        {
            Vector3[] arr=data.getCalibratedParams(i);
            if (arr == null)
            {
                Debug.LogError("Translations is not yet calculated, please press calculate Translations on TrackingManager GameObject.");
                return;
            }
            TrackingManager.instance.KinectSensors[i].CalibratedRotation = arr[0];
            TrackingManager.instance.KinectSensors[i].CalibratedDirection = arr[1];
            TrackingManager.instance.KinectSensors[i].CalibratedKinectsMiddlePoint = arr[2];
            TrackingManager.instance.KinectSensors[i].CalibratedCorrectMiddlePoint = arr[3];
        }
    }
    [NonSerialized]
    public bool[] recording = new bool[0];
    public void checkNewPositions()
    { 
        Vector3 htcPos = TrackingManager.instance.getHtcPosition();
        if (recording.Length != trackingData.kinectDevices) return;
        for (int i = 0; i < trackingData.kinectDevices; i++)
        {
            if (recording[i] == false) continue;
            Vector3 kinectPos = TrackingManager.instance.getKinectPosition(i);

            if (htcPos != -Vector3.one && kinectPos != -Vector3.one)
            {
                trackingData.addRecord(i, kinectPos, htcPos);
                drawPathWires.instance.drawPoints(TrackingManager.instance.getKinectPosition(0), drawPathWires.instance.KinectLineRenderer);
                drawPathWires.instance.drawPoints(TrackingManager.instance.TrackedPosition, drawPathWires.instance.HTCLineRenderer);
            }
        }
    } 


    float t = 0;
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t >= 1 / (recordRate * 1.0f))
        {
            if (isCalibrationActive) checkNewPositions();
            t = 0;
        }
    }

    public void drawPoints(string name, List<Vector3> points, Color col)
    {
        GameObject obj = new GameObject();
        obj.transform.position = Vector3.zero;
        obj.name = name;
        for (int i = 0; i < points.Count; i++)
        {
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            child.transform.localScale = Vector3.one / 15;
            child.transform.parent = obj.transform;
            child.transform.localPosition = points[i];
            child.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
            child.GetComponent<MeshRenderer>().material.color = col;
        }
        //GameObject obj = new GameObject();
        //obj.AddComponent<LineRenderer>();
        //LineRenderer lineRenderer = ((LineRenderer)obj.GetComponent("LineRenderer"));
        //lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        //lineRenderer.startColor = col;
        //lineRenderer.endColor = col;
        //lineRenderer.positionCount = 0;
        //lineRenderer.startWidth = 0.1f;
        //lineRenderer.endWidth = 0.1f;

        //lineRenderer.positionCount = points.Count;
        //Vector3[] arr = new Vector3[points.Count];
        //for (int i = 0; i < points.Count; i++)
        //{
        //    arr[i] = points[i];
        //  //  Debug.Log(arr[i]);
        //}
        //lineRenderer.SetPositions(arr);
    }
}
