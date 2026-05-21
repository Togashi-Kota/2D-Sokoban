using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public Vector2Int playerPos;
    public List<Vector2Int> boxPositions;
}

public static class StageSession
{
    public static StageData SelectedStage;
}

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    [Header("Stage List")]
    [SerializeField] private List<StageData> stages = new List<StageData>();
    [SerializeField] private int currentStageIndex = 0;

    [Header("Current Stage")]
    [SerializeField] private StageData currentStage;

    [Header("Runtime References")]
    [SerializeField] private StageView stageView;
    [SerializeField] private GameObject mainCamera;

    [Header("Prefab")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Box boxPrefab;

    [Header("Parents")]
    [SerializeField] private Transform objectRoot;

    private Player player;
    private List<Box> boxes = new List<Box>();

    public TileType[,] map;

    public IReadOnlyList<Box> Boxes => boxes;
    public GameState InitialState { get; private set; }



    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (StageSession.SelectedStage != null)
        {
            LoadStage(StageSession.SelectedStage);
        }
        else if (currentStage != null)
        {
            LoadStage(currentStage);
        }
        else if (stages.Count > 0)
        {
            LoadStageByIndex(0);
        }
        else
        {
            Debug.LogWarning("StageDataが登録されていません。");
        }
    }

    public void LoadStage(StageData stageData)
    {
        if (stageData == null)
        {
            Debug.LogWarning("LoadStage に null の StageData が渡されました。");
            return;
        }

        currentStage = stageData;

        // tiles 数の最低チェック
        int requiredTileCount = stageData.width * stageData.height;
        if (stageData.tiles == null || stageData.tiles.Count < requiredTileCount)
        {
            Debug.LogError($"StageData [{stageData.stageName}] の tiles 数が不足しています。必要数: {requiredTileCount}, 実際: {stageData.tiles?.Count ?? 0}");
            return;
        }

        if (stageData.boxStartPositions == null)
        {
            Debug.LogError($"StageData [{stageData.stageName}] の boxStartPositions が null です。");
            return;
        }

        /*
        if (boxes.Count != stageData.boxStartPositions.Count)
        {
            Debug.LogError(
                $"ステージ [{stageData.stageName}] の箱数が一致していません。Scene上の箱数: {boxes.Count}, StageData側: {stageData.boxStartPositions.Count}"
            );
            return;
        }
        */

        // マップ生成
        map = new TileType[stageData.width, stageData.height];

        for (int y = 0; y < stageData.height; y++)
        {
            for (int x = 0; x < stageData.width; x++)
            {
                int index = x + y * stageData.width;
                map[x, y] = stageData.tiles[index];
            }
        }

        //見た目生成
        if (stageView != null) stageView.Build(stageData);

        ClearStageObjects();
        SpawnStageObjects(stageData);

        //カメラ位置取得・反映
        mainCamera.transform.position = new Vector3(0,0,-10) + (Vector3)stageData.cameraPosition;
        mainCamera.transform.GetComponent<Camera>().orthographicSize = stageData.cameraSize;

        // 初期状態保存
        SaveInitialState();

        // ステージ切り替え時にUndo履歴を消す
        if (player != null) player.ClearHistory();

        Debug.Log($"ステージ読み込み: {stageData.stageName}");
    }

    public void LoadStageByIndex(int index)
    {
        if (index < 0 || index >= stages.Count)
        {
            Debug.LogWarning($"無効なステージ番号です: {index}");
            return;
        }

        currentStageIndex = index;
        LoadStage(stages[index]);
    }

    public void LoadNextStage()
    {
        int nextindex = currentStageIndex + 1;

        if (nextindex < stages.Count)
        {
            LoadStageByIndex(nextindex);
        }
        else
        {
            Debug.Log("全ステージクリア！");
        }
    }

    public void SaveInitialState()
    {
        if(player == null)
        {
            InitialState = null;
            return;
        }

        InitialState = new GameState();
        InitialState.playerPos = player.gridPos;
        InitialState.boxPositions = new List<Vector2Int>();

        foreach (Box box in boxes)
        {
            InitialState.boxPositions.Add(box.gridPos);
        }
    }

    public Box GetBoxAt(Vector2Int pos)
    {
        foreach (Box box in boxes)
        {
            if (box.gridPos == pos)
            {
                return box;
            }
        }

        return null;
    }

    public bool IsWall(Vector2Int pos) //その座標が壁かどうか. Map外は壁として扱う.
    {
        if (currentStage == null)
        {
            return true;
        }

        if (pos.x < 0 || pos.x >= currentStage.width || pos.y < 0 || pos.y >= currentStage.height)
        {
            return true;
        }

        return map[pos.x, pos.y] == TileType.Wall;
    }

    public bool IsClear() //ゴールの上に箱がすべてあればゴール
    {
        foreach (var box in boxes)
        {
            if (map[box.gridPos.x, box.gridPos.y] != TileType.Goal)
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnStageObjects(StageData stageData)
    {
        if (playerPrefab == null) { Debug.LogError("PlayerPrefab が未設定です。"); return; }
        if (boxPrefab == null) { Debug.LogError("BoxPrefab が未設定です。"); return; }

        Transform parent = objectRoot != null ? objectRoot : transform;

        player = Instantiate(playerPrefab, parent);
        player.SetGridPosition(stageData.playerStartPos);

        boxes.Clear();

        for (int i = 0; i < stageData.boxStartPositions.Count; i++)
        {
            Box box = Instantiate(boxPrefab, parent);
            box.SetGridPosition(stageData.boxStartPositions[i]);
            boxes.Add(box);
        }
    }

    private void ClearStageObjects()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        foreach (Box box in boxes)
        {
            if (box != null)
            {
                Destroy(box.gameObject);
            }
        }

        boxes.Clear();
    }
}
