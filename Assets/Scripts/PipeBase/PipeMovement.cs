using UnityEngine;

namespace Assets.Scripts.PipeBase
{
    public class PipeMovement : MonoBehaviour
    {
        private float PipeSpeed;

        void Update()
        {
            PipeSpeed = FindObjectOfType<PipeManager>().PipeSpeed; 
            transform.position += Vector3.left * Time.deltaTime * PipeSpeed;
        }
    }
}
