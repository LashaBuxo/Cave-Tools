using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
public class configFileManager : MonoBehaviour {

   
    public static string path = "Assets/Resources/configFile.txt";

    public static bool read = false;
  
    public static bool isSaved()
    {
        return read;
    }
    public static void readData()
    {
        try
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            if (!File.Exists(path))
            {
                File.Create(path);
                return;
            }  
            
            StreamReader reader = new StreamReader(path);

            BlendWarp_Data_NativeVariables[] data  = JsonHelper.getJsonArray<BlendWarp_Data_NativeVariables>(reader.ReadToEnd());
           // Debug.Log(data.Length);
            for (int i = 0; i < BlendWarpManager.instance.cameraEditors.Length; i++)
            {
               
                data[i].ID = BlendWarpManager.instance.cameraEditors[i].data.ID;

               BlendWarpManager.instance.cameraEditors[i].data.RowCount= data[i].RowCount;
               BlendWarpManager.instance.cameraEditors[i].data.ColCount = data[i].ColCount;

                BlendWarpManager.instance.cameraEditors[i].data.Rows= data[i].Rows;
              BlendWarpManager.instance.cameraEditors[i].data.Cols = data[i].Cols;

                 BlendWarpManager.instance.cameraEditors[i].data.Brightness = data[i].Brightness;

               BlendWarpManager.instance.cameraEditors[i].data.BottomBlending = data[i].BottomBlending;
                BlendWarpManager.instance.cameraEditors[i].data.leftBlending = data[i].leftBlending;
            BlendWarpManager.instance.cameraEditors[i].data.RightBlending = data[i].RightBlending;
             BlendWarpManager.instance.cameraEditors[i].data.TopBlending= data[i].TopBlending;

                BlendWarpManager.instance.cameraEditors[i].data.Grid = new List<Vector2d>();
                for (int k = 0; k < data[i].Grid_x.Count ; k++)
                {
                    BlendWarpManager.instance.cameraEditors[i].data.Grid.Add( new Vector2d(data[i].Grid_x[k], data[i].Grid_y[k]));
 
                }
            } 
            
            reader.Close();
            read = true;
            Debug.Log("Data Loaded.");
            GUImessages.instance.showMessage("Data Loaded.",Color.green,true);
        } catch (System.FormatException  e)
        {
            Debug.Log("Data Load Failed. Reason:"+e.Message);
            GUImessages.instance.showMessage("Data Load Failed.", Color.red,true);
            read = false;
        }
    }

    public class JsonHelper 
    {
    //Usage:
    //YouObject[] objects = JsonHelper.getJsonArray<YouObject> (jsonString);
    public static T[] getJsonArray<T>(string json)
    {
        
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.array;
    }
    //Usage:
    //string jsonString = JsonHelper.arrayToJson<YouObject>(objects);
    public static string arrayToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.array = array;
        return JsonUtility.ToJson(wrapper,true);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}


public static void saveData( )
    {
        StreamWriter writer = new StreamWriter(path, false);

        // JsonHelper.arrayToJson

        BlendWarp_Data_NativeVariables[] data = new BlendWarp_Data_NativeVariables[BlendWarpManager.instance.cameraEditors.Length];
         
     
        for (int i=0;i < BlendWarpManager.instance.cameraEditors.Length; i++)
        {
            data[i] = new BlendWarp_Data_NativeVariables();
            data[i].ID = BlendWarpManager.instance.cameraEditors[i].data.ID;

            data[i].RowCount = BlendWarpManager.instance.cameraEditors[i].data.RowCount;
            data[i].ColCount = BlendWarpManager.instance.cameraEditors[i].data.ColCount;

            data[i].Rows = BlendWarpManager.instance.cameraEditors[i].data.Rows;
            data[i].Cols = BlendWarpManager.instance.cameraEditors[i].data.Cols;

            data[i].Brightness = BlendWarpManager.instance.cameraEditors[i].data.Brightness;

            data[i].BottomBlending= BlendWarpManager.instance.cameraEditors[i].data.BottomBlending ;
            data[i].leftBlending = BlendWarpManager.instance.cameraEditors[i].data.leftBlending;
            data[i].RightBlending = BlendWarpManager.instance.cameraEditors[i].data.RightBlending;
            data[i].TopBlending = BlendWarpManager.instance.cameraEditors[i].data.TopBlending;

            data[i].Grid_x = new List<double>();
            data[i].Grid_y = new List<double>();

            for (int k = 0; k < BlendWarpManager.instance.cameraEditors[i].data.Grid.Count; k++)
            {
                data[i].Grid_x .Add( BlendWarpManager.instance.cameraEditors[i].data.Grid[k].x);
                data[i].Grid_y.Add(BlendWarpManager.instance.cameraEditors[i].data.Grid[k].y);
            }
        }
       // Debug.Log(JsonHelper.arrayToJson(data));
        writer.WriteLine(JsonHelper.arrayToJson<BlendWarp_Data_NativeVariables>(data));

        writer.Close();
        Debug.Log("Data Saved.");
        GUImessages.instance.showMessage("Data Saved.",Color.green,true);
    } 

    private void Awake()
    {
      
        readData();
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
