using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonRotasi : MonoBehaviour
{
    public GameObject model3D;
    public float speed = 20f;
    private bool rotateRight;
    private bool rotateLeft;

    void Update(){
        Rotasi();
    }

    private void Rotasi(){
        if(rotateRight){
             model3D.transform.Rotate(Vector3.up * speed * Time.deltaTime);
        }else if(rotateLeft){
            model3D.transform.Rotate(Vector3.down * speed * Time.deltaTime);
        }
    }

    public void RotasiKanan(){
        rotateRight = true;
        rotateLeft = false;
    }

    public void RotasiKiri(){
        rotateRight = false;
        rotateLeft = true;
    }

    public void Stop(){
        rotateRight = false;
        rotateLeft = false;
    }


}
