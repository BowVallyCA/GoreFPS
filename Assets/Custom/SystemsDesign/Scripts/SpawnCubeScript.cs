using _Project.Code.Gameplay.Input;
using UnityEngine;
using _Project.Code.Core.Events;
using Unity.Cinemachine;

public class SpawnCubeScript : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject fleshCubePrefab;
    public AudioSource audioSource;
    public float spawnCooldown = 1f;

    private float cooldownTimer;
    private HoldObjectScript holdManager;
    private HealthScript health;

    void Start()
    {
        holdManager = GetComponent<HoldObjectScript>();
        health = GetComponent<HealthScript>();

        EventBus.Instance.Subscribe<InteractInputEvent>(this, HandleSpawn);
        EventBus.Instance.Subscribe<DodgeInputEvent>(this, HandleThrow);
    }

    //void OnDestroy()
    //{
    //    EventBus.Instance.Unsubscribe<InteractInputEvent>(HandleSpawn);
    //    EventBus.Instance.Unsubscribe<DodgeInputEvent>(HandleThrow);
    //}

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    void HandleThrow(DodgeInputEvent input)
    {
        if (holdManager.GetHeldObject() == null)
            holdManager.TryPickup();
        else
            holdManager.ThrowObject();
    }

    void HandleSpawn(InteractInputEvent input)
    {
        if (holdManager.GetHeldObject() == null && cooldownTimer <= 0f && health.currentHealth > 0)
        {
            Instantiate(fleshCubePrefab, holdManager.holdPoint.position, holdManager.holdPoint.localRotation);
            health.TakeDamage(10);
            cooldownTimer = spawnCooldown;

            if (audioSource != null)
                audioSource.Play();

            CameraShakeManager.Shake();
        }
        else if (holdManager.GetHeldObject() != null)
        {
            Destroy(holdManager.GetHeldObject().gameObject);
            health.GainHealth(10);
            holdManager.ClearHeldObject();
        }
    }
}
