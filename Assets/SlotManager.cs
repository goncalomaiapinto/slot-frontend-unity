using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using System.Collections;

public class SlotManager : MonoBehaviour
{
    public List<ReelController> reels;
    public UIManager uiManager;
    public GameObject symbolPrefab;
    public Transform freeSpinsIndicator;
    public float spinDuration = 1.5f;

    public List<SpriteMapping> spriteMappings;
    private Dictionary<string, Sprite> symbolSprites = new Dictionary<string, Sprite>();

    private float saldo = 20.0f;
    private float aposta = 0.2f;
    private int freeSpins = 0;
    private Dictionary<string, int> stickyWilds = new Dictionary<string, int>();

    private bool isSpinning = false;

    private void Start()
    {
        foreach (var mapping in spriteMappings)
        {
            symbolSprites[mapping.symbolName] = mapping.sprite;
        }

        FillInitialSymbols();

        uiManager.UpdateUI(saldo, aposta, 0);
    }

    private void FillInitialSymbols()
    {
        float[] reelOffsets = { 0f, 130f, 259f, 385.6f, 514.7f };

        for (int i = 0; i < reels.Count; i++)
        {
            reels[i].SetReelOffset(reelOffsets[i]);
            reels[i].SetReelIndex(i); // Set the reel index for position lookup

            List<string> randomSymbols = new List<string>();
            for (int j = 0; j < 5; j++)
            {
                int index = Random.Range(0, spriteMappings.Count);
                randomSymbols.Add(spriteMappings[index].symbolName);
            }

            reels[i].FillReel(randomSymbols, symbolSprites, symbolPrefab);
        }
    }

    public void Spin()
    {
        if (isSpinning || (freeSpins == 0 && saldo < aposta))
            return;

        isSpinning = true;

        if (freeSpins == 0)
            saldo -= aposta;

        StartCoroutine(SendSpinRequest());
    }

    private IEnumerator SendSpinRequest()
    {
        string url = "http://localhost:8080/spin";

        SpinRequest request = new SpinRequest
        {
            bet = aposta,
            freeSpins = freeSpins,
            stickyWilds = stickyWilds
        };

        string json = JsonConvert.SerializeObject(request);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro na Spin: " + www.error);
            isSpinning = false;
        }
        else
        {
            SpinResult result = JsonConvert.DeserializeObject<SpinResult>(www.downloadHandler.text);
            Debug.Log(result.reels[0][0] + " " + result.reels[0][1] + " " + result.reels[0][2]);
            StartCoroutine(HandleSpinResult(result));
        }
    }

    private IEnumerator HandleSpinResult(SpinResult result)
    {
        // Preparar os símbolos finais para cada reel (apenas 3 símbolos da API)
        List<List<string>> finalReels = new List<List<string>>();
        for (int i = 0; i < reels.Count; i++)
        {
            List<string> finalSymbols = new List<string>
            {
                result.reels[i][0], // Primeiro símbolo da API
                result.reels[i][1], // Segundo símbolo da API
                result.reels[i][2]  // Terceiro símbolo da API
            };
            finalReels.Add(finalSymbols);
        }

        // Iniciar a animação de rotação para cada reel com um pequeno atraso
        for (int i = 0; i < reels.Count; i++)
        {
            reels[i].StartSpin(spinDuration, finalReels[i]);
            yield return new WaitForSeconds(0.2f);
        }

        // Aguardar cada reel parar sequencialmente
        for (int i = 0; i < reels.Count; i++)
        {
            yield return new WaitForSeconds(spinDuration + 0.9f); // spinDuration + desaceleração (0.6f) + bounce (0.3f)
        }

        // Atualizar saldo, giros grátis e sticky wilds
        saldo += result.win;
        freeSpins = result.freeSpins;
        stickyWilds = result.stickyWilds;

        uiManager.UpdateUI(saldo, aposta, result.win);
        uiManager.UpdateFreeSpins(freeSpins);

        isSpinning = false;
    }

    private string GetRandomSymbol()
    {
        List<string> keys = new List<string>(symbolSprites.Keys);
        return keys[Random.Range(0, keys.Count)];
    }

    public void IncreaseBet()
    {
        aposta += 0.2f;
        uiManager.UpdateUI(saldo, aposta, 0);
    }

    public void DecreaseBet()
    {
        if (aposta > 0.2f)
        {
            aposta -= 0.2f;
            uiManager.UpdateUI(saldo, aposta, 0);
        }
    }
}

[System.Serializable]
public class SpriteMapping
{
    public string symbolName;
    public Sprite sprite;
}

[System.Serializable]
public class SpinRequest
{
    public float bet;
    public int freeSpins;
    public Dictionary<string, int> stickyWilds;
}

[System.Serializable]
public class SpinResult
{
    public List<List<string>> reels;
    public float win;
    public List<List<int>> winningLines;
    public int freeSpins;
    public Dictionary<string, int> stickyWilds;
}