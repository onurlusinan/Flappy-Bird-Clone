using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomizeSprite : MonoBehaviour
{
    public List<Sprite> spriteList;
    public Image image;

    private void Start()
    {
        image.sprite = spriteList[Random.Range(0, spriteList.Count)];
    }
}
