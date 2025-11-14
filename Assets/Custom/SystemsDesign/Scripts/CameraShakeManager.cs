using UnityEngine;
using System;
using Unity.Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static event Action OnShakeEvent;

    [SerializeField] private CinemachineCamera cineCamera;
    [SerializeField] CinemachineBasicMultiChannelPerlin noisePerlin;

    [Header("Shake Settings")]
    public float shakeAmplitude = 2f;
    public float shakeFrequency = 2f;
    public float shakeDuration = 0.25f;

    private void Awake()
    {
        //cineCamera = GetComponent<CinemachineCamera>();
        //if (cineCamera != null)
        //    noisePerlin = cineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        OnShakeEvent += TriggerShake;
    }

    private void OnDestroy()
    {
        OnShakeEvent -= TriggerShake;
    }

    private void TriggerShake()
    {
        if (noisePerlin != null)
            StartCoroutine(ShakeRoutine());
    }

    private System.Collections.IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        noisePerlin.AmplitudeGain = shakeAmplitude;
        noisePerlin.FrequencyGain = shakeFrequency;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        noisePerlin.AmplitudeGain = 0f;
        noisePerlin.FrequencyGain = 0f;
    }

    // Public static call method for external scripts
    public static void Shake()
    {
        OnShakeEvent?.Invoke();
    }
}
