using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using System.Linq;

public class APIManager : MonoBehaviour
{
    // Api key untuk mengakses GEMINI API
    private const string API_KEY = "AIzaSyB5Tx7wryEPkpY7FN0SRKfsckk5lhZbDEU";
    
    // URL endpoint API GEMINI
    private const string API_URL = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent";
    
    // Referensi ke ChatManager untuk mengelola CHAT UI
    [SerializeField] private ChatManager chatManager;

    // Delay sebelum repons pertama ditampilkan
    [SerializeField] private float initialResponseDelay = 5f;

    // Delay untuk efek mengetik
    [SerializeField] private float typingDelay = 0f;  // Delay untuk efek mengetik
    
    // Dictionary untuk menyimpan basis pengetahuan
    private Dictionary<string, string> knowledgeBase = new Dictionary<string, string>();

    // Set untuk menyimpan kata-kata sapaan
    private HashSet<string> greetingPhrases = new HashSet<string> {
        "bisakah", "bisa", "tolong", "bantu", "membantu",
        "halo", "hai", "pagi", "siang", "sore", "malam", "permisi"
    };

    // Menyimpan kategori organ yang sedang aktif
    private string currentCategory;

    // Method yang dipanggil saat objek pertama diinisialisasi
    private void Start()
    {
        // Cek apakah chat manager sudah diatur.
        if (chatManager == null)
        {   
            // Jika belum akan error
            Debug.LogError("ChatManager belum diatur di Inspector!");
            return;
        }

        // Ambil kategori yang disimpan dalam Playerprefs
        currentCategory = PlayerPrefs.GetString("CurrentCategory","");

        // Load basis pengetahuan
        LoadKnowledgeBase();
    }

    // Method untuk memuat basis pengetahuan dari file
    private void LoadKnowledgeBase()
    {
        // Path ke folder KnowledgeBase di StreamingAssets
        string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "KnowledgeBase");
        
        // Cek apakah folder KnowledgeBase ada
        if (!Directory.Exists(streamingAssetsPath))
        {
            Debug.LogError("KnowledgeBase folder tidak ditemukan di StreamingAssets!");
            return;
        }
        
        // Membaca semua file .txt dalam folder
        foreach (string filePath in Directory.GetFiles(streamingAssetsPath, "*.txt"))
        {
            try
            {
                // Ambil nama file tanpa ekstensi
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // Membaca isi file dan menyimpan dalam dictionary
                knowledgeBase[fileName] = File.ReadAllText(filePath);
                
                // Load sukse memuat file
                Debug.Log($"Loaded knowledge base: {fileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file {filePath}: {e.Message}");
            }
        }
    }

    // ini akan dipanggil saat user mengirimkan sebuah prompt dari UI.
    public void GetRequest(string prompt)
    {
        // Memeriksa apakah prompt kosong
        if (string.IsNullOrEmpty(prompt)) 
        {
            // Log warning jika prompt kosong
            Debug.LogWarning("Input prompt kosong!");
            return;
        }

        // Menampilkan pesan jika sedang diproses
        chatManager.SendResponseMessage("Sedang diproses...");

        // Memulai coroutine untuk memproses Request
        StartCoroutine(ProcessRequest(prompt));
    }

    // Untuk memeriksa apakah prompt mengandung nama organ selain currentCategory
     private bool ContainsOtherOrganNames(string prompt)
    {
        // Mengubah prompt ke Lowercase
        string promptLower = prompt.ToLower();

        // Memeriksa setiap nama organ
        foreach (var organ in organNames)
        {
            // Jika prompt mengandung nama organ
            if (promptLower.Contains(organ.ToLower()) && 
                !organ.Equals(currentCategory, StringComparison.OrdinalIgnoreCase))
            {
                // Kembalikan True
                return true;
            }
        }
        return false;
    }

    // menangani logika pemrosesan prompt pengguna
    private IEnumerator ProcessRequest(string prompt)
    {
        // Menunggu Delay Awal
        yield return new WaitForSeconds(initialResponseDelay);

        // Mengubah Prompt ke Lowercase
        string promptLower = prompt.ToLower();
        
        // Cek apakah pertanyaan membahas organ dalam lain 
         if (ContainsOtherOrganNames(prompt))
        {
            // Menunggu Delay Typing
            yield return new WaitForSeconds(typingDelay);
            
            // Menampilkan pesan bahwa pertanyaan tidak bisa dijawab
            chatManager.UpdateLastResponseMessage("Maaf, pertanyaan itu tidak bisa dijawab disini. Silakan memilih organ dalam yang sesuai.", "response");
            yield break;
        }

        // Memeriksa apakah ini adalh sapaan
        if (IsSimpleGreeting(promptLower))
        {
            // Menunggu Delay Typing
            yield return new WaitForSeconds(typingDelay);

            // Menampilkan Pesan Sapaan
            chatManager.UpdateLastResponseMessage($"Halo! Saya adalah asisten khusus untuk organ {currentCategory}. Silakan tanyakan apa saja yang ingin Anda ketahui tentang {currentCategory}!", "response");
            yield break;
        }

        // Memeriksa apakah ini permintaan bantuan
        if (IsHelpRequest(promptLower))
        {
            // Menunggu Delay Typing
            yield return new WaitForSeconds(typingDelay);

            // Menampilkan Pesan Bantuan
            chatManager.UpdateLastResponseMessage($"Ya, saya bisa membantu Anda dengan informasi khusus tentang {currentCategory}.Silakan ajukan pertanyaan Anda seputar {currentCategory}", "response");
            yield break;
        }

        // Memulai Request ke Gemini API
        StartCoroutine(ProcessGeminiRequest(prompt));
    }

    // Memeriksa apakah prompt adalah sapaan
    private bool IsSimpleGreeting(string prompt)
    {
        // Memeriksa apakah prompt dimulai kata sapaan
        return greetingPhrases.Any(phrase => prompt.StartsWith(phrase));
    }

    // Memeriksa apakah prompt adalah permintaan bantuan
    private bool IsHelpRequest(string prompt)
    {
        // Memeriksa kombinasi kata bantuan
        return prompt.Contains("bisa") && 
               (prompt.Contains("bantu") || prompt.Contains("membantu") || prompt.Contains("tolong"));
    }

    // Coroutine untuk memproses request ke Gemini API
    private IEnumerator ProcessGeminiRequest(string prompt)
    {
        // Membuat prompt dengan kontekskan sebelum dikirim
        string contextualizedPrompt = CreateContextualizedPrompt(prompt);

        // Membuat body Request dalam bentuk JSON yang akan dikirim ke API
        var requestBody = new
        {
            // Isi permintaan dikemas dalam Array contents
            contents = new[]
            {
                new
                {
                    // Setiap content memiliki bagian-bagian yang berisi teks yang sudah dikontekskan
                    parts = new[]
                    {
                        // Menambahkan Teks prompt yang telah diproses
                        new { text = contextualizedPrompt }
                    }
                }
            }
        };

        // Mengkonversi body request ke JSON
        string jsonBody = JsonConvert.SerializeObject(requestBody);

        // Mengkonversi JSON ke Bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // Membuat Request ke API
        using (UnityWebRequest request = new UnityWebRequest($"{API_URL}?key={API_KEY}", "POST"))
        {
            // Mengatur handler untuk upload dan Download
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Mengatur header content-Type
            request.SetRequestHeader("Content-Type", "application/json");

            // Mengirim Request
            yield return request.SendWebRequest();

            // Memeriksa apakah Request berhasil
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Mengkonversi Response ke JSON
                var responseData = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                string response = responseData.candidates[0].content.parts[0].text;

                // Menunggu Delay Typing                
                yield return new WaitForSeconds(typingDelay);

                // Membatasi panjang Response
                string trimmedResponse = LimitWordsInResponse(response, 50);
                
                // Menampilkan response
                chatManager.UpdateLastResponseMessage(trimmedResponse, "response");
            }
            else
            {
                // Log Eror jika request gagal
                Debug.LogError($"Error: {request.error}");
                
                // Menunggu delay typing
                yield return new WaitForSeconds(typingDelay);
                
                // Menampilkan pesan eror
                chatManager.UpdateLastResponseMessage("Maaf, saya sedang mengalami kesulitan dalam memproses permintaan Anda. Mohon coba lagi.", "response");
            }
        }
    }


    // Method untuk membatasi jumlah kata dalam response
    private string LimitWordsInResponse(string response, int wordLimit)
    {
        // Memisahkan response menjad array kata-kata
        string[] words = response.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        // Jika jumlah kata kurang dari limit, kembalikan response
        if (words.Length <= wordLimit) return response;
        
        // Mengambil sejumlah kata sesuai limit dan menambahkan ellipsis
        return string.Join(" ", words.Take(wordLimit)) + "...";
    }

    // Set nama-nama organ
    private HashSet<string> organNames = new HashSet<string> {
        "Hati", "Usus Kecil", "Ginjal", "Vagina", "Usus Besar", "Tulang Belakang", "Trakea",  "Panggul", "Otak", "Lambung", "Rangka", "Jantung",
    };


    // Method untuk membuat Prompt dengan konteks
    private string CreateContextualizedPrompt(string userPrompt)
    {
    // Membuat StringBuilder untuk membangun prompt
    StringBuilder contextBuilder = new StringBuilder();

    // Menambahkan Instruksi peran
    contextBuilder.AppendLine($"PENTING: Anda adalah asisten khusus untuk organ {currentCategory} berdasarkan buku teks.");

    // Menambahkan aturan wajib
    contextBuilder.AppendLine("ATURAN WAJIB:");
    contextBuilder.AppendLine($"1. HANYA gunakan informasi tentang {currentCategory} dari buku teks di bawah ini");
    contextBuilder.AppendLine($"2. HANYA jawab tentang organ {currentCategory}");
    contextBuilder.AppendLine("3. Untuk informasi yang tidak ada dalam buku teks, jawab dengan format:");
    contextBuilder.AppendLine($"  'Maaf, saya tidak memiliki informasi tentang [sebutkan hal spesifik] pada {currentCategory}'");
    
    // Menambahkan contoh format jawaban
    contextBuilder.AppendLine("Contoh format jawaban ketika informasi tidak tersedia:");
    contextBuilder.AppendLine($"- 'Maaf, saya tidak memiliki informasi pada {currentCategory}'");
    contextBuilder.AppendLine("---");
    
    // Menambahkan pengetahuan jika tersedia
    if(!string.IsNullOrEmpty(currentCategory) && knowledgeBase.ContainsKey(currentCategory))
    {
        contextBuilder.AppendLine($"BUKU TEKS TENTANG {currentCategory}:");
        contextBuilder.AppendLine(knowledgeBase[currentCategory]);
        contextBuilder.AppendLine("---");
    }


    // Menamabhkan pertanyaan user dan aturan tambahan
    contextBuilder.AppendLine("Pertanyaan: " + userPrompt);
    contextBuilder.AppendLine("ATURAN TAMBAHAN:");
    contextBuilder.AppendLine($"1. Jawaban WAJIB spesifik tentang {currentCategory}");
    contextBuilder.AppendLine("2. Jika informasi tidak ada dalam buku teks, WAJIB sebutkan hal spesifik yang tidak tersedia");
    contextBuilder.AppendLine($"3. SELALU sertakan kata '{currentCategory}' dalam jawaban");
    contextBuilder.AppendLine("4. Jawaban maksimal 50 kata");
    

    // Mengembalikan prompt lengkap
    return contextBuilder.ToString();
    }

    // Kelas untuk deserialisasi respons API
    [Serializable]
    private class ResponseData
    {
        public Candidates[] candidates;
    }

    // Kelas untuk deserialisasi kandidat response
    [Serializable]
    private class Candidates
    {
        public Content content;
    }

    // Kelas untuk deserialisasi konten response
    [Serializable]
    private class Content
    {
        public Parts[] parts;
    }

    // Kelas untuk deserialisasi bagian konten response
    [Serializable]
    private class Parts
    {
        public string text;
 }
}