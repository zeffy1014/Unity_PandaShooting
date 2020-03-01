using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;

    // Enemyランダム生成のための決め打ち画面範囲
    float screenWidth = 5.0f;
    float screenHeight = 10.0f;

    // Start is called before the first frame update
    public void GameStart()
    {
        Invoke("GenEnemy", 1.0f);
        // InvokeRepeating("GenEnemy", 1, Random.value);
    }

    void GenEnemy()
    {
        // とりあえず決め打ちの範囲でランダム生成
        Instantiate(enemyPrefab, new Vector3(-screenWidth/2 + screenWidth * Random.value, screenHeight/2 + 2.0f, 0), Quaternion.identity);

        // 次のEnemy生成
        Invoke("GenEnemy", 0.5f + 1.0f * Random.value);
    }
}
