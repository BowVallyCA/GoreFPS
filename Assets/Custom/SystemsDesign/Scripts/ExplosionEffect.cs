using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource.Play();
    }

    void Update()
    {
        //Probobly a better way to do this

        if (audioSource != null && !audioSource.isPlaying)
        {
            Destroy(gameObject != null ? gameObject : gameObject);
        }
    }
}
