using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GUImessages : MonoBehaviour {
    public bool enabled = false;
    public string GuiMessage = "";
    string GuiID;
    // Use this for initialization
    public static GUImessages instance;
    private void Awake()
    {
        instance = this;
    }
    void Start () {

        guiStyle.fontSize = 50;
       
    }

    public GUIStyle guiStyle = new GUIStyle();
    private void OnGUI()
    {
        
       if (enabled) GUI.Label(new Rect(0, 0, 500, 150), GuiMessage, guiStyle);
    }
     
    GameObject debug;
    float   lastMessage;
    public  void showMessage(string message,Color col,bool toAll)
    {
        if (toAll == false)
        {
            int ind = BlendWarpManager.instance.activeCameraDebugger;
            debug = BlendWarpManager.instance.cameraEditors[ind].debugObj.gameObject;

            debug.GetComponent<TextMeshPro>().text = message;
            debug.GetComponent<TextMeshPro>().color = col;
        } else
        {
            foreach (BlendWarp_Editor obj in BlendWarpManager.instance.cameraEditors)
            {
                if (obj.debugObj != null)
                {
                    obj.debugObj.GetComponent<TextMeshPro>().text = message;
                    obj.debugObj.GetComponent<TextMeshPro>().color = col;
                }
            }
        }
        enabled = true;
        GuiMessage = message;
        lastMessage = Time.time;
    }
     
    // Update is called once per frame
    void Update () {
       
        if (lastMessage + 3 < Time.time)
        {
          
            foreach (BlendWarp_Editor obj in BlendWarpManager.instance.cameraEditors)
            {

                obj.debugObj.GetComponent<TextMeshPro>().text = "";
                GuiMessage = "";
            }
        }
    }
}
