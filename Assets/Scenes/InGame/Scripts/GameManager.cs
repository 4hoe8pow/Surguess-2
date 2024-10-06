using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Text timerText; // タイマー表示用テキスト
    public Button button1; // ボタン1
    public Button button2; // ボタン2
    public Button button3; // ボタン3
    public Button button4; // ボタン4
    public GameObject scoreWindow; // スコアウィンドウのUIオブジェクト
    public Text scoreText; // 現在のスコア表示用テキスト
    public Text bestScoreText; // ベストスコア表示用テキスト
    public Button restartButton; // RESTARTボタン

    private ColorRandomizer colorRandomizer; // ColorRandomizerのインスタンス
    private float timer = 60f; // 60秒のカウントダウン
    private int currentScore = 0; // 現在のスコア

    void Start()
    {
        colorRandomizer = GetComponent<ColorRandomizer>();
        if (colorRandomizer == null)
        {
            Debug.LogError("ColorRandomizerコンポーネントが見つかりません。");
            return;
        }

        // 初期カラーランダマイズ
        colorRandomizer.RandomizeColors();
        UpdateButtonColors();

        // タイマー開始
        StartCoroutine(TimerCountdown());

        // RESTARTボタンにリスナーを追加
        restartButton.onClick.AddListener(RestartGame);
    }

    void UpdateButtonColors()
    {
        Color[] colors = colorRandomizer.GetColors();
        button1.image.color = colors[0];
        button2.image.color = colors[1];
        button3.image.color = colors[2];
        button4.image.color = colors[3];
    }

    IEnumerator TimerCountdown()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(timer).ToString();
            yield return null; // 次のフレームまで待機
        }

        GameEnd();
    }

    public void OnButtonPressed(int buttonIndex)
    {
        Color mostPrevalentColor = colorRandomizer.GetMostPrevalentColor();

        if (colorRandomizer.GetColors()[buttonIndex] == mostPrevalentColor)
        {
            // 正解
            currentScore += 1; // スコアを加算
            timer += 2f; // 2秒追加
            Debug.Log("正解！2秒追加。");
            colorRandomizer.RandomizeColors(); // 再度カラーランダマイズ
            UpdateButtonColors(); // ボタンの色を更新
        }
        else
        {
            // 不正解
            Debug.Log("不正解。ゲーム終了。");
            GameEnd();
        }
    }

    void GameEnd()
    {
        StopAllCoroutines();
        DisableButtons(); // ボタンを非活性化
        ShowScoreWindow(); // スコアウィンドウを表示

        UpdateBestScore(); // ベストスコアの更新

        UpdateScoreText(); // スコアを表示
        UpdateBestScoreText(); // ベストスコアを表示
        Debug.Log("ゲーム終了");
    }

    void UpdateBestScore()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0); // ベストスコアを取得

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore); // プレファレンスに保存
            PlayerPrefs.Save();
        }
    }

    void DisableButtons()
    {
        button1.interactable = false;
        button2.interactable = false;
        button3.interactable = false;
        button4.interactable = false;

        // 正解のボタン以外の色を透明に
        Color mostPrevalentColor = colorRandomizer.GetMostPrevalentColor();
        foreach (Button button in new Button[] { button1, button2, button3, button4 })
        {
            if (button.image.color != mostPrevalentColor)
            {
                Color transparentColor = button.image.color;
                transparentColor.a = 0f; // アルファを0にする
                button.image.color = transparentColor;
            }
        }
    }

    void ShowScoreWindow()
    {
        scoreWindow.SetActive(true); // スコアウィンドウを表示
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + currentScore.ToString(); // 現在のスコアを表示
    }

    void UpdateBestScoreText()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0); // ベストスコアを取得
        Debug.Log($"BEST =>{bestScore}");
        bestScoreText.text = "Best Score: " + bestScore.ToString(); // ベストスコアを表示
    }

    // RESTARTボタンが押されたときの処理
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 現在のシーンをリロード
    }
}
