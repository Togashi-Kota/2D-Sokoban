using UnityEngine;
using System.Collections.Generic;
public enum TileType
{
    Empty,
    Floor,
    Wall,
    Goal
}

/// <summary>
/// GroundTilemap
///     WallTile
///     FloorTile
///     GoalTile
/// ObjectTilemap
///     PlayerStart
///     BoxStart
///     
///     StageDataに変換したい意味
///     ・壁の位置一覧
///     ・ゴールの位置一覧
///     ・箱の開始位置一覧
///     ・プレイヤー開始位置
///     ・ステージの幅・高さ
/// </summary>

[CreateAssetMenu(fileName = "NewStageData", menuName = "Sokoban/StageData")]
public class StageData : ScriptableObject
{
    public string stageName;
    public int stageId;

    public int width;
    public int height;

    public Vector2Int playerStartPos;
    public List<Vector2Int> boxStartPositions = new List<Vector2Int>();

    public List<TileType> tiles = new List<TileType>();

    public Vector2 cameraPosition;
    public float cameraSize;
}
