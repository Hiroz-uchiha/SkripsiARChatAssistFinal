using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class TestingSpeechToText : MonoBehaviour
{
   [SerializeField] private TMP_Text resultText;
    private string apiKey = "AIzaSyA7qfpaB2GJureQKD1ipCHrnapffEvA6JU";  // Ganti dengan API Key Anda
    private bool isProcessing = false; // Untuk melacak status pemrosesan

    [System.Serializable]
    private class RequestData
    {
        public Config config;
        public Audio audio;
    }

    [System.Serializable]
    private class Config
    {
        public string encoding;
        public int sampleRateHertz;
        public string languageCode;
    }

    [System.Serializable]
    private class Audio
    {
        public string content;
    }

    [System.Serializable]
    private class SpeechResponse
    {
        public SpeechResult[] results;
    }

    [System.Serializable]
    private class SpeechResult
    {
        public Alternative[] alternatives;
    }

    [System.Serializable]
    private class Alternative
    {
        public string transcript;
    }

    public void SendAudioForTranscription(byte[] audioData)
    {
        if (isProcessing)
        {
            Debug.LogWarning("Already processing. Please wait.");
            return;
        }
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Audio data is empty!");
            resultText.text = "No audio recorded.";
            return;
        }

        isProcessing = true; // Tandai sedang memproses
        StartCoroutine(UploadAudio(audioData));
    }

    private IEnumerator UploadAudio(byte[] audioData)
    {
        // Buat objek request data
        var requestData = new RequestData
        {
            config = new Config
            {
                encoding = "LINEAR16",
                sampleRateHertz = 44100,
                languageCode = "id-ID"
            },
            audio = new Audio
            {
                content = System.Convert.ToBase64String(audioData)
            }
        };

        // Konversi ke JSON
        string jsonData = JsonUtility.ToJson(requestData);

        string url = "https://speech.googleapis.com/v1/speech:recognize?key=" + apiKey;
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending request to Google Speech-to-Text...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                SpeechResponse response = JsonUtility.FromJson<SpeechResponse>(request.downloadHandler.text);

                if (response.results != null && response.results.Length > 0 &&
                    response.results[0].alternatives != null &&
                    response.results[0].alternatives.Length > 0)
                {
                    string transcript = response.results[0].alternatives[0].transcript;
                    resultText.text = transcript;
                    Debug.Log("Transcript: " + transcript);
                }
                else
                {
                    resultText.text = "No speech detected";
                    Debug.LogWarning("No speech detected in the audio.");
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                resultText.text = "Error during transcription";
            }

                        isProcessing = false; // Tandai pemrosesan selesai

        }
    }

}
