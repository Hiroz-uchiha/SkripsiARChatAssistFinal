using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;  // Untuk manajemen scene
using System.Collections;

public class GoToChatAssist : MonoBehaviour
{
    public Sprite micOnImage;  // Sprite untuk Mic On
    public Sprite micOffImage; // Sprite untuk Mic Off
    private bool isRecording = false;
    private AudioClip audioClip;

    private Button[] microphoneButtons;  // Array untuk menyimpan tombol

    // Akan aktif setelah pengguna menekan tombol
    public void OnMicrophoneClick()
    {
        //1. Memastikan GoogleSpeechToText itu ada
        EnsureGoogleSpeechToText();

        //2. Mengubah status Rekaman
        ToggleRecording();
    }

    // Mengecek apakah GoogleSpeechToText itu ada
    private void EnsureGoogleSpeechToText()
    {
        // Jika belum ada
        if (GoogleSpeechToText.Instance == null)
        {
            //1. Maka dia akan buat object baru
            GameObject googleSpeechObject = new GameObject("GoogleSpeechToText");

            //2. Lalu menambahkan komponen GST ke objek tersebut
            googleSpeechObject.AddComponent<GoogleSpeechToText>();
            
            //3. Lalu menandainya dengan destroyOnLoad agar tetap ada setelah pindah Scene
            DontDestroyOnLoad(googleSpeechObject);
        }
    }

    // Mengubah Status Rekaman
    public void ToggleRecording()
    {
        //1. Jika sedang merekam
        if (isRecording)
        {
            //2. Maka hentikan rekaman
            StopRecording();
        }
        else
        {
            //3. Jika tidak Merekam maka mulai rekaman
            StartRecording();
        }

        //4. Setelah itu dia akan memperbarui tampilan tombol mirofon
        UpdateButtonImages();
    }


    // Memulai perekaman
    private void StartRecording()
    {   
        // Cek apakah sudah merekam, jika sudah maka hentikan
        if (isRecording) return;

        // Jika belum maka, maka nilai jadi true
        isRecording = true;

        // Untuk Memulai perekaman
        audioClip = Microphone.Start(null, false, 10, 44100);
        Debug.Log("Recording started...");
    }

    // Menghentikan perekaman dan pindah scene saat berhenti.
    private void StopRecording()
    {
        // Cek apakah sedang merekam, jika tidak maka keluar
        if (!isRecording) return;

        //Nilai isRecording jadi false
        isRecording = false;

        // Menghentikan perekaman
        Microphone.End(null);
        Debug.Log("Recording stopped.");

        // Pindah ke scene HalamanChatAssist
        SceneManager.sceneLoaded += OnSceneLoaded;  // Daftarkan callback untuk scene loaded
        SceneManager.LoadScene("HalamanChatAssist");
    }

    // Akan dijalankan saat scene baru selesai dimuat
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Jika nama scenenya HalamanChatAssist
        if (scene.name == "HalamanChatAssist")
        {
            // Maka kirimkan audio ke Google API
            SendAudioToGoogle();

            // Lepaskan event agat tidak memproses event lebih dari 1
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }


    // Mengirim audio ke Google Speech API
    private void SendAudioToGoogle()
    {   
        // Jika tidak ada, tampilkan error lalu hentikan
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }

        // Jika GSP tidak ada maka tampilkan eror
        if (GoogleSpeechToText.Instance == null)
        {
            Debug.LogError("GoogleSpeechToText is not initialized.");
            return;
        }

        // Jika semuanya ada 
        float[] samples = new float[audioClip.samples * audioClip.channels];
        //maka ambil data sampel audio
        audioClip.GetData(samples, 0);

        // Lalu ubah data sampel audio ke byte
        byte[] audioData = ConvertSamplesToBytes(samples);

        // Kirim data audio ke Google Speech API
        GoogleSpeechToText.Instance.SendAudioForTranscription(audioData);
    }


    // Mengubah sampel audio ke byte
    private byte[] ConvertSamplesToBytes(float[] samples)
    {
        // 
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
        }

        System.Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        return bytesData;
    }

    // Fungsi untuk memperbarui gambar pada semua tombol
    private void UpdateButtonImages()
    {
        // Mencari semua tombol dengan tag "ButtonMicrophone"
        microphoneButtons = GameObject.FindGameObjectsWithTag("ButtonMicrophone")
                                      .Select(go => go.GetComponent<Button>())
                                      .Where(button => button != null) // Menambahkan pengecekan null jika Button tidak ditemukan
                                      .ToArray();

        foreach (var button in microphoneButtons)
        {
            Image buttonImage = button.image; // Mendapatkan Image dari tombol
            if (buttonImage != null)
            {
                buttonImage.sprite = isRecording ? micOnImage : micOffImage;
                Debug.Log($"Button Image updated. Mic is {(isRecording ? "On" : "Off")}");
            }
            else
            {
                Debug.LogError("Button does not have an Image component.");
            }
        }
    }
}
