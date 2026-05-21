using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(StageSource))]
public class StageSourceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StageSource source = (StageSource)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Tilemap座標を集める"))
        {
            LogTilemapSummary("Floor", source.floorTilemap);
            LogTilemapSummary("Wall", source.wallTilemap);
            LogTilemapSummary("Goal", source.goalTilemap);
            LogObjectSummary(source);

            BoundsInt bounds = GetMergedBounds(source.floorTilemap, source.wallTilemap, source.goalTilemap, source.objectTilemap);

            Debug.Log("bounds : (xMin = " + bounds.xMin + ", yMin = " + bounds.yMin + ")");
        }

        if (GUILayout.Button("StageDataを生成する"))
        {
            ConvertToStageData(source);
        }

    }

    //--------------------------------------------------------//

    private static void ConvertToStageData(StageSource source)
    //登録したStageSourceからTilesに変換→StageDataに保存。
    {
        if (source.outputStageData == null) { Debug.LogError("outputStageData が未設定です。"); return; }

        BoundsInt bounds = GetMergedBounds(
            source.floorTilemap,
            source.wallTilemap,
            source.goalTilemap,
            source.objectTilemap
            );

        int width = bounds.size.x;
        int height = bounds.size.y;

        if (width <= 0 || height <= 0) { Debug.LogError("有効なステージ範囲を取得できませんでした。"); return; }

        List<TileType> tiles = BuildTiles(source, bounds);
        Vector2Int playerStartPos = FindPlayerStart(source, bounds);
        List<Vector2Int> boxStartPositions = FindBoxPositions(source, bounds);

        if (playerStartPos.x < 0 || playerStartPos.y < 0) { Debug.LogError("有効なステージ範囲を取得できませんでした。"); return; }

        Undo.RecordObject(source.outputStageData, "Convert Tilemap To StageData");

        source.outputStageData.stageName = string.IsNullOrEmpty(source.stageName)
            ? source.outputStageData.name : source.stageName;

        if (source.targetCamera == null)
        {
            Debug.LogWarning("TargetCamera が未設定です。");
        }
        else
        {
            //source.outputStageData.cameraPosition = source.targetCamera.transform.position;
            Vector3 camWorldPos = source.targetCamera.transform.position;
            source.outputStageData.cameraPosition = new Vector2(camWorldPos.x - bounds.xMin, camWorldPos.y - bounds.yMin);

            Camera cam = source.targetCamera.GetComponent<Camera>();

            if (cam == null)
            {
                Debug.LogWarning("TargetCamera より Camera コンポーネントを取得できませんでした。");
            }
            else
            {
                source.outputStageData.cameraSize = cam.orthographicSize;
            }
        }


        if (!ValidateStageData(width, height, tiles, boxStartPositions, playerStartPos))
        {
            return;
        }


        source.outputStageData.width = width;
        source.outputStageData.height = height;
        source.outputStageData.playerStartPos = playerStartPos;
        source.outputStageData.boxStartPositions = boxStartPositions;
        source.outputStageData.tiles = tiles;

        EditorUtility.SetDirty(source.outputStageData);
        AssetDatabase.SaveAssets();

        Debug.Log(
            "StageData 生成完了: " + source.outputStageData.stageName +
            " / size = (" + width + ", " + height + ")" +
            " / boxCount = " + boxStartPositions.Count +
            " / cameraPosition = " + source.outputStageData.cameraPosition
            );
    }

    private static List<TileType> BuildTiles(StageSource source, BoundsInt bounds)
    //StageSourceからTilesに変換。
    {
        List<TileType> tiles = new List<TileType>();

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileType tile = GetTileTypeAt(source, pos);
                tiles.Add(tile);
            }
        }

        return tiles;
    }

    private static TileType GetTileTypeAt(StageSource source, Vector3Int pos)
    //指定した座標のTileTypeを取得。
    {
        if (source.wallTilemap != null && source.wallTilemap.HasTile(pos)) { return TileType.Wall; }
        if (source.goalTilemap != null && source.goalTilemap.HasTile(pos)) { return TileType.Goal; }
        if (source.floorTilemap != null && source.floorTilemap.HasTile(pos)) { return TileType.Floor; }

        return TileType.Empty;
    }

    private static Vector2Int FindPlayerStart(StageSource source, BoundsInt bounds)
    //StageSourceからプレイヤー位置を取得。
    {
        if (source.playerTile == null || source.objectTilemap == null) { return new Vector2Int(-1, -1); }

        List<Vector3Int> positions = CollectTilePositions(source.objectTilemap);

        foreach (Vector3Int pos in positions)
        {
            if (source.playerTile == source.objectTilemap.GetTile(pos))
            {
                return ToLocalPosition(pos, bounds);
            }
        }

        return new Vector2Int(-1, -1);
    }

    private static List<Vector2Int> FindBoxPositions(StageSource source, BoundsInt bounds)
    //StageSourceから箱位置を取得。
    {
        List<Vector2Int> boxPositions = new List<Vector2Int>();

        if (source.boxTile == null || source.objectTilemap == null) return boxPositions;

        List<Vector3Int> positions = CollectTilePositions(source.objectTilemap);

        foreach (Vector3Int pos in positions)
        {
            if (source.boxTile == source.objectTilemap.GetTile(pos))
            {
                boxPositions.Add(ToLocalPosition(pos, bounds));
            }
        }

        return boxPositions;
    }

    private static Vector2Int ToLocalPosition(Vector3Int tilemapPos, BoundsInt bounds)
    //Tilemap実座標→ゲーム内ローカル座標へと変換
    {
        int localx = tilemapPos.x - bounds.xMin;
        int localy = tilemapPos.y - bounds.yMin;
        return new Vector2Int(localx, localy);
    }

    private static BoundsInt GetMergedBounds(params Tilemap[] tilemaps)
    //指定したTilemapから合わせたBoundsを取得。
    {
        bool initialized = false;
        BoundsInt result = new BoundsInt();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap == null) continue;

            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;

            if (!initialized)
            {
                result = bounds;
                initialized = true;
            }
            else
            {
                result.xMin = Mathf.Min(result.xMin, bounds.xMin);
                result.xMax = Mathf.Max(result.xMax, bounds.xMax);
                result.yMin = Mathf.Min(result.yMin, bounds.yMin);
                result.yMax = Mathf.Max(result.yMax, bounds.yMax);
            }
        }

        return result;
    }

    private static void LogObjectSummary(StageSource source)
    //StageSourceから各Objectを取得→ログに出力する。
    {
        if (source.objectTilemap == null)
        {
            Debug.LogWarning("Object は未設定です。");
            return;
        }

        if (source.playerTile == null)
        {
            Debug.LogWarning("playerTile は未設定です。");
        }

        if (source.boxTile == null)
        {
            Debug.LogWarning("boxTile は未設定です。");
        }

        List<Vector3Int> positions = CollectTilePositions(source.objectTilemap);

        Vector3Int playerPos = new Vector3Int(999, 999, 0);
        List<Vector3Int> boxPositions = new List<Vector3Int>();

        foreach (Vector3Int pos in positions)
        {
            TileBase tile = source.objectTilemap.GetTile(pos);

            if (tile == source.playerTile)
            {
                playerPos = pos;
            }
            else if (tile == source.boxTile)
            {
                boxPositions.Add(pos);
            }
        }

        if (playerPos.x == 999)
        {
            Debug.Log("Player開始位置は見つかりませんでした");
        }
        else
        {
            Debug.Log("Player開始位置 = " + playerPos);
        }

        Debug.Log("Box数 = " + boxPositions.Count);

        int logCount = Mathf.Min(5, boxPositions.Count);
        for (int i = 0; i < logCount; i++)
        {
            Debug.Log("Box[" + i + "] = " + boxPositions[i]);
        }

    }

    private static void LogTilemapSummary(string label, Tilemap tilemap)
    //指定したタイルマップから集めて数、位置、タイル名を出力。
    {
        if (tilemap == null)
        {
            Debug.LogWarning(label + " は未設定です。");
            return;
        }

        List<Vector3Int> positions = CollectTilePositions(tilemap);

        Debug.Log(label + " のタイル数 = " + positions.Count);

        int logCount = Mathf.Min(5, positions.Count); // 5個まで出力。
        for (int i = 0; i < logCount; i++)
        {
            TileBase tile = tilemap.GetTile(positions[i]);
            Debug.Log(label + "[" + i + "] 座標 = " + positions[i] + " / 名前 = " + tile.name);
        }
    }

    private static List<Vector3Int> CollectTilePositions(Tilemap tilemap)
    //Tilemapから座標をListとして取り出す。
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (tilemap.HasTile(pos))
                {
                    positions.Add(pos);
                }
            }
        }

        return positions;
    }

    private static bool ValidateStageData(
        int width,
        int height,
        List<TileType> tiles,
        List<Vector2Int> boxPositions,
        Vector2Int playerPos
        )
    {
        if (tiles == null)
        {
            return false;
        }

        if (tiles.Count != width * height)
        {
            Debug.LogError("tiles数が不正です。 tiles.Count = " + tiles.Count + ", width * height = " + width * height);
            return false;
        }

        if (playerPos == new Vector2Int(-1, -1))
        {
            Debug.LogError("Player が配置されていません。");
            return false;
        }

        int boxCount = boxPositions.Count;
        int goalCount = tiles.Count(tiles => tiles == TileType.Goal);

        if (boxCount == 0)
        {
            Debug.LogError("Box が1つも配置されていません。");
            return false;
        }

        if (goalCount == 0)
        {
            Debug.LogError("Goal が1つも配置されていません。");
            return false;
        }

        if (boxCount != goalCount)
        {
            Debug.LogError("Box数(" + boxCount + ") と Goal数(" + goalCount + ") が一致していません。");
            return false;
        }

        return true;
    }
}
