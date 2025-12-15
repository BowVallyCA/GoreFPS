using UnityEngine;

public class HeadlightScript : MonoBehaviour
{
    [Header("Headlight Settings")]
    public Light headlight; // Reference to the light object
    public AudioSource audioSource; // Reference to the AudioSource for sound
    public AudioClip toggleSound; // Sound to play when toggling the headlight
    public bool isOn = false; // Track headlight status

    [Header("Interaction Settings")]
    public KeyCode toggleKey = KeyCode.F; // Key to toggle the headlight

    private void Start()
    {
        if (headlight == null)
        {
            Debug.LogError("Headlight not assigned!", this);
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not assigned, the headlight toggle sound will not play.");
        }

        // Set initial state
        headlight.enabled = isOn;
    }

    private void Update()
    {
        // Check for interaction input (can be replaced by your interaction system, e.g. EventBus)
        if (Input.GetKeyDown(toggleKey)) // Replace with your interaction system if needed
        {
            ToggleHeadlight();
        }
    }

    // Toggle the headlight's state
    private void ToggleHeadlight()
    {
        isOn = !isOn;

        // Set headlight's enabled state
        headlight.enabled = isOn;

        // Play the toggle sound
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }

        // Optionally, you could trigger animations or other effects here
        Debug.Log($"Headlight is now {(isOn ? "ON" : "OFF")}");
    }
}
