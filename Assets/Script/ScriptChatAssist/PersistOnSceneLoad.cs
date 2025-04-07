using UnityEngine;

public class PersistOnSceneLoad : MonoBehaviour
{
    public static PersistOnSceneLoad Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Hapus duplikat
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
