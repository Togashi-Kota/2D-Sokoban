using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Sokoban/StageDatabase")]
public class StageDatabase : ScriptableObject
{
    public List<StageData> stages = new List<StageData>();
}
