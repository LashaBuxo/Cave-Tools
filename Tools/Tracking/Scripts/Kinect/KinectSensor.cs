using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectSensor : MonoBehaviour
{
    public int ID;
    public int Port;

    public Vector3 PlayerPos_FromKinect;
    public Vector3 PlayerPos_FromOrigin;
      
    public Vector3 KinectPosition;
    public Vector3 KinectRotation;
       
    public float ease;
    private float PlayerSpeed;
    private float timeOfUpdate; 
    private Color SensorColor; 
    private bool debug;
    private Vector3 InitPosition;
    GameObject kinectObject;
    GameObject debugTargetObject; 

    [Header("Calibration Values")]
    public Vector3 CalibratedRotation;
    public Vector3 CalibratedDirection;
    public Vector3 CalibratedKinectsMiddlePoint;
    public Vector3 CalibratedCorrectMiddlePoint;

    public void declareVariables(int ID, Color color, Vector3 InitPosition, Vector3 KinectPosition, Vector3 KinectRotation, int port, bool debug)
    {
        this.ID = ID;
        this.SensorColor = color;
        this.InitPosition = InitPosition;
        this.KinectPosition = KinectPosition;
        this.debug = debug;  
        this.KinectRotation = KinectRotation;
        this.Port = port;
        kinectObject = gameObject;

        createSceneObjects();
    }

    private void createSceneObjects()
    {  
        kinectObject.transform.localScale = Vector3.one;
        kinectObject.transform.localPosition = KinectPosition;
        kinectObject.transform.eulerAngles = KinectRotation;
        kinectObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        kinectObject.GetComponent<MeshRenderer>().material.color = SensorColor;
        kinectObject.transform.name = "Kinect ID=" + ID;
         
        debugTargetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
        debugTargetObject.transform.parent = kinectObject.transform;
        setPlayerPosition_FromOrigin(InitPosition);
        debugTargetObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        debugTargetObject.GetComponent<MeshRenderer>().material.color = SensorColor; 
        debugTargetObject.name = "debugTargetObject";
        debugTargetObject.AddComponent<TargetMoveSimulation>();
         
        KinectReceiver receiver= kinectObject.AddComponent<KinectReceiver>();
        receiver.startListening(Port);

        if (!debug)
        {
            debugTargetObject.SetActive(false);
            kinectObject.SetActive(false);
        }
    }

    float lastUpdate = 0;
    float speedLimit = 1;

    public Vector3 localPosToGlobal(Vector3 local)
    {
        debugTargetObject.transform.localPosition = new Vector3(-local.x, local.y, local.z);
        return debugTargetObject.transform.position;
    }

    public Vector3 origanlPosition;

    //Would be called when new position arrives from Kinect
    public void GetUpdatedData(Vector3 newPosition)
    {
        if (debugTargetObject == null) return;
        origanlPosition = newPosition;
        debugTargetObject.transform.localPosition = new Vector3(-newPosition.x, newPosition.y, newPosition.z);

        SetUpdatedPosition();
    }

    public float getTimeOfUpdate()
    {
        return timeOfUpdate;
    }

    public void SetUpdatedPosition()
    {
        timeOfUpdate = Time.time; 
    }

    public void setPlayerPosition_FromOrigin(Vector3 pos)
    {
        debugTargetObject.transform.position = debugTargetObject.transform.parent.parent.TransformPoint(pos);
        timeOfUpdate = Time.time; 
    }

    private void Update()
    {
        PlayerPos_FromKinect = debugTargetObject.transform.localPosition;
        PlayerPos_FromOrigin = debugTargetObject.transform.parent.parent.InverseTransformPoint(debugTargetObject.transform.position);
    }

    //public void GetUpdatedData(Vector3 newPosition)
    //{
    //    localPosition = newPosition;
    //    realPosition = getCalibratedKinectPos(localPosition);

    //    if (localDebugging) localDebugObj.transform.position = localPosition;
    //    if (realDebugging) realDebugObj.transform.position = realPosition;
    //}

    public Vector3 getCalibratedKinectPos(  Vector3 pos)
    {
        pos = multiplyVectors((pos - CalibratedKinectsMiddlePoint), CalibratedDirection);
        pos = Rotate(pos, CalibratedRotation);
        pos +=CalibratedCorrectMiddlePoint;
        return pos;
    }

    private Vector3 Rotate(Vector3 aVec, Vector3 aAngles)
    {
        aAngles *= Mathf.Deg2Rad;
        float sx = Mathf.Sin(aAngles.x);
        float cx = Mathf.Cos(aAngles.x);
        float sy = Mathf.Sin(aAngles.y);
        float cy = Mathf.Cos(aAngles.y);
        float sz = Mathf.Sin(aAngles.z);
        float cz = Mathf.Cos(aAngles.z);
        aVec = new Vector3(aVec.x * cz - aVec.y * sz, aVec.x * sz + aVec.y * cz, aVec.z);
        aVec = new Vector3(aVec.x, aVec.y * cx - aVec.z * sx, aVec.y * sx + aVec.z * cx);
        aVec = new Vector3(aVec.x * cy + aVec.z * sy, aVec.y, -aVec.x * sy + aVec.z * cy);
        return aVec;
    }

    private Vector3 rotatedPos(Vector3 origin, Vector3 pos, Vector3 rotation)
    {
        return Rotate(pos - origin, rotation) + origin;
    }

    private Vector3 multiplyVectors(Vector3 a, Vector3 b)
    {
        a.x = a.x * b.x;
        a.y = a.y * b.y;
        a.z = a.z * b.z;
        return a;
    } 
}