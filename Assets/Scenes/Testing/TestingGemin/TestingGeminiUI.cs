using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class TestingGeminiUI : MonoBehaviour
{
     [Header("UI References")]
    [SerializeField] private TMP_InputField promptInput;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private UnityEngine.UI.Button submitButton;
    [SerializeField] private GameObject loadingIndicator;

    private TestingGemini geminiManager;

    void Start()
    {
        geminiManager = gameObject.AddComponent<TestingGemini>();
        submitButton.onClick.AddListener(OnSubmitPressed);
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    private async void OnSubmitPressed()
    {
        if (string.IsNullOrEmpty(promptInput.text))
        {
            resultText.text = "Mohon masukkan prompt terlebih dahulu!";
            return;
        }

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
        submitButton.interactable = false;

        try
        {
            string result = await geminiManager.GenerateContent(promptInput.text);
            
            if (result != null)
            {
                resultText.text = result;
            }
            else
            {
                resultText.text = "Terjadi kesalahan saat memproses permintaan.";
            }
        }
        catch (System.Exception e)
        {
            resultText.text = $"Error: {e.Message}";
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            submitButton.interactable = true;
        }
    }
}
