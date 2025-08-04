using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI saldoText;
    public TextMeshProUGUI apostaText;
    public TextMeshProUGUI premioText;
    public TextMeshProUGUI freeSpinsText;

    public void UpdateUI(float saldo, float aposta, float premio)
    {
        saldoText.text = "Saldo: €" + saldo.ToString("F2");
        apostaText.text = "Aposta: €" + aposta.ToString("F2");
        premioText.text = "Prémio: €" + premio.ToString("F2");
    }

    public void UpdateFreeSpins(int freeSpins)
    {
        if (freeSpins > 0)
            freeSpinsText.text = "Free Spins: " + freeSpins;
        else
            freeSpinsText.text = "";
    }
}
