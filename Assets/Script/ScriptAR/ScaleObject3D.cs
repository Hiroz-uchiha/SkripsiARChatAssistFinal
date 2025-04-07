using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObject3D : MonoBehaviour
{
    public Transform targetObject;
    private float scaleStep = 1f;
    public Vector3 minScale = new Vector3(1f,1f,1f);
    public Vector3 maxScale = new Vector3(3f, 3f, 3f); // Batas maksimal

    public void highScale()
    {
        if (targetObject != null)
        {
            Vector3 newScale = targetObject.localScale + new Vector3(scaleStep, scaleStep, scaleStep);
            targetObject.localScale = Vector3.Min(newScale, maxScale); // Batas maksimal
        }
    }

    public void lowScale(){
        if(targetObject != null){
            Vector3 newScale = targetObject.localScale - new Vector3(scaleStep, scaleStep, scaleStep);
            targetObject.localScale = Vector3.Max(newScale,minScale);
        }
    }

}
