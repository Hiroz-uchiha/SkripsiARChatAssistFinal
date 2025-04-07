using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneAController : MonoBehaviour
{
    // Prefab untuk menampilkan pesan request
    public GameObject requestPrefab;     

    // Prefab untuk menampilkan pesan response 
    public GameObject responsePrefab;

    // Prefab untuk menampilkan pemisah tanggal
    public GameObject dateSeparatorPrefab;

     // Parent object untuk meletakkan chat items 
    public Transform contentParent;      

    // Untuk menampilkan nama organ
    public TextMeshProUGUI titleText;

    // Untuk nama organ teks gambar
    public GameObject imageText;     
    
    // Path ke database SQlite
    private string dbPath;
    
    // Jarak vertical antara items dan chat
    public float verticalSpacing = 20f;   // Spacing antara items

    // Method yang dipanggil saat scene dimulai
    void Start()
    {   
        // Sembunyikan teks gambar saat awal 
        imageText.SetActive(false);

        // Set path database ke persistent data path
        dbPath = Path.Combine(Application.persistentDataPath, "RiwayatPertanyaan.db");
        
        // Cek keberadaaan dan akses database
        if (!CheckDatabase())
        {
            // Jike bernasalah tampilkan
            Debug.LogError("Database tidak ditemukan atau tidak dapat diakses!");
            titleText.text = "Error: Database tidak ditemukan";
            return;
        }


        // Ambil nama organ dari PlayerPrefs
        string currentOrgan = PlayerPrefs.GetString("SceneContent", "Default");

        // Ambil data chat dari database
        var chatDataList = GetChatDataFromDatabase(currentOrgan);

        // Set judul dengan nama organ
        titleText.text = currentOrgan;

        // Jika tidak ada chat 
        if (chatDataList.Count == 0)
        {
            // Set judul
            titleText.text = currentOrgan;
            // Tampilkan teks gambar
            imageText.SetActive(true);
            return;
        }

        // Buat dan atur chat items dengan pemisah tanggal
        CreateChatItemsWithDateSeparator(chatDataList);
    }

    // Method untuk membuat items chat dengan pemisah tanggal
    private void CreateChatItemsWithDateSeparator(List<ChatDatas> chatDataList)
    {
        // Posisi Awal
        float currentY = 0f;

        // Tanggal Terakhir untuk pembandingan
        string lastDate = null;

        // Hapus semua child objects yang ada sebelumnya
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Iterasi setiap data chat
        foreach (var chat in chatDataList)
        {
            // Tambahkan separator tanggal jika tanggal berubah
            if (lastDate != chat.Date)
            {
                AddDateSeparator(chat.Date);
                lastDate = chat.Date;
            }

            // Buat dan atur request item
            GameObject requestItem = Instantiate(requestPrefab, contentParent);

            // Mengambil komponent RectTransform untuk mengatur Posisi
            RectTransform requestRect = requestItem.GetComponent<RectTransform>();

            // Mengambil komponen teks untuk menampilkan pesan request
            TextMeshProUGUI requestText = requestItem.GetComponentInChildren<TextMeshProUGUI>();

            // Mengatur Teks Request
            requestText.text = chat.Request;

            // Atur posisi response
            requestRect.anchoredPosition = new Vector2(0, currentY);

            // Update Y position berdasarkan tinggi request
            currentY -= (requestRect.sizeDelta.y + verticalSpacing);

            // Buat response item
            GameObject responseItem = Instantiate(responsePrefab, contentParent);

            // Mengambil Komponen RectTransform untuk mengatur posisi
            RectTransform responseRect = responseItem.GetComponent<RectTransform>();
            
            // Mengambil komponen TextMeshProUGUI untuk menampilkan pesan response
            TextMeshProUGUI responseText = responseItem.GetComponentInChildren<TextMeshProUGUI>();

            // Set Teks response
            responseText.text = chat.Response;

            // Atur Posisi response
            responseRect.anchoredPosition = new Vector2(0, currentY);

            // Update Y position berdasarkan tinggi response
            currentY -= (responseRect.sizeDelta.y + verticalSpacing);
        }

        // Update content size fitter jika ada
        if (contentParent.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
        {
            contentSizeFitter.SetLayoutVertical();
        }
    }

    // Method untuk menambah pemisah tanggal
    private void AddDateSeparator(string date)
    {
        // Buat object separator tanggal
        GameObject separator = Instantiate(dateSeparatorPrefab, contentParent);
        
        // Ambil komponen Rect Transform
        RectTransform separatorRect = separator.GetComponent<RectTransform>();
        
        // Ambil komponen TextMeshProUGUI
        TextMeshProUGUI separatorText = separator.GetComponentInChildren<TextMeshProUGUI>();

        // Format Tanggal
        string formattedDate = DateTime.Parse(date).ToString("dd MMMM yyyy");
        // Set Teks Tanggal
        separatorText.text = formattedDate;
    }

    // Untuk Memeriksa database
    private bool CheckDatabase()
    {
        try
        {
            // Cek keberadaan file detabase
            if (!File.Exists(dbPath))
            {
                Debug.LogError($"File database tidak ditemukan di: {dbPath}");
                return false;
            }
            // Buat string koneksi
            string connectionString = "URI=file:" + dbPath;
            
            // Buat koneksi ke database
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open(); // Membuka koneksi ke database
                using (var cmd = connection.CreateCommand())
                {
                    // Cek keberadaan tabel RiwayatPertanyaan
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='RiwayatPertanyaan'";

                    // Eksekusi query dan ambil nilai pertama
                    var result = cmd.ExecuteScalar();

                    // Jika null berarti RiwayatPertanyaan tidak ada
                    if (result == null)
                    {
                        Debug.LogError("Tabel RiwayatPertanyaan tidak ditemukan dalam database!");
                        return false; // Mengembalikan false karena error
                    }
                }
                // Menutup koneksi kke database
                connection.Close();

                // Mengembalikan true karena database dan tabel telah ditemukan
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saat memeriksa database: {ex.Message}");
            return false;
        }
    }

    // Method untuk mengambil data chat dari database
    private List<ChatDatas> GetChatDataFromDatabase(string organName)
    {
        // Query SQL untuk mengambil data
        string query = "SELECT Request, Response, Tanggal FROM RiwayatPertanyaan WHERE NamaOrgan = @organName ORDER BY Tanggal ASC";
  
        // List untuk menampung data chat
        List<ChatDatas> chatDataList = new List<ChatDatas>();

        // Menyusun koneksi string untuk koneksi ke database
        string connectionString = "URI=file:" + dbPath; 
        try
        {
            // Buat koneksi ke database 
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open(); // Membuka koneksi ke database
                using (var cmd = connection.CreateCommand())
                {
                    // Set query dan parameter
                    cmd.CommandText = query;
                    // Menambahkan parameter untuk query untuk menghindari SQL Injection
                    cmd.Parameters.AddWithValue("@organName", organName);
                    
                    // Eksekusi query dan baca hasil
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        // Iterasi setiap record yang dibaca dari database
                        while (reader.Read())
                        {
                            // Membuat objek ChatDatas dan menambahkan ke list dengan mengambil data dari database
                            chatDataList.Add(new ChatDatas
                            {
                                // Mengambil nilai dari kolom request
                                Request = reader.GetString(0),
                                // Mengambil nilai dari kolom response
                                Response = reader.GetString(1),
                                //Mengambil nilai dari kolom tanggal
                                Date = reader.GetString(2)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saat mengambil data: {ex.Message}");
        }
        // Kembalikan list data chat
        return chatDataList;
    }

    // Method untuk kembali ke scene sebelumnya
    public void BackToScene(string sceneName)
    {
        // Load scene HalamanRiwayat
        SceneManager.LoadScene("HalamanRiwayat");
    }
}


// Kelas untuk menyimmpan data chat
public class ChatDatas
{
    // Untuk menyimpan pesan request
    public string Request { get; set; }
    // Untuk menyimpan pesan response
    public string Response { get; set; }
    // Untuk menyimpan tanggal
    public string Date { get; set; }
}
