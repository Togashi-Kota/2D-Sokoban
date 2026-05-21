using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void BackToStageSelect()
    {
        StageSession.SelectedStage = null;
        SceneManager.LoadScene("StageSelectScene");
    }

    public void BackToTitle()
    {
        StageSession.SelectedStage = null;
        SceneManager.LoadScene("TitleScene");
    }
}
