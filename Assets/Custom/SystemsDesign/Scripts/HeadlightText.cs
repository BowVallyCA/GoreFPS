using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class WorldTextTrigger : MonoBehaviour
{
    [Header("References")]
    public TMP_Text worldText;          // TextMeshPro (3D or World Space)
    public Animator textAnimator;       // Animator controlling fade in/out

    [Header("Animator Triggers")]
    public string fadeInTrigger = "FadeIn";
    public string fadeOutTrigger = "FadeOut";

    private void Reset()
    {
        // Ensure the collider is a trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void Start()
    {
        if (worldText == null)
            worldText = GetComponentInChildren<TMP_Text>();

        if (textAnimator == null)
            textAnimator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (textAnimator != null)
            textAnimator.SetTrigger(fadeInTrigger);
        Debug.Log("FadeIn");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (textAnimator != null)
            textAnimator.SetTrigger(fadeOutTrigger);
        Debug.Log("FadeOut");
    }
}
