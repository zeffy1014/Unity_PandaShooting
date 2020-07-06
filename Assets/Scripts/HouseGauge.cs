using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

// TODO:いずれはHouseGaugeではなく家そのものを画面下部に置いて当たり判定・ライフ管理する

public class HouseGauge : MonoBehaviour
{
    public Text hpLabel;
    public Image hpSprite;
    public Image moveSprite;

    public int maxHp = 1000; // 最大耐久値
    public int autoHealAmount = 1; // 自然回復量
    public float autoHealCycle = 0.05f; // 自然回復にかかる時間

    private int nowHp;
    // private int trueHp;   // ReactivePropertyで置き換え
    private Vector4 damageColor;
    private Vector4 healColor;
    private float waitHeal;

    // 現在のライフをReactivePropertyで監視できるようにする
    private ReactiveProperty<int> _lifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> lifeReactiveProperty { get { return _lifeReactiveProperty; } }


    void Awake()
    {
        // 各種初期化
        damageColor = new Vector4(0.5f, 0, 0, 1);
        healColor = new Vector4(0.3f, 1, 0.5f, 1);
        _lifeReactiveProperty.Value = maxHp;
        nowHp = maxHp;
        waitHeal = 0.0f;

        hpSprite.fillAmount = (float)nowHp / (float)maxHp;
        moveSprite.fillAmount = (float)_lifeReactiveProperty.Value / (float)maxHp;

        hpLabel.text = nowHp + "/" + maxHp;
    }

    void Start()
    {
    }

    public void OnDamage(int damage)
    {
        // Play中のみの動作
        if (GameState.Play == GameStateProperty.GetState())
        {
            _lifeReactiveProperty.Value = (damage > _lifeReactiveProperty.Value)
                                        ? (0)
                                        : (_lifeReactiveProperty.Value -= damage);

            // 死んでいたら… -> ライフをGameControllerが監視して処理する
            /*
            if (trueHp <= 0)
            {
                trueHp = 0;
                EventHandlerExtention.SendEvent(new GameOverEventData());
            }
            */
        }
    }

    public void OnHeal(int heal)
    {
        _lifeReactiveProperty.Value += heal;
        if (_lifeReactiveProperty.Value > maxHp) _lifeReactiveProperty.Value = maxHp;
    }

    void Update()
    {
        // Play中のみの動作
        if (GameState.Play == GameStateProperty.GetState())
        {
            // 死んでなければ自然回復
            waitHeal += Time.deltaTime;
            if (autoHealCycle <= waitHeal && 0 < _lifeReactiveProperty.Value)
            {
                OnHeal(autoHealAmount);
                waitHeal = 0;
            }
        }

        // ゲージを動かす(_lifeReactiveProperty.Valueが正しい耐久値なのでそれに合わせて見た目を変える)
        if (nowHp != _lifeReactiveProperty.Value)
        {
            if (nowHp > _lifeReactiveProperty.Value)
            {
                // damage
                nowHp -= Mathf.FloorToInt(maxHp * Time.deltaTime * 1.0f);
                if (nowHp < _lifeReactiveProperty.Value) nowHp = _lifeReactiveProperty.Value;

                moveSprite.color = damageColor;
                hpSprite.fillAmount = (float)_lifeReactiveProperty.Value / (float)maxHp;
                moveSprite.fillAmount = (float)nowHp / (float)maxHp;
            }
            else
            {
                // heal
                nowHp += Mathf.FloorToInt(maxHp * Time.deltaTime * 0.3f);
                if (nowHp > _lifeReactiveProperty.Value) nowHp = _lifeReactiveProperty.Value;

                moveSprite.color = healColor;
                hpSprite.fillAmount = (float)nowHp / (float)maxHp;
                moveSprite.fillAmount = (float)_lifeReactiveProperty.Value / (float)maxHp;

            }
            hpLabel.text = nowHp + "/" + maxHp;
        }
    }
}

