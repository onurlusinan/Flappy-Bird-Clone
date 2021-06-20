using UnityEngine;

using Flappy.Gameplay;

namespace Flappy.Pipes
{
    public class PipeManager : MonoBehaviour
    {
        public GameObject Pipe;

        public float HeightConstraint = 2.5f; // Constraint for the height change of the pipes
        public float PipeSpeed = 2.5f;

        private float timer = 0;
        public float PipeWaitTime = 1.3f; // Can be changed, default (imo best) is 1.3 for the Pipe Speed of 2.5

        private bool GameOver;

        void Update()
        {
            GameOver = FindObjectOfType<BirdController>().GameOver;

            if (timer > PipeWaitTime && !GameOver)
            {
                GameObject PipeNew = Instantiate(Pipe);
                PipeNew.transform.position = GameObject.Find("PipeStart").transform.position + new Vector3(0, Random.Range(HeightConstraint, -HeightConstraint), 0);
                PipeNew.transform.parent = transform;
                timer = 0;
            }

            timer += Time.deltaTime;
        }
    }
}

