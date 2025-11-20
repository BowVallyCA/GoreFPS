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

    [SerializeField] private Vector3 holdOffset = new Vector3(0.3f, -0.3f, 1f);

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
            // World-space offset from camera
            Vector3 offsetWorld = playerCamera.transform.TransformDirection(holdOffset);

            // Offset position beside the camera view
            Vector3 targetPosition = playerCamera.transform.position + offsetWorld;

            // Move object
            heldObject.MovePosition(targetPosition);

            // Rotate weapon to match the camera's exact aiming direction
            heldObject.MoveRotation(Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up));
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
                { heldObject = rb; heldObject.useGravity = false; 
                  heldObject.linearDamping = 10f; 
                  heldObject.transform.position = holdPoint.position; 
                  heldObject.transform.rotation = holdPoint.rotation; 
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
