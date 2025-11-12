using UnityEngine;

using TMPro;
using UnityEngine;

public class GoldUIController : MonoBehaviour
{
    public TMP_Text goldText;

    public void SetGold(int amount)
    {
        goldText.text = amount.ToString();
    }
}

