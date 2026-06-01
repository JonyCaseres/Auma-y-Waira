using System.Collections.Generic;
using UnityEngine;

public class ComicSequenceComponent : MonoBehaviour
{
    [Header("Viñetas (usar como componente en escena)")]
    public List<Sprite> panels = new List<Sprite>();
    public float fadeDuration = 0.2f;
    public bool waitForInteract = true;
    public float autoAdvanceDelay = 1.5f;
    public bool loadNextScene = true;
    public string sceneName = "";
}