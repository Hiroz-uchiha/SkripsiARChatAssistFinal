using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

using TMPro; // Menggunakan TextMeshPro

// Menyimpan data riwayat percakpaan
public class ChatEntry
{
    // Menyimpan permintaan Pengguna
    public string userRequest;

    // Menyimpan jawaban dari chat assist
    public string botResponse;
}
public class ChatManager : MonoBehaviour
{
    // Properti  statis singleton, hanya ada 1 instance dari chat manager sehingga bisa diakses darimana saja
    public static ChatManager Instance { get; private set; }

    // Tampilan visual untuk instance baru setiap kali pesan dikirim
    public GameObject chatBubblePrefab;  

    public Transform content; // UI tempat chat Bubble akan ditambahkan

    // Untuk membaca respons dari chat
    public GoogleTextToSpeech textToSpeechScript;

    // List untuk menyimpan diwayat pesan antara pengguna dan chat assist
    private List<ChatEntry> chatHistory = new List<ChatEntry>();
    public float spacing = 18f; // Jarak antar pesan

    // Mengatur letak pesan secara vertical
    private VerticalLayoutGroup verticalLayoutGroup; 

    // Akan menerapkan pola singleton
    void Awake()
    {
        // Jika instance sudah ada dan bukan instance ini
        if (Instance != null && Instance != this)
        {
            // Maka hancurkan instance ini
            Destroy(gameObject);
            return;
        }
        // Jika belum maka inisiaisasi instance dengan referensi ke objek saat ini.
        Instance = this;
    }

    // Fungsi untuk mengirim pesan dari pengguna
    public new void SendMessage(string message)
    {
        // Cek pesan kosong
        if (!string.IsNullOrEmpty(message))
        {
            // Jika pesan tidak kosong maka lanjukan dengan menunda pembuatan chat buble selama 1 detik, sehingga pesan muncul dengan delay
            StartCoroutine(DelayedCreateChatBubble(message, "user"));

            // Lalu menambahkan entri baru ke Chat History dengan nilai awal kosong.
            chatHistory.Add(new ChatEntry() { userRequest = "", botResponse = "" });
            Debug.Log("Pesan = " + message);
        }
    }

    // Fungsi untuk mengirim pesan sebagai response
    public void SendResponseMessage(string response)
    {
        // Memanggil Coroutine untuk menampilkan tombol speaker
        StartCoroutine(DelayedCreateChatBubble(response, "response", true));
    }

    // Untuk mengatur tombol speaker agar ketika diklik akan mengucapkan response
    private void SetupSpeakerButton(Transform speakerTransform, string response)
    {
        // Jika null maka cetak error
        if (speakerTransform == null)
        {
            Debug.LogError("'Speaker' tidak ditemukan!");
            return;
        }

        // Jika tidak ada speaker maka cetak error
        Transform imgButtonTransform = speakerTransform.Find("SpeakerButton");
        if (imgButtonTransform == null)
        {
            Debug.LogError("'imgButton' tidak ditemukan dalam 'Speaker'!");
            return;
        }

        // Mengambil komponen Button dari imgButton
        Button speakerButton = imgButtonTransform.GetComponent<Button>();
        // Jika null maka cetak error
        if (speakerButton == null)
        {
            Debug.LogError("Komponen Button tidak ditemukan pada 'imgButton'!");
            return;
        }

        // Menghapus listener lama jika ada agar tidak duplikasi
        speakerButton.onClick.RemoveAllListeners();  
        // Menambahkan listener baru untuk mengucapkan response
        speakerButton.onClick.AddListener(() =>
        {
            Debug.Log("Button 'imgButton' diklik!");
            // Untuk mensintesis suara dari teks response
            textToSpeechScript.SynthesizeText(response);
        });
    }

    // Coroutine untuk menunggu dan membuat chat bubble setelah delay
    private IEnumerator DelayedCreateChatBubble(string message, string type, bool addSpeaker = false)
    {
        // Menunggu 1 detik 
        yield return new WaitForSeconds(1f);

        // Setelah 1 detik, buat chat bubble
        CreateChatBubble(message, type, addSpeaker);
    }

    // Membuat tampilan chat buble dari prefab yang sudah ada
    private void CreateChatBubble(string message, string type, bool addSpeaker = false)
    {   
        // Sebagai Anak dari content
        GameObject newMessage = Instantiate(chatBubblePrefab, content);
        
        // Sebagai wadah untuk teks dan gambar 
        Transform wrapper = newMessage.transform.Find("ContentChat/ChatWrapper");
        
        // Mengambil komponen TextMeshPro untuk menampilkan pesan
        TMP_Text messageText = wrapper?.Find("ChatBg/ChatText")?.GetComponent<TMP_Text>();

        // Jika teks ditemukan
        if (messageText != null)
        {
            // Maka set nilai dari messageText dengan message
            messageText.text = message;

            // Untuk mengatur posisi chat Bubble, kanan untuk user, kiri untuk response
            SetMessagePosition(newMessage, type);

            // Jika type adalah response dan addSpeaker bernilai true
            if (type == "response" && addSpeaker)
            {
                // Maka set background color dengan warna Ungu 
                SetBackgroundColor(wrapper, new Color(197f / 255f, 198f / 255f, 255f / 255f)); 

                // Cari Game Objek speaker
                GameObject speakerObject = newMessage.transform.Find("ContentChat/Speaker")?.gameObject;

                // Tampilkan jika dia tidak null
                if (speakerObject != null)
                {
                    speakerObject.SetActive(true);
                }
            }
            else
            {
                // Untuk pesan "user", pastikan speaker tidak muncul
                GameObject speakerObject = newMessage.transform.Find("ContentChat/Speaker")?.gameObject;
                if (speakerObject != null)
                {
                    speakerObject.SetActive(false); // Nonaktifkan speaker jika bukan response
                }
            }
        }
        else
        {
            Debug.LogError("Textbox/Text component not found!");
        }

        // Menyesuaikan layout untuk menampilkan chat bubble
        RebuildLayout(newMessage.transform);
        RebuildLayout(content);

        // Scroll ke bawah setelah layout diperbarui
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }



    // Fungsi untuk mengatur posisi pesan pengguna dan balasan
    private void SetMessagePosition(GameObject message, string type)
    {
        // Mengambil Komponen RectTransform dari ChatBubble Untuk mengatur posisi
        RectTransform messageRect = message.GetComponent<RectTransform>();

        // Mengambil Komponen VerticalLayoutGroup untuk mengatur Tata letak grid
        var contentLayout = messageRect.GetComponent<VerticalLayoutGroup>();

        // Jika contentLayout itu ada nilainya
        if (contentLayout != null)
        {
            // Maka cek lagi jika tipe pesan adalah user, atur alginement
            contentLayout.childAlignment = type == "user" ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        }
    }


    // Fungsi untuk mengatur warna background dari chat bubble
    private void SetBackgroundColor(Transform wrapper, Color color)
    {
        // Mencari object ChatBg dan mengambil komponen Image
        Image bubbleImage = wrapper?.Find("ChatBg")?.GetComponent<Image>();

        // Jika komponen image ditemukan
        if (bubbleImage != null)
        {
            // set warnanya  sesuai parameter color
            bubbleImage.color = color;
        }
        else
        {   // Jika tidak ditemukan tampilkan error
            Debug.LogError("Textbox/Image component not found!");
        }
    }

    // Memperbarui pesan terakhir yang dikirim oleh pengguna atau chat assist
    public void UpdateLastResponseMessage(string newMessage, string messageType)
    {
        // Jika kosong
        if (chatHistory.Count == 0)
        {
            // Tambahkan entri default untuk menghindari error
            chatHistory.Add(new ChatEntry() { userRequest = "", botResponse = "" });
        }

        // Tentukan apakah ini request atau response
        if (chatHistory.Count > 0)
        {
            // Jike tipe user
            if (messageType == "user")
            {
                //Perbarui properti userRequest pada entry terakhir
                chatHistory[chatHistory.Count - 1].userRequest = newMessage;
            }
            else if (messageType == "response")
            {
                // Perbarui properti botResponse pada entry terakhir
                chatHistory[chatHistory.Count - 1].botResponse = newMessage;
                // Setelah respons diupdate, atur tombol speaker untuk respons
                
                if (content.childCount > 0)
                {
                    Transform lastBubble = content.GetChild(content.childCount - 1);
                    Transform speakerTransform = lastBubble?.Find("ContentChat/Speaker");

                    // Pastikan speakerTransform ditemukan dan siap untuk di-setup
                    if (speakerTransform != null)
                    {
                        SetupSpeakerButton(speakerTransform, newMessage);
                    }
                    else
                    {
                        Debug.LogWarning("Speaker transform not found in last bubble!");
                    }
                }
            }

            // Jika ingin memperbarui tampilan chat bubble
            if (content.childCount > 0)
            {
                Transform lastBubble = content.GetChild(content.childCount - 1);
                Transform wrapper = lastBubble.Find("ContentChat/ChatWrapper");
                TMP_Text messageText = wrapper?.Find("ChatBg/ChatText")?.GetComponent<TMP_Text>();

                if (messageText != null)
                {
                    messageText.text = newMessage;

                    // Perbarui tata letak untuk menyesuaikan perubahan
                    LayoutRebuilder.ForceRebuildLayoutImmediate(lastBubble.GetComponent<RectTransform>());
                }
                else
                {
                    Debug.LogError("Textbox/Text component not found in last bubble!");
                }
            }
        }
        else
        {
            Debug.LogWarning("No chat history to update!");
        }

        RebuildLayout(content);

        // Scroll ke bawah setelah layout diperbarui
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // Memaksa unity membangun lang layout secara instan
    private void RebuildLayout(Transform target)
    {
        // Jika target tidak null
        if (target == null) return;
        // Ambil RectTransformnya
        LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>());
    }

        public void ClearChatHistory()
    {
        chatHistory.Clear();
        Debug.Log("Riwayat percakapan telah dikosongkan.");
    }
    
    // Fungsi untuk mengambil riwayat percakapan dan mengirimkan ke SaveButtonHandler
    public List<ChatEntry> GetChatHistory()
    {
        return chatHistory;
    }
}

