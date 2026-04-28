using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PrototypeHazard : MonoBehaviour
    {
        private Rigidbody2D _body;
        private Vector2 _direction = Vector2.right;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void FixedUpdate()
        {
            var game = PrototypeGameController.Instance;
            if (game == null)
                return;

            Vector2 extents = game.ArenaExtents - new Vector2(0.4f, 0.4f);
            Vector2 position = transform.position;

            if (position.x <= -extents.x || position.x >= extents.x)
                _direction.x *= -1f;
            if (position.y <= -extents.y || position.y >= extents.y)
                _direction.y *= -1f;

            _direction = _direction.sqrMagnitude < 0.001f ? Vector2.right : _direction.normalized;
            _body.linearVelocity = _direction * PrototypeDebugOptions.Current.PrototypeHazardSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PrototypePlayer>() == null)
                return;

            PrototypeGameController.Instance?.HitHazard();
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction.sqrMagnitude < 0.001f ? Vector2.right : direction.normalized;
        }
    }
}
