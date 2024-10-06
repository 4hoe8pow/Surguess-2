using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // タップまたはクリックを検知
        if (Input.GetMouseButtonDown(0)) // 左クリックまたはタップを検知
        {
            LoadInGameScene(); // InGameシーンをロード
        }
    }

    void LoadInGameScene()
    {
        SceneManager.LoadScene("InGame"); // InGameシーンの名前を指定してロード
    }
}
