using UnityEngine;
using UnityEngine.AI;
using DunGen;
using DunGen.Adapters;
using System.Collections.Generic;
using Unity.AI.Navigation; // Namespace cần thiết cho NavMeshSurface

/// <summary>
/// Một adapter tùy chỉnh cho DunGen để bake một NavMesh DUY NHẤT cho toàn bộ dungeon tại runtime.
/// </summary>
[AddComponentMenu("DunGen/Adapters/Full Dungeon Baker (Custom)")]
public class RuntimeTileBaker : BaseAdapter
{
    public RuntimeTileBaker()
    {
        // Đặt Priority thấp (-1) để đảm bảo nó chạy TRƯỚC các adapter khác nếu cần
        Priority = -1;
    }

    /// <summary>
    /// Ghi đè phương thức Run của BaseAdapter.
    /// DunGen sẽ tự động gọi phương thức này trong quá trình tạo dungeon.
    /// </summary>
    /// <param name="generator">Đối tượng DungeonGenerator đang hoạt động.</param>
    protected override void Run(DungeonGenerator generator)
    {
        Dungeon dungeon = generator.CurrentDungeon;

        // SỬA LỖI: Chỉ cần kiểm tra dungeon có null không. 
        // Nếu dungeon tồn tại, dungeon.gameObject cũng sẽ tồn tại.
        if (dungeon == null)
        {
            Debug.LogError("Custom Baker: Dungeon object is null. Cannot bake NavMesh.");
            return;
        }

        Debug.Log("Custom Baker: Starting to bake a single NavMesh for the entire dungeon...");

        // SỬA LỖI: Truy cập GameObject của component Dungeon bằng "dungeon.gameObject"
        GameObject dungeonRoot = dungeon.gameObject;

        // 1. Tìm hoặc tạo một NavMeshSurface duy nhất trên đối tượng Root của dungeon
        NavMeshSurface surface = dungeonRoot.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = dungeonRoot.AddComponent<NavMeshSurface>();
        }

        // 2. Thiết lập các thông số cho surface
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surface.agentTypeID = 0;
        surface.RemoveData();

        // 3. Thực hiện bake NavMesh cho toàn bộ các đối tượng đã thu thập
        surface.BuildNavMesh();

        // SỬA LỖI: Sử dụng dungeonRoot.name để lấy tên
        Debug.Log($"Custom Baker: Successfully built a unified NavMesh for the dungeon '{dungeonRoot.name}'.");
    }
}