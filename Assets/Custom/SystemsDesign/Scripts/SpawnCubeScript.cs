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
    private LimbHealth health;

    void Start()
    {
        holdManager = GetComponent<HoldObjectScript>();
        health = GetComponent<LimbHealth>();

        EventBus.Instance.Subscribe<InteractInputEvent>(this, HandleSpawn);
        EventBus.Instance.Subscribe<DodgeInputEvent>(this, HandleThrow);
    }

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
        // Player cannot spawn a cube if BODY health is zero
        if (holdManager.GetHeldObject() == null &&
            cooldownTimer <= 0f &&
            health.body.CurrentHealth > 0)
        {
            // Spawn cube
            Instantiate(
                fleshCubePrefab,
                holdManager.holdPoint.position,
                holdManager.holdPoint.localRotation
            );

            // Apply damage to a random limb
            health.TakeDamageRandom(10);

            cooldownTimer = spawnCooldown;

            if (audioSource != null)
                audioSource.Play();

            CameraShakeManager.Shake();
        }
        else if (holdManager.GetHeldObject() != null)
        {
            // Destroy cube and heal player body
            Destroy(holdManager.GetHeldObject().gameObject);

            health.HealLimb(LimbType.Body, 10);

            holdManager.ClearHeldObject();
        }
    }
}