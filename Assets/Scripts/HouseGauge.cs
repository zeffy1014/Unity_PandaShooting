using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HouseGauge : MonoBehaviour
{
    public Text hpLabel;
    public Image hpSprite;
    public Image moveSprite;

    public int maxHp = 1000; // 最大耐久値
    public int autoHealAmount = 1; // 自然回復量
    public float autoHealCycle = 0.05f; // 自然回復にかかる時間

    private int nowHp;
    private int trueHp;
    private Vector4 damageColor;
    private Vector4 healColor;
    private float waitHeal;

    GameObject gameController;

    void Awake()
    {
        // 各種初期化
        damageColor = new Vector4(0.5f, 0, 0, 1);
        healColor = new Vector4(0.3f, 1, 0.5f, 1);
        trueHp = maxHp;
        nowHp = maxHp;
        waitHeal = 0.0f;

        hpSprite.fillAmount = (float)nowHp / (float)maxHp;
        moveSprite.fillAmount = (float)trueHp / (float)maxHp;

        hpLabel.text = nowHp + "/" + maxHp;
    }

    void Start()
    {
        // 開始時にイベントを飛ばす対象を登録しておく
        gameController = GameObject.FindWithTag("GameController");
    }

    public void OnDamage(int damage)
    {
        trueHp -= damage;

        // 死んでいたら…
        if (trueHp <= 0)
        {
            trueHp = 0;
            ExecuteEvents.Execute<IGameEventReceiver>(
                target: gameController,
                eventData: null,
                functor: (receiver, eventData) => receiver.OnGameOver()
            );
        }
    }

    public void OnHeal(int heal)
    {
        trueHp += heal;
        if (trueHp > maxHp) trueHp = maxHp;
    }

    public void GetHp(ref int nowHp, ref int maxHp)
    {
        nowHp = this.trueHp;
        maxHp = this.maxHp;
        return;
    }

    void Update()
    {
        // 死んでなければ自然回復
        waitHeal += Time.deltaTime;
        if (autoHealCycle <= waitHeal && 0 < trueHp)
        {
            OnHeal(autoHealAmount);
            waitHeal = 0;
        }

        // ゲージを動かす(trueHpが正しい耐久値なのでそれに合わせて見た目を変える)
        if (nowHp != trueHp)
        {
            if (nowHp > trueHp)
            {
                // damage
                nowHp -= Mathf.FloorToInt(maxHp * Time.deltaTime * 0.3f);
                if (nowHp < trueHp) nowHp = trueHp;

                moveSprite.color = damageColor;
                hpSprite.fillAmount = (float)trueHp / (float)maxHp;
                moveSprite.fillAmount = (float)nowHp / (float)maxHp;
            }
            else
            {
                // heal
                nowHp += Mathf.FloorToInt(maxHp * Time.deltaTime * 0.3f);
                if (nowHp > trueHp) nowHp = trueHp;

                moveSprite.color = healColor;
                hpSprite.fillAmount = (float)nowHp / (float)maxHp;
                moveSprite.fillAmount = (float)trueHp / (float)maxHp;

            }
            hpLabel.text = nowHp + "/" + maxHp;
        }
    }
}

