using UnityEngine;

public class OutOfBoundsTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        RespawnPlayer(other.gameObject);
    }

    private void RespawnPlayer(GameObject player)
    {
        // Handle CharacterController
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.SetPositionAndRotation(
                respawnPoint.position,
                respawnPoint.rotation
            );
            controller.enabled = true;
            return;
        }
    }
}