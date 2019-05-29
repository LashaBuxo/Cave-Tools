using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranformLinker : MonoBehaviour {
    public enum Space
    {
        World,
        Local
    }
    [Header("Source Transform")]
    public Space sourceSpace=Space.Local;
    public Transform source;

    [Header("Target Transform")]
    public Space targetSpace = Space.Local;
    public Transform target;

    [Header("Parameters to Link")]
    public bool linkPosition = true;
    [Range(1,10)]
    public float Smooth=5;
    public bool linkRotation = false;
    public bool linkScale = false;

     
	// Update is called once per frame
	void Update () {
        if (source==null || target == null)
        {
            Debug.LogWarning("Linker source or target is not declared.");
            return;
        }
		if (linkPosition)
            setPosition(target, targetSpace, getPosition(source, sourceSpace));

        if (linkRotation)
            setRotation(target, targetSpace, getRotation(source, sourceSpace));

        if (linkScale)
            setScale(target, targetSpace, getScale(source, sourceSpace));

    }
    public Vector3 getPosition(Transform transform,Space space)
    {
        if (space == Space.Local)
            return transform.localPosition;
        return transform.position;
    }
    public void setPosition(Transform transform, Space space,Vector3 parameter)
    {
        if (space == Space.Local)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, parameter, Time.deltaTime * Smooth);
            return;
        }
        transform.position =Vector3.Lerp(transform.position, parameter,Time.deltaTime*Smooth);
    }

    public Vector3 getRotation(Transform transform, Space space)
    {
        if (space == Space.Local)
            return transform.localEulerAngles;
        return transform.eulerAngles;
    }
    public void setRotation(Transform transform, Space space, Vector3 parameter)
    {
        if (space == Space.Local)
        {
            transform.localEulerAngles = parameter;
            return;
        }
        transform.eulerAngles=parameter;
    }

    public Vector3 getScale(Transform transform, Space space)
    {
        if (space == Space.Local)
            return transform.localScale;
        return transform.lossyScale;
    }
    public void setScale(Transform transform, Space space, Vector3 parameter)
    {
        if (space == Space.Local) {
            transform.localScale = parameter;
            return;
        }
        SetGlobalScale(transform,parameter);
    }

    public void SetGlobalScale( Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
}
