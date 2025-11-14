using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.Input;

public class ShootingScript : MonoBehaviour
{
    //[SerializeField] private ParticleSystem particleEffect;

    private HoldObjectScript holdManager;

    void Start()
    {
        holdManager = GetComponent<HoldObjectScript>();
        EventBus.Instance.Subscribe<AttackInputEvent>(this, HandleFire);
    }

    //void OnDestroy()
    //{
    //    EventBus.Instance.Unsubscribe<AttackInputEvent>(HandleFire);
    //}

    void HandleFire(AttackInputEvent input)
    {
        Rigidbody held = holdManager.GetHeldObject();
        if (held == null)
        {
            Debug.Log("Not holding anything to shoot.");
            return;
        }

        FleshCube gun = held.GetComponent<FleshCube>();
        ParticleSystem particleEffect = held.GetComponentInChildren<ParticleSystem>();
        if (gun != null)
        {
            gun.Shoot();

            // Play the particle effect
            if (particleEffect != null)
            {
                particleEffect.Play();
                Debug.Log("Particle play");
            }
            else
            {
                Debug.Log("No Particle");
            }
        }
        else
        {
            Debug.Log("Held object is not a gun.");
        }
    }
}
