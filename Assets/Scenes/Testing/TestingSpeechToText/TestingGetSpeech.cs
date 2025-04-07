using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingGetSpeech : MonoBehaviour
{
   
  public TestingSpeechToText googleSpeechToText;
    private bool isRecording = false;
    private AudioClip audioClip;

    public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }


    private void StartRecording()
    {
        if (isRecording) return;

        isRecording = true;
        audioClip = Microphone.Start(null, false, 10, 44100);
        Debug.Log("Recording started...");
    }

    private void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        Microphone.End(null);
        Debug.Log("Recording stopped.");
        SendAudioToGoogle();
    }

    private void SendAudioToGoogle()
    {
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }

        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        byte[] audioData = ConvertSamplesToBytes(samples);
        googleSpeechToText.SendAudioForTranscription(audioData);
    }

    private byte[] ConvertSamplesToBytes(float[] samples)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
        }

        System.Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        return bytesData;
    }
}
