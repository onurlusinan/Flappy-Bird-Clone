using UnityEngine;

using Flappy.Gameplay;
using Flappy.Helpers;
using DG.Tweening;

namespace Flappy.Pipes
{
    public class PipeManager : MonoBehaviour
    {
        public GameObject pipe;
        public ObjectPool pipePool;

        public float heightConstraint = 2.5f; // Constraint for the height change of the pipes
        public float pipeSpeed = 2.5f;

        private float timer = 0;
        public float pipeWaitTime = 1.3f; // Can be changed, default (imo best) is 1.3 for the Pipe Speed of 2.5

        private bool gameOver;

        private void Start()
        {
            pipePool.InitPool(pipe, 4);
        }

        private void MovePipes()
        { 
                                    
        }

        void Update()
        {
           
        }
    }
}

