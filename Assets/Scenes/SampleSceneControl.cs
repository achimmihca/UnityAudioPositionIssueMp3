using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SampleSceneControl : MonoBehaviour
{
    public AudioSource audioSource;

    public float startTime = 72.308f;

    public bool streamAudio = true;

    public int clickCount;

    public void StopAudio()
    {
        audioSource.Stop();
    }

    public void IncreaseClickCount()
    {
        clickCount++;
        Debug.Log($"Click count: {clickCount}");
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
        // There is an audible difference to the clip that works correctly.
        StartAudio("Scenes/audio-broken.mp3");
    }

    private void StartAudio(string relativePath)
    {
        using var d1 = new DisposableStopwatch("StartAudio took <ms> ms");

        string fullPath = Path.Combine(Application.dataPath, relativePath);
        StartCoroutine(LoadAudioCoroutine(fullPath, streamAudio, audioClip =>
        {
            using var d2 = new DisposableStopwatch("StartAudio callback took <ms> ms");

            audioSource.clip = audioClip;

            audioSource.time = startTime;

            Debug.Log($"Set time to {startTime} s");
            Debug.Log($"AudioSource.time: {audioSource.time} s, length: {audioSource.clip.length} s");
            Debug.Log($"AudioSource.timeSamples: {audioSource.timeSamples}, length in samples: {audioSource.clip.samples}, channels: {audioSource.clip.channels}");

            audioSource.Play();
        }));
    }

    private IEnumerator LoadAudioCoroutine(string uri, bool streamAudio, Action<AudioClip> callback)
    {
        using var d1 = new DisposableStopwatch("LoadAudioCoroutine took <ms> ms");

        UnityWebRequest request = CreateAudioClipRequest(new Uri(uri), streamAudio);
        Debug.Log("LoadAudioCoroutine After CreateAudioClipRequest");

        using (var d2 = new DisposableStopwatch("LoadAudioCoroutine SendWebRequest took <ms> ms"))
        {
            yield return request.SendWebRequest();
            // Debug.Log("LoadAudioCoroutine After SendWebRequest");
        }

        AudioClip clip;
        using (var d3 = new DisposableStopwatch("LoadAudioCoroutine DownloadHandlerAudioClip.GetContent took <ms> ms"))
        {
            clip = DownloadHandlerAudioClip.GetContent(request);
            // Debug.Log("LoadAudioCoroutine After DownloadHandlerAudioClip.GetContent");
        }

        Debug.Log($"Downloaded AudioClip: {clip}");
        callback(clip);
    }

    private static UnityWebRequest CreateAudioClipRequest(Uri uriHandle, bool streamAudio)
    {
        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uriHandle, AudioType.UNKNOWN);
        DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
        downloadHandler.streamAudio = streamAudio;
        return webRequest;
    }
}