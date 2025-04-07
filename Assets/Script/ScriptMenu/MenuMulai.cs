using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMulai : MonoBehaviour
{
   public void HalamanAR(){
    SceneManager.LoadScene("HalamanAR");
   }

   public void HalamanMenu(){
    SceneManager.LoadScene("HalamanMenu");
   }
   
   public void HalamanTambahBuku(){
      SceneManager.LoadScene("HalamanTambahBuku");
   }

   public void HalamanRiwayat(){
       SceneManager.LoadScene("HalamanRiwayat");
   }
   public void HalamanAbout(){
    SceneManager.LoadScene("HalamanAbout");
   }

   public void HalamanChatAssistKembaliKeAR(){
    SceneManager.LoadScene("HalamanAR");
   }
   
   public void ExitApk(){
      Application.Quit();
   }

}
