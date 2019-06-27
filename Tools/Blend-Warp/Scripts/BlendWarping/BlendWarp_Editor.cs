using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Assets.Blend_Warp.Script.BlendWarping;
using System;
[Serializable]
public class BlendWarp_Editor : MonoBehaviour
{
    [Header("Blend-Warp Editor")]
    [Range(1, 8)]
    public int ID;

    [Serializable]
    public class AttachedQuads
    {
        [Serializable]
        public class attachedQuadRow
        {
            public List<BlendWarp_Editor> cells;
        }
        public List<attachedQuadRow> rows;
    }
    public bool hasCamera = true;
    public AttachedQuads attachedQuads;
    public BlendWarp_Editor parent;

    public bool state = false;
    public bool exportsMesh;

    [Header("Projector Display")]
    public Vector2Int projectorResolution = new Vector2Int(1920, 1080);

    [Range(0.01f, 2)]
    public float scaleFactor;


    [Header("Quad")]
    public GameObject targetQuadObj;


    [Header("Warping Tools")]
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public float gridWidth = 0.01f;



    [Header("Blending Tools")]
    [Range(0, 100)]
    public float Brightness = 100;

    [Range(0, 100)]
    public float leftBlending = 0;
    [Range(0, 100)]
    public float RightBlending = 0;

    [Range(0, 100)]
    public float UpBlending = 0;
    [Range(0, 100)]
    public float DownBlending = 0;
     
    /* 
     * Following variable defines degree of function in shader
     * which applies blending
     * 
     * Hotkey for editing: 'Y'
     * */
    [Range(0.1f, 5)]
    public float FunctionDegree = 1;


    public Material BlendingMaterial;
    public RenderTexture renderTexture;

    [NonSerialized]
    public GameObject debugObj;

    
    private void Awake()
    {
        if (attachedQuads!=null)
        foreach (AttachedQuads.attachedQuadRow row in attachedQuads.rows)
        {
            foreach (BlendWarp_Editor cell in row.cells)
            {
                cell.parent = this;
            }
        }
        if (BlendWarpManager.instance.shaderEffect != null)
        {
            BlendingMaterial = new Material(BlendWarpManager.instance.shaderEffect);
        }
        if (GetComponent<Camera>().targetTexture != null)
        {
            GetComponent<Camera>().targetTexture.width = projectorResolution.x;
            GetComponent<Camera>().targetTexture.height = projectorResolution.y;
            renderTexture = GetComponent<Camera>().targetTexture;
        } else
        {
            renderTexture = new RenderTexture(projectorResolution.y, projectorResolution.x, 24);
            GetComponent<Camera>().targetTexture = renderTexture;
        }
        //  Debug.Log(GetComponent<Camera>().pixelWidth + " " + GetComponent<Camera>().pixelHeight);
        Rect viewPort = GetComponent<Camera>().rect;
        viewPort.width = viewPort.height = 1;
        GetComponent<Camera>().rect = viewPort;
        //   Debug.Log(GetComponent<Camera>().pixelWidth + " " + GetComponent<Camera>().pixelHeight);
        //   Debug.Log(GetComponent<Camera>().scaledPixelWidth + " " + GetComponent<Camera>().scaledPixelHeight);

    }

    List<float> rows = null;
    List<float> cols = null;
    List<List<Vector2>> grid = null;

    Transform lookingPlane;

    Vector3 quadPosition;
    void Start()
    {
        lookingPlane = GetComponent<Frustum>().lookPlane;

        quadPosition = new Vector3(1000 + ID * 200, 0, 0);
        if (parent != null) quadPosition = parent.getQuadPosition(ID);

        if (configFileManager.read == false)
            createNew_TargetQuad();
        else
        {
            load_TargetQuad();
        }
 
    }
    public BlendWarp_Data data;

    public Vector3 getQuadPosition(int id)
    {
        int x=-1, y=-1;
        for (int i = 0; i < attachedQuads.rows.Count; i++)
        {
            for (int j = 0; j < attachedQuads.rows[i].cells.Count; j++)
            {
                if (attachedQuads.rows[i].cells[j].ID == id)
                {
                    y = i;
                    x = j;
                }
            }
        }
        return new Vector3(1000 + ID * 200 + x * 20, y * -11.25f, 0);
    }

    void load_TargetQuad()
    {
        if (targetQuadObj != null) DestroyImmediate(targetQuadObj);
        targetQuadObj = new GameObject("BlendWarp_Quad_" + ID, typeof(MeshRenderer), typeof(MeshFilter), typeof(BlendWarpQuad), typeof(BlendWarp_Grid));
        // targetQuadObj.transform.parent = transform;
        // quadPlane.AddComponent<BlendWarpQuad>().
        targetQuadObj.transform.position = quadPosition;
        
        //targetQuadObj.transform.rotation = Quaternion.Euler(0, 180, 0);

        // data = new BlendWarp_Data();
        //  data.ID = ID;
        //  data.Rows = new List<float>(2) { 0, 10 };
        //  data.Cols = new List<float>(2) { 0, 10 * transform.Find("Plane").localScale.z / (1.0f * transform.Find("Plane").localScale.x) };
        //  data.RowCount = gridSize.x;
        //   data.ColCount = gridSize.y;
        RightBlending = data.RightBlending;
        leftBlending = data.leftBlending;
        UpBlending = data.UpBlending;
        DownBlending = data.DownBlending;

        Brightness = data.Brightness;
        startquad();
         
        debugObj = Instantiate(BlendWarpManager.instance.Debugger, targetQuadObj.transform);
    }
    public void startquad()
    {
        if (hasCamera == false)
        {
            targetQuadObj.GetComponent<BlendWarpQuad>().startQuad(this, data, false);
        }
        else
        {
            if (parent == null) targetQuadObj.GetComponent<BlendWarpQuad>().startQuad(this, data, true, 1, 1);
            else
           targetQuadObj.GetComponent<BlendWarpQuad>().startQuad(this, data, true, attachedQuads.rows[0].cells.Count,
                                                              attachedQuads.rows.Count);
        }
    }
    public void createNew_TargetQuad()
    {
        if (targetQuadObj != null) DestroyImmediate(targetQuadObj);
        targetQuadObj = new GameObject("BlendWarp_Quad_" + ID, typeof(MeshRenderer), typeof(MeshFilter), typeof(BlendWarpQuad), typeof(BlendWarp_Grid));
        // targetQuadObj.transform.parent = transform;
        // quadPlane.AddComponent<BlendWarpQuad>().
        targetQuadObj.transform.position = quadPosition;
        //targetQuadObj.transform.rotation = Quaternion.Euler(0, 180, 0);

        data = new BlendWarp_Data();
        data.ID = ID;
        data.Rows = new List<double>(2) { 0, 10 };
        // Debug.Log(transform.Find("Plane").localScale.z + " " + transform.Find("Plane").localScale.x);
        data.Cols = new List<double>(2) { 0, 10 * lookingPlane.lossyScale.z / (1.0 * lookingPlane.lossyScale.x) };
        data.RowCount = gridSize.x;
        data.ColCount = gridSize.y;
        startquad();
        debugObj = Instantiate(BlendWarpManager.instance.Debugger, targetQuadObj.transform);
    }
    void Update()
    {
        if (targetQuadObj != null)
        {
            Material mat = targetQuadObj.GetComponent<Renderer>().material;
            mat.SetFloat("_BrightnessAmount", Brightness / 100.0f);
            mat.SetFloat("_BlendingLeft", leftBlending / 100.0f);
            mat.SetFloat("_BlendingRight", RightBlending / 100.0f);
            mat.SetFloat("_BlendingUp", UpBlending / 100.0f);
            mat.SetFloat("_BlendingDown", DownBlending / 100.0f);
            mat.SetFloat("_FunctionDegree", FunctionDegree);
        }
        if (state == false) return;
        
        if (Input.GetKey(KeyCode.B))
        {

            if (Input.GetKey(KeyCode.RightArrow))
            {

                if (Input.GetKey(KeyCode.LeftShift))
                {

                    Brightness = Math.Min(100, Brightness + 10 * BlendWarpManager.instance.blendMoveSpeed);
                }
                else
                {

                    Brightness = Math.Min(100, Brightness + BlendWarpManager.instance.blendMoveSpeed);
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Brightness = Math.Max(0, Brightness - 10 * BlendWarpManager.instance.blendMoveSpeed);
                }
                else
                {
                    Brightness = Math.Max(0, Brightness - BlendWarpManager.instance.blendMoveSpeed);
                }
            }
            data.Brightness = Brightness;
            GUImessages.instance.showMessage("Brightness: " + (int)(data.Brightness) + "%", Color.green, true);
        }

        if (ControlKeySet.BlendPermitted())
        {
            int amplifierSpeed = 10;

            int amplify = Input.GetKey(ControlKeySet.GetControlKey(Controls.Amplify)) ? 1 : 0;

            int blendingLeft = Input.GetKey(ControlKeySet.GetModeKey(ControllingModes.BlendingLeft)) ? 1 : 0;
            int blendingRight = Input.GetKey(ControlKeySet.GetModeKey(ControllingModes.BlendingRight)) ? 1 : 0;
            int blendingUp = Input.GetKey(ControlKeySet.GetModeKey(ControllingModes.BlendingUp)) ? 1 : 0;
            int blendingDown = Input.GetKey(ControlKeySet.GetModeKey(ControllingModes.BlendingDown)) ? 1 : 0;

            int increaseBlending = Input.GetKey(ControlKeySet.GetControlKey(Controls.BlendingIncrease)) ? 1 : 0;
            int decreaseBlending = Input.GetKey(ControlKeySet.GetControlKey(Controls.BlendingDecrease)) ? 1 : 0;

            if (blendingLeft > 0)
            leftBlending = Math.Max(0, Math.Min(100, blendingLeft * (leftBlending
                                                       + increaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed
                                                       - decreaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed)));
            if (blendingRight > 0)
            RightBlending = Math.Max(0, Math.Min(100, blendingRight * (RightBlending
                                                       + increaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed
                                                       - decreaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed)));
            if (blendingUp > 0)
            UpBlending = Math.Max(0, Math.Min(100, blendingUp * (UpBlending
                                                       + increaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed
                                                       - decreaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed)));
            if (blendingDown > 0)
            DownBlending = Math.Max(0, Math.Min(100, blendingDown * (DownBlending
                                                       + increaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed
                                                       - decreaseBlending * (amplify * (amplifierSpeed - 1) + 1) * BlendWarpManager.instance.blendMoveSpeed)));
             
            data.leftBlending = leftBlending;
            data.RightBlending = RightBlending;
            data.UpBlending = UpBlending;
            data.DownBlending = DownBlending;

            GUImessages.instance.showMessage("Blend-Left-Right-Up-Down: " + (int)(data.leftBlending) + " " + (int)(data.RightBlending) + " " + (int)(data.UpBlending) + " " + (int)(data.DownBlending), Color.green, true);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                FunctionDegree = Math.Max(1, FunctionDegree - 1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                FunctionDegree = Math.Min(5, FunctionDegree + 1);
            }
            GUImessages.instance.showMessage("Degree " + (int)FunctionDegree, Color.green, true);
        }
    }

    void assignCamera(GameObject obj)
    {
        GameObject cam = new GameObject(transform.name + "_camera", typeof(Camera));
        cam.GetComponent<Camera>().orthographic = true;
        cam.GetComponent<Camera>().orthographicSize = 6;
        cam.GetComponent<Camera>().targetDisplay = ID;
        cam.GetComponent<Camera>().backgroundColor = new Color(0, 0, 0);
        cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cam.GetComponent<Camera>().farClipPlane = 100;
        cam.transform.parent = obj.transform;

        cam.transform.localPosition = new Vector3(0, 0, 1);
        cam.transform.localRotation = Quaternion.Euler(180, 0, 180);
    }


    public void turnEditor(bool on)
    {
        targetQuadObj.GetComponent<BlendWarp_Grid>().turnGrid(on);
        state = on;
    }

    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    BlendingMaterial.SetFloat("_BrightnessAmount", Brightness / 100.0f);
    //    BlendingMaterial.SetFloat("_BlendingLeft", leftBlending / 100.0f);
    //    BlendingMaterial.SetFloat("_BlendingRight", RightBlending / 100.0f);
    //    BlendingMaterial.SetFloat("_BlendingUp", UpBlending / 100.0f);
    //    BlendingMaterial.SetFloat("_BlendingDown", DownBlending / 100.0f);
    //    BlendingMaterial.SetFloat("_FunctionDegree", FunctionDegree);
    //    Graphics.Blit(source, destination, BlendingMaterial); 
    //}


}