using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class ProgressionManager : MonoBehaviour
{
    private GameProgression progression; // Dữ liệu tiến trình

    private string configJsonPath = "JSON/progressionData"; // Đường dẫn tương đối cho Resources
    private string configJsonFullPath; // Đường dẫn tuyệt đối cho ghi file

    [SerializeField] private ProgressionSequenceDataSO progressionSequenceDataSO;
    [SerializeField] private ProgressionDataSO progressionDataSO; // ScriptableObject tổng hợp
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private LootDatabase lootDatabase;
    [SerializeField] private SaveGameManager saveGameManager;
    [SerializeField] private UserAccountManager userAccountManager;

    void Awake()
    {
        //Debug.Log("Đang kiểm tra tài nguyên");
        InitializePaths(); // Khởi tạo đường dẫn
        //ReLoadProgression();
    }


    /// <summary>
    /// Khởi tạo các đường dẫn file.
    /// </summary>
    private void InitializePaths()
    {
        string resourcesDevJsonDir = Path.Combine(Application.dataPath, "Resources_DEV", "JSON");

#if UNITY_EDITOR
        if (!Directory.Exists(resourcesDevJsonDir))
        {
            Directory.CreateDirectory(resourcesDevJsonDir);
            Debug.Log($"Đã tạo thư mục: {resourcesDevJsonDir}");
        }
#endif

        configJsonFullPath = Path.Combine(resourcesDevJsonDir, "progressionData.json");
        //Debug.Log($"Initialized configJsonFullPath: {configJsonFullPath}");
    }

    /// <summary>
    /// Xuất dữ liệu từ ScriptableObject sang file JSON. Chỉ hoạt động trong Edit Mode.
    /// </summary>
    public void ExportToJson()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogError("ExportToJson is only allowed in Edit Mode to protect progressionData.json!");
            return;
        }

        if (progressionDataSO == null)
        {
            Debug.LogError("ProgressionDataSO is not assigned!");
            return;
        }

        progression = progressionDataSO.ToGameProgression();
        // Tự động cập nhật ProgressionSequenceDataSO
        if (progressionSequenceDataSO != null && progression != null)
        {
            progressionSequenceDataSO.MainProcessOrder = progression.MainProcesses
                .Select(p => p.Id)
                .ToList();

            EditorUtility.SetDirty(progressionSequenceDataSO);
            AssetDatabase.SaveAssets();

            Debug.Log($"Updated ProgressionSequenceDataSO with {progressionSequenceDataSO.MainProcessOrder.Count} entries.");
        }
        else
        {
            Debug.LogWarning("ProgressionSequenceDataSO is not assigned or progression is null.");
        }
        if (progression == null || progression.MainProcesses == null)
        {
            Debug.LogError("Failed to convert ProgressionDataSO to GameProgression!");
            return;
        }

        if (progression.MainProcesses.Count == 0)
        {
            Debug.LogWarning("ProgressionDataSO contains no MainProcesses. Exported JSON will be empty.");
        }
        else
        {
            Debug.Log($"Exporting {progression.MainProcesses.Count} MainProcesses...");
            foreach (var main in progression.MainProcesses)
            {
                Debug.Log($"MainProcess: {main.Id}, SubProcesses: {main.SubProcesses?.Count ?? 0}, Rewards: {main.Rewards?.Count ?? 0}");
                foreach (var sub in main.SubProcesses)
                {
                    Debug.Log($"  SubProcess: {sub.Id}, Conditions: {sub.Conditions?.Count ?? 0}, Rewards: {sub.Rewards?.Count ?? 0}");
                }
            }
        }

        if (string.IsNullOrEmpty(configJsonFullPath))
        {
            InitializePaths();
        }

        if (string.IsNullOrEmpty(configJsonFullPath))
        {
            Debug.LogError("configJsonFullPath is null or empty!");
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(configJsonFullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"Created directory: {directory}");
            }

            string json = JsonSerializationHelper.SerializeGameProgression(progression);
            Debug.Log($"JSON content to write: {json}");
            File.WriteAllText(configJsonFullPath, json);
            Debug.Log($"Exported ProgressionDataSO to {configJsonFullPath}");

            string relativePath = "Assets/Resources_DEV/JSON/progressionData.json";
            UnityEditor.AssetDatabase.ImportAsset(relativePath);
            Debug.Log($"Refreshed asset: {relativePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export JSON: {e.Message}");
        }
#else
        Debug.LogError("ExportToJson is only available in Unity Editor!");
#endif
    }

    /// <summary>
    /// Tải lại dữ liệu tiến trình.
    /// </summary>
    public void ReLoadProgression()
    {
        if (saveGameManager == null)
        {
            Debug.LogError("SaveGameManager is not assigned!");
            return;
        }

        string userName = userAccountManager.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("CurrentUserNamePlaying is not set!");
            return;
        }

        // Kiểm tra thư mục lưu game mới nhất của người dùng
        string saveFolder = saveGameManager.GetLatestSaveFolder(userName);
        if (saveFolder != null)
        {
            // Kiểm tra file JSON trong thư mục lưu game
            var files = saveGameManager.LoadJsonFiles(saveFolder);
            var file = files.FirstOrDefault(f => f.fileName == "playerProgression.json");
            if (file.fileName != null)
            {
                string json = file.json;
                progression = JsonSerializationHelper.DeserializeGameProgression(json);
                Debug.LogError($"Loaded player progression from: {saveFolder}/playerProgression.json");
                return;
            }
        }

        // Nếu không tìm thấy save game, kiểm tra file JSON trong Resources
        TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
        if (jsonText != null)
        {
            progression = JsonSerializationHelper.DeserializeGameProgression(jsonText.text);
            Debug.Log("Loaded progression from JSON: JSON/progressionData");
            string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
            saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", jsonText.text);
            Debug.Log($"Created new player progression file in: {newSaveFolder}");
            return;
        }

        // Nếu không tìm thấy file JSON trong Resources, kiểm tra ScriptableObject
        if (progressionDataSO != null)
        {
            progression = progressionDataSO.ToGameProgression();
            Debug.Log("Loaded progression from ProgressionDataSO");
            string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
            string json = JsonSerializationHelper.SerializeGameProgression(progression);
            saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
            Debug.Log($"Created new player progression file in: {newSaveFolder}");
            return;
        }

        Debug.LogError("No progression data found!");
    }

    /// <summary>
    /// Tải dữ liệu tiến trình từ file đươc tryền vào.
    /// </summary>
    public void LoadProgression(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            progression = JsonSerializationHelper.DeserializeGameProgression(json);
            GetCurrentOrNextMainProcess();
        }
        else
        {
            Debug.LogError("Progression JSON content is null or empty!");
            ReLoadProgression();
        }
    }

    /// <summary>
    /// Tạo một save game mới.
    /// </summary>
    public void CreateNewGame()
    {
        if (saveGameManager == null)
        {
            Debug.LogError("SaveGameManager is not assigned!");
            return;
        }

        string userName = userAccountManager.currentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("CurrentUserNamePlaying is not set!");
            return;
        }

        string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
        TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
        string json;
        if (jsonText != null)
        {
            json = jsonText.text;
            Debug.Log("Using default progression from JSON/progressionData");
        }
        else if (progressionDataSO != null)
        {
            progression = progressionDataSO.ToGameProgression();
            json = JsonSerializationHelper.SerializeGameProgression(progression);
            Debug.Log("Using default progression from ProgressionDataSO");
        }
        else
        {
            Debug.LogError("No default progression data found!");
            return;
        }

        saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
        progression = JsonSerializationHelper.DeserializeGameProgression(json);
        Debug.Log($"Created new game save: {newSaveFolder}");
    }

    /// <summary>
    /// Lưu dữ liệu tiến trình vào file JSON.
    /// </summary>
    public void SaveProgression()
    {
        if (saveGameManager == null)
        {
            Debug.LogError("SaveGameManager is not assigned!");
            return;
        }

        string userName = userAccountManager.currentUserBaseName;
        if (string.IsNullOrEmpty(userName))

        {
            Debug.LogError("CurrentUserNamePlaying is not set!");
            return;
        }

        string saveFolder = saveGameManager.GetLatestSaveFolder(userName) ?? saveGameManager.CreateNewSaveFolder(userName);
        string json = JsonSerializationHelper.SerializeGameProgression(progression);
        saveGameManager.SaveJsonFile(saveFolder, "playerProgression.json", json);
    }

    /// <summary>
    /// trả về dữ liệu tiến trình theo ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public object GetProcessData(string id)
    {
        var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
        if (mainProcess != null)
            return mainProcess;

        foreach (var main in progression.MainProcesses)
        {
            var subProcess = main.SubProcesses.Find(s => s.Id == id);
            if (subProcess != null)
                return subProcess;
        }

        Debug.LogWarning($"Process with ID {id} not found!");
        return null;
    }

    /// <summary>
    /// Mở khóa tiến trình theo ID.
    /// trả về true nếu mở khóa thành công.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool UnlockProcess(string id)
    {
        var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
        if (mainProcess != null && mainProcess.Status == "Locked")
        {
            mainProcess.Status = "InProgress";
            SaveProgression();
            Debug.Log($"Unlocked MainProcess {id}");
            return true;
        }

        foreach (var main in progression.MainProcesses)
        {
            var subProcess = main.SubProcesses.Find(s => s.Id == id);
            if (subProcess != null && subProcess.Status == "Locked")
            {
                subProcess.Status = "InProgress";
                SaveProgression();
                Debug.Log($"Unlocked SubProcess {id}");
                return true;
            }
        }

        Debug.LogWarning($"Cannot unlock process {id}: Not found or not locked!");
        return false;
    }

    /// <summary>
    /// Kiểm tra điều kiện hoàn thành tiến trình con và cấp phần thưởng nếu hoàn thành.
    /// trả về true nếu tiến trình con đã hoàn thành.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool CheckProcessCompletion(string id, object data)
    {
        foreach (var main in progression.MainProcesses) // Duyệt từng tiến trình chính
        {
            var subProcess = main.SubProcesses.Find(s => s.Id == id);
            if (subProcess != null && subProcess.Status != "Completed")
            {
                bool allConditionsMet = subProcess.Conditions.All(c => c.IsSatisfied(data));
                if (allConditionsMet)
                {
                    subProcess.Status = "Completed";
                    foreach (var reward in subProcess.Rewards)
                        GrantReward(reward); // Cấp phần thưởng cho tiến trình con

                    // Kiểm tra xem tiến trình chính có hoàn thành không
                    if (main.SubProcesses.All(s => s.Status == "Completed"))
                    {
                        main.Status = "Completed";
                        foreach (var reward in main.Rewards)
                            GrantReward(reward);
                    }

                    SaveProgression();
                    Debug.Log($"SubProcess {id} completed!");
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Cấp phần thưởng cho tiến trình.
    /// nó truy cập vào cơ sở dữ liệu vũ khí và loot để lấy thông tin chi tiết về phần thưởng.
    /// được gọi khi tiến trình hoàn thành.
    /// </summary>
    /// <param name="reward"></param>
    private void GrantReward(Reward reward)
    {
        if (reward is ItemReward itemReward)
        {
            if (itemReward.ItemType == "Weapon")
            {
                var weapon = weaponDatabase.GetWeaponConfig(itemReward.ItemName);
                if (weapon != null)
                    Debug.Log($"Granted {itemReward.Amount} {weapon.Name} (Power: {weapon.Power})");
                else
                    Debug.LogWarning($"Weapon {itemReward.ItemName} not found!");
            }
            else if (itemReward.ItemType == "Loot")
            {
                var loot = lootDatabase.GetLootConfig(itemReward.ItemName);
                if (loot != null)
                    Debug.Log($"Granted {itemReward.Amount} {loot.Name} (Power: {loot.Power})");
                else
                    Debug.LogWarning($"Loot {itemReward.ItemName} not found!");
            }
            // Nếu không phải là vũ khí hoặc loot, có thể thêm xử lý khác ở đây
            else
            {
                Debug.LogWarning($"Unknown item type: {itemReward.ItemType}");
            }
        }
        reward.Grant();
    }

    /// <summary>
    /// Cập nhật trạng thái tiến trình theo ID.
    /// được gọi mỗi khi tiến trình thay đổi trạng thái.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public bool UpdateProcessStatus(string id, string newStatus)
    {
        if (!new[] { "Locked", "InProgress", "Completed" }.Contains(newStatus))
        {
            Debug.LogError($"Invalid status: {newStatus}");
            return false;
        }

        var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
        if (mainProcess != null)
        {
            mainProcess.Status = newStatus;
            SaveProgression();
            Debug.Log($"Updated MainProcess {id} to {newStatus}");
            return true;
        }

        foreach (var main in progression.MainProcesses)
        {
            var subProcess = main.SubProcesses.Find(s => s.Id == id);
            if (subProcess != null)
            {
                subProcess.Status = newStatus;
                SaveProgression();
                Debug.Log($"Updated SubProcess {id} to {newStatus}");
                return true;
            }
        }

        Debug.LogWarning($"Process {id} not found!");
        return false;
    }

    // Cấp phần thưởng cho tiến trình
    public void GrantRewards(string id)
    {
        var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
        if (mainProcess != null)
        {
            foreach (var reward in mainProcess.Rewards)
                GrantReward(reward);
            return;
        }

        foreach (var main in progression.MainProcesses)
        {
            var subProcess = main.SubProcesses.Find(s => s.Id == id);
            if (subProcess != null)
            {
                foreach (var reward in subProcess.Rewards)
                    GrantReward(reward);
                return;
            }
        }

        Debug.LogWarning($"Process {id} not found!");
    }

    /// <summary>
    /// Lấy danh sách ID tiến trình theo trạng thái.
    /// trả về một Dictionary với các trạng thái là key và danh sách ID "value".
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, List<string>> GetProcessIdsByStatus()
    {
        var result = new Dictionary<string, List<string>> // Khởi tạo danh sách ID theo trạng thái
        {
            { "All", new List<string>() },
            { "Completed", new List<string>() },
            { "Locked", new List<string>() },
            { "InProgress", new List<string>() }
        };

        foreach (var main in progression.MainProcesses)
        {
            result["All"].Add(main.Id);
            result[main.Status].Add(main.Id);

            foreach (var sub in main.SubProcesses)
            {
                result["All"].Add(sub.Id);
                result[sub.Status].Add(sub.Id);
            }
        }

        return result;
    }

    /// <summary>
    /// Lấy tiến trình chính hiện tại hoặc tiếp theo chưa hoàn thành.
    /// </summary>
    /// <returns></returns>
    public MainProcess GetCurrentOrNextMainProcess()
    {
        if (progression == null || progression.MainProcesses == null)
        {
            Debug.LogError("Progression data not loaded!");
            return null;
        }

        foreach (var id in progressionSequenceDataSO.MainProcessOrder)
        {
            var main = progression.MainProcesses.Find(p => p.Id == id);
            if (main == null)
            {
                Debug.LogWarning($"Progression data missing MainProcess ID: {id}");
                continue;
            }

            if (main.Status != "Completed") // Chưa hoàn thành
            {
                Debug.Log($"Current or Next Progression is: {main.Description}");
                return main;
            }
        }

        Debug.LogWarning("All progressions completed.");
        return null;
    }



}