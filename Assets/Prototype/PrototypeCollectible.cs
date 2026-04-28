using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Collider2D))]
    public class PrototypeCollectible : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PrototypePlayer>() == null)
                return;

            PrototypeGameController.Instance?.Collect(this);
        }
    }
}
