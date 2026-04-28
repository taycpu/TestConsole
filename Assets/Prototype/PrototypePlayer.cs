using DebugPanel.Options;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prototype
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PrototypePlayer : MonoBehaviour
    {
        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            Vector2 input = ReadMoveInput();
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            _body.linearVelocity = input * DebugOptions.Current.PrototypePlayerSpeed;
            ClampToArena();
        }

        public void ResetPlayer(Vector3 position)
        {
            transform.position = position;
            if (_body != null)
                _body.linearVelocity = Vector2.zero;
        }

        private void ClampToArena()
        {
            var game = PrototypeGameController.Instance;
            if (game == null)
                return;

            Vector2 extents = game.ArenaExtents - new Vector2(0.4f, 0.4f);
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, -extents.x, extents.x);
            position.y = Mathf.Clamp(position.y, -extents.y, extents.y);
            transform.position = position;
        }

        private static Vector2 ReadMoveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

            return new Vector2(x, y);
        }
    }
}
