using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TestingGemini : MonoBehaviour
{
    private const string API_KEY = "AIzaSyCpByEWj3I99n4Dj8UHfhuxycwL686XViA";
    private const string API_URL = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent";
    
    private Dictionary<string, string> knowledgeBase = new Dictionary<string, string>();
    
    private void Start()
    {
        LoadKnowledgeBase();
    }
    
    private void LoadKnowledgeBase()
    {
        string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "KnowledgeBase");
        
        if (!Directory.Exists(streamingAssetsPath))
        {
            Debug.LogError("KnowledgeBase folder tidak ditemukan di StreamingAssets!");
            return;
        }
        
        string[] txtFiles = Directory.GetFiles(streamingAssetsPath, "*.txt");
        
        foreach (string filePath in txtFiles)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string content = File.ReadAllText(filePath);
                knowledgeBase[fileName] = content;
                Debug.Log($"Loaded knowledge base: {fileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file {filePath}: {e.Message}");
            }
        }
    }

    public async Task<string> GenerateContent(string prompt)
    {
        try
        {
            // Gabungkan konteks dari knowledge base dengan prompt
            string contextualizedPrompt = CreateContextualizedPrompt(prompt);

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = contextualizedPrompt }
                            }
                        }
                    }
                };

                string jsonBody = JsonConvert.SerializeObject(requestBody);
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{API_URL}?key={API_KEY}"),
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<ResponseData>(responseContent);
                    return responseData.candidates[0].content.parts[0].text;
                }
                else
                {
                    Debug.LogError($"Error: {response.StatusCode} - {responseContent}");
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
            return null;
        }
    }

    private string CreateContextualizedPrompt(string userPrompt)
    {
        StringBuilder contextBuilder = new StringBuilder();
        
        contextBuilder.AppendLine("Gunakan HANYA informasi berikut ini untuk menjawab pertanyaan:");
        contextBuilder.AppendLine("---");
        
        foreach (var entry in knowledgeBase)
        {
            contextBuilder.AppendLine($"[{entry.Key}]:");
            contextBuilder.AppendLine(entry.Value);
            contextBuilder.AppendLine("---");
        }
        
        contextBuilder.AppendLine("Pertanyaan: " + userPrompt);
        contextBuilder.AppendLine("Jawablah HANYA menggunakan informasi di atas. Jika informasinya tidak ada di atas, katakan 'Maaf, saya tidak memiliki informasi tentang hal tersebut dalam basis pengetahuan saya.'");
        
        return contextBuilder.ToString();
    }

    // Kelas untuk deserialisasi response (sama seperti sebelumnya)
    [Serializable]
    private class ResponseData
    {
        public Candidates[] candidates;
    }

    [Serializable]
    private class Candidates
    {
        public Content content;
    }

    [Serializable]
    private class Content
    {
        public Parts[] parts;
    }

    [Serializable]
    private class Parts
    {
        public string text;
    }
}
