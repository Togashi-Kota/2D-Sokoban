using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int gridPos;

    private Vector2Int startPos;
    Stack<GameState> history = new Stack<GameState>(); //箱、プレイヤー位置の保存

    void Start()
    {
        startPos = gridPos;
        UpdatePosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) Move(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.Z)) Undo();
        if(Input.GetKeyDown(KeyCode.R)) ResetToInitialState();

        // テスト用
        if (Input.GetKeyDown(KeyCode.Alpha1)) GameMaster.Instance.LoadStageByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameMaster.Instance.LoadStageByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameMaster.Instance.LoadStageByIndex(2);
    }


    private void Move(Vector2Int dir)
    {
        Vector2Int nextPos = gridPos + dir;

        // 壁チェック
        if (GameMaster.Instance.IsWall(nextPos)) return;

        // 箱チェック
        Box box = GameMaster.Instance.GetBoxAt(nextPos);

        if (box != null)
        {
            Vector2Int boxNext = nextPos + dir;

            // 箱の先が 壁 or 箱 ならNG
            if (GameMaster.Instance.IsWall(boxNext) || GameMaster.Instance.GetBoxAt(boxNext))
            {
                return;
            }

            SaveState();

            box.Move(boxNext);
        }
        else
        {
            SaveState();
        }

        gridPos = nextPos;
        UpdatePosition();

        if (GameMaster.Instance.IsClear())
        {
            Debug.Log("クリア！");
            GameMaster.Instance.LoadNextStage();
        }
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        gridPos = newPos;
        UpdatePosition();
    }

    public void ClearHistory()
    {
        history.Clear();
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3(gridPos.x, gridPos.y, 0);
    }

    private void SaveState()
    {
        GameState state = new GameState();
        state.playerPos = gridPos;
        state.boxPositions = new List<Vector2Int>();

        foreach (var box in GameMaster.Instance.Boxes)
        {
            state.boxPositions.Add(box.gridPos);
        }

        history.Push(state);
    }

    private void Undo()
    {
        if (history.Count == 0) return;

        GameState state = history.Pop();
        ApplyState(state);
    }

    public void ResetToInitialState()
    {
        if (GameMaster.Instance.InitialState == null)
        {
            return;
        }

        ApplyState(GameMaster.Instance.InitialState);
        history.Clear();
    }

    private void ApplyState(GameState state)
    {
        gridPos = state.playerPos;
        UpdatePosition();

        for(int i = 0; i < GameMaster.Instance.Boxes.Count; i++)
        {
            GameMaster.Instance.Boxes[i].SetGridPosition(state.boxPositions[i]);
        }
    }
}
