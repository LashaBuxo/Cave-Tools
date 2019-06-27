using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutsidePlayer : MonoBehaviour
{
    public MultiCamScene mainScript;

    private GameObject targetObj;
   
    // Start is called before the first frame update
    void Start()
    {
        targetObj = mainScript.insideCameras;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = targetObj.transform.localPosition;

        if (mainScript.Asymetric_X)
            newPos.x = 10 - newPos.x;
        if (mainScript.Asymetric_Y)
            newPos.y = 10 - newPos.y;
        if (mainScript.Asymetric_Z)
            newPos.z = 10 - newPos.z;

        transform.localPosition = newPos;
    }
}
