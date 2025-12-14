using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UiManager : MonoBehaviour
{
    [Header("Overall Health (Optional)")]
    public Slider overallHealthSlider;

    [Header("Limb Sliders")]
    public Slider headSlider;
    public Slider bodySlider;
    public Slider armsSlider;
    public Slider legsSlider;

    [Header("UI Text / Screens")]
    public TMP_Text nearDeathText;
    public TMP_Text countdownText;
    public Image loseScreen;

    [Header("Blood Overlay")]
    [SerializeField] private Image bloodOverlay;
    [SerializeField] private float maxBloodAlpha = 0.85f;
    [SerializeField] private float bloodLerpSpeed = 6f;
    [SerializeField] private bool useNonLinearResponse = true;
    [SerializeField] private float nonLinearExponent = 2f;

    private LimbHealth limbHealth;

    private float countdownValue = 0f;
    private float countdownValueMax = 5f;
    private bool isDying = false;

    private Coroutine bloodCoroutine;
    private Coroutine pulseCoroutine;

    private void OnEnable()
    {
        LimbHealth.OnLimbHealthChanged += OnLimbUpdate;
    }

    private void OnDisable()
    {
        LimbHealth.OnLimbHealthChanged -= OnLimbUpdate;
    }

    private void UpdateLimbSlider(LimbType limb, float newHealth)
    {
        if (limb == LimbType.Arms)
            armsSlider.value = newHealth;
        else if (limb == LimbType.Legs)
            legsSlider.value = newHealth;
        else if (limb == LimbType.Body)
            bodySlider.value = newHealth;
        else if (limb == LimbType.Head)
            headSlider.value = newHealth;
    }

    private void Start()
    {
        limbHealth = FindAnyObjectByType<LimbHealth>();
        countdownValue = countdownValueMax;

        if (bloodOverlay != null)
        {
            bloodOverlay.raycastTarget = false;
            SetImageAlpha(bloodOverlay, 0f);
            bloodOverlay.gameObject.SetActive(false);
        }

        InitializeLimbSliders();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (limbHealth == null) return;

        float totalPercent = limbHealth.GetTotalHealthPercent();

        if (overallHealthSlider != null)
            overallHealthSlider.value = totalPercent;

        UpdateBloodForHealth(totalPercent);

        if (isDying)
        {
            countdownValue -= Time.deltaTime;
            countdownText.text = countdownValue.ToString("F1");

            if (countdownValue <= 0f)
            {
                loseScreen.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    // ----------------------------------------------------------------------
    // Limb Slider Setup + Update
    // ----------------------------------------------------------------------

    private void InitializeLimbSliders()
    {
        if (limbHealth == null) return;

        SetSlider(headSlider, limbHealth.head);
        SetSlider(bodySlider, limbHealth.body);
        SetSlider(armsSlider, limbHealth.arms);
        SetSlider(legsSlider, limbHealth.legs);
    }

    private void SetSlider(Slider slider, LimbData limb)
    {
        if (slider == null) return;

        slider.minValue = 0;
        slider.maxValue = limb.MaxHealth;
        slider.value = limb.CurrentHealth;
    }

    public void OnLimbUpdate(LimbType limb, int current, int max)
    {
        switch (limb)
        {
            case LimbType.Head:
                if (headSlider != null) headSlider.value = current;
                break;
            case LimbType.Body:
                if (bodySlider != null) bodySlider.value = current;
                break;
            case LimbType.Arms:
                if (armsSlider != null) armsSlider.value = current;
                break;
            case LimbType.Legs:
                if (legsSlider != null) legsSlider.value = current;
                break;
        }

        float total = limbHealth.GetTotalHealthPercent();
        HandleNearDeathState(total);
    }

    // ----------------------------------------------------------------------
    // Near Death Logic
    // ----------------------------------------------------------------------

    private void HandleNearDeathState(float percent)
    {
        if (percent <= 0f)
        {
            EnterNearDeath();
        }
        else if (percent > 0f && isDying)
        {
            nearDeathText.gameObject.SetActive(false);
            isDying = false;
            countdownValue = countdownValueMax;

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
        }
    }

    private void EnterNearDeath()
    {
        isDying = true;
        nearDeathText.gameObject.SetActive(true);

        if (bloodOverlay != null)
        {
            ShowBloodInstant(maxBloodAlpha * 0.95f);

            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);

            pulseCoroutine = StartCoroutine(DyingPulse(0.08f, 1.12f, 0.9f));
        }
    }

    // ==========================================================
    // NEAR-DEATH COUNTDOWN (called by LimbHealth)
    // ==========================================================

    public void BeginDeathCountdown(float time)
    {
        countdownValue = time;
        isDying = true;

        countdownText.gameObject.SetActive(true);
        nearDeathText.gameObject.SetActive(true);
    }

    public void UpdateDeathTimer(float remaining)
    {
        if (!isDying) return;

        countdownValue = remaining;
        countdownText.text = countdownValue.ToString("F1");

        if (countdownValue <= 0f)
        {
            ShowLoseScreen();
        }
    }

    public void CancelDeathCountdown()
    {
        isDying = false;

        countdownText.gameObject.SetActive(false);
        nearDeathText.gameObject.SetActive(false);

        countdownValue = countdownValueMax;
    }

    public void ShowLoseScreen()
    {
        loseScreen.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);
        nearDeathText.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }

    // ----------------------------------------------------------------------
    // Blood Screen Logic
    // ----------------------------------------------------------------------

    private void UpdateBloodForHealth(float percent)
    {
        if (bloodOverlay == null)
            return;

        float inverse = 1f - percent;
        float response = useNonLinearResponse
            ? Mathf.Pow(inverse, nonLinearExponent)
            : inverse;

        float targetAlpha = Mathf.Clamp01(response) * maxBloodAlpha;

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
        bloodOverlay.gameObject.SetActive(true);

        Color c = bloodOverlay.color;
        float startAlpha = c.a;
        float t = 0f;

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
        Color c = bloodOverlay.color;
        float startAlpha = c.a;
        float t = 0f;
        float duration = 0.35f;

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

    private IEnumerator DyingPulse(float speedMultiplier, float amplitudeMultiplier, float baselineFactor)
    {
        float baseAlpha = bloodOverlay.color.a;
        float elapsed = 0f;

        while (isDying)
        {
            baseAlpha = Mathf.Lerp(baseAlpha, bloodOverlay.color.a, Time.deltaTime * 2f);

            elapsed += Time.deltaTime;
            float sin = Mathf.Sin(elapsed / Mathf.Max(0.01f, speedMultiplier) * Mathf.PI * 2f);
            float pulse = Mathf.Lerp(baselineFactor, amplitudeMultiplier, (sin + 1f) * 0.5f);
            float target = Mathf.Clamp01(baseAlpha * pulse);

            SetImageAlpha(bloodOverlay, target);
            yield return null;
        }

        yield return StartCoroutine(LerpBloodAlpha(baseAlpha));
        pulseCoroutine = null;
    }

    private void ShowBloodInstant(float alpha)
    {
        bloodOverlay.gameObject.SetActive(true);
        SetImageAlpha(bloodOverlay, Mathf.Clamp01(alpha));
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}