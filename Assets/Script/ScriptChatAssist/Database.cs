using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections;
using TMPro;
using System.IO;

public class Database : MonoBehaviour
{
    private string dbName; // Nama database agar bisa diakses dalam 1 script.
    public GameObject alertPanel; // UI Panel untuk notifikasi

    void Awake()
    {
        // Menentukan lokasi database di penyimpanan perangkat
        dbName = Path.Combine(Application.persistentDataPath, "RiwayatPertanyaan.db");
        alertPanel.SetActive(false);  // Sembunyikan panel notifikasi di awal
    }


    void Start()
    {
        // Pastikan direktori penyimpanan database ada
        string directory = Path.GetDirectoryName(dbName);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory); // Buat direktori jika belum ada
            Debug.Log("Created directory: " + directory);
        }
        // Cek apakah file database sudah ada, jika belum maka buat baru
        if (!File.Exists(dbName))
        {
            CreateDB();
        }
    }

    // Membuat database dan tabel ChatHistory
    void CreateDB()
    {
        string connectionString = "URI=file:" + dbName;
        using (var connection = new SqliteConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Perintah SQL untuk membuat tabel jika belum ada
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS RiwayatPertanyaan (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            NamaOrgan TEXT,
                            Request TEXT,
                            Response TEXT,
                            Tanggal TEXT
                        );
                    ";
                    command.ExecuteNonQuery();
                }
             //   connection.Close();
                Debug.Log("Database and table created successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to create database: " + e.Message);
            }
        }
       // Debug.Log("Database and table created successfully!");
    }

    // Menyimpan data percakapan ke dalam database
    public void SaveData(string namaOrgan, string request, string response, string tanggal)
    {
        string connectionString = "URI=file:" + dbName;
        using (var connection = new SqliteConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Query untuk menyimpan data ke tabel RiwayatPertanyaan
                    command.CommandText = @"
                        INSERT INTO RiwayatPertanyaan (NamaOrgan, Request, Response, Tanggal)
                        VALUES (@namaOrgan, @request, @response, @tanggal);
                    ";

                    // Menambahkan parameter untuk menghindari SQL Injection
                    command.Parameters.Clear();
                    command.Parameters.Add(new SqliteParameter("@namaOrgan", namaOrgan));
                    command.Parameters.Add(new SqliteParameter("@request", request));
                    command.Parameters.Add(new SqliteParameter("@response", response));
                    command.Parameters.Add(new SqliteParameter("@tanggal", tanggal));
                    command.ExecuteNonQuery(); // Menjalankan SQL INSERT tanpa mengembalikan hasil
                }
                Debug.Log("Data saved successfully.");
                ShowAlert("Percakapan berhasil disimpan!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save data: " + e.Message);
                ShowAlert("Gagal menyimpan data: " + e.Message, 3f);
            }
        }
    }

    // Membaca data dari database dan menampilkannya di Unity Console
    public void ReadData1()
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM RiwayatPertanyaan";  // Query untuk mengambil semua data
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Baca kolom dengan urutan yang benar
                        int id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0); // Id
                        string namaObject = reader.IsDBNull(1) ? "N/A" : reader.GetString(1); // NamaObject
                        string userRequest = reader.IsDBNull(2) ? "N/A" : reader.GetString(2); // UserRequest
                        string outputResult = reader.IsDBNull(3) ? "N/A" : reader.GetString(3); // OutputResult
                        string tanggal = reader.IsDBNull(3) ? "N/A" : reader.GetString(4); // OutputResult

                        // Tampilkan data di Console Unity
                        Debug.Log($"Id: {id}, NamaObject: {namaObject}, UserRequest: {userRequest}, OutputResult: {outputResult}");
                    }
                }
            }
        }
    }

    // Menghapus tabel
    public void DropTable(string tableName)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DROP TABLE IF EXISTS {tableName};";
                command.ExecuteNonQuery();
            }
        }
        Debug.Log($"Tabel {tableName} berhasil dihapus.");
    }

    // Menampilkan notifikasi dalam UI
    public void ShowAlert(string message, float duration = 2f)
    {
        TMP_Text alertText = alertPanel.transform.Find("NotificationWrapper/NotificationText").GetComponent<TMP_Text>(); // Ambil Text dari UI
        alertText.text = message; // Set pesan pada UI
        alertPanel.SetActive(true); // Tampilkan panel
        StartCoroutine(HideAlertAfterDelay(duration)); // Sembunyikan setelah durasi tertentu
    }

    // Coroutine untuk menyembunyikan notifikasi setelah waktu tertentu
    private IEnumerator HideAlertAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration); // Tunggu selama durasi yang ditentukan
        alertPanel.SetActive(false); // Sembunyikan panel notifikasi
    }
}

