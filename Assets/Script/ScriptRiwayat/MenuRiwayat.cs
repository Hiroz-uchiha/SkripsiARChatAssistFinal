using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuRiwayat : MonoBehaviour
{
    public void GoToScene(string sceneName){
        PlayerPrefs.SetString("SceneContent",sceneName);
        SceneManager.LoadScene("HalamanChatRiwayat");
    }

}
