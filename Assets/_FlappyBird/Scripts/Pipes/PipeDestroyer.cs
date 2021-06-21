using UnityEngine;

namespace Flappy.Pipes
{
    public class PipeDestroyer : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("PipeParent"))
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
