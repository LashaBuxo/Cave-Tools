using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
[CustomEditor(typeof(KinectsCalibrator))]
public class KinectsCaliratorButtons : Editor
{
     
     int kinectsNum;
    public static KinectsCaliratorButtons instance;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        { 
            kinectsNum = KinectsCalibrator.instance.trackingData.kinectDevices;
            if (KinectsCalibrator.instance. recording.Length != kinectsNum)
            {
                instance = this;
                KinectsCalibrator.instance.recording = new bool[kinectsNum];
            }
          //  Debug.Log(kinectsNum);
            for (int i = 0; i < kinectsNum; i++)
            {
                if (KinectsCalibrator.instance.recording[i] == true)
                {
                    if (GUILayout.Button("{Reset Recording id=" + i + ", amount="  + KinectsCalibrator.instance.trackingData.getPositionsCount(i) +"}"))
                    {
                        KinectsCalibrator.instance.recording[i] = false;
                        KinectsCalibrator.instance.trackingData.resetDataFor(i);
                    } 
                }
                else
                { 
                    if (GUILayout.Button("{Start Recording, id=" + i+"}"))
                    {
                        KinectsCalibrator.instance.recording[i] = true;
                        KinectsCalibrator.instance.trackingData.resetDataFor(i);
                    }
                }
               
            }
            
            if (GUILayout.Button("Calculate Translations"))
            {
                KinectsCalibrator.instance.trackingData.calculateTranslations();
                KinectsCalibrator.instance.loadTranslationsToSensors(KinectsCalibrator.instance.trackingData);
                Debug.Log("Translations calculated.");
            }
            if (GUILayout.Button("Save Data"))
            {
                KinectsCalibrator.instance.saveTrackingData();
                Debug.Log("Tracking data Saved.");
            }
            if (GUILayout.Button("Clear Data"))
            {
                KinectsCalibrator.instance.trackingData.resetData();
                Debug.Log("Tracking data Cleared.");
            }
        }
    }

    // Use this for initialization
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {

    }
}

#endif