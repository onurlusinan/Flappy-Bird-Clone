using Flappy.Gameplay;
using UnityEngine;

namespace Flappy.Pipes
{
    public class PipeMovement : MonoBehaviour
    {
        private float PipeSpeed;

        void Update()
        {
            PipeSpeed = GameObject.FindGameObjectWithTag("PipeManager").GetComponent<PipeManager>().pipeSpeed; 
            transform.position += Vector3.left * Time.deltaTime * PipeSpeed;
        }
    }
}
