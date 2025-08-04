using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    private struct SymbolPosition
    {
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
    }

    private readonly SymbolPosition[][] reelPositions = new SymbolPosition[][]
    {
        // Coluna 1
        new SymbolPosition[]
        {
            new SymbolPosition { AnchoredPosition = new Vector2(-13.0166f, 106.9344f), SizeDelta = new Vector2(156.3668f, 124.9311f) },
            new SymbolPosition { AnchoredPosition = new Vector2(-13.0166f, -27.6112f), SizeDelta = new Vector2(156.3668f, 127.6405f) },
            new SymbolPosition { AnchoredPosition = new Vector2(-13.0166f, -162.7641f), SizeDelta = new Vector2(156.3668f, 126.0592f) }
        },
        // Coluna 2
        new SymbolPosition[]
        {
            new SymbolPosition { AnchoredPosition = new Vector2(19.816f, 106.9344f), SizeDelta = new Vector2(154.2f, 124.9311f) },
            new SymbolPosition { AnchoredPosition = new Vector2(19.81531f, -27.5766f), SizeDelta = new Vector2(154.2f, 127.1439f) },
            new SymbolPosition { AnchoredPosition = new Vector2(19.816f, -162.7f), SizeDelta = new Vector2(154.2f, 127.1439f) }
        },
        // Coluna 3
        new SymbolPosition[]
        {
            new SymbolPosition { AnchoredPosition = new Vector2(52.60167f, 106.9f), SizeDelta = new Vector2(155.0256f, 125.1469f) },
            new SymbolPosition { AnchoredPosition = new Vector2(52.60167f, -27.5766f), SizeDelta = new Vector2(155.0256f, 127.1439f) },
            new SymbolPosition { AnchoredPosition = new Vector2(52.60167f, -162.7f), SizeDelta = new Vector2(155.0256f, 127.1439f) }
        },
        // Coluna 4
        new SymbolPosition[]
        {
            new SymbolPosition { AnchoredPosition = new Vector2(89.9f, 106.9f), SizeDelta = new Vector2(155.0256f, 125.1469f) },
            new SymbolPosition { AnchoredPosition = new Vector2(89.9f, -27.5766f), SizeDelta = new Vector2(155.0256f, 127.1439f) },
            new SymbolPosition { AnchoredPosition = new Vector2(89.9f, -162.7f), SizeDelta = new Vector2(155.0256f, 127.1439f) }
        },
        // Coluna 5
        new SymbolPosition[]
        {
            new SymbolPosition { AnchoredPosition = new Vector2(123.5f, 106.9344f), SizeDelta = new Vector2(156.3668f, 124.9311f) },
            new SymbolPosition { AnchoredPosition = new Vector2(123.5f, -27.6112f), SizeDelta = new Vector2(156.3668f, 127.6405f) },
            new SymbolPosition { AnchoredPosition = new Vector2(123.5f, -162.7641f), SizeDelta = new Vector2(156.3668f, 126.0592f) }
        }
    };

    private Dictionary<string, Sprite> symbolSprites;
    private GameObject symbolPrefab;
    private float reelOffsetX = 0f;
    private List<GameObject> symbolObjects = new List<GameObject>();
    private bool isSpinning = false;
    private float spinSpeed = 5000f;
    private int reelIndex = 0;

    public void SetReelOffset(float offsetX)
    {
        reelOffsetX = offsetX;
    }

    public void SetReelIndex(int index)
    {
        reelIndex = Mathf.Clamp(index, 0, reelPositions.Length - 1);
    }

    public void FillReel(List<string> symbols, Dictionary<string, Sprite> sprites, GameObject prefab)
    {
        ClearSymbols();

        symbolSprites = sprites;
        symbolPrefab = prefab;

        for (int i = 0; i < symbols.Count + 12; i++)
        {
            CreateSymbol(symbols[i % symbols.Count], i);
        }

        AlignSymbolsInitially();
    }

    private void CreateSymbol(string symbolName, int index)
    {
        GameObject symbolObj = Instantiate(symbolPrefab, transform);
        Image img = symbolObj.GetComponent<Image>();
        RectTransform rect = symbolObj.GetComponent<RectTransform>();

        if (symbolSprites.ContainsKey(symbolName))
            img.sprite = symbolSprites[symbolName];
        else
            Debug.LogWarning($"Sprite not found for symbol: {symbolName}");

        int posIndex = (2 - (index % 3) + 3) % 3;
        float yOffset = -index / 3 * (reelPositions[reelIndex][0].AnchoredPosition.y - reelPositions[reelIndex][2].AnchoredPosition.y);

        Vector2 targetPos = reelPositions[reelIndex][posIndex].AnchoredPosition;
        targetPos.x += reelOffsetX;
        targetPos.y += yOffset;
        rect.anchoredPosition = targetPos;
        rect.sizeDelta = reelPositions[reelIndex][posIndex].SizeDelta;

        symbolObjects.Add(symbolObj);
    }

    private void AlignSymbolsInitially()
    {
        float targetY = reelPositions[reelIndex][1].AnchoredPosition.y;
        float closestY = GetClosestSymbolY(targetY);
        float offset = targetY - closestY;

        foreach (var symbol in symbolObjects)
        {
            RectTransform rect = symbol.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;
            pos.y += offset;
            rect.anchoredPosition = pos;
        }
    }

    private void ClearSymbols()
    {
        foreach (var symbol in symbolObjects)
        {
            DestroyImmediate(symbol);
        }
        symbolObjects.Clear();
    }

    public void StartSpin(float duration, List<string> finalSymbols)
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinReel(duration, finalSymbols));
        }
    }

    private IEnumerator SpinReel(float duration, List<string> finalSymbols)
    {
        isSpinning = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            foreach (var symbol in symbolObjects)
            {
                RectTransform rect = symbol.GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;
                pos.y -= spinSpeed * Time.deltaTime;
                rect.anchoredPosition = pos;

                float bottomThreshold = reelPositions[reelIndex][2].AnchoredPosition.y - 400f;
                if (pos.y < bottomThreshold)
                {
                    float highestY = GetHighestSymbolY();
                    rect.anchoredPosition = new Vector2(pos.x, highestY + (reelPositions[reelIndex][0].AnchoredPosition.y - reelPositions[reelIndex][1].AnchoredPosition.y));
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        UpdateSymbolsForFinalResult(finalSymbols);
        yield return StartCoroutine(StopReelSmoothly(finalSymbols));
        isSpinning = false;
    }

    private void UpdateSymbolsForFinalResult(List<string> finalSymbols)
    {
        // Assign finalSymbols to symbols that will end up at Linha 1, Linha 2, Linha 3
        float[] targetYs = new float[]
        {
            reelPositions[reelIndex][0].AnchoredPosition.y, // Linha 1 (top)
            reelPositions[reelIndex][1].AnchoredPosition.y, // Linha 2 (middle)
            reelPositions[reelIndex][2].AnchoredPosition.y  // Linha 3 (bottom)
        };

        List<GameObject> assignedSymbols = new List<GameObject>();

        // Assign symbols in order: finalSymbols[0] to Linha 1, [1] to Linha 2, [2] to Linha 3
        for (int i = 0; i < Mathf.Min(3, finalSymbols.Count); i++)
        {
            GameObject closestSymbol = null;
            float minDistance = float.MaxValue;

            // Find the closest unassigned symbol to targetYs[i]
            foreach (var symbol in symbolObjects)
            {
                if (assignedSymbols.Contains(symbol))
                    continue;

                RectTransform rect = symbol.GetComponent<RectTransform>();
                float y = rect.anchoredPosition.y;
                float distance = Mathf.Abs(y - targetYs[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSymbol = symbol;
                }
            }

            if (closestSymbol != null && symbolSprites.ContainsKey(finalSymbols[i]))
            {
                Image img = closestSymbol.GetComponent<Image>();
                img.sprite = symbolSprites[finalSymbols[i]];
                assignedSymbols.Add(closestSymbol);
            }
        }
    }

    private IEnumerator StopReelSmoothly(List<string> finalSymbols)
    {
        float decelerationTime = 0.6f;
        float bounceTime = 0.3f;
        float elapsedTime = 0f;
        float initialSpeed = spinSpeed;

        float targetY = reelPositions[reelIndex][1].AnchoredPosition.y; // Center on Linha 2
        float overshootDistance = (reelPositions[reelIndex][1].AnchoredPosition.y - reelPositions[reelIndex][2].AnchoredPosition.y) * 0.2f;

        // Deceleration phase
        while (elapsedTime < decelerationTime)
        {
            float t = elapsedTime / decelerationTime;
            float currentSpeed = Mathf.Lerp(initialSpeed, 0f, t * t);

            foreach (var symbol in symbolObjects)
            {
                RectTransform rect = symbol.GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;
                pos.y -= currentSpeed * Time.deltaTime;
                rect.anchoredPosition = pos;

                float bottomThreshold = reelPositions[reelIndex][2].AnchoredPosition.y - 400f;
                if (pos.y < bottomThreshold)
                {
                    float highestY = GetHighestSymbolY();
                    rect.anchoredPosition = new Vector2(pos.x, highestY + (reelPositions[reelIndex][0].AnchoredPosition.y - reelPositions[reelIndex][1].AnchoredPosition.y));
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Bounce phase
        elapsedTime = 0f;
        float startY = GetClosestSymbolY(targetY);
        float bounceStartY = startY - overshootDistance;

        while (elapsedTime < bounceTime)
        {
            float t = elapsedTime / bounceTime;
            float bounceT = 1f - Mathf.Pow(1f - t, 3);
            float yOffset = Mathf.Lerp(bounceStartY, targetY, bounceT);

            float currentY = GetClosestSymbolY(yOffset);
            float deltaY = yOffset - currentY;

            foreach (var symbol in symbolObjects)
            {
                RectTransform rect = symbol.GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;
                pos.y += deltaY;
                rect.anchoredPosition = pos;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to exact positions
        float[] targetPositions = new float[]
        {
            reelPositions[reelIndex][0].AnchoredPosition.y, // Linha 1 (top)
            reelPositions[reelIndex][1].AnchoredPosition.y, // Linha 2 (middle)
            reelPositions[reelIndex][2].AnchoredPosition.y  // Linha 3 (bottom)
        };

        // Assign the closest unassigned symbols to each target position
        List<GameObject> assignedSymbols = new List<GameObject>();

        for (int i = 0; i < Mathf.Min(3, symbolObjects.Count); i++)
        {
            GameObject closestSymbol = null;
            float minDistance = float.MaxValue;

            foreach (var symbol in symbolObjects)
            {
                if (assignedSymbols.Contains(symbol))
                    continue;

                RectTransform rect = symbol.GetComponent<RectTransform>();
                float y = rect.anchoredPosition.y;
                float distance = Mathf.Abs(y - targetPositions[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSymbol = symbol;
                }
            }

            if (closestSymbol != null)
            {
                RectTransform rect = closestSymbol.GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;
                pos.y = targetPositions[i];
                rect.anchoredPosition = pos;
                assignedSymbols.Add(closestSymbol);
            }
        }
    }

    private float GetHighestSymbolY()
    {
        float highestY = float.MinValue;
        foreach (var symbol in symbolObjects)
        {
            float y = symbol.GetComponent<RectTransform>().anchoredPosition.y;
            if (y > highestY)
            {
                highestY = y;
            }
        }
        return highestY;
    }

    private float GetClosestSymbolY(float targetY)
    {
        float closestY = float.MaxValue;
        foreach (var symbol in symbolObjects)
        {
            float y = symbol.GetComponent<RectTransform>().anchoredPosition.y;
            if (Mathf.Abs(y - targetY) < Mathf.Abs(closestY - targetY))
            {
                closestY = y;
            }
        }
        return closestY;
    }
}