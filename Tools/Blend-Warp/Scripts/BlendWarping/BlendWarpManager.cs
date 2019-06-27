using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class BlendWarpManager : MonoBehaviour
{
    public static BlendWarpManager instance;

    public Color defaultColor;
    public Color selectedColor;
    public GameObject IntersectPoint;
    public Shader shaderEffect;
    public Material gridMat;
    public GameObject planeLookingCamera;
    public GameObject Debugger;
    [Range(0, 0.5f)]
    public float blendMoveSpeed;
    [Range(0,2)]
    public float pointSpeed = 0.5f;
    public enum WarpingMode
    {
        SinglePoint_Move,
        Row_Move,
        Col_Move,
        CornersRowCol_Move
    }
    public static bool hide = false;
    public static WarpingMode warpingMode = WarpingMode.CornersRowCol_Move;
    public static int Mode;

    public int activeCameraDebugger;
    public BlendWarp_Editor[] cameraEditors; 
     
    
    //   public GameObject wall;
    //   public Toggle testToggle;
    private void Awake()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 10;
        instance = this; 
    }
    void Start()
    {
         
        Debug.Log("displays connected: " + Display.displays.Length);
    
        for (int i = 1; i < 8; i++)
        {
            if (Display.displays.Length > i)
                Display.displays[i].Activate();
        }
        
        activeCameraDebugger = 0;

    }
   
    //public void TestImage( )
    //{
    //    if (testToggle.isOn)
    //    {
    //        wall.GetComponent<MeshRenderer>().material = testMat;
    //    } else
    //    {
    //        wall.GetComponent<MeshRenderer>().material = VideoMat;
    //    }
    //}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
           // Debug.Log(activeCameraDebugger);
            if (activeCameraDebugger + 1 == cameraEditors.Length)
            {
                activeCameraDebugger = 0;
            }
            else activeCameraDebugger++;
            for (int i = 0; i < cameraEditors.Length; i++)
                if (i == activeCameraDebugger)
                    cameraEditors[i].turnEditor(true);
                else
                {
                    cameraEditors[i].turnEditor(false);
                }
            GUImessages.instance.showMessage("Current Display Selected: " + cameraEditors[activeCameraDebugger].ID,Color.cyan,false);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {

            if (warpingMode == WarpingMode.CornersRowCol_Move) warpingMode = WarpingMode.Row_Move;
            else
                if (warpingMode == WarpingMode.Row_Move) warpingMode = WarpingMode.Col_Move;
            else
                if (warpingMode == WarpingMode.Col_Move) warpingMode = WarpingMode.SinglePoint_Move;
            else
                if (warpingMode == WarpingMode.SinglePoint_Move) warpingMode = WarpingMode.CornersRowCol_Move;
            GUImessages.instance.showMessage("Current Warping Mode is: " + warpingMode.ToString(),Color.white,false);
            writeMessage(warpingMode.ToString());
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            hide = !hide;
             for (int i = 0; i < cameraEditors.Length; i++)
               cameraEditors[i].targetQuadObj.GetComponent<BlendWarp_Grid>().hide(hide);
        }
       
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                configFileManager.saveData();
            }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {

            resetGrid();
        }
    }
    public void resetGrid()
    {
        for (int i = 0; i < cameraEditors.Length; i++)
        {
            cameraEditors[i].createNew_TargetQuad();
        }
    }
    

    public void writeMessage(string s,float duration=2)
    {
        message = s;
        t = duration;
        showMessage = true;
    }

    public string message = "";
    bool showMessage=false;
    float t = 0;
    private void OnGUI()
    {
        if (showMessage)
        if (GUI.Button(new Rect(10, 10, 200, 50), message))
            Debug.Log("Clicked the button with an image");

        if (t > Time.deltaTime) {
            t -= Time.deltaTime;
            showMessage = true;
        } else
        {
            t = 0;
            showMessage = false;
        } 
    }
     
}