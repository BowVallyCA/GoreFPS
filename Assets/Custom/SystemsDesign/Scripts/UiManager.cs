using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UiManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Slider healthSlider;
    [SerializeField] public TMP_Text nearDeathText;
    [SerializeField] public TMP_Text countdownText;
    [SerializeField] public Image loseScreen;
    [SerializeField] private Animator flashAnimator;

    [Header("Blood Overlay")]
    [Tooltip("UI Image placed on top of the screen (full-screen) that contains a semi-transparent blood/splatter sprite.")]
    [SerializeField] private Image bloodOverlay;
    [Tooltip("Maximum alpha the blood overlay can reach when at 0 health.")]
    [SerializeField] private float maxBloodAlpha = 0.85f;
    [Tooltip("Smoothing speed when changing blood intensity.")]
    [SerializeField] private float bloodLerpSpeed = 6f;
    [Tooltip("If true, blood intensity uses a nonlinear curve (more dramatic as health gets low).")]
    [SerializeField] private bool useNonLinearResponse = true;
    [Tooltip("Exponent applied when useNonLinearResponse is true. Higher => steeper curve near zero health.")]
    [SerializeField] private float nonLinearExponent = 2f;

    private float countdownValue = 0f;
    private float countdownValueMax = 5f;
    private bool isDying = false;

    // coroutines
    private Coroutine bloodCoroutine;
    private Coroutine pulseCoroutine;

    private void Reset()
    {
        // helpful default if user adds component in-editor
        maxBloodAlpha = 0.85f;
        bloodLerpSpeed = 6f;
        nonLinearExponent = 2f;
    }

    private void Start()
    {
        //AnimationClip Anim = flashAnim.GetComponent<AnimationClip>();

        countdownValue = countdownValueMax;
        Cursor.lockState = CursorLockMode.Locked; // Locks cursor to screen

        // Ensure blood overlay is set up correctly
        if (bloodOverlay != null)
        {
            bloodOverlay.raycastTarget = false; // don't block input
            SetImageAlpha(bloodOverlay, 0f);
            bloodOverlay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDying)
        {
            // bad way to do it but I plan to fix it later
            countdownValue -= Time.deltaTime;
            countdownText.text = countdownValue.ToString("F1");

            if (countdownValue <= 0f)
            {
                loseScreen.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to health change event
        HealthScript.OnHealthChanged += UpdateHealthUI;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        HealthScript.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int newHealth)
    {
        if (healthSlider != null)
            healthSlider.value = newHealth;

        // Update blood overlay based on health ratio
        UpdateBloodForHealth(newHealth);

        if (newHealth <= 0)
        {
            NearDeath();
        }
        else if (newHealth >= 0 & isDying == true)
        {
            nearDeathText.gameObject.SetActive(false);
            //countdownText.gameObject.SetActive(false);

            isDying = false;
            countdownValue = countdownValueMax;

            // Stop pulse if it was running
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
        }
    }

    private void NearDeath()
    {
        isDying = true;

        nearDeathText.gameObject.SetActive(true);
        //countdownText.gameObject.SetActive(true);

        // make blood overlay more intense and start pulsing
        if (bloodOverlay != null)
        {
            ShowBloodInstant(maxBloodAlpha * 0.95f); // immediate heavy blood
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(DyingPulse(0.08f, 1.12f, 0.9f));
        }
    }

    /// <summary>
    /// Compute target alpha from health and start a coroutine to lerp the overlay.
    /// </summary>
    private void UpdateBloodForHealth(int newHealth)
    {
        if (bloodOverlay == null)
            return;

        float maxVal = 1f;
        if (healthSlider != null)
            maxVal = Mathf.Max(1f, healthSlider.maxValue);

        float healthRatio = Mathf.Clamp01(newHealth / maxVal); // 1 when full, 0 when empty

        // We want blood alpha to be 0 at full health, max at zero health.
        float inverse = 1f - healthRatio;

        // optionally make response non-linear for more dramatic low-health effect
        float response = useNonLinearResponse ? Mathf.Pow(inverse, nonLinearExponent) : inverse;

        float targetAlpha = Mathf.Clamp01(response) * maxBloodAlpha;

        // If target is basically zero, hide overlay; otherwise show and lerp.
        if (targetAlpha <= 0.001f)
        {
            if (bloodCoroutine != null)
            {
                StopCoroutine(bloodCoroutine);
                bloodCoroutine = null;
            }
            StartCoroutine(HideBloodSmoothly());
        }
        else
        {
            if (bloodCoroutine != null)
                StopCoroutine(bloodCoroutine);
            bloodCoroutine = StartCoroutine(LerpBloodAlpha(targetAlpha));
        }
    }

    private IEnumerator LerpBloodAlpha(float targetAlpha)
    {
        if (bloodOverlay == null) yield break;

        bloodOverlay.gameObject.SetActive(true);

        Color c = bloodOverlay.color;
        float startAlpha = c.a;
        float t = 0f;
        // Use a smooth exponential-like lerp for responsiveness
        while (t < 1f)
        {
            t += Time.deltaTime * bloodLerpSpeed;
            float a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            SetImageAlpha(bloodOverlay, a);
            yield return null;
        }
        SetImageAlpha(bloodOverlay, targetAlpha);
        bloodCoroutine = null;
    }

    private IEnumerator HideBloodSmoothly()
    {
        if (bloodOverlay == null) yield break;

        Color c = bloodOverlay.color;
        float startAlpha = c.a;
        float t = 0f;
        float duration = Mathf.Max(0.15f, 0.35f); // quick fade out
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, t / duration);
            SetImageAlpha(bloodOverlay, a);
            yield return null;
        }
        SetImageAlpha(bloodOverlay, 0f);
        bloodOverlay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Small pulsing effect to give a heartbeat-like intensification when dying.
    /// amplitudeMultiplier: how much to multiply the current alpha for the peak (e.g. 1.1 -> +10%)
    /// speedMultiplier: speed of the sin wave
    /// baselineFactor: how much of the current alpha to use as baseline (0..1)
    /// </summary>
    private IEnumerator DyingPulse(float speedMultiplier = 0.1f, float amplitudeMultiplier = 1.12f, float baselineFactor = 0.85f)
    {
        if (bloodOverlay == null) yield break;

        // Ensure overlay active
        bloodOverlay.gameObject.SetActive(true);

        // capture a stable base alpha (in case other coroutines change it)
        float baseAlpha = bloodOverlay.color.a;
        float elapsed = 0f;

        while (isDying)
        {
            // recompute baseAlpha smoothly in case other updates occur
            baseAlpha = Mathf.Lerp(baseAlpha, bloodOverlay.color.a, Time.deltaTime * 2f);

            elapsed += Time.deltaTime;
            float sin = Mathf.Sin(elapsed / Mathf.Max(0.01f, speedMultiplier) * Mathf.PI * 2f); // -1..1
            float pulse = Mathf.Lerp(baselineFactor, amplitudeMultiplier, (sin + 1f) * 0.5f); // map to baseline..amplitude
            float target = Mathf.Clamp01(baseAlpha * pulse);
            SetImageAlpha(bloodOverlay, target);
            yield return null;
        }

        // when stopping pulse, ensure overlay returns to base alpha (or hide)
        yield return StartCoroutine(LerpBloodAlpha(baseAlpha));
        pulseCoroutine = null;
    }

    private void ShowBloodInstant(float alpha)
    {
        if (bloodOverlay == null) return;
        bloodOverlay.gameObject.SetActive(true);
        SetImageAlpha(bloodOverlay, Mathf.Clamp01(alpha));
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = Mathf.Clamp01(alpha);
        img.color = c;
    }

    public void GreenFlash()
    {
        flashAnimator.Play("GreenFlash");
    }
}