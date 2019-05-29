using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Blend_Warp.Script.BlendWarping;

public class BlendWarp_Grid : MonoBehaviour
{
    GameObject gridObj;
    Mesh currentMesh;
    BlendWarp_Editor Editor;
    // Use this for initialization
    List<LineRenderer> lineRenderers = new List<LineRenderer>();
    BlendWarp_Data Data;


    Vector3d curPointPos;

    void Start()
    {

    }

    public void drawGrid(BlendWarp_Data data, BlendWarp_Editor editor)
    {
        Data = data;
        Editor = editor;
        rowCounts = Data.RowCount;
        colCounts = Data.ColCount;

        if (gridObj != null) DestroyImmediate(gridObj);
        gridObj = new GameObject("gridObj");
        gridObj.transform.parent = transform;
        gridObj.transform.localPosition = new Vector3(0, 0, 0);

        currentMesh = GetComponent<MeshFilter>().mesh;
        List<Vector3> vertices = new List<Vector3>();
        currentMesh.GetVertices(vertices);

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = localVertToWorld(vertices[i]);
        }

        for (int i = 0; i < Data.RowCount; i++)
        {
            List<Vector3> verts = new List<Vector3>();
            for (int j = i * Data.ColCount; j < i * Data.ColCount + Data.ColCount; j += 1)
            {
                verts.Add(vertices[j]);
            }
            drawLine(verts, "Line_row_" + i);
        }
        for (int i = 0; i < Data.ColCount; i++)
        {
            List<Vector3> verts = new List<Vector3>();
            for (int j = i; j < vertices.Count; j += Data.ColCount)
            {
                verts.Add(vertices[j]);
            }
            drawLine(verts, "Line_col_" + i);
        }
    }

    public void drawLine(List<Vector3> vertices, string name)
    {
        GameObject line = new GameObject(name, typeof(LineRenderer));
        line.transform.parent = gridObj.transform;
        line.transform.localPosition = new Vector3(0, 0, 0);
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

        lineRenderer.material = BlendWarpManager.instance.gridMat;
        lineRenderer.widthMultiplier = Editor.gridWidth;
        lineRenderer.positionCount = vertices.Count;

        lineRenderer.endColor = BlendWarpManager.instance.defaultColor;
        lineRenderer.startColor = BlendWarpManager.instance.defaultColor;

        for (int i = 0; i < vertices.Count; i++)
        {
            lineRenderer.SetPosition(i, vertices[i]);
        }
        lineRenderers.Add(lineRenderer);
    }

    public Vector3 localVertToWorld(Vector3 point)
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        return localToWorld.MultiplyPoint3x4(point);
    }





    GameObject currentPoint;
    int rowCounts;
    int colCounts;
    int curRow;
    int curCol;


    bool state = false;
    public void turnGrid(bool on)
    {
        if (on == false)
        {
            for (int i = 0; i < lineRenderers.Count; i++)
            {
                lineRenderers[i].endColor = BlendWarpManager.instance.defaultColor;
                lineRenderers[i].startColor = BlendWarpManager.instance.defaultColor;
            }
            DestroyImmediate(currentPoint);
        }
        else
        {
            for (int i = 0; i < lineRenderers.Count; i++)
            {
                lineRenderers[i].endColor = BlendWarpManager.instance.selectedColor;
                lineRenderers[i].startColor = BlendWarpManager.instance.selectedColor;
            }
            if (currentPoint == null)
                currentPoint = Instantiate(BlendWarpManager.instance.IntersectPoint, transform);

            curPointPos = getPointFromGrid(0, 0);
            updateCurPoint();


            curRow = curCol = 0;
        }
        state = on;
    }

    void updateCurPoint()
    {
        currentPoint.transform.position = localVertToWorld(curPointPos.toVector3());
    }

    void Update()
    {
        if (state == false) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            updatePointPosition(curRow + 1, curCol, "up");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            updatePointPosition(curRow - 1, curCol, "down");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            updatePointPosition(curRow, curCol + 1, "right");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            updatePointPosition(curRow, curCol - 1, "left");
        }
        //  Debug.Log(curRow + " " + curCol);


        if (ControlKeySet.WarpPermitted())
        {
            int speedAmplifier = 10;

            int amplify = Input.GetKey(ControlKeySet.GetControlKey(Controls.Amplify)) ? 1 : 0;

            int increaseWarpingX = Input.GetKey(ControlKeySet.GetControlKey(Controls.WarpingIncreaseX)) ? 1 : 0;
            int decreaseWarpingX = Input.GetKey(ControlKeySet.GetControlKey(Controls.WarpingDecreaseX)) ? 1 : 0;
            int increaseWarpingY = Input.GetKey(ControlKeySet.GetControlKey(Controls.WarpingIncreaseY)) ? 1 : 0;
            int decreaseWaroingY = Input.GetKey(ControlKeySet.GetControlKey(Controls.WarpingDecreaseY)) ? 1 : 0;

            replacePoint(new Vector2d(-(decreaseWarpingX * (Time.deltaTime * BlendWarpManager.instance.pointSpeed * (1 + amplify * (speedAmplifier - 1))))
                                      + (increaseWarpingX * (Time.deltaTime * BlendWarpManager.instance.pointSpeed * (1 + amplify * (speedAmplifier - 1)))),
                                      -(decreaseWaroingY * (Time.deltaTime * BlendWarpManager.instance.pointSpeed * (1 + amplify * (speedAmplifier - 1))))
                                      + (increaseWarpingY * (Time.deltaTime * BlendWarpManager.instance.pointSpeed * (1 + amplify * (speedAmplifier - 1))))));
        }
    }


    public void hide(bool hid)
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            lineRenderers[i].enabled = hid;
        }
    }
    private void OnGUI()
    {

        if (Input.GetKey(KeyCode.Space))
        {
            GUI.Button(new Rect(0, 0, 200, 200), "adasdas");
        }
    }



    void updatePointPosition(int x, int y, string dir)
    {
        //   Debug.Log(rowCounts + " " + colCounts);
        if (BlendWarpManager.warpingMode == BlendWarpManager.WarpingMode.CornersRowCol_Move)
        {
            if (dir == "up") { curRow = rowCounts - 1; }
            if (dir == "down") { curRow = 0; }
            if (dir == "right") { curCol = colCounts - 1; }
            if (dir == "left") { curCol = 0; }
            if (currentPoint == null) currentPoint = Instantiate(BlendWarpManager.instance.IntersectPoint, transform);
            curPointPos = getPointFromGrid(curRow, curCol);
            updateCurPoint();
            return;
        }

        if (x >= rowCounts || y >= colCounts || x < 0 || y < 0) return;
        if (currentPoint == null) currentPoint = Instantiate(BlendWarpManager.instance.IntersectPoint, transform);

        curRow = x;
        curCol = y;

        curPointPos = getPointFromGrid(curRow, curCol);
        updateCurPoint();
    }

    public Vector2d getPointFromGrid(int x, int y)
    {
        return Data.Grid[x * colCounts + y];
    }

    public void setPointOnGrid(int x, int y, Vector2d point)
    {
        Data.Grid[x * colCounts + y] = point;
    }

    void replacePoint(Vector2d add)
    {
        if (currentPoint == null) return;

        curPointPos = new Vector3d(curPointPos.x + add.x, curPointPos.y + add.y, curPointPos.z);
        updateCurPoint();

        switch (BlendWarpManager.warpingMode)
        {
            case BlendWarpManager.WarpingMode.CornersRowCol_Move:
                {
                    Vector2d[] srcCorners = new Vector2d[4];
                    srcCorners[0] = getPointFromGrid(0, 0);
                    srcCorners[1] = getPointFromGrid(rowCounts - 1, colCounts - 1);
                    srcCorners[2] = getPointFromGrid(rowCounts - 1, 0);
                    srcCorners[3] = getPointFromGrid(0, colCounts - 1);

                    setPointOnGrid(curRow, curCol, curPointPos);

                    Vector2d[] dstCorners = new Vector2d[4];
                    dstCorners[0] = getPointFromGrid(0, 0);
                    dstCorners[1] = getPointFromGrid(rowCounts - 1, colCounts - 1);
                    dstCorners[2] = getPointFromGrid(rowCounts - 1, 0);
                    dstCorners[3] = getPointFromGrid(0, colCounts - 1);

                    //lineRenderers[curRow].SetPosition(curCol, currentPoint.transform.position);
                    //  lineRenderers[rowCounts + curCol].SetPosition(curRow, currentPoint.transform.position);



                    //gantolebis amoxsna srcCorners gadadis dstCorners wveroebshi
                    double[] matrix = new double[16];
                    Homography.FindHomography(ref srcCorners, ref dstCorners, ref matrix);

                    for (int p = 0; p < rowCounts; p++)
                    {
                        for (int q = 0; q < colCounts; q++)
                        {
                            if ((p == 0 && q == 0) || (p == 0 && q == colCounts - 1) || (p == rowCounts - 1 && q == 0) || (p == rowCounts - 1 && q == colCounts - 1))
                                continue;
                            Vector2d initialPos = getPointFromGrid(p, q);
                            Vector2d posi = BlendWarp_Functions.TransformedPoint(initialPos, matrix);
                            setPointOnGrid(p, q, posi);
                        }
                    }
                }
                break;
            case BlendWarpManager.WarpingMode.Row_Move:
                {
                    for (int i = 0; i < colCounts; i++)
                    {
                        Vector2d posi = getPointFromGrid(curRow, i);
                        posi = new Vector2d((posi.x + add.x), (posi.y + add.y));
                        setPointOnGrid(curRow, i, posi);
                    }
                }
                break;

            case BlendWarpManager.WarpingMode.Col_Move:
                {
                    for (int i = 0; i < rowCounts; i++)
                    {
                        Vector2d posi = getPointFromGrid(i, curCol);
                        posi = new Vector2d((posi.x + add.x), (posi.y + add.y));
                        setPointOnGrid(i, curCol, posi);
                    }
                }
                break;

            case BlendWarpManager.WarpingMode.SinglePoint_Move:
                {
                    Vector2d posi = getPointFromGrid(curRow, curCol);
                    posi = new Vector2d((posi.x + add.x), (posi.y + add.y));
                    setPointOnGrid(curRow, curCol, posi);
                }
                break;
        }

        updateMesh();
        updateGrid();
    }

    public void updateGrid()
    {

        for (int i = 0; i < rowCounts; i++)
            for (int j = 0; j < colCounts; j++)
            {
 
                lineRenderers[i].SetPosition(j, localVertToWorld(getPointFromGrid(i, j).toVector2()));
                lineRenderers[j + rowCounts].SetPosition(i, localVertToWorld(getPointFromGrid(i, j).toVector2()));

            }
    }
    public void updateMesh()
    {
        currentMesh.SetVertices(BlendWarp_Functions.list2dDoubleTo3dFloat(Data.Grid));
        currentMesh.RecalculateNormals();
        currentMesh.RecalculateBounds();
    }

    private Vector3 newPoint(Vector3 initialPos, float[] matrix)
    {
        throw new NotImplementedException();
    }
}