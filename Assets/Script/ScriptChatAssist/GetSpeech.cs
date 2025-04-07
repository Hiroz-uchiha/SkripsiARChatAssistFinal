using UnityEngine;
using UnityEngine.UI;

public class MicrophoneRecorder : MonoBehaviour
{
     // untuk menerapkan pola Singleton, dmn hanya ada satu instance dari MicrophoneRecorder dalam game. 
    public static MicrophoneRecorder Instance { get; private set; }
    private Button myButton; // Referensi button di UI

     // Sprite untuk mengganti tampilan tombol saat mic aktif atau nonaktif
    public Sprite micOnImage;  // Sprite untuk Mic On
    public Sprite micOffImage; // Sprite untuk Mic Off

    private bool isRecording = false; // Status perekaman
    private AudioClip audioClip; // Menyimpan rekaman suara dalam bentuk AudioClip

    private void Awake()
    {
        // Jika sudah ada instance lain, hancurkan objek ini untuk mencegah duplikasi
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;  // Menetapkan instance baru
        DontDestroyOnLoad(gameObject);  // Mencegah objek ini dihancurkan ketika berpindah scene
    }

    // Mendapatkan atau membuat instance MicrophoneRecorder jika belum ada  
    // public static MicrophoneRecorder GetOrCreateInstance()
    // {
    //     // Jika instance belum ada, buat objek baru
    //     if (Instance == null)
    //     {
    //         GameObject recorderObject = new GameObject("MicrophoneRecorder");
    //         Instance = recorderObject.AddComponent<MicrophoneRecorder>();
    //         DontDestroyOnLoad(recorderObject); // Mencegah objek ini dihancurkan ketika berpindah scene
    //     }

    //     return Instance;
    // }

    // Mengaktifkan atau menonaktifkan perekaman suara
    public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording(); // Jika sedang merekam, hentikan rekaman
        }
        else
        {
            StartRecording(); // Jika tidak sedang merekam, mulai rekaman
        }
        UpdateButtonImage(); // Perbarui tampilan tombol berdasarkan status rekaman
    }

    // Memulai perekaman suara dari mikrofon
    private void StartRecording()
    {
        // Jika sudah merekam, keluar dari fungsi untuk mencegah perekaman ganda
        if (isRecording) return;
        isRecording = true; // Set status rekaman menjadi aktif
        audioClip = Microphone.Start(null, false, 10, 44100); // Memulai perekaman suara menggunakan mikrofon dengan durasi 10 detik dan sample rate 44100 Hz
        Debug.Log("Recording started...");
    }

    // Menghentikan perekaman suara
    private void StopRecording()
    {
        // Jika tidak sedang merekam, keluar dari fungsi
        if (!isRecording) return;
        isRecording = false; // Set status rekaman menjadi tidak aktif
        Microphone.End(null); // Menghentikan perekaman dari mikrofon
        Debug.Log("Recording stopped.");
        SendAudioToGoogle(); // Jalankan funtion SendAudioToGoogle()
    }

    // Mengirim rekaman audio ke Google Speech-to-Text API
    private void SendAudioToGoogle()
    {
        // Jika tidak ada rekaman audio, tampilkan error dan keluar dari fungsi
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }

        // Jika instance GoogleSpeechToText belum dibuat, tampilkan error
        if (GoogleSpeechToText.Instance == null)
        {
            Debug.LogError("GoogleSpeechToText is not initialized.");
            return;
        }

        // Mengambil data audio dari rekaman dalam bentuk float array
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        // Mengonversi data float menjadi byte array
        byte[] audioData = ConvertSamplesToBytes(samples);

        // Mengirimkan data audio ke Google Speech-to-Text API untuk ditranskripsi
        GoogleSpeechToText.Instance.SendAudioForTranscription(audioData);
    }

    // Mengonversi data audio dari float array ke byte array
    private byte[] ConvertSamplesToBytes(float[] samples)
    {
        short[] intData = new short[samples.Length]; // Membuat array short untuk menyimpan data yang dikonversi
        byte[] bytesData = new byte[samples.Length * 2]; // Membuat array byte untuk menyimpan data hasil konversi

        // Mengonversi setiap nilai float menjadi nilai short (16-bit PCM)
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
        }

        // Menyalin data dari short array ke byte array
        System.Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        return bytesData;
    }

    // Memperbarui tampilan tombol rekaman
    private void UpdateButtonImage()
    {
        // Mencari tombol berdasarkan nama GameObject (misalnya, "Microphone")
        myButton = GameObject.Find("Microphone")?.GetComponent<Button>();

        // Jika tombol tidak ditemukan, tampilkan pesan error
        if (myButton == null)
        {
            Debug.LogError("Button not found! Please ensure the button is in the scene.");
        }

        // Mengambil komponen Image dari tombol
        Image buttonImage = myButton.image;

        // Jika tombol memiliki Image, ubah sprite sesuai dengan status rekaman
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
     
    // // Mengecek apakah rekaman sudah berhenti
    // public bool IsRecordingStopped()
    // {
    //     return !isRecording;
    // }
}
