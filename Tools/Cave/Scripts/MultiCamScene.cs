using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MultiCamScene : MonoBehaviour {
    [Header("Cave Details")]
    public int FPS = 30;
    public float movementSmooth = 5; 
    public GameObject offset;
     
     
    [Header("Draw Cube Planes")]
    [Range(0, 1)]
    public float transparentBorders = 0.5f;
    public Color camerasBackgroundColor = Color.black;

    [Header("Outside Objects Show Strategy")]
    public bool Asymetric_X=false;
    public bool Asymetric_Y=false;
    public bool Asymetric_Z=false;

    [Header("Draw corrected Plane with points")]
    public bool drawCorrectPoints=false;

    [Header("Editor Options")]
    public bool drawGizmos=true;
    public bool drawCamerasFrustrums = true;

    [Header("Child Objects")] 
    public GameObject insideCameras;
    public GameObject outsideCameras;

    public GameObject LookingPlanes;
    public GameObject mirrorPlanes;

    public GameObject testPlane;

    public static MultiCamScene instance;
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = FPS;
        QualitySettings.vSyncCount = 0;
    }
    // Use this for initialization
    void Start () {
		
	}
	
    public Vector3  globalPosToLocal(Vector3 pos)
    {
        Vector3 Dimensions = transform.localScale;
        pos.x /= Dimensions.x;
        pos.y /= Dimensions.y;
        pos.z /= Dimensions.z;
        return pos;
    }
	// Update is called once per frame
	void Update () {
        if (instance==null) instance = this;
       // //assign trackedPosition
       // Vector3 pos = Vector3.zero;
       // if (TrackingManager.instance!=null) pos= TrackingManager.instance.getPlayerLocation();

       // if (pos == Vector3.zero)
       //     pos = Vector3.one * 5;
       // else
       //     pos = pos ;
         
       //if (TrackingManager.instance != null)
       // { 
       //     insideCameras.transform.localPosition=Vector3.Lerp(insideCameras.transform.localPosition,
       //                     pos, 
       //                     Time.deltaTime * movementSmooth); 
       // }

        // offset.transform
 
       // LookingPlanes.transform.localScale = Dimensions ;
       // mirrorPlanes.transform.localScale = Dimensions ;

        foreach (Transform plane in LookingPlanes.transform.Find("Offset"))
        {
            if (plane.GetComponent<MeshRenderer>()!=null && plane.GetComponent<MeshRenderer>().sharedMaterial!=null)
            plane.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", new Color(1, 1, 1, transparentBorders));
        }
 
        foreach(Transform camera in insideCameras.transform)
        {
            camera.gameObject.GetComponent<Camera>().backgroundColor = camerasBackgroundColor;
        }

        foreach (Transform camera in outsideCameras.transform)
        {
            camera.gameObject.GetComponent<Camera>().backgroundColor = camerasBackgroundColor;
        }

        if (drawCorrectPoints)
        {
            testPlane.active = true;
            testPlane.GetComponent<Frustum>().lookPlane.gameObject.active = true;
        }
        else
        {
            testPlane.active = false;
            testPlane.GetComponent<Frustum>().lookPlane.gameObject.active = false;
        }

        if (!Asymetric_X && !Asymetric_Y && !Asymetric_Z)
        {
            mirrorPlanes.active = false;
            outsideCameras.active = false;
        } else
        {
            mirrorPlanes.active = true;
            outsideCameras.active = true;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Asymetric_X = !Asymetric_X;
            GUImessages.instance.showMessage
                ("(X,Y,Z) Asymetric (" + Asymetric_X + "," + Asymetric_Y + "," + Asymetric_Z + ")",Color.yellow,true);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Asymetric_Y = !Asymetric_Y;
            GUImessages.instance.showMessage
                ("(X,Y,Z) Asymetric (" + Asymetric_X + "," + Asymetric_Y + "," + Asymetric_Z + ")", Color.yellow, true);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Asymetric_Z = !Asymetric_Z;
            GUImessages.instance.showMessage
                ("(X,Y,Z) Asymetric (" + Asymetric_X + "," + Asymetric_Y + "," + Asymetric_Z + ")", Color.yellow, true);
        } 
    } 

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position , this.transform.localRotation, this.transform.lossyScale);
        Gizmos.matrix = rotationMatrix; 

        Gizmos.DrawWireCube( Vector3.one  * 5, Vector3.one * 10);

        foreach (Transform camera in insideCameras.transform)
        {
            camera.gameObject.GetComponent<Frustum>().drawNearCone = drawCamerasFrustrums; 
        }
        foreach (Transform camera in outsideCameras.transform)
        {
            camera.gameObject.GetComponent<Frustum>().drawNearCone = drawCamerasFrustrums; 
        }
    } 
}
