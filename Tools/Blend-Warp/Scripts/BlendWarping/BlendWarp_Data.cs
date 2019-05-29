using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

 

[Serializable]
public class BlendWarp_Data   {
  
        public int ID;
        public int RowCount;
        public int ColCount;
    
        public List< Vector2d > Grid;
        public List<double> Rows;
        public List<double> Cols;

        public float Brightness = 100;

        public float leftBlending = 0;
        public float RightBlending = 0;
        public float TopBlending = 0;
        public float BottomBlending = 0;
}

[Serializable]
public class BlendWarp_Data_NativeVariables
{

    public int ID;
    public int RowCount;
    public int ColCount;

    public List<double> Grid_x;
    public List<double> Grid_y;
    public List<double> Rows;
    public List<double> Cols;

    public float Brightness = 100;

    public float leftBlending = 0;
    public float RightBlending = 0;
    public float TopBlending = 0;
    public float BottomBlending = 0;
}