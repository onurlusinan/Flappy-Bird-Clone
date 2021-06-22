using UnityEngine;

using Flappy.Audio;

namespace Flappy.Gameplay  // This script includes the controls, animations and sfx of flappy bird 
{
    public class BirdController : MonoBehaviour
    {
        private Rigidbody2D rb;

        public float velocity_Multiplier = 1; // 5 is a good value
        public float smooth = 0.1f;
        public int score;

        public bool GameOver;
        public bool GameOngoing;
        private bool isCollided; // for sfx
        private bool GamePaused;

        void Start()
        {
            rb = transform.GetComponent<Rigidbody2D>(); // Get the rigidbody2D component and initialize as rb

            score = 0;

            GameOver = false;
            GameOngoing = false;
            isCollided = false;
        }

        private void OnCollisionEnter2D(Collision2D collision) 
        {
            if (collision.gameObject.CompareTag("Ground") && !isCollided) // For the ground collision
            {
                SoundManager.Instance.Play(Sounds.hit);
                GameOver = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Pipe") && !isCollided)
            {
                Physics2D.IgnoreCollision(GetComponent<CapsuleCollider2D>(), GetComponent<Collider2D>(), true); // Fall on the ground
                SoundManager.Instance.Play(Sounds.hit);
                SoundManager.Instance.Play(Sounds.die);
                isCollided = true;
                GameOver = true;
            }

            if (collision.gameObject.CompareTag("TopTrigger") && !isCollided) // The Trigger at the top
            {
                SoundManager.Instance.Play(Sounds.hit);
                SoundManager.Instance.Play(Sounds.die);
                isCollided = true;
                GameOver = true;
            }

            if (collision.gameObject.CompareTag("PipeParent")) // Trigger for the score increase
            {
                SoundManager.Instance.Play(Sounds.point);
                score++;
            }
        }

        void Update()
        {
            Physics2D.IgnoreLayerCollision(8, 10, false);

            // Rotation animation when falling
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -90), Time.deltaTime * smooth);
            
            
            if (Input.GetMouseButtonDown(0)) // jump when pressed
            {
                rb.velocity = Vector2.up * velocity_Multiplier; //jump
                transform.rotation = Quaternion.Euler(0, 0, 30);

                SoundManager.Instance.Play(Sounds.wing); // Wing sfx
            }

            if (isCollided)
            {
                Physics2D.IgnoreLayerCollision(8, 10, true); // Ignore collision between the bird and the pipes after collision
            }

            if (GameOver)
            {
                transform.rotation = Quaternion.Euler(0, 0, -90); // Change Rotation when Game is Over
            }
        }
    }
}