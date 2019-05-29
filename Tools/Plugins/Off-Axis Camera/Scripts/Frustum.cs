using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Frustum : MonoBehaviour
{
    [SerializeField] public  Transform lookPlane;
   
    [SerializeField] public bool drawNearCone, drawFrustum;

    //Camera camera = Camera.main;
    public bool markAligments;
    public bool setNearPlane;
    public int farPlane=5000;
    
    void Start()
    {
        
    }

    void LateUpdate()
    {
        GetComponent<Camera>().farClipPlane = farPlane;
        Transform[] Corners = new Transform[4];

        Corners[0] = lookPlane .Find("BotLeft");
        Corners[1] = lookPlane .Find("BotRight");
        Corners[2] = lookPlane .Find("TopLeft");
        Corners[3] = lookPlane .Find("TopRight");

        Vector3d pa, pb, pc, pd;
        pa = new Vector3d(Corners[0].position); 
        pb = new Vector3d(Corners[1].position); 
        pc = new Vector3d(Corners[2].position); 
        pd = new Vector3d(Corners[3].position); 

        Vector3d pe = new Vector3d(GetComponent<Camera>().transform.position);// eye position

        Vector3d vr = (pb - pa).normalized; // right axis of screen
        Vector3d vu = (pc - pa).normalized; // up axis of screen
        Vector3d vn = Vector3d.Cross(vr, vu).normalized; // normal vector of screen

        Vector3d va = pa - pe; // from pe to pa
        Vector3d vb = pb - pe; // from pe to pb
        Vector3d vc = pc - pe; // from pe to pc
        Vector3d vd = pd - pe; // from pe to pd

        double n =  0.01f; // distance to the near clip plane (screen)
        if (setNearPlane)
        {
            n= 2*Vector3.Dot(va.toVector3(), vn.toVector3())+0.001f;
        }
        double f = GetComponent<Camera>().farClipPlane; // distance of far clipping plane

        double d = Vector3d.Dot(va, vn);
        double l = Vector3d.Dot(vr, va) * n/d;
        double r = Vector3d.Dot(vr, vb) * n/d;
        double b = Vector3d.Dot(vu, va) * n/d;
        double t = Vector3d.Dot(vu, vc) * n/d;

        //     Low precision code
        //       double d = Vector3 .Dot(va.toVector3(), vn.toVector3()); // distance from eye to screen
        //       double l = Vector3 .Dot(vr.toVector3(), va.toVector3()) * n / d; // distance to left screen edge from the 'center'
        //       double r = Vector3 .Dot(vr.toVector3(), vb.toVector3()) * n / d; // distance to right screen edge from 'center'
        //       double b = Vector3 .Dot(vu.toVector3(), va.toVector3()) * n / d; // distance to bottom screen edge from 'center'
        //       double t = Vector3 .Dot(vu.toVector3(), vc.toVector3()) * n / d; // distance to top screen edge from 'center'

        GetComponent<Camera>().projectionMatrix = GetProjection(n, f, d, l, r, b, t); ; // Assign matrix to camera
        //Debug.Log(GetComponent<Camera>().projectionMatrix+" lashaaaaa");
        if (drawNearCone || markAligments)
        { //Draw lines from the camera to the corners f the screen

            drawRay(GetComponent<Camera>().transform.position, va.toVector3(), Color.green,0);
            drawRay(GetComponent<Camera>().transform.position, vb.toVector3(), Color.green, 1);
            drawRay(GetComponent<Camera>().transform.position, vc.toVector3(), Color.green, 2);
            drawRay(GetComponent<Camera>().transform.position, vd.toVector3(), Color.green, 3);
        }

        if (drawFrustum) DrawFrustum(GetComponent<Camera>()); //Draw actual camera frustum
      
    }
   
   List<GameObject> objects ;
    

    public void drawRay(Vector3 pos,Vector3 dir,Color col,int ind)
    {
        if (drawNearCone)
        {
            Debug.DrawRay(pos, dir, col);
        }
        if (markAligments)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos, dir, out hit, Mathf.Infinity))
            {
                drawPoint(hit.point, ind);
            }
        }
    }
    void drawPoint(Vector3 pos, int i)
    {
        transform.GetChild(i).position = pos;
       
        //objects[i].transform.localScale = Vector3.zero * 0.01f;
   }
    Matrix4x4 GetProjection (double n, double f, double d, double l, double r, double b, double t)
    {
        double[,] p = new double[4, 4];
        p[0, 0] = (double)(2.0f * n / (r - l));
        p[0, 2] = (double)((r + l) / (r - l));
        p[1, 1] = (double)(2.0f * n / (t - b));
        p[1, 2] = (double)((t + b) / (t - b));
        p[2, 2] = (double)((f + n) / (n - f));

        p[2, 3] = (double)((2.0f * f * n / (n - f)) * 0.5f);
        p[3, 2] = -1.0f;

        return GetMatrix4X4FromDoubles(p);
    }

    Matrix4x4 GetMatrix4X4FromDoubles (double[,] p)
    {
        Matrix4x4 matrix = new Matrix4x4();
        for (int i = 0; i < 4; i ++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)p[i, j];
            }
        }
        return matrix;
    }

    Vector3d ThreePlaneIntersection(Plane p1, Plane p2, Plane p3)
    { //get the intersection point of 3 planes
        return ((-p1.distance * Vector3d.Cross(new Vector3d( p2.normal), new Vector3d(p3.normal))) +
                (-p2.distance * Vector3d.Cross(new Vector3d(p3.normal), new Vector3d(p1.normal))) +
                (-p3.distance * Vector3d.Cross(new Vector3d(p1.normal), new Vector3d(p2.normal)))) /

            (Vector3d.Dot(new Vector3d(p1.normal), Vector3d.Cross(new Vector3d(p2.normal), new Vector3d(p3.normal ))) )   ;
    }

    void DrawFrustum(Camera cam)
    {
        Vector3d[] nearCorners = new Vector3d[4]; //Approx'd nearplane corners
        Vector3d[] farCorners = new Vector3d[4]; //Approx'd farplane corners
        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(cam); //get planes from matrix
        Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = ThreePlaneIntersection(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]); //near corners on the created projection matrix
            farCorners[i] = ThreePlaneIntersection(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]); //far corners on the created projection matrix
        }

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine( nearCorners[i].toVector3(), nearCorners[(i + 1) % 4].toVector3(), Color.yellow, Time.deltaTime, false); //near corners on the created projection matrix
            Debug.DrawLine(farCorners[i].toVector3(), farCorners[(i + 1) % 4].toVector3(), Color.yellow, Time.deltaTime, false); //far corners on the created projection matrix
            Debug.DrawLine(nearCorners[i].toVector3(), farCorners[i].toVector3(), Color.yellow, Time.deltaTime, false); //sides of the created projection matrix
        }
    }
}
