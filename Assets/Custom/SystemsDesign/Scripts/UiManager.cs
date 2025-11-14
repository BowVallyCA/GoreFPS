using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Slider healthSlider;
    [SerializeField] public TMP_Text nearDeathText;
    [SerializeField] public TMP_Text countdownText;
    [SerializeField] public Image loseScreen;

    private float countdownValue = 0f;
    private float countdownValueMax = 5f;
    private bool isDying = false;

    private void Start()
    {
        countdownValue = countdownValueMax;
        Cursor.lockState = CursorLockMode.Locked; // Locks cursor to screen
    }

    private void Update()
    {
        if(/*newHealth <= 0 &*/ isDying == true)
        {
            //bad way to do it but I plan to fix it later

            countdownValue -= Time.deltaTime;
            countdownText.text = countdownValue.ToString();

            if(countdownValue <= 0f)
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
        }
    }

    private void NearDeath()
    {
        isDying = true;

        nearDeathText.gameObject.SetActive(true);
        //countdownText.gameObject.SetActive(true);
    }
}
