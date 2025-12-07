using UnityEngine;

public class LimbDamageTester : MonoBehaviour
{
    [SerializeField] private LimbHealth limbSystem; // Reference to your limb system

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            limbSystem.TakeDamageRandom(10);
        }
    }
}
