using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    // タイトルに戻る
    public void ReturnTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    // リトライする
    public void RetryGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

}
