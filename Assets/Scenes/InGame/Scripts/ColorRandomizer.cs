using UnityEngine;

public class ColorRandomizer : MonoBehaviour
{
    public GameObject targetObject; // 対象のオブジェクト
    public Color[] colors = new Color[4]; // 4つの色
    private readonly float[] colorRatios = new float[4]; // 各色の比率

    public int textureWidth = 256; // テクスチャの幅
    public int textureHeight = 256; // テクスチャの高さ

    void Start()
    {
        RandomizeColors();
    }

    public void RandomizeColors()
    {
        // 色相のランダムな開始値を決定
        float hueStart = Random.Range(0f, 1f); // 0〜1の間でランダムな色相を選出

        // 4つの色を均等に配置
        for (int i = 0; i < colors.Length; i++)
        {
            float hue = (hueStart + (i * 0.25f)) % 1f; // 25%ずつずらす
            colors[i] = Color.HSVToRGB(hue, 1f, 1f); // 彩度と明度を最大に設定
            Debug.Log($"Color {i} assigned: {colors[i]}");
        }

        // 色の比率をランダムに決定 (16%-30%)
        float total = 0;
        for (int i = 0; i < colorRatios.Length; i++)
        {
            if (i < 3)
            {
                colorRatios[i] = Random.Range(0.16f, 0.30f); // ランダムな割合を割り当て
                total += colorRatios[i];
                Debug.Log($"Color {i} ratio assigned: {colorRatios[i] * 100}%");
            }
            else
            {
                colorRatios[i] = 1f - total; // 最後の色は残りの割合
                Debug.Log($"Color {i} ratio assigned: {colorRatios[i] * 100}% (remaining percentage)");
            }
        }

        ApplyTextureToMesh();
    }

    void ApplyTextureToMesh()
    {
        if (targetObject == null)
        {
            Debug.LogError("ターゲットオブジェクトが指定されていません！");
            return;
        }

        if (!targetObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            Debug.LogError("ターゲットオブジェクトにMeshRendererがありません！");
            return;
        }

        if (!targetObject.TryGetComponent<MeshFilter>(out var meshFilter))
        {
            Debug.LogError("ターゲットオブジェクトにMeshFilterがありません！");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector2[] uvs = mesh.uv; // UVマッピングを取得
        Vector3[] normals = mesh.normals; // 頂点の法線ベクトルを取得

        // 新しいテクスチャを作成
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        int colorStartX = 0;

        // 各色の領域をテクスチャに描画
        for (int i = 0; i < colors.Length; i++)
        {
            int colorWidth = Mathf.FloorToInt(colorRatios[i] * textureWidth); // 各色の領域幅を計算
            for (int x = colorStartX; x < colorStartX + colorWidth; x++)
            {
                for (int y = 0; y < textureHeight; y++)
                {
                    // 面が表かどうか判定
                    if (IsFrontFace(normals, x, y, textureWidth, textureHeight))
                    {
                        texture.SetPixel(x, y, colors[i]); // 表面のみに色を設定
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear); // 裏面は透明にする
                    }
                }
            }
            colorStartX += colorWidth;
            Debug.Log($"Applied color {i} to texture from {colorStartX - colorWidth} to {colorStartX}");
        }

        // テクスチャを適用
        texture.Apply();
        meshRenderer.material.mainTexture = texture;
        Debug.Log("テクスチャがメッシュに適用されました。");
    }

    // 法線ベクトルを使って表面かどうかを判定
    bool IsFrontFace(Vector3[] normals, int x, int y, int textureWidth, int textureHeight)
    {
        int vertexIndex = (y * textureWidth) + x; // 現在の頂点のインデックスを計算
        if (vertexIndex >= normals.Length)
            return true; // 安全対策：頂点が存在しない場合は表面とみなす

        Vector3 normal = normals[vertexIndex]; // 法線ベクトルを取得

        // 法線ベクトルが正面を向いているかどうか
        return Vector3.Dot(normal, Vector3.forward) > 0;
    }

    // カラーを取得するメソッド
    public Color[] GetColors()
    {
        return colors;
    }

    // 最も面積の多い色を取得するメソッド（仮実装）
    public Color GetMostPrevalentColor()
    {
        // colorRatiosが最大の色のインデックスを見つける
        int maxIndex = 0;
        float maxRatio = colorRatios[0];

        for (int i = 1; i < colorRatios.Length; i++)
        {
            if (colorRatios[i] > maxRatio)
            {
                maxRatio = colorRatios[i];
                maxIndex = i;
            }
        }

        return colors[maxIndex]; // 最大の比率を持つ色を返す
    }

}
