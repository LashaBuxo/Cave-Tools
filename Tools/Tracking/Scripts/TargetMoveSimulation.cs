using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMoveSimulation : MonoBehaviour {

    // Use this for initialization
    KinectSensor parentSensor;
      
    Vector3 pos;
    private void Start()
    {
        StartCoroutine(movementSimulation());
    }
    Vector3 curPosition = Vector3.zero;
    Vector3 curTarget;
  
    IEnumerator movementSimulation()
    {
        while (true)
        {
            float i = Random.Range(0, 10);
            float j = Random.Range(0, 10);
            float k = Random.Range(0, 10);

            curTarget = new Vector3(i, j, k);
            yield return new WaitForSeconds(1f);
                 
        }
    }
    void Update () {
		if (Mathf.Abs((curPosition - curTarget).magnitude)>0.1f)
        {
            curPosition = curPosition + (curTarget- curPosition)*Time.deltaTime;
            transform.parent.GetComponent<KinectSensor>().setPlayerPosition_FromOrigin(curPosition);
        } 
	}

}
