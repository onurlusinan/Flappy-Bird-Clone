using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flappy.Helpers
{
    public class ObjectPool : MonoBehaviour
    {
        private List<GameObject> pool;
        private GameObject prefab;
        private int initialAmount;

        /// <summary>
        /// Initializes the pool, fills up the pool if we are missing some
        /// </summary>
        internal void InitPool(GameObject prefab, int initialAmount)
        {
            this.prefab = prefab;
            this.initialAmount = initialAmount;

            // Check the gameobjects that we already have in our 
            Transform[] myObjects = this.transform.GetComponentsInChildren<Transform>(true);
            pool = new List<GameObject>();

            for (int i = 0; i < myObjects.Length; i++)
            {
                if (myObjects[i] != this.transform)
                    pool.Add(myObjects[i].gameObject);
            }

            if (pool.Count < initialAmount)
            {
                // We need to create a few more
                int toCreate = initialAmount - pool.Count;

                for (int i = 0; i < toCreate; i++)
                {
                    CreateObject();
                }
            }
        }

        /// <summary>
        /// Creates an object to add to the pool
        /// </summary>
        /// <returns></returns>
        public GameObject CreateObject()
        {
            GameObject newObject = Instantiate(prefab, this.transform);
            newObject.SetActive(false);
            pool.Add(newObject);
            return newObject;
        }

        /// <summary>
        /// Returns an object we can use from the pool, if no object is available it will create one
        /// </summary>
        /// <returns></returns>
        public GameObject GetObject()
        {
            GameObject toReturn = null;

            if (pool.Count > 0)
                toReturn = pool[0];
            else
                toReturn = CreateObject();

            pool.Remove(toReturn);
            return toReturn;
        }

        /// <summary>
        /// Gets multiple objects from the pool
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public GameObject[] GetObjects(int amount)
        {
            GameObject[] toReturn = new GameObject[amount];

            for (int i = 0; i < amount; i++)
            {
                toReturn[i] = GetObject();
            }

            return toReturn;
        }

        /// <summary>
        /// When we are done with an object, we need to return it to the pool
        /// </summary>
        /// <param name="go"></param>
        public void ReturnToPool(GameObject go)
        {
            go.transform.SetParent(this.transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }
}
