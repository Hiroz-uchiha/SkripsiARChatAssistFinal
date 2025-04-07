using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeakerHandler : MonoBehaviour
{
    [SerializeField] private GameObject responsePanel;
    public GoogleTextToSpeech textToSpeechScript;

    private void Start()
    {
        if (responsePanel != null)
        {
            Transform[] testResponses = responsePanel.GetComponentsInChildren<Transform>();

            foreach (Transform testResponse in testResponses)
            {
                Button speakerButton = testResponse.Find("SpeakerButton")?.GetComponent<Button>();
                if (speakerButton != null)
                {
                    speakerButton.onClick.AddListener(() => OnSpeakerImageClicked(testResponse));
                }
            }
        }
        else
        {
            Debug.LogError("Response Panel is not assigned!");
        }
    }

    private void OnSpeakerImageClicked(Transform testResponse)
    {
        TMP_Text textElement = testResponse.Find("ResponseText")?.GetComponent<TMP_Text>();

        if (textElement != null)
        {
            string text = textElement.text;

            if (textToSpeechScript != null)
            {
                textToSpeechScript.SynthesizeText(text);
            }
            else
            {
                Debug.LogError("GoogleTextToSpeech script is not assigned!");
            }
        }
        else
        {
            Debug.LogError("Text Response not found in Test Response!");
        }
    }
}
