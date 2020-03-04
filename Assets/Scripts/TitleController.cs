using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public Text startLabel;
    public Text scoreLabel;

    // Start is called before the first frame update
    void Start()
    {
        int score = 0;
        if (PlayerPrefs.HasKey("HighScore"))
        {
            score = PlayerPrefs.GetInt("HighScore");
        }

        scoreLabel.text = "High Score: " + score;
        startLabel.text = "Push any key ->";

    }

    // Update is called once per frame
    void Update()
    {
        // 文字を点滅
        Color color = startLabel.color;
        color.a = Mathf.Sin(Time.time * 5.0f);
        startLabel.color = color;

        // なにか押されていたら開始
        if (Input.anyKey) UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }


}
