using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamaOrgan : MonoBehaviour
{

    public void GetName(string namaOrgan)
    {
       PlayerPrefs.SetString("CurrentCategory", namaOrgan);
       Debug.Log("Current Category: " + PlayerPrefs.GetString("CurrentCategory"));
    }

}
