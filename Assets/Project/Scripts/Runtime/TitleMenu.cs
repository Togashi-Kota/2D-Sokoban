using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public void OpenStageSelect()
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
