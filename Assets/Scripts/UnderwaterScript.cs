using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterScript : MonoBehaviour
{
    [Header("Depth Parameters")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float depth = 0f;
    [SerializeField] private bool useDepthAutoTrigger = true;

    [Header("Fog (RenderSettings)")]
    public bool tweenFog = true;
    public float fogTweenDuration = 2f;
    public AnimationCurve fogCurve = AnimationCurve.Linear(0,0,1,1);

    [Header("Fog Presets")]
    public float surfaceFogStart = 500f;
    public float surfaceFogEnd   = 1000f;
    public float surfaceFogDensity = 0.005f;
    public float underwaterFogStart = 200f;
    public float underwaterFogEnd   = 800f;
    public float underwaterFogDensity = 0.02f;

    [Header("Post Processing Volumes")]
    [SerializeField] private Volume surfaceVolume;
    [SerializeField] private Volume underwaterVolume;
    public float ppBlendDuration = 2f;
    public AnimationCurve ppCurve = AnimationCurve.Linear(0,0,1,1);

    private bool isUnderwater = false;
    private Coroutine fogCo, ppCo;

    void Awake()
    {
        RenderSettings.fog = true;
        ApplyFogInstant(surfaceFogStart, surfaceFogEnd, surfaceFogDensity);
        SetPPWeights(1f, 0f);
    }

    void Update()
    {
        if (!useDepthAutoTrigger || mainCamera == null) return;

        if (!isUnderwater && mainCamera.position.y < depth)
            EnterUnderwater();
        else if (isUnderwater && mainCamera.position.y >= depth)
            ExitUnderwater();
    }

    public void EnterUnderwater()
    {
        if (isUnderwater) return;
        isUnderwater = true;
        StartFogTween(underwaterFogStart, underwaterFogEnd, underwaterFogDensity);
        StartPPTween(1f, 0f, 0f, 1f); // surface 1->0, underwater 0->1
    }

    public void ExitUnderwater()
    {
        if (!isUnderwater) return;
        isUnderwater = false;
        StartFogTween(surfaceFogStart, surfaceFogEnd, surfaceFogDensity);
        StartPPTween(surfaceVolume.weight, underwaterVolume.weight, 1f, 0f); // vuelve
    }

    void SetPPWeights(float surfaceW, float underwaterW)
    {
        if (surfaceVolume)   surfaceVolume.weight = Mathf.Clamp01(surfaceW);
        if (underwaterVolume) underwaterVolume.weight = Mathf.Clamp01(underwaterW);
    }

    void StartPPTween(float surfFrom, float underFrom, float surfTo, float underTo)
    {
        if (ppCo != null) StopCoroutine(ppCo);
        ppCo = StartCoroutine(PPRoutine(surfFrom, underFrom, surfTo, underTo));
    }

    IEnumerator PPRoutine(float sFrom, float uFrom, float sTo, float uTo)
    {
        float t = 0f;
        while (t < ppBlendDuration)
        {
            float k = ppCurve.Evaluate(t / ppBlendDuration);
            SetPPWeights(Mathf.Lerp(sFrom, sTo, k), Mathf.Lerp(uFrom, uTo, k));
            t += Time.deltaTime;
            yield return null;
        }
        SetPPWeights(sTo, uTo);
        ppCo = null;
    }

    void ApplyFogInstant(float start, float end, float density)
    {
        RenderSettings.fogStartDistance = start;
        RenderSettings.fogEndDistance   = end;
        RenderSettings.fogDensity       = density;
    }

    void StartFogTween(float targetStart, float targetEnd, float targetDensity)
    {
        if (!tweenFog)
        {
            ApplyFogInstant(targetStart, targetEnd, targetDensity);
            return;
        }
        if (fogCo != null) StopCoroutine(fogCo);
        fogCo = StartCoroutine(FogRoutine(targetStart, targetEnd, targetDensity));
    }

    IEnumerator FogRoutine(float tStart, float tEnd, float tDensity)
    {
        float d = Mathf.Max(0.0001f, fogTweenDuration);
        float s0 = RenderSettings.fogStartDistance;
        float e0 = RenderSettings.fogEndDistance;
        float den0 = RenderSettings.fogDensity;

        while (d > 0f)
        {
            float k = fogCurve.Evaluate(1f - (d / fogTweenDuration));
            RenderSettings.fogStartDistance = Mathf.Lerp(s0, tStart, k);
            RenderSettings.fogEndDistance   = Mathf.Lerp(e0, tEnd,   k);
            RenderSettings.fogDensity       = Mathf.Lerp(den0, tDensity, k);
            d -= Time.deltaTime;
            yield return null;
        }

        ApplyFogInstant(tStart, tEnd, tDensity);
        fogCo = null;
    }
}
