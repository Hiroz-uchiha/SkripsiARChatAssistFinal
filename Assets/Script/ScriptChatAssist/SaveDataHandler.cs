using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SaveDataHandler : MonoBehaviour
{
    public Database database;  // Referensi ke script SimpleDB
    public ChatManager chatManager; // Referensi ke ChatManager
    

    // Fungsi ini akan dipanggil saat tombol ditekan
    public void OnSaveButtonClick()
    {
        List<ChatEntry> chatHistory = chatManager.GetChatHistory(); // Ambil riwayat percakapan

        if (chatHistory.Count == 0)
        {
            Debug.LogWarning("Tidak ada percakapan untuk disimpan.");
            return;
        }

        // Simpan data ke database
        if (database != null)
        {
            string organDalamName =  PlayerPrefs.GetString("CurrentCategory","");
            // Simpan setiap request dan response ke dalam database
            foreach (var entry in chatHistory)
            {
                string namaOrgan = organDalamName;
                string tanggal = System.DateTime.Now.ToString("yyyy-MM-dd");
                database.SaveData(namaOrgan, entry.userRequest, entry.botResponse, tanggal);
            }
            Debug.Log("Data percakapan berhasil disimpan ke database!");

            // Kosongkan chat history setelah data disimpan
            chatManager.ClearChatHistory();
        }
        else
        {
            Debug.LogError("Database tidak ditemukan!");
        }
    }
}
