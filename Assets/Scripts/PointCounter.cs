using UnityEngine;
using TMPro;

public class PointCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text pointText;
    private int points;

    private void Start()
    {
        UpdateText();
    }

    public void GetPoint(int amount = 1)
    {
        points += amount;
        UpdateText();
    }

    private void UpdateText()
    {
        pointText.text = points.ToString();
    }
}

