using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;
using System.Net.Http;

public class GoogleSpeechToText : MonoBehaviour
{
    private static readonly HttpClient client = new HttpClient();
    // untuk menerapkan pola Singleton, dmn hanya ada satu instance dari GoogleSpeechToText dalam game. 
    public static GoogleSpeechToText Instance { get; private set; }

    public ChatManager chatManager; //Referensi ke ChatManager
    private string apiKey = "AIzaSyA7qfpaB2GJureQKD1ipCHrnapffEvA6JU"; //API Key Google untuk mengakses layanan Speech-to-text
    private bool isProcessing = false; //status untuk mencegah permintaan ganda dalam 1 waktu

    // struktur data untuk request & response
    [System.Serializable]
    private class RequestData
    {
        public Config config; // /pengaturan audio
        public Audio audio; // data suara
    }

    // Menyimpan konfigurasi teknis request
    [System.Serializable]
    private class Config
    {
        public string encoding; //format audio
        public int sampleRateHertz; //kecepatan sampel audio
        public string languageCode; //kode bahasa yang digunakan
    }

    // Menyimpan data audio dalam bentuk string yang telah dikodekan ke Base64.
    [System.Serializable]
    private class Audio
    {
        public string content;
    }

    // Menyimpan respons dari Google Speech-to-Text API, yang berisi hasil transkripsi suara yang terdeteksi.
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

    // hanya ada satu instance dari GoogleSpeechToText yang aktif
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Mengirim data audio untuk diproses
    public void SendAudioForTranscription(byte[] audioData)
    {
        // melakukan pengecekan apakah sedang diproses atau tidak
        if (isProcessing)
        {
            Debug.LogWarning("Already processing. Please wait.");
            return;
        }
        // menangani jika tidak ada audio yang tertangkap
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Audio data is empty!");
            return;
        }

        isProcessing = true;

        // inisialisasi chatmanager untuk mengirim data statis berisi status proses
        ChatManager getChatManager = FindObjectOfType<ChatManager>();
        if (getChatManager != null)
        {
            chatManager = getChatManager;
            chatManager.SendMessage("Suara sedang diproses...");
        }

        // memulai coroutine untuk mengirim permintaan ke API
        StartCoroutine(UploadAudio(audioData));
    }

    //  mengonversi audio ke Base64, membentuk JSON, dan mengirim POST ke Google Speech-to-Text API.
    private IEnumerator UploadAudio(byte[] audioData)
    {
        // Buat/menyusun objek request data
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

        // Mengonversi objek requestData menjadi JSON.
        string jsonData = JsonUtility.ToJson(requestData);

        // url google speech to text
        string url = "https://speech.googleapis.com/v1/speech:recognize?key=" + apiKey;

        // Mengirim permintaan HTTP POST dengan data audio.
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData); // membentuk body dari request json menjadi array byte
            request.uploadHandler = new UploadHandlerRaw(bodyRaw); // upload body request
            request.downloadHandler = new DownloadHandlerBuffer(); // untuk response nantinya
            request.SetRequestHeader("Content-Type", "application/json"); // set header request

            Debug.Log("Sending request to Google Speech-to-Text...");
            yield return request.SendWebRequest(); // mengirim dan menunggu response

            // jika request berhasil
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);

                // Memproses respons dari API dan menampilkan transkripsi jika tersedia.
                SpeechResponse response = JsonUtility.FromJson<SpeechResponse>(request.downloadHandler.text);

                if (response.results != null && response.results.Length > 0 &&
                    response.results[0].alternatives != null &&
                    response.results[0].alternatives.Length > 0)
                {
                    //    string referenceText = "Sebutkan fungsi usus halus dan usus besar serta bagaimana keduanya saling melengkapi dalam proses pencernaan makanan";
                    string transcript = response.results[0].alternatives[0].transcript;
                    Debug.Log("Transcript: " + transcript);

                    // mengirim hasil response ke chatmanager
                    chatManager.UpdateLastResponseMessage(transcript, "user");

                    // inisialisasi script apiManager
                    APIManager apiManager = FindObjectOfType<APIManager>();
                    if (apiManager != null)
                    {
                        // mengirim response ke apimanager (gemini)
                        apiManager.GetRequest(transcript);
                        Debug.Log("menuju ke api manager");
                    }
                 //   StartCoroutine(SendToFlask(referenceText, transcript));
                }
                else
                {
                    Debug.LogWarning("No speech detected in the audio.");

                    // mengirim pesan kesalahan ke chatmanager jika hasil transkripsi suara null
                    chatManager.UpdateLastResponseMessage("Tidak ada suara yang terdeteksi.", "user");
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                
                // mengirim pesan kesalahan ke chatmanager jika request tidak berhasil
                chatManager.UpdateLastResponseMessage("Terjadi kesalahan saat memproses suara.", "user");

            }

            isProcessing = false; // Tandai pemrosesan selesai

        }
        //   }
        //private IEnumerator SendToFlask(string reference, string hypothesis)
        //  {
        // Buat JSON untuk permintaan ke Flask
        //     string json = "{\"reference\":\"" + reference + "\",\"hypothesis\":\"" + hypothesis + "\"}";

        //      string flaskUrl = "http://127.0.0.1:5000/calculate-wer";

        //    using (UnityWebRequest request = new UnityWebRequest(flaskUrl, "POST"))
        //    {
        //         byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        //        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //        request.downloadHandler = new DownloadHandlerBuffer();
        //        request.SetRequestHeader("Content-Type", "application/json");

        //        Debug.Log("Sending request to Flask for WER calculation...");
        //        yield return request.SendWebRequest();

        //       if (request.result == UnityWebRequest.Result.Success)
        //      {
        //          Debug.Log("WER Response: " + request.downloadHandler.text);
        //       }
        //      else
        //      {
        //         Debug.LogError("Error sending to Flask: " + request.error);
        //     }
        //     }
        /// }
    }
}