using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    float fallSpeed;

    public float fallSpeedBase;

    public Sprite[] enemySprites;
    SpriteRenderer mainSpriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        fallSpeed = 3.0f * Random.value + fallSpeedBase;

        mainSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Sprite sprite = null;
        int index = Random.Range(0, enemySprites.Length);
        sprite = enemySprites[index];
        mainSpriteRenderer.sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, -fallSpeed * Time.deltaTime, 0, Space.World);
    }
}
