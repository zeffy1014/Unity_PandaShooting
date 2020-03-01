using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGController : MonoBehaviour
{
    public float scrollSpeed = 1.0f;
    float spriteHeight;

    // Start is called before the first frame update
    void Start()
    {
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, -scrollSpeed * Time.deltaTime, 0);

        // 画像サイズ分移動したら上に移動する
        if (-spriteHeight >= transform.position.y) LoopScroll();
    }

    void LoopScroll()
    {
        // 画像2枚分移動
        transform.Translate(0, (spriteHeight * 2), 0);
    }
}
