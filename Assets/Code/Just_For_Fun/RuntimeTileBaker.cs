using UnityEngine;
using UnityEngine.AI;
using DunGen;
using DunGen.Adapters;
using System.Collections.Generic;
using Unity.AI.Navigation; // Namespace cần thiết cho NavMeshSurface

/// <summary>
/// Một adapter tùy chỉnh cho DunGen để bake NavMesh cho từng tile một cách riêng lẻ tại runtime.
/// Nó sẽ chạy trước Unity NavMesh Adapter mặc định.
/// </summary>
[AddComponentMenu("DunGen/Adapters/Runtime Tile Baker")]
public class RuntimeTileBaker : BaseAdapter
{
    public RuntimeTileBaker()
    {
        // Đặt Priority thấp (-1) để đảm bảo nó chạy TRƯỚC Unity NavMesh Adapter (mặc định Priority = 0)
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

        if (dungeon == null)
        {
            Debug.LogError("RuntimeTileBaker: Dungeon object is null. Cannot bake tiles.");
            return;
        }

        Debug.Log("RuntimeTileBaker: Starting to bake individual tiles...");

        // Tạo một danh sách để chứa tất cả các NavMeshSurface cần bake
        List<NavMeshSurface> surfacesToBake = new List<NavMeshSurface>();

        foreach (var tile in dungeon.AllTiles)
        {
            if (tile == null)
                continue;

            // Kiểm tra xem tile đã có NavMeshSurface chưa
            NavMeshSurface surface = tile.GetComponent<NavMeshSurface>();

            if (surface == null)
            {
                // Nếu chưa có, thêm mới một component NavMeshSurface
                surface = tile.gameObject.AddComponent<NavMeshSurface>();
            }

            // Thiết lập các thông số cho surface
            // Thu thập tất cả các đối tượng con để bake
            surface.collectObjects = CollectObjects.All;
            // Sử dụng Render Meshes thay vì colliders để xác định hình dạng NavMesh
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            // Bake cho tất cả các agent type
            surface.agentTypeID = 0;

            // Thêm surface vừa tạo hoặc tìm thấy vào danh sách cần bake
            surfacesToBake.Add(surface);
        }

        // Bake tất cả các surface đã thu thập
        foreach (var surface in surfacesToBake)
        {
            surface.BuildNavMesh();
            Debug.Log($"RuntimeTileBaker: Successfully built NavMesh for tile: {surface.gameObject.name}");
        }

        Debug.Log("RuntimeTileBaker: Finished baking all tiles.");
    }
}