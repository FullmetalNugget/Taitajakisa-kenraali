using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonAction : MonoBehaviour
{
    public ActionType action;

    public void Execute()
    {
        switch (action)
        {
            case ActionType.LoadScene1:
                SceneManager.LoadScene(1);
                break;

            case ActionType.Quit:
                Application.Quit();
                break;
        }
    }
}

public enum ActionType
{
    LoadScene1,
    Quit
}

