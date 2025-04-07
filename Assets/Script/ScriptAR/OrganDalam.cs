using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganDalam : MonoBehaviour
{
    public float speed = 1f;

    // Start is called before the first frame update
    void Update()
    {
        Rotasi();
    }

    private void Rotasi(){
        transform.Rotate(Vector3.up * speed);
    }
}
