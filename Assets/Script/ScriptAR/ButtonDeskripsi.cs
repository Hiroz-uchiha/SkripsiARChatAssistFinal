using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDeskripsi : MonoBehaviour
{
    public GameObject deskripsiImg;

    void Start(){
        deskripsiImg.SetActive(false);
    }

    public void TombolDeskripsi()
    {
        if(deskripsiImg != null)
        {
            deskripsiImg.SetActive(!deskripsiImg.activeSelf);
        }else{
            Debug.LogWarning("Deskripsi Organ Belum diset");
        }
    }
}
