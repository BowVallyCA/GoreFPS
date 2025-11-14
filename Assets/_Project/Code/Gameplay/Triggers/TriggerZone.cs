using UnityEngine;
using _Project.Code.Gameplay.PlayerControllers.Base;

namespace _Project.Code.Gameplay.Triggers
{
    [RequireComponent(typeof(Collider))]
    public abstract class TriggerZone : MonoBehaviour
    {
        protected Collider Trigger { get; private set; }

        protected virtual void Awake()
        {
            Trigger = GetComponent<Collider>();
            Trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsValidTrigger(other))
            {
                OnZoneEntered(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsValidTrigger(other))
            {
                OnZoneExited(other.gameObject);
            }
        }

        protected virtual bool IsValidTrigger(Collider other)
        {
            return other.GetComponent<BasePlayerController>() != null;
        }

        protected abstract void OnZoneEntered(GameObject obj);
        protected abstract void OnZoneExited(GameObject obj);

        protected virtual void OnDrawGizmos()
        {
            if (Trigger == null)
            {
                Trigger = GetComponent<Collider>();
            }

            if (Trigger == null) return;

            Gizmos.color = GetGizmoColor();
            Gizmos.matrix = transform.localToWorldMatrix;

            if (Trigger is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (Trigger is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }

        protected virtual Color GetGizmoColor()
        {
            return new Color(0, 1, 1, 0.3f);
        }
    }
}
