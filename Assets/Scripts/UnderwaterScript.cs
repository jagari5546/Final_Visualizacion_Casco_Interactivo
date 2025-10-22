using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterScript : MonoBehaviour
{
    [Header("Depth Parameters")]
    [SerializeField] private Transform mainCamera;
    [Tooltip("Umbral para ENTRAR bajo el agua")]
    [SerializeField] private float enterDepthY = 0f;
    [Tooltip("Umbral para SALIR del agua (mayor que enterDepthY)")]
    [SerializeField] private float exitDepthY  = 1f;  // histeresis: evita rebote
    [SerializeField] private bool useDepthAutoTrigger = true;

    [Header("Fog (RenderSettings) – SOLO END DISTANCE")]
    public bool tweenFogEnd = true;
    public float fogTweenDuration = 2f;
    public AnimationCurve fogCurve = AnimationCurve.Linear(0,0,1,1);

    [Header("Fog End Targets")]
    public float surfaceFogEndStart  = 200f;  // valor inicial al cargar
    public float surfaceFogEndFinal  = 200f;  // target al salir (tu 200)
    public float underwaterFogEndStart = 200f; // punto A al entrar
    public float underwaterFogEndFinal = 450f; // punto B al entrar (tu 450)

    [Header("Post Processing Volumes (weights blend)")]
    [SerializeField] private Volume surfaceVolume;     // weight 1
    [SerializeField] private Volume underwaterVolume;  // weight 0
    public float ppBlendDuration = 1f;
    public AnimationCurve ppCurve = AnimationCurve.Linear(0,0,1,1);

    private bool isUnderwater = false;
    private bool tweenLock = false;    // evita re-disparos durante tween
    private Coroutine fogCo, ppCo;

    void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogEndDistance = surfaceFogEndStart;
        SetPPWeights(1f, 0f);
    }

    void Update()
    {
        if (!useDepthAutoTrigger || mainCamera == null || tweenLock) return;

        float y = mainCamera.position.y;

        if (!isUnderwater && y < enterDepthY)
            EnterUnderwater();
        else if (isUnderwater && y > exitDepthY)
            ExitUnderwater();
    }

    // ===== Entrar/Salir públicos =====
    public void EnterUnderwater()
    {
        if (isUnderwater) return;
        isUnderwater = true;
        StartTransitions(
            fromEnd:  RenderSettings.fogEndDistance,
            toEnd:    underwaterFogEndFinal,
            ppSurfTo: 0f, ppUnderTo: 1f
        );
    }

    public void ExitUnderwater()
    {
        if (!isUnderwater) return;
        isUnderwater = false;
        StartTransitions(
            fromEnd:  RenderSettings.fogEndDistance,
            toEnd:    surfaceFogEndFinal,
            ppSurfTo: 1f, ppUnderTo: 0f
        );
    }

    // ===== Lanzador unificado con lock =====
    void StartTransitions(float fromEnd, float toEnd, float ppSurfTo, float ppUnderTo)
    {
        tweenLock = true;

        if (fogCo != null) StopCoroutine(fogCo);
        fogCo = StartCoroutine(FogEndRoutine(fromEnd, toEnd));

        if (ppCo != null) StopCoroutine(ppCo);
        ppCo = StartCoroutine(PPRoutine(surfaceVolume.weight, underwaterVolume.weight, ppSurfTo, ppUnderTo));
    }

    // ===== PP blend =====
    void SetPPWeights(float surfaceW, float underwaterW)
    {
        if (surfaceVolume)    surfaceVolume.weight = Mathf.Clamp01(surfaceW);
        if (underwaterVolume) underwaterVolume.weight = Mathf.Clamp01(underwaterW);
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

    // ===== SOLO FogEndDistance tween con fuerza al final =====
    IEnumerator FogEndRoutine(float endFrom, float endTo)
    {
        float t = 0f;
        float startValue = endFrom; // partimos del valor actual

        while (t < fogTweenDuration)
        {
            float k = fogCurve.Evaluate(t / fogTweenDuration);
            RenderSettings.fogEndDistance = Mathf.Lerp(startValue, endTo, k);
            t += Time.deltaTime;
            yield return null;
        }

        // Fuerza valor final y libera el lock
        RenderSettings.fogEndDistance = endTo;
        fogCo = null;
        tweenLock = false;
    }

    void OnDisable()
    {
        // Garantiza estado surface al deshabilitar (opcional)
        RenderSettings.fogEndDistance = isUnderwater ? underwaterFogEndFinal : surfaceFogEndFinal;
    }
}
