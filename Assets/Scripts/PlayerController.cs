using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movSpeed = 5.0f;
    public GameObject bullet;
    public GameObject damageEffect;
    public int defaultLife = 3;

    public int Life { get; set; }
    public bool Playing { get; set; } = false;

    public LifePanel lifePanel;

    public AudioClip shotSE;
    public AudioClip damageSE;
    AudioSource audioSource;

    // 移動範囲制限のための決め打ち画面範囲
    float screenWidth = 5.0f;
    float screenHeight = 10.0f;

    private void Start()
    {
        Life = defaultLife;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Playing)
        {
            if (Input.GetKey(KeyCode.LeftArrow) &&  -screenWidth/2 < transform.position.x)
            {
                transform.Translate(-movSpeed * Time.deltaTime, 0, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow) && screenWidth / 2 > transform.position.x)
            {
                transform.Translate(movSpeed * Time.deltaTime, 0, 0);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 genPos = transform.position;
                genPos.y += 1.0f;
                Vector3 genRot = transform.rotation.eulerAngles;
                genRot.z += 360.0f * Random.value;

                // 弾生成
                Instantiate<GameObject>(bullet, genPos, Quaternion.Euler(genRot));
                // 音も出す
                audioSource.PlayOneShot(shotSE);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate(damageEffect, transform.position, Quaternion.identity);
            // 音も鳴らす
            audioSource.PlayOneShot(damageSE);

            Destroy(other.gameObject);

            Life--;
            lifePanel.UpdateLife(Life);

            if (0 >= Life) gameObject.SetActive(false); // Destroy(gameObject);
        }
    }

    public void ForceGameOver()
    {
        // エフェクトつける
        Instantiate(damageEffect, transform.position, Quaternion.identity);

        // ライフゼロになって強制終了
        Life = 0;
        lifePanel.UpdateLife(Life);
        gameObject.SetActive(false);
    }
}
