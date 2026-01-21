using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HungerManager : MonoBehaviour
{
    public static HungerManager instance;

    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float drainPerSecond = 5f;
    public float hungerGainOnKill = 20f;

    private float currentHunger;

    [Header("UI")]
    [SerializeField] private Slider hungerSlider;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentHunger = maxHunger;
        UpdateUI();
    }

    void Update()
    {
        DrainHunger();
    }

    void DrainHunger()
    {
        currentHunger -= drainPerSecond * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        UpdateUI();

        if (currentHunger <= 0f)
        {
            LoseGame();
        }
    }

    public void RestoreHunger()
    {
        currentHunger += hungerGainOnKill;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = currentHunger / maxHunger;
        }
    }

    void LoseGame()
    {
        SceneManager.LoadScene("Main Menu");
    }
}