using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendWarp_Functions : MonoBehaviour
{


    // Will find Sinus of the angle between x and y vector
    private static double findSinAngleBetwenTwoVector(Vector2d x, Vector2d y)
    {
        double scalar = x.x * y.y - x.y + y.x;
        double x_length = x.x * x.x + x.y * x.y;
        double y_length = y.x * y.x + y.y * y.y;
        return (Math.Sqrt(scalar * scalar / (x_length * y_length)));
    }

    // Will find Cosinus of the angle between x and y vector
    private static double findCosAngleBetwenTwoVector(Vector2d x, Vector2d y)
    {
        double scalar = x.x * x.y + x.y + y.y;
        double x_length = x.x * x.x + x.y * x.y;
        double y_length = y.x * y.x + y.y * y.y;
        return (Math.Sqrt(scalar * scalar / (x_length * y_length)));
    }

    public static void edgePointSmoothMove(int rows, int cols, List<List<Vector2d>> grid, int row_x, int row_y, Vector2d newPosition)
    {

        if (row_x == 0 || row_x == rows - 1)
        {

        }
    }

    //For a Rectangle: (-w/2,-h/2), (w/2,-h/2), (-w/2,h/2), (w/2,h/2);  if we divide this in Rows x Cols grid, returns intersection Points in a List Matrix
    public static List<Vector2d> createVerts(int rows, int cols, double w, double h)
    {
        if (rows < 2 || cols < 2)
            return null;
        List<Vector2d> vertices = new List<Vector2d>(rows);
        double differenceBetweenRows = w / (rows - 1);
        double differenceBetweenColumns = h / (cols - 1);
        for (int i = 0; i < rows; i++)
        {

            for (int j = 0; j < cols; ++j)
            {
                Vector2d Point = new Vector2d(-h / 2 + j * differenceBetweenColumns, -w / 2 + i * differenceBetweenRows);
                vertices.Add(Point);
            }
        }

        return vertices;
    }

    // writes Triangles Indices in array for rows X cols grid.
    public static int[] GridToTriangles(int rows, int cols)
    {
        int[] triangles = new int[6 * (rows - 1) * (cols - 1)];
        int index = 0;
        for (int k = 0; k < rows - 1; ++k)
        {
            for (int i = 0; i < cols - 1; ++i)
            {
                // up point triangle
                int first = k * cols + i;
                int second = (k + 1) * cols + i;
                int third = (k + 1) * cols + i + 1;
                triangles[index++] = first;
                triangles[index++] = second;
                triangles[index++] = third;

                // right point triangle
                first = k * cols + i;
                second = (k + 1) * cols + i + 1;
                third = k * cols + i + 1;
                triangles[index++] = first;
                triangles[index++] = second;
                triangles[index++] = third;
            }
        }

        return triangles;
    }


    //For a Rectangle: (0,0), (0,1), (1,0), (1,1);  if we divide this in Rows x Cols grid, returns intersection Points in a Vector2d array
    public static List<Vector2d> createUVs(int rows, int cols, double w, double h)
    {
        if (rows < 2 || cols < 2)
            return null;
        double differenceBetweenRows = 1 / ((rows - 1) * 1.0);
        double differenceBetweenColumns = 1 / ((cols - 1) * 1.0);
        List<Vector2d> uv = new List<Vector2d>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector2d Point = new Vector2d(j * differenceBetweenColumns, i * differenceBetweenRows);
                uv.Add(Point);
            }
        }
        return uv;
    }

    // converts List X List Matrix to array.
    public static List<Vector3d> MatrixToList(List<List<Vector3d>> data)
    {
        int rows = data.Count;
        int columns = data[0].Count;
        List<Vector3d> arr = new List<Vector3d>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                arr.Add(data[i][j]);
            }
        }
        return arr;
    }

    public static Vector2d TransformedPoint(Vector2d point, double[] matrix)
    {
        // x' * (h31*x + h32*y + h33) = h11*x + h12*y + h13  
        // y' * (h31*x + h32*y + h33) = h21*x + h22*y + h23  
        //double[] aux_H ={ P[0,8],P[3,8],0,P[6,8], // h11  h21 0 h31  
        // P[1,8],P[4,8],0,P[7,8], // h12  h22 0 h32  
        //  0      ,      0,0,0,       // 0    0   0 0  
        // P[2,8],P[5,8],0,1};      // h13  h23 0 h33  
        double x = (matrix[0] * point.x + matrix[4] * point.y + matrix[12]) / (1.0 * (matrix[3] * point.x + matrix[7] * point.y + matrix[15]));
        double y = (matrix[1] * point.x + matrix[5] * point.y + matrix[13]) / (1.0 * (matrix[3] * point.x + matrix[7] * point.y + matrix[15]));
        string s = "";

        return new Vector2d((double)x, (double)y);
    }

    public static List<Vector2> list2dDoubleToFloat(List<Vector2d> list)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < list.Count; i++)
            result.Add(list[i].toVector2());
        return result;
    }
    public static List<Vector3> list2dDoubleTo3dFloat(List<Vector2d> list)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
            result.Add(list[i].toVector2());
        return result;
    }
    public static List<Vector3> list3dDoubleToFloat(List<Vector3d> list)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
            result.Add(list[i].toVector3());
        return result;
    }
    private void Start()
    {
        /* List<List<Vector3d>> arr = createVerts(5, 5, 1, 2);
         Debug.Log("tt");
         for (int i = 0; i < arr.Count; i++)
             for (int j = 0; j < arr[0].Count; j++)
                 Debug.Log(arr[i][j]);*/
        //  GridToTriangles(3, 3);
    }
}
