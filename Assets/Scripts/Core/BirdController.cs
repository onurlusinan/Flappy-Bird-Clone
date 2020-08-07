﻿using UnityEngine;

namespace Assets.Scripts.Core  // This script includes the controls, animations and sfx of flappy bird 
{
    public class BirdController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private GameManager GameManager; 

        public float velocity_Multiplier = 1; // 5 is a good value
        public float smooth = 0.1f;
        public int Score;

        public bool GameOver;
        public bool GameOngoing;
        private bool isCollided; // for sfx
        private bool GamePaused;
        private void Awake()
        {
            GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }

        void Start()
        {
            rb = transform.GetComponent<Rigidbody2D>(); // Get the rigidbody2D component and initialize as rb

            Score = 0;

            GameOver = false;
            GameOngoing = false;
            isCollided = false;
        }

        private void OnCollisionEnter2D(Collision2D collision) 
        {
            if (collision.gameObject.CompareTag("Ground") && !isCollided) // For the ground collision
            {
                GameManager.AudioManager.Play("hit");
                GameOver = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Pipe") && !isCollided)
            {
                Physics2D.IgnoreCollision(GetComponent<CapsuleCollider2D>(), GetComponent<Collider2D>(), true); // Fall on the ground
                GameManager.AudioManager.Play("hit");
                GameManager.AudioManager.Play("die");
                isCollided = true;
                GameOver = true;
            }

            if (collision.gameObject.CompareTag("TopTrigger") && !isCollided) // The Trigger at the top
            {
                GameManager.AudioManager.Play("hit");
                GameManager.AudioManager.Play("die");
                isCollided = true;
                GameOver = true;
            }

            if (collision.gameObject.CompareTag("PipeParent")) // Trigger for the score increase
            {
                GameManager.AudioManager.Play("point");
                Score++;
            }
        }

        void Update()
        {
            GameOngoing = GameManager.GameOngoing;
            GamePaused = GameManager.GamePaused;

            Physics2D.IgnoreLayerCollision(8, 10, false);

            if (!GameOngoing && !GameOver)
            {   // The animation at the beginning
                transform.position = Vector3.Lerp(transform.position + new Vector3(0, 0.01f ,0), transform.position + new Vector3(0, -0.01f, 0), Mathf.PingPong(Time.time / smooth, 1));
            }

            if (GameOngoing)
            {   // Rotation animation when falling
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -90), Time.deltaTime * smooth);
            }
            
            if (Input.GetMouseButtonDown(0) && GameOngoing && !GamePaused) // jump when pressed
            {
                GameManager.AudioManager.Play("wing"); // Wing sfx
                rb.velocity = Vector2.up * velocity_Multiplier; //jump
                transform.rotation = Quaternion.Euler(0, 0, 30); 
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