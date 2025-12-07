using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;
using static ModularWeapon;

public class HoldObjectScript : MonoBehaviour
{
    [Header("Pickup Settings")]
    public string pickableTag = "Pickable";
    public float pickupRange = 3f;
    public Transform holdPoint;
    public float throwForce = 10f;

    private Camera playerCamera;
    private Rigidbody heldObject;

    [SerializeField] private Vector3 holdOffset = new Vector3(0.3f, -0.3f, 1f);

    // --- Interaction Additions ---
    [Header("Spawn Settings")]
    public AudioSource audioSource;
    public float spawnCooldown = 1f;

    private float cooldownTimer;
    private LimbHealth limbHealth;

    void Start()
    {
        playerCamera = Camera.main;

        // Limb system reference
        limbHealth = GetComponent<LimbHealth>();

        // Subscribe to input events (copied exactly from SpawnCubeScript)
        EventBus.Instance.Subscribe<InteractInputEvent>(this, HandleSpawn);
        EventBus.Instance.Subscribe<DodgeInputEvent>(this, HandleThrow);
    }

    void Update()
    {
        if (heldObject != null)
        {
            // Calculate world offset
            Vector3 offsetWorld = playerCamera.transform.TransformDirection(holdOffset);
            Vector3 targetPosition = playerCamera.transform.position + offsetWorld;

            // Smooth follow without parenting
            heldObject.MovePosition(targetPosition);

            // Rotate item to match player aim
            heldObject.MoveRotation(
                Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up)
            );
        }

        // Cooldown timer
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    // =======================
    //       PICKUP
    // =======================
    public void TryPickup()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag(pickableTag))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                    EquipWeapon(rb.gameObject);
            }
        }
    }

    // =======================
    //        THROW
    // =======================
    public void ThrowObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;
        heldObject.linearDamping = 0f;
        heldObject.angularDamping = 0.05f;

        heldObject.linearVelocity = playerCamera.transform.forward * throwForce;

        heldObject = null;
    }

    // =======================
    //        EQUIP
    // =======================
    public void EquipWeapon(GameObject weapon)
    {
        if (weapon == null) return;

        // Drop old object (throw it)
        if (heldObject != null)
            ThrowObject();

        Rigidbody rb = weapon.GetComponent<Rigidbody>();
        if (rb == null) return;

        heldObject = rb;
        rb.useGravity = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = holdPoint.position;
        rb.rotation = holdPoint.rotation;
    }

    public Rigidbody GetHeldObject() => heldObject;

    public void ClearHeldObject() => heldObject = null;


    // ================================================================
    //                ======  Interaction Functions  ======
    //        (Copied exactly from SpawnCubeScript and merged here)
    // ================================================================

    void HandleThrow(DodgeInputEvent input)
    {
        if (heldObject == null)
            TryPickup();
        else
            ThrowObject();
    }

    void HandleSpawn(InteractInputEvent input)
    {
        // If NO weapon held: spawn one
        //if (heldObject == null &&
        //    cooldownTimer <= 0f &&
        //    limbHealth.body.CurrentHealth > 0)
        //{
        //    // Spawn cube or weapon
        //    limbHealth.TakeDamageRandom(10);

        //    cooldownTimer = spawnCooldown;

        //    if (audioSource != null)
        //        audioSource.Play();

        //    CameraShakeManager.Shake();
        //}
        if (heldObject != null)
        {
            // Player is holding a weapon and wants to consume it

            // Identify which limb should be healed
            WeaponOrigin origin = heldObject.GetComponent<WeaponOrigin>();

            LimbType limbToHeal = LimbType.Body;   // default
            if (origin != null)
                limbToHeal = origin.spawnedFromLimb;

            // Heal that limb
            limbHealth.HealLimb(limbToHeal, 10);

            // Destroy held weapon
            Destroy(heldObject.gameObject);
            ClearHeldObject();
        }
    }
}