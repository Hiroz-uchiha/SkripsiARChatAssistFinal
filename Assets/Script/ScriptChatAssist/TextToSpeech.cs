using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleTextToSpeech : MonoBehaviour
{
    [Header("Google API Settings")]
    [SerializeField] private string apiKey; // API Key untuk mengakses Google Cloud TTS
    [SerializeField] private AudioSource audioSource; // Komponen AudioSource untuk memutar hasil suara
    private string apiUrl = "https://texttospeech.googleapis.com/v1/text:synthesize"; // URL API Google TTS

    // Memastikan AudioSource sudah tersedia
    void Start()
    {
        // Jika audioSource belum di-assign di Inspector, buat AudioSource secara otomatis
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();  // Coba ambil AudioSource yang ada pada GameObject ini
        }

        // Jika masih null, buatkan AudioSource baru secara manual
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();  // Menambahkan komponen AudioSource secara otomatis
        }
    }

    // Mengirim teks ke Google Cloud TTS untuk dikonversi menjadi suara
    public void SynthesizeText(string text)
    {
        StartCoroutine(SendTextToSpeechRequest(text)); // Memulai coroutine untuk mengirim request ke API
    }
    // Coroutine untuk mengirim request ke Google TTS API dan mendapatkan respons audio
    private IEnumerator SendTextToSpeechRequest(string text)
    {
        // Membuat data request dalam format JSON
        TTSRequest requestData = new TTSRequest
        {
            input = new TTSRequest.Input { text = text },
            voice = new TTSRequest.Voice
            {
                languageCode = "id-ID",
                name = "id-ID-Standard-D"
            },
            audioConfig = new TTSRequest.AudioConfig
            {
                audioEncoding = "MP3"
            }
        };
        
        string json = JsonUtility.ToJson(requestData); // Mengonversi objek request ke JSON string

        // Membuat request HTTP POST ke API Google TTS
        UnityWebRequest request = new UnityWebRequest($"{apiUrl}?key={apiKey}", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json); // Mengubah JSON ke byte array
        request.uploadHandler = new UploadHandlerRaw(bodyRaw); // Menetapkan upload handler
        request.downloadHandler = new DownloadHandlerBuffer(); // Menetapkan download handler
        request.SetRequestHeader("Content-Type", "application/json"); // Menetapkan header untuk request JSON

        // Mengirim request ke API dan menunggu respons
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Text-to-Speech request successful!");
            string responseJson = request.downloadHandler.text; // jika request sukses, ambil responsenya
            var response = JsonUtility.FromJson<TTSResponse>(responseJson); // Parsing respons JSON yang berisi audio dalam format Base64
            byte[] audioData = System.Convert.FromBase64String(response.audioContent); // Decode Base64 ke byte array
            PlayAudio(audioData); // Mainkan audio
        }
        else
        {
            Debug.LogError("Text-to-Speech request failed: " + request.error);
        }
    }

    // Menyimpan audio yang diterima dalam bentuk file MP3 dan memainkannya
    private void PlayAudio(byte[] audioData)
    {
        string filePath = Application.persistentDataPath + "/speech.mp3"; // Menentukan path file penyimpanan sementara di perangkat
        System.IO.File.WriteAllBytes(filePath, audioData); // Menyimpan byte data audio sebagai file MP3 di perangkat
        StartCoroutine(LoadAndPlay(filePath)); // Memuat file audio yang telah disimpan dan memainkannya
    }

    // Memuat file audio dari penyimpanan dan memainkannya menggunakan AudioSource.
    private IEnumerator LoadAndPlay(string filePath)
    {
        // Membuka file audio dengan UnityWebRequestMultimedia
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest(); // Menunggu file dimuat

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Mengonversi file yang diunduh menjadi AudioClip
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                // Mengatur audio ke AudioSource dan memutarnya
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio: " + www.error);
            }
        }
    }


    // Struktur JSON untuk request ke Google Cloud TTS API.
    [System.Serializable]
    private class TTSRequest
    {
        public Input input; // Objek input (teks yang akan diubah ke suara)
        public Voice voice; // Objek pengaturan suara (bahasa, jenis suara)
        public AudioConfig audioConfig; // Objek konfigurasi audio

        [System.Serializable]
        public class Input
        {
            public string text; // Teks yang akan dikonversi menjadi suara
        }

        [System.Serializable]
        public class Voice
        {
            public string languageCode; // Kode bahasa
            public string name; // Nama suara Google TTS
        }

        [System.Serializable]
        public class AudioConfig 
        {
            public string audioEncoding;  // Format output audio (contoh: "MP3")
        }
    }

    // Struktur JSON untuk respons dari Google Cloud TTS API
    [System.Serializable]
    private class TTSResponse
    {
        public string audioContent; // Data audio dalam format Base64
    }
}
