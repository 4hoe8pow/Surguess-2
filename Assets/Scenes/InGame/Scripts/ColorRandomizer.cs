using UnityEngine;

public class ColorRandomizer : MonoBehaviour
{
    public GameObject targetObject; // 対象のオブジェクト
    public Color[] colors = new Color[4]; // 4つの色
    private readonly float[] colorRatios = new float[4]; // 各色の比率

    public int textureWidth = 256; // テクスチャの幅
    public int textureHeight = 256; // テクスチャの高さ
    public float noiseScale = 0.1f; // ノイズのスケール
    public float sinFrequency = 3.0f; // 三角関数の周波数
    public int stampCount = 5; // スタンプの数
    public int stampRadius = 30; // スタンプの半径

    void Start()
    {
        RandomizeColors();
    }

    public void RandomizeColors()
    {
        // ランダムに色を生成
        float hueStart = Random.Range(0f, 1f);

        for (int i = 0; i < colors.Length; i++)
        {
            float hue = (hueStart + (i * 0.25f)) % 1f;
            colors[i] = Color.HSVToRGB(hue, 1f, 1f); // 彩度と明度を最大に
            Debug.Log($"Color {i} assigned: {colors[i]}");
        }

        // 色の比率を決定 (16%〜30%) 
        float total = 0;
        for (int i = 0; i < colorRatios.Length; i++)
        {
            if (i < 3)
            {
                colorRatios[i] = Random.Range(0.16f, 0.30f);
                total += colorRatios[i];
                Debug.Log($"Color {i} ratio assigned: {colorRatios[i] * 100}%");
            }
            else
            {
                colorRatios[i] = 1f - total; // 残りの割合
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

        // 新しいテクスチャを作成
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // テクスチャにランダムに色を塗る処理
        Color[] pixelColors = new Color[textureWidth * textureHeight];

        // ランダムなノイズと三角関数でベースの塗りを作成
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                float sinValue = Mathf.Sin((x + y) * sinFrequency);
                float mixedValue = (noiseValue + sinValue) * 0.5f;

                Color selectedColor = GetColorBasedOnMix(mixedValue);
                pixelColors[y * textureWidth + x] = selectedColor;
            }
        }

        // スタンプをランダムな位置に適用
        for (int i = 0; i < stampCount; i++)
        {
            int stampX = Random.Range(stampRadius, textureWidth - stampRadius);
            int stampY = Random.Range(stampRadius, textureHeight - stampRadius);
            Color stampColor = colors[Random.Range(0, colors.Length)];
            ApplyStamp(pixelColors, stampX, stampY, stampRadius, stampColor);
        }

        // 色の比率を調整
        AdjustColorRatios(pixelColors);

        texture.SetPixels(pixelColors);
        texture.Apply();
        meshRenderer.material.mainTexture = texture;
        Debug.Log("テクスチャがメッシュに適用されました。");
    }

    // スタンプの適用
    void ApplyStamp(Color[] pixelColors, int centerX, int centerY, int radius, Color stampColor)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius) // 円形スタンプ
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;

                    if (pixelX >= 0 && pixelX < textureWidth && pixelY >= 0 && pixelY < textureHeight)
                    {
                        pixelColors[pixelY * textureWidth + pixelX] = stampColor;
                    }
                }
            }
        }
    }

    // 混合値に基づいて色を選択
    Color GetColorBasedOnMix(float mixValue)
    {
        float cumulativeRatio = 0f;
        for (int i = 0; i < colorRatios.Length; i++)
        {
            cumulativeRatio += colorRatios[i];
            if (mixValue <= cumulativeRatio)
            {
                return colors[i];
            }
        }
        return colors[colors.Length - 1]; // 最後の色を返す (安全対策)
    }

    // 色の比率を計算し調整
    void AdjustColorRatios(Color[] pixelColors)
    {
        int[] colorCounts = new int[colors.Length];
        int totalPixels = pixelColors.Length;

        // 現在の色のピクセル数をカウント
        for (int i = 0; i < pixelColors.Length; i++)
        {
            for (int j = 0; j < colors.Length; j++)
            {
                if (pixelColors[i] == colors[j])
                {
                    colorCounts[j]++;
                    break;
                }
            }
        }

        // 目標の色比率に基づいて調整
        for (int i = 0; i < colors.Length; i++)
        {
            int targetCount = Mathf.FloorToInt(colorRatios[i] * totalPixels);

            if (colorCounts[i] < targetCount)
            {
                // 不足分のピクセルを増やす
                int pixelsToAdd = targetCount - colorCounts[i];
                AddColorPixels(pixelColors, colors[i], pixelsToAdd);
            }
            else if (colorCounts[i] > targetCount)
            {
                // 過剰なピクセルを減らす
                int pixelsToRemove = colorCounts[i] - targetCount;
                RemoveColorPixels(pixelColors, colors[i], pixelsToRemove);
            }
        }
    }

    // 指定された色のピクセルを増やす
    void AddColorPixels(Color[] pixelColors, Color color, int count)
    {
        int added = 0;
        for (int i = 0; i < pixelColors.Length && added < count; i++)
        {
            if (pixelColors[i] != color)
            {
                pixelColors[i] = color;
                added++;
            }
        }
    }

    // 指定された色のピクセルを減らす
    void RemoveColorPixels(Color[] pixelColors, Color color, int count)
    {
        int removed = 0;
        for (int i = 0; i < pixelColors.Length && removed < count; i++)
        {
            if (pixelColors[i] == color)
            {
                pixelColors[i] = GetAlternateColor(color);
                removed++;
            }
        }
    }

    // 別の色をランダムに取得
    Color GetAlternateColor(Color excludeColor)
    {
        Color newColor;
        do
        {
            newColor = colors[Random.Range(0, colors.Length)];
        } while (newColor == excludeColor);

        return newColor;
    }

    // カラーを取得するメソッド
    public Color[] GetColors()
    {
        return colors;
    }

    // 最も面積の多い色を取得するメソッド
    public Color GetMostPrevalentColor()
    {
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
