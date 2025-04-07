using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingUser : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.transform);
        
        // Membalik skala X berdasarkan arah menghadap
        Vector3 localScale = transform.localScale;

        if (transform.forward.x < 0)
        {
            localScale.x = -Mathf.Abs(localScale.x); // Pastikan skala X negatif
        }
        else
        {
            localScale.x = Mathf.Abs(localScale.x); // Pastikan skala X positif
        }

        transform.localScale = localScale; // Terapkan perubahan skala
    
    }
}
