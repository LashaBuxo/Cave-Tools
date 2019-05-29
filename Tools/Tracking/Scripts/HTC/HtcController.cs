using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HtcController : MonoBehaviour {

    public static HtcController instance;
    public bool isDrawingActivated = true;
    public int FPS = 30;
    public float trayDuration = 10;
    public Color col;

    GameObject teleporter;
    private void Awake()
    {
        instance = this;
        teleporter = transform.Find("Teleporter").gameObject;
    }

    float lastTrigger;
    public Transform targetObj;


    public void getUpdatedData(Vector3 pos, Quaternion rot,float trigger)
    {
        if (transform == null || TrackingManager.instance == null) return;
         transform.position = (MultiCamScene.instance.offset.transform.position + pos );
        
        transform.rotation = rot;
  
        if (isDrawingActivated)
        {

            if (lastTrigger < 1 && trigger >= 1)
            {
                CreateNewLineRenderer();
            }
            drawPoints(targetObj.position, brushRenderer, trigger);
            transform.Find("Brush").gameObject.SetActive(true);
            teleporter.SetActive(false);
        } else
        {
            teleporter.SetActive(true);
            transform.Find("Brush").gameObject.SetActive(false);
            Debug.Log(trigger);
            if (lastTrigger < 1 && trigger >= 1)
            {
                Vector3 posi = teleporter.transform.Find("Quad").position;
                 posi.y += 6.8f;
                MultiCamScene.instance.transform.position = posi;
            }
             
        }

        lastTrigger = trigger;
    }

    
    // Use this for initialization
    GameObject HTCbrush ;
    LineRenderer brushRenderer;
    void Start () {
        HTCbrush = new GameObject("HTCbrushRenderer");
        HTCbrush.AddComponent<LineRenderer>();

        brushRenderer = ((LineRenderer)HTCbrush.GetComponent("LineRenderer"));
        brushRenderer.material = new Material(Shader.Find("Particles/Additive"));
        brushRenderer.startColor = col;
        brushRenderer.endColor = col;
        brushRenderer.positionCount = 0;
        brushRenderer.startWidth = 0.1f;
        brushRenderer.endWidth = 0.1f;
        arr = new Vector3[(int)(FPS * trayDuration)];
    }
    public void CreateNewLineRenderer()
    {
        HTCbrush = new GameObject("HTCbrushRenderer");
        HTCbrush.AddComponent<LineRenderer>();

        brushRenderer = ((LineRenderer)HTCbrush.GetComponent("LineRenderer"));
        brushRenderer.material = new Material(Shader.Find("Particles/Additive"));
        brushRenderer.startColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        brushRenderer.endColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        brushRenderer.positionCount = 0;
        brushRenderer.startWidth =Random.Range(0.02f,0.1f);
        brushRenderer.endWidth = Random.Range(0.02f, 0.3f);
        arr = new Vector3[(int)(FPS * trayDuration)];
    }
    Vector3[] arr;
    void drawPoints(Vector3 pos, LineRenderer renderer,float strength )
    {
    //   Debug.Log(strength);
        if (strength < 1) return;
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
    // Update is called once per frame
    void Update () {
		
	}
}
