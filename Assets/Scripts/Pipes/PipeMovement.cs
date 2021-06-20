using Flappy.Core;
using UnityEngine;

namespace Flappy.Pipes
{
    public class PipeMovement : MonoBehaviour
    {
        private float PipeSpeed;

        void Update()
        {
            PipeSpeed = GameObject.FindGameObjectWithTag("PipeManager").GetComponent<PipeManager>().PipeSpeed; 
            transform.position += Vector3.left * Time.deltaTime * PipeSpeed;
        }
    }
}
