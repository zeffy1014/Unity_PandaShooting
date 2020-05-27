using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_G_eggs : EnemyController
{
    // 孵化するGのGameObjectとPrefab参照先
    GameObject g = null;
    string gPrefabPath = "Prefabs/Enemy/EnemyPrefab G";

    float judgeStartTime = 3.0f;     // 出現してからこの秒数経過したら
    float judgeInterval = 0.5f;      // この間隔で孵化判定して
    float hatchProbability = 0.1f;  // この確率で孵化が決定して
    float waitHatchTime = 3.0f;      // この秒数経過で孵化する

    bool judgeStart = false;       // 判定開始フラグ
    bool startHatching = false;    // 孵化開始フラグ

    float elaspedTime = 0.0f; // もろもろ経過時間

    new void Start()
    {
        base.Start();
    }

    void Update()
    {
        elaspedTime += Time.deltaTime;

        //背景に合わせてスクロール
        GoStraight(this.moveSpeed);

        // 孵化判定
        if (false == startHatching)
        {
            startHatching = JudgeHatching();
        }
        else
        {
            // 孵化中は点滅する
            Color color = this.GetComponent<SpriteRenderer>().color;
            color.a = Mathf.Sin(Time.time * 20.0f);
            this.GetComponent<SpriteRenderer>().color = color;

            if (waitHatchTime < elaspedTime)
            {
                // ふか！とりあえず3匹で
                int hatchNum = 3;
                Hatch(hatchNum);

                // そして己は消える
                Destroy(this.gameObject);
            }
        }

            return;
    }

    /*****G卵専用内部関数*****/
    // 孵化開始から決定まで判定 最終的に孵化が決定したらtrueが返る
    bool JudgeHatching()
    {
        bool ret = false;

        if (false == judgeStart)
        {
            if (judgeStartTime < elaspedTime)
            {
                // 判定開始
                elaspedTime = 0.0f;
                judgeStart = true;
            }
        }
        else
        {
            if (judgeInterval < elaspedTime)
            {
                // 判定実施
                elaspedTime = 0.0f;
                if (hatchProbability > Random.value) ret = true;  // 乱数が一定の値を下回ったら孵化決定
            }
        }

        return ret;
    }

    // Gが孵化
    void Hatch(int hatchNum)
    {
        // GのPrefab取得して指定数出現させてから小さくする
        for (int num = 0; num < hatchNum; num++)
        {
            Vector3 initialAngle = this.transform.eulerAngles;
            initialAngle.z = Random.value * 360.0f;

            GameObject enemy = Instantiate(
                GetGPrefab(),
                this.transform.position,
                Quaternion.Euler(initialAngle)
            );
            enemy.transform.localScale *= 0.5f;
            enemy.GetComponent<EnemyController>().SetGameArea(this.gameArea);
        }
    }

    // Gプレファブを取得(未取得ならロード)
    GameObject GetGPrefab()
    {
        if (null == g)
        {
            g = (GameObject)Resources.Load(gPrefabPath);
        }
        GameObject cloneG = new GameObject();
        cloneG = g;
        return cloneG;
    }

}
