using UnityEngine;

public class GameObjectLoader : MonoBehaviour
{
    public GameObject chatAssistGameObjectPrefab; // Prefab dari GameObject di Chat Assist

    private void Start()
    {
        // Periksa apakah GameObject sudah ada di scene
        if (GameObject.Find(chatAssistGameObjectPrefab.name) == null)
        {
            // Jika tidak ada, instantiate prefab
            Instantiate(chatAssistGameObjectPrefab);
            Debug.Log($"GameObject {chatAssistGameObjectPrefab.name} berhasil dimuat.");
        }
        else
        {
            Debug.Log($"GameObject {chatAssistGameObjectPrefab.name} sudah ada.");
        }
    }
}
