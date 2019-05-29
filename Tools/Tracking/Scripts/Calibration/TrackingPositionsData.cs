using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class TrackingPositionsData
{
    public int CalStepSegments = 4;  // Minimum:3; Higher value increase working time;
    public int CalPrecision = 15;  // Precision of correct rotation; Higher value increase working time;

    public int kinectDevices;
    public int recordsNeeded;

    public List<Vector3> KinectPositions;
    public List<Vector3> CorrectPositions;

    public List<int> recordCounts;

    public TrackingPositionsData(int devicesCount, int recordsNeeded)
    {
        this.recordsNeeded = recordsNeeded;
        kinectDevices = devicesCount;
        resetData();
    }

    public void addRecord(int id, Vector3 kinectPos, Vector3 HtcPos)
    {
        if (getPositionsCount(id) >= recordsNeeded) return;
        KinectPositions[recordsNeeded * id + getPositionsCount(id)] = kinectPos;
        CorrectPositions[recordsNeeded * id + getPositionsCount(id)] = HtcPos;
        recordCounts[id]++;
    }
    public int getPositionsCount(int id)
    {
        if (recordCounts == null || recordCounts.Count <= id) return 0;
        return recordCounts[id];
    }
    public Vector3 getKinectPos(int id, int ind)
    {
        return KinectPositions[id * recordsNeeded + ind];
    }
    public Vector3 getCorrectPos(int id, int ind)
    {
        return CorrectPositions[id * recordsNeeded + ind];
    }
    public void setCorrectPos(int id, int ind,Vector3 pos)
    {
         CorrectPositions[id * recordsNeeded + ind]= pos;
    }

    public void resetData()
    {
        if (KinectPositions != null)
        {
            KinectPositions.Clear();
            CorrectPositions.Clear();
            recordCounts.Clear();
        }
        else
        {
            KinectPositions = new List<Vector3>();
            CorrectPositions = new List<Vector3>();
            recordCounts = new List<int>();
        }
        for (int i = 0; i < kinectDevices; i++)
        {
            recordCounts.Add(0);
        }
        for (int i = 0; i < kinectDevices * recordsNeeded; i++)
        {
            KinectPositions.Add(Vector3.zero);
            CorrectPositions.Add(Vector3.zero);
        }
        
    }
    public void resetDataFor(int kinect)
    {
        for (int i = 0; i < recordsNeeded; i++)
        {
            KinectPositions[kinect * recordsNeeded + i] = Vector3.zero;
            CorrectPositions[kinect * recordsNeeded + i] = Vector3.zero;
        }
    }

     
    public Vector3[] FoundRotation;
    public Vector3[] FoundDirection;
    public Vector3[] FoundKinectsMiddlePoint;
    public Vector3[] FoundCorrectMiddlePoint;

    public Vector3[] getCalibratedParams(int kinectId)
    {
        if (FoundRotation.Length <= kinectId) return null;
        Vector3[] arr = new Vector3[4];
        arr[0] = FoundRotation[kinectId];
        arr[1] = FoundDirection[kinectId];
        arr[2] = FoundKinectsMiddlePoint[kinectId];
        arr[3] = FoundCorrectMiddlePoint[kinectId];
        return arr;
    }
    public void calculateTranslationFor(int kinectId)
    {
        List<Vector3> kinectPos = new List<Vector3>();
        List<Vector3> correctPos = new List<Vector3>();
        for (int i = 0; i < getPositionsCount(kinectId); i++)
        {
            kinectPos.Add(getKinectPos(kinectId, i));
            correctPos.Add(getCorrectPos(kinectId, i));
        }

        int n = kinectPos.Count;
        Vector3 kinectMiddlePoint = Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            kinectMiddlePoint += kinectPos[i];
        }
        kinectMiddlePoint /= n;

        Vector3 correctMiddlePoint = Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            correctMiddlePoint += correctPos[i];
        }
        correctMiddlePoint /= n;

        for (int i = 0; i < n; i++)
        {
            kinectPos[i] -= kinectMiddlePoint;
            correctPos[i] -= correctMiddlePoint;
        }

         


        float minDist = 0;
        Vector3 foundRot = Vector3.zero;
        Vector3 foundDirection = Vector3.one;

        for (int directionBitmask = 0; directionBitmask < 8; directionBitmask++)
        {
            int isXrotated = directionBitmask % 2;
            int isYrotated = (directionBitmask / 2) % 2;
            int isZrotated = (directionBitmask / 4) % 2;
            Vector3 direction = Vector3.one;
            if (isXrotated == 0) direction.x *= -1;
            if (isYrotated == 0) direction.y *= -1;
            if (isZrotated == 0) direction.z *= -1;
            for (int i = 0; i < n; i++)
            {
                kinectPos[i] = multiplyVectors(kinectPos[i], direction);
            }

            //find correct rotation
            Vector3 lowerDegrees = Vector3.one * -180;
            Vector3 upperDegree = Vector3.one * 180;
            for (int prec = 0; prec < CalPrecision; prec++)
            {
                for (float x = lowerDegrees.x; x < upperDegree.x; x += (upperDegree.x - lowerDegrees.x) / (CalStepSegments * 1.0f))
                {
                    for (float y = lowerDegrees.y; y < upperDegree.y; y += (upperDegree.y - lowerDegrees.y) / (CalStepSegments * 1.0f))
                    {
                        for (float z = lowerDegrees.z; z < upperDegree.z; z += (upperDegree.z - lowerDegrees.z) / (CalStepSegments * 1.0f))
                        {
                            float curDist = 0;
                            for (int i = 0; i < n; i++)
                            {
                                Vector3 pos1 = kinectPos[i];
                                pos1 = Rotate(pos1, new Vector3(x, y, z));

                                Vector3 pos2 = correctPos[i];
                                curDist += Vector3.Distance(pos1, pos2);
                            }
                            if (minDist == 0 || minDist > curDist)
                            {
                                minDist = curDist;
                                foundRot = new Vector3(x, y, z);
                                foundDirection = direction;
                            }
                        }
                    }
                }
                float step = (upperDegree.x - lowerDegrees.x) / (CalStepSegments * 1.0f);
                lowerDegrees.x = foundRot.x - step;
                upperDegree.x = foundRot.x + step;

                step = (upperDegree.y - lowerDegrees.y) / (CalStepSegments * 1.0f);
                lowerDegrees.y = foundRot.y - step;
                upperDegree.y = foundRot.y + step;

                step = (upperDegree.z - lowerDegrees.z) / (CalStepSegments * 1.0f);
                lowerDegrees.z = foundRot.z - step;
                upperDegree.z = foundRot.z + step;
            }

            //change kinect positions back
            for (int i = 0; i < n; i++)
            {
                kinectPos[i] = multiplyVectors(kinectPos[i], direction);
            }
        }

        for (int i = 0; i < n; i++)
        {
            kinectPos[i] = multiplyVectors(kinectPos[i], foundDirection);
        }
        for (int i = 0; i < n; i++)
        {
            kinectPos[i] = Rotate(kinectPos[i], foundRot);
            kinectPos[i] += correctMiddlePoint;
            correctPos[i] += correctMiddlePoint;
        }
        FoundKinectsMiddlePoint[kinectId] = kinectMiddlePoint;
        FoundCorrectMiddlePoint[kinectId] = correctMiddlePoint;
        FoundDirection[kinectId] = foundDirection;
        FoundRotation[kinectId] = foundRot;
    }
    public void calculateTranslations()
    {
        FoundRotation = new Vector3[kinectDevices];
        FoundDirection = new Vector3[kinectDevices];
        FoundKinectsMiddlePoint = new Vector3[kinectDevices];
        FoundCorrectMiddlePoint = new Vector3[kinectDevices];

        for (int i = 0; i < kinectDevices; i++)
            calculateTranslationFor(i);
    }

    private Vector3 multiplyVectors(Vector3 a, Vector3 b)
    {
        a.x = a.x * b.x;
        a.y = a.y * b.y;
        a.z = a.z * b.z;
        return a;
    }
    private Vector3 Rotate(Vector3 aVec, Vector3 aAngles)
    {
        aAngles *= Mathf.Deg2Rad;
        float sx = Mathf.Sin(aAngles.x);
        float cx = Mathf.Cos(aAngles.x);
        float sy = Mathf.Sin(aAngles.y);
        float cy = Mathf.Cos(aAngles.y);
        float sz = Mathf.Sin(aAngles.z);
        float cz = Mathf.Cos(aAngles.z);
        aVec = new Vector3(aVec.x * cz - aVec.y * sz, aVec.x * sz + aVec.y * cz, aVec.z);
        aVec = new Vector3(aVec.x, aVec.y * cx - aVec.z * sx, aVec.y * sx + aVec.z * cx);
        aVec = new Vector3(aVec.x * cy + aVec.z * sy, aVec.y, -aVec.x * sy + aVec.z * cy);
        return aVec;
    }

    private Vector3 rotatedPos(Vector3 origin, Vector3 pos, Vector3 rotation)
    {
        return Rotate(pos - origin, rotation) + origin;
    }
    public Vector3 getPredictedKinectPos(int id, Vector3 pos)
    {
        pos = multiplyVectors((pos - FoundKinectsMiddlePoint[id]), FoundDirection[id]);
        pos = Rotate(pos, FoundRotation[id]);
        pos += FoundCorrectMiddlePoint[id];
        return pos;
    }
    GameObject obj;
    GameObject target;
    public Vector3 rotatePosAround(Vector3 pivotPos, Vector3 targetPos, Vector3 targetRot, float targetScale)
    {
        obj.transform.position = pivotPos;
        target.transform.localPosition = targetPos;
        obj.transform.eulerAngles = targetRot;
        obj.transform.localScale = new Vector3(targetScale, targetScale, targetScale);

        return target.transform.position;
    }
}
