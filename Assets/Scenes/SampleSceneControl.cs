using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class SampleSceneControl : MonoBehaviour
{
    public AudioSource audioSource;

    public float startTime = 72.308f;

    public void StopAudio()
    {
        audioSource.Stop();
    }

    public void StartAudioOk()
    {
        // This AudioClip starts playback at the correct position.
        // One can verify in another app (e.g. Audacity) that this corresponds to the correct playback position.
        StartAudio("Scenes/audio-ok.ogg");
    }

    public void StartAudioBroken()
    {
        // This AudioClip starts playback slightly off, approx. 1 second off.
        // There is an audible difference to the clip that works correctly..
        StartAudio("Scenes/audio-broken.mp3");
    }

    private void StartAudio(string relativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, relativePath);

        AudioClip audioClip = LoadAudioClipImmediately(fullPath, true);
        audioSource.clip = audioClip;

        audioSource.time = startTime;

        Debug.Log($"Set time to {startTime} s");
        Debug.Log($"AudioSource.time: {audioSource.time} s, length: {audioSource.clip.length} s");
        Debug.Log($"AudioSource.timeSamples: {audioSource.timeSamples}, length in samples: {audioSource.clip.samples}, channels: {audioSource.clip.channels}");

        audioSource.Play();
    }

    private static AudioClip LoadAudioClipImmediately(string uri, bool streamAudio)
    {
        Uri uriHandle = new Uri(uri);
        using UnityWebRequest webRequest = CreateAudioClipRequest(uriHandle, streamAudio);
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            Debug.LogWarning("Waiting for AudioClip to load via Thread.Sleep");
            Thread.Sleep(10);
        }

        if (webRequest.result
            is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error Loading Audio: " + uri);
            Debug.LogError(webRequest.error);
        }

        AudioClip audioClip = (webRequest.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
        string fileName = Path.GetFileName(uriHandle.LocalPath);
        audioClip.name = $"Audio file '{fileName}'";
        return audioClip;
    }

    private static UnityWebRequest CreateAudioClipRequest(Uri uriHandle, bool streamAudio)
    {
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uriHandle, AudioType.UNKNOWN);
        DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
        downloadHandler.streamAudio = streamAudio;
        return webRequest;
    }
}