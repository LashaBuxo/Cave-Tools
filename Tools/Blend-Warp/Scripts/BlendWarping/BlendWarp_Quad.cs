using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BlendWarpQuad : MonoBehaviour {
    BlendWarp_Editor Editor;
    BlendWarp_Data Data;
      

    public void startQuad(BlendWarp_Editor editor,BlendWarp_Data data,bool hasCamera,int camx=-1,int camy=-1)
    {
        Editor = editor;
        Data = data;
         
        attachMesh();
        attachMaterial();
        attachGrid();

         if (hasCamera)
        attachCamera(camx,camy);
    }

    public void attachMesh()
    {
        if (Data.Grid == null)
        {
            //creates mesh based rectangular grid vertices;
            Data.Grid = BlendWarp_Functions.createVerts(Data.RowCount,
                                                                   Data.ColCount,
                                                                   Data.Cols[Data.Cols.Count - 1],
         
                                                                   Data.Rows[Data.Rows.Count - 1]);
        }
        GetComponent<MeshFilter>().mesh = createMesh(Data.Grid);
    } 
    public void attachMaterial()
    {
        GetComponent<MeshRenderer>().material = Editor.BlendingMaterial;
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Editor.renderTexture);
    }
    Mesh createMesh( List<Vector2d>  vertices)
    {
        Mesh mesh = new Mesh(); ;
        mesh.name =  name + "_mesh";
       
        mesh.SetVertices(BlendWarp_Functions.list2dDoubleTo3dFloat( vertices));
        mesh.SetIndices(BlendWarp_Functions.GridToTriangles(Data.RowCount, Data.ColCount), MeshTopology.Triangles, 0);
         
        mesh.SetUVs(0,BlendWarp_Functions.list2dDoubleToFloat( BlendWarp_Functions.createUVs(Data.RowCount, Data.ColCount, 1, 1)));

        GetComponent<MeshFilter>().mesh = mesh;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
         
#if UNITY_EDITOR
        if (Editor.exportsMesh)
        {
            if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
                Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");

            AssetDatabase.CreateAsset(mesh, "Assets/StreamingAssets/" +  name + ".mesh");
            AssetDatabase.SaveAssets();
        }
#endif
        return mesh;
    }

    public void attachGrid()
    {
        GetComponent<BlendWarp_Grid>().drawGrid(Data ,Editor);
    }

    public void attachCamera(int x,int y)
    {
        GameObject cam= Instantiate(BlendWarpManager.instance.planeLookingCamera, transform);
        cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cam.GetComponent<Camera>().backgroundColor = Color.black;
        cam.GetComponent<Camera>().targetDisplay = Editor.ID-1;
        Vector3 pos = cam.transform.position;

        Vector3 scal = cam.transform.Find("Plane").localScale;
        scal.z = y*2*Editor.projectorResolution.y / (1.0f*Editor.projectorResolution.x);
        scal.x = x*2*1;

   

        cam.transform.Find("Plane").localScale = scal;

        pos.z = -3;
        pos.x += 10 * (x-1);
        pos.y += -5.625f * (y - 1);
        cam.transform.position = pos;

    }
}
