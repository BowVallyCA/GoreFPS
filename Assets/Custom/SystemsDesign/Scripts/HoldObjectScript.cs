using UnityEngine;

public class HoldObjectScript : MonoBehaviour
{
    [Header("Pickup Settings")]
    public string pickableTag = "Pickable";
    public float pickupRange = 3f;
    public Transform holdPoint;
    public float throwForce = 10f;

    private Camera playerCamera;
    private Rigidbody heldObject;

    void Start()
    {
        playerCamera = Camera.main;
        if (holdPoint == null)
            Debug.LogError("HoldPoint not assigned.");
    }

    void Update()
    {
        if (heldObject != null)
        {
            // Make the hold point follow where the camera is looking
            Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * Vector3.Distance(playerCamera.transform.position, holdPoint.position);

            // Smoothly move and rotate held object to that position/orientation
            heldObject.MovePosition(targetPosition);
            heldObject.MoveRotation(Quaternion.LookRotation(playerCamera.transform.forward));
        }
    }

    public void TryPickup()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag(pickableTag))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    heldObject = rb;
                    heldObject.useGravity = false;
                    heldObject.linearDamping = 10f;

                    // Offset so it sits below the camera view
                    Vector3 belowCameraOffset = playerCamera.transform.forward * 0.8f - playerCamera.transform.up * 0.5f;
                    heldObject.transform.position = playerCamera.transform.position + belowCameraOffset;
                    heldObject.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward);
                }
            }
        }
    }

    public void ThrowObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;
        heldObject.linearDamping = 1f;
        heldObject.transform.parent = null;
        heldObject.linearVelocity = playerCamera.transform.forward * throwForce;
        heldObject = null;
    }

    public Rigidbody GetHeldObject() => heldObject;
    public void ClearHeldObject() => heldObject = null;
}
