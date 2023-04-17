using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Tilemaps;
#endif
public class MapEditor
{
#if UNITY_EDITOR
    // % (Ctrl), # (Shift), & (Alt)
    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        GameObject go = GameObject.Find("Grid");
        if (null == go)
        {
            Debug.LogError("not found Grid");
            return;
        }

        Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
        if (null == tm)
        {
            Debug.LogError("not found Tilemap_Collision");
            return;
        }
        // List<Vector3Int> blocked = new ListStack<Vector3Int>();
        // foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)
        // {
        //     TileBase tile = tm.GetTile(pos);
        //     if (null == tile)
        //         continue;
        //     blocked.Add(pos);
        // }
        
        using (var writer = File.CreateText("Assets/Resources/Map/output.txt"))
        {
            // map size 
            writer.WriteLine(tm.cellBounds.xMin);
            writer.WriteLine(tm.cellBounds.xMax);
            writer.WriteLine(tm.cellBounds.yMin);
            writer.WriteLine(tm.cellBounds.yMax);
                
            // 왼쪽 상단을 원점으로 (y+가 위,x-가 왼)
            for(int y=tm.cellBounds.yMax;y>=tm.cellBounds.yMin;y--)
            {
                for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                {
                    TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                    writer.Write(null == tile ? "0" : "1");
                }

                writer.WriteLine();
            }
        }
        
        Debug.Log("Complete");
    }
    
#endif    
}
