using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComicSequence", menuName = "ScriptableObjects/Comic Sequence", order = 100)]
public class ComicSequence : ScriptableObject
{
    [Header("Identificación")]
    public string sequenceName;

    [Header("Viñetas")]
    public List<Sprite> panels = new List<Sprite>();

    [Header("Transición")]
    public float fadeDuration = 0.2f;
    public bool waitForInteract = true;
    public float autoAdvanceDelay = 1.5f;

    [Header("Carga de escena")]
    public bool loadNextScene = true;
    public string sceneName = "";
}