using System;
using Game.Pipes;
using UnityEngine;

namespace Game.Bird
{
    // Movement and collision only; GameFlow decides when to launch and flap, and reacts to the events.
    [DisallowMultipleComponent]
    public sealed class BirdController : MonoBehaviour
    {
        private const float FlapTilt = 30f;
        private const float DeadTilt = -90f;

        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Animator animator;
        [SerializeField] private float flapVelocity = 7f;

        // Per-frame Slerp factor towards the nose-down pose while falling; lower dives more lazily.
        [SerializeField] private float diveRotationSpeed = 0.4f;
        [SerializeField] private float hoverAmplitude = 0.06f;
        [SerializeField] private float hoverPeriod = 0.8f;

        private static readonly Quaternion DiveRotation = Quaternion.Euler(0f, 0f, DeadTilt);

        private BirdState _state;
        private Vector3 _hoverOrigin;

        public event Action Scored;
        public event Action<DeathCause> Died;

        private enum BirdState
        {
            Hovering,
            Flying,
            Dead,
        }

        private void Update()
        {
            switch (_state)
            {
                case BirdState.Hovering:
                    var phase = Time.time / hoverPeriod * 2f * Mathf.PI;
                    transform.position = _hoverOrigin + Vector3.up * (Mathf.Sin(phase) * hoverAmplitude);
                    break;
                case BirdState.Flying:
                    transform.rotation = Quaternion.Slerp(transform.rotation, DiveRotation,
                        Time.deltaTime * diveRotationSpeed);
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_state != BirdState.Flying)
            {
                return;
            }

            if (other.TryGetComponent<Pipe>(out _))
            {
                Scored?.Invoke();
            }
            else if (other.TryGetComponent<Hazard>(out _))
            {
                Die(DeathCause.Obstacle);
            }
        }

        // The ground is the only solid collider, so a dead bird still falls through pipes and lands on it.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_state == BirdState.Flying && collision.gameObject.TryGetComponent<Hazard>(out _))
            {
                Die(DeathCause.Ground);
            }
        }

        public void Initialize()
        {
            _state = BirdState.Hovering;
            _hoverOrigin = transform.position;
            transform.rotation = Quaternion.identity;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.linearVelocity = Vector2.zero;
        }

        public void Launch()
        {
            if (_state != BirdState.Hovering)
            {
                return;
            }

            _state = BirdState.Flying;
            body.bodyType = RigidbodyType2D.Dynamic;
        }

        public void Flap()
        {
            if (_state != BirdState.Flying)
            {
                return;
            }

            body.linearVelocity = Vector2.up * flapVelocity;
            transform.rotation = Quaternion.Euler(0f, 0f, FlapTilt);
        }

        private void Die(DeathCause cause)
        {
            _state = BirdState.Dead;
            transform.rotation = DiveRotation;
            animator.enabled = false;
            Died?.Invoke(cause);
        }
    }
}
