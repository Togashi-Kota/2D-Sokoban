using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectMenu : MonoBehaviour
{
    [SerializeField] private StageDatabase stageDatabase;
    [SerializeField] private Button stageButtonPrefab;
    [SerializeField] private Transform buttonContainer;

    private void Start()
    {
        GenerateStageButtons();
    }

    private void GenerateStageButtons()
    {
        if (stageDatabase == null || stageButtonPrefab == null || buttonContainer == null) return;

        for (int i = 0; i < stageDatabase.stages.Count; i++)
        {
            int index = i;
            StageData stage = stageDatabase.stages[i];

            Button btn = Instantiate(stageButtonPrefab, buttonContainer);

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = stage.stageName;

            btn.onClick.AddListener(() => SelectStage(index));
        }
    }

    public void SelectStage(int index)
    {
        if (stageDatabase == null) return;
        if (index < 0 || index >= stageDatabase.stages.Count) return;

        StageSession.SelectedStage = stageDatabase.stages[index];
        SceneManager.LoadScene("GameScene");
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
