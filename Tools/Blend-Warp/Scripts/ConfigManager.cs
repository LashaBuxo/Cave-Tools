using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

using System.IO;
using System;

[CustomEditor(typeof(TrackingManager))]
 

public class ConfigManager : Editor {
   
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrackingManager myScript = (TrackingManager)target;
        if (GUILayout.Button("Save Configs"))
        {
            try
            {
              
                TrackingManager.TrackingParameters obj = new TrackingManager.TrackingParameters(); 
                obj = myScript.tracking;

                string json = JsonUtility.ToJson(obj);
                string path = Application.dataPath + "/StreamingAssets/trackingParams.json";

                if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
                    Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
                 
                //Write some text to the test.txt file
                StreamWriter writer = new StreamWriter(path, false);
                writer.WriteLine(json);
                writer.Close();

                Debug.Log("Configs successfully saved.");
            } catch (Exception e)
            {

                Debug.Log("Configs NOT saved. Exception:"+e.Message);
            }
        }
        if (GUILayout.Button("Load Configs"))
        {
            try
            {
                TrackingManager.loadConfigs(myScript);
                Debug.Log("Configs successfully loaded.");
            }
            catch
            {

                Debug.Log("Configs NOT loaded.");
            }
        }
    }
 
    // Use this for initialization
    void Start () { 

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}



