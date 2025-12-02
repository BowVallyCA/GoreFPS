using UnityEngine;
using UnityEngine.VFX;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

public class ShootingScript : MonoBehaviour
{
    private HoldObjectScript holdManager;

    void Start()
    {
        holdManager = GetComponent<HoldObjectScript>();
        EventBus.Instance.Subscribe<AttackInputEvent>(this, HandleFire);
    }

    void HandleFire(AttackInputEvent input)
    {
        Rigidbody held = holdManager.GetHeldObject();
        if (held == null)
        {
            Debug.Log("Not holding anything to shoot.");
            return;
        }

        FleshCube gun = held.GetComponent<FleshCube>();
        VisualEffect vfx = held.GetComponentInChildren<VisualEffect>();

        if (gun != null)
        {
            gun.Shoot();

            if (vfx != null)
            {
                vfx.Play();
                Debug.Log("VFX play");
            }
            else
            {
                Debug.Log("No VFX component found");
            }
        }
        else
        {
            Debug.Log("Held object is not a gun.");
        }
    }
}
