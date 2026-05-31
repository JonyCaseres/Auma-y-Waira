using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RuntimeAudioLoader : MonoBehaviour
{
    public AudioSource targetSource;
    public string fileName = "voice.mp3"; // nombre en StreamingAssets

    public void StartLoading() => StartCoroutine(LoadClipCoroutine());

    private IEnumerator LoadClipCoroutine()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (uwr.result != UnityWebRequest.Result.Success)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError($"Error cargando audio: {uwr.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                targetSource.clip = clip;
                // No loop por defecto; usa targetSource.Play() o controlalo desde tu script
                targetSource.Play();
            }
        }
    }
}               