using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Procession
{
    public class ProgressionManager : MonoBehaviour, ISaveable
    {
        public static ProgressionManager Instance { get; private set; }

        [SerializeField] private ProgressionDataSO progressionDataSO;
        [SerializeField] private LootDatabase lootDatabase;

        private GameProgression progression;
        // === ISaveable Implementation ===
        public string FileName => "playerProgression.json";

        // === THÊM ===
        // Danh sách tuần tự eventId (lấy từ progression.MainProcesses → SubProcesses theo Order)
        private List<string> _eventSequence;
        private int _currentEventIndex = 0;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (progression == null && progressionDataSO != null)
            {
                progression = progressionDataSO.ToGameProgression();
                Debug.Log("[ProgressionManager] Loaded default progression from SO.");
            }

            BuildEventSequence();
        }

        #region === CÁC HÀM SAVE/LOAD ===

        public string SaveToJson()
        {
            return JsonSerializationHelper.SerializeGameProgression(progression);
        }

        public void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("LoadFromJson: progression json is null or empty.");
                return;
            }

            progression = JsonSerializationHelper.DeserializeGameProgression(json);
            Debug.Log("[ProgressionManager] Progression loaded from JSON.");
            BuildEventSequence();
        }

#if UNITY_EDITOR
        private static string GetEditorProgressionPath()
        {
            string path = Path.Combine(Application.dataPath, "Resources_DEV", "JSON");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[ProgressionManager] Created path: {path}");
            }
            return Path.Combine(path, "progressionData.json");
        }

        public void ExportToJson()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("ExportToJson is only allowed in Edit Mode.");
                return;
            }

            if (progressionDataSO == null)
            {
                Debug.LogError("ProgressionDataSO is not assigned!");
                return;
            }

            var prog = progressionDataSO.ToGameProgression();
            string json = JsonSerializationHelper.SerializeGameProgression(prog);
            string fullPath = GetEditorProgressionPath();

            File.WriteAllText(fullPath, json);
            Debug.Log($"Exported progression to: {fullPath}");

            string relativePath = "Assets/Resources_DEV/JSON/progressionData.json";
            UnityEditor.AssetDatabase.ImportAsset(relativePath);
            Debug.Log($"Refreshed asset: {relativePath}");
        }
#endif

        /// <summary>
        /// Tải dữ liệu tiến trình từ file JSON hoặc ScriptableObject.
        /// </summary>
        //public void LoadProgression()
        //{
        //    // ----- Bỏ qua save JSON, chỉ dùng SO trực tiếp ----- Lộc thêm vào để debug, xóa sau nhé, tại vì cái này dính tới phần save
        //    if (progressionDataSO != null)
        //    {
        //        progression = progressionDataSO.ToGameProgression();
        //        Debug.Log("Loaded progression trực tiếp từ ProgressionDataSO (bypass save).");
        //        return;
        //    }
        //    Debug.LogError("ProgressionDataSO bị null, không thể load progression!");


        //    if (saveGameManager == null)
        //    {
        //        Debug.LogError("SaveGameManager is not assigned!");
        //        return;
        //    }

        //    string userName = userAccountManager.CurrentUserBaseName;
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        Debug.LogWarning("CurrentUserNamePlaying is not set!");
        //        return;
        //    }

        //    string saveFolder = saveGameManager.GetLatestSaveFolder(userName);
        //    if (saveFolder != null)
        //    {
        //        string json = saveGameManager.LoadJsonFile(saveFolder, "playerProgression.json");
        //        if (!string.IsNullOrEmpty(json))
        //        {
        //            progression = JsonSerializationHelper.DeserializeGameProgression(json);
        //            Debug.Log($"Loaded player progression from: {saveFolder}/playerProgression.json");
        //            return;
        //        }
        //    }

        //    TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
        //    if (jsonText != null)
        //    {
        //        progression = JsonSerializationHelper.DeserializeGameProgression(jsonText.text);
        //        Debug.Log("Loaded progression from JSON: JSON/progressionData");
        //        string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
        //        saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", jsonText.text);
        //        Debug.Log($"Created new player progression file in: {newSaveFolder}");
        //        return;
        //    }

        //    if (progressionDataSO != null)
        //    {
        //        progression = progressionDataSO.ToGameProgression();
        //        Debug.Log("Loaded progression from ProgressionDataSO");
        //        string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
        //        string json = JsonSerializationHelper.SerializeGameProgression(progression);
        //        saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
        //        Debug.Log($"Created new player progression file in: {newSaveFolder}");
        //        return;
        //    }

        //    Debug.LogError("No progression data found!");
        //}

        /// <summary>
        /// Tạo một save game mới.
        /// </summary>
        //public void CreateNewGame()
        //{
        //    if (saveGameManager == null)
        //    {
        //        Debug.LogError("SaveGameManager is not assigned!");
        //        return;
        //    }

        //    string userName = userAccountManager.currentUserBaseName;
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        Debug.LogError("CurrentUserNamePlaying is not set!");
        //        return;
        //    }

        //    string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
        //    TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
        //    string json;
        //    if (jsonText != null)
        //    {
        //        json = jsonText.text;
        //        Debug.Log("Using default progression from JSON/progressionData");
        //    }
        //    else if (progressionDataSO != null)
        //    {
        //        progression = progressionDataSO.ToGameProgression();
        //        json = JsonSerializationHelper.SerializeGameProgression(progression);
        //        Debug.Log("Using default progression từ ProgressionDataSO");
        //    }
        //    else
        //    {
        //        Debug.LogError("No default progression data found!");
        //        return;
        //    }

        //    saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
        //    progression = JsonSerializationHelper.DeserializeGameProgression(json);
        //    Debug.Log($"Created new game save: {newSaveFolder}");
        //}

        /// <summary>
        /// Lưu dữ liệu tiến trình vào file JSON.
        /// </summary>
        //public void SaveProgression()
        //{
        //    if (saveGameManager == null)
        //    {
        //        Debug.LogError("SaveGameManager is not assigned!");
        //        return;
        //    }

        //    string userName = userAccountManager.currentUserBaseName;
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        Debug.LogError("CurrentUserNamePlaying is not set!");
        //        return;
        //    }

        //    string saveFolder = saveGameManager.GetLatestSaveFolder(userName) ?? saveGameManager.CreateNewSaveFolder(userName);
        //    string json = JsonSerializationHelper.SerializeGameProgression(progression);
        //    saveGameManager.SaveJsonFile(saveFolder, "playerProgression.json", json);
        //}

        #endregion


        #region === XỬ LÍ LOGIC PROCESSSION===

        /// <summary>
        /// Xây dựng eventSequence phẳng từ MainProcesses → SubProcesses theo Order.
        /// Gọi ngay sau khi LoadProgression(), để _eventSequence có thể dùng cho GetNextEventId().
        /// </summary>
        private void BuildEventSequence()
        {
            _eventSequence = new List<string>();

            if (progression == null || progression.MainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] progression hoặc MainProcesses là null khi BuildEventSequence!");
                return;
            }

            // Sort MainProcesses theo Order tăng dần
            var sortedMain = progression.MainProcesses.OrderBy(mp => mp.Order).ToList();
            foreach (var main in sortedMain)
            {
                if (main.SubProcesses == null) continue;

                // Sort SubProcesses theo Order rồi add ID vào sequence
                var sortedSubs = main.SubProcesses.OrderBy(sp => sp.Order).ToList();
                foreach (var sub in sortedSubs)
                {
                    _eventSequence.Add(sub.Id);
                }
            }

            Debug.Log($"[ProgressionManager] Event sequence built. Count = {_eventSequence.Count}");
        }

        /// <summary>
        /// Cấp phần thưởng cho một danh sách Reward.
        /// </summary>
        private void GrantRewards_List(List<Reward> rewards)
        {
            if (rewards == null) return;
            foreach (var reward in rewards)
            {
                GrantReward(reward);
            }
        }

        /// <summary>
        /// Cấp phần thưởng cho tiến trình con hoặc chính.
        /// </summary>
        private void GrantReward(Reward reward)
        {
            if (reward == null) return;

            if (reward is ItemReward itemReward)
            {
                if (itemReward.ItemType == "Loot")
                {
                    var loot = lootDatabase.GetLootConfig(itemReward.ItemName);
                    if (loot != null)
                        Debug.Log($"[ProgressionManager] Granted {itemReward.Amount} x {loot.Name} (Power:{loot.Power})");
                    else
                        Debug.LogWarning($"[ProgressionManager] Loot {itemReward.ItemName} not found!");
                }
                else
                {
                    Debug.LogWarning($"[ProgressionManager] Unknown ItemType: {itemReward.ItemType}");
                }
            }
            reward.Grant();
        }

        #endregion


        #region Lộc thêm vào để quản lý sự kiện

        /// <summary>
        /// Được gọi khi EventManager báo eventId vừa hoàn thành.
        /// Tại đây ta sẽ:
        ///  1) Đánh dấu SubProcess (hoặc MainProcess) tương ứng → Completed
        ///  2) Cấp Rewards nếu có
        ///  3) Nếu toàn bộ SubProcess của một MainProcess đã Complete, đánh dấu MainProcess → Completed và grant Main rewards
        ///  4) Tăng _currentEventIndex nếu eventId khớp đúng thứ tự phẳng trong _eventSequence
        ///  5) SaveProgression() để lưu lại tiến độ mới
        /// </summary>
        public void HandleEventFinished(string eventId)
        {
            Debug.Log($"[ProgressionManager] HandleEventFinished: {eventId}");

            if (progression == null || progression.MainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] progression hoặc MainProcesses là null khi HandleEventFinished!");
                return;
            }

            bool found = false;

            // 1) Tìm subProcess có Id == eventId
            foreach (var main in progression.MainProcesses)
            {
                var sub = main.SubProcesses.Find(s => s.Id == eventId);
                if (sub != null && sub.Status != MainProcess.ProcessStatus.Completed)
                {
                    // Đánh dấu sub → Completed
                    sub.Status = MainProcess.ProcessStatus.Completed;
                    Debug.Log($"[ProgressionManager] SubProcess '{eventId}' marked Completed.");

                    // Cấp reward cho sub
                    GrantRewards_List(sub.Rewards);

                    // 2) Kiểm tra xem tất cả sub của main đã xong chưa
                    if (main.SubProcesses.All(s2 => s2.Status == MainProcess.ProcessStatus.Completed))
                    {
                        main.Status = MainProcess.ProcessStatus.Completed;
                        Debug.Log($"[ProgressionManager] MainProcess '{main.Id}' marked Completed.");

                        // Cấp reward cho main
                        GrantRewards_List(main.Rewards);
                    }

                    found = true;
                    break;
                }
            }

            // 3) Nếu không tìm trong SubProcess, thử tìm thẳng MainProcess.Id == eventId
            if (!found)
            {
                var mainMatch = progression.MainProcesses.Find(m => m.Id == eventId);
                if (mainMatch != null && mainMatch.Status != MainProcess.ProcessStatus.Completed)
                {
                    mainMatch.Status = MainProcess.ProcessStatus.Completed;
                    Debug.Log($"[ProgressionManager] MainProcess '{eventId}' marked Completed (direct).");
                    GrantRewards_List(mainMatch.Rewards);
                    found = true;
                }
            }

            if (!found)
            {
                Debug.LogWarning($"[ProgressionManager] Không tìm thấy bất kỳ Main/Sub Process nào có ID = '{eventId}'");
            }

            // 4) Tăng currentEventIndex nếu eventId khớp đúng thứ tự
            if (_eventSequence != null &&
                _currentEventIndex < _eventSequence.Count &&
                _eventSequence[_currentEventIndex] == eventId)
            {
                _currentEventIndex++;
            }
            else if (_eventSequence != null)
            {
                int idx = _eventSequence.IndexOf(eventId);
                if (idx >= 0 && idx == _currentEventIndex)
                {
                    _currentEventIndex++;
                }
                else
                {
                    Debug.LogWarning($"[ProgressionManager] eventId '{eventId}' không khớp sequence tại index {_currentEventIndex} (expected: {_eventSequence.ElementAtOrDefault(_currentEventIndex)})");
                }
            }

            // 5) Lưu lại tiến trình
            SaveGameManager.Instance?.SaveNow();
        }

        /// <summary>
        /// Trả về eventId kế tiếp dựa trên thứ tự trong _eventSequence.
        /// Nếu đã hết, trả về null.
        /// </summary>
        public string GetNextEventId()
        {
            if (_eventSequence == null)
            {
                Debug.LogWarning("[ProgressionManager] _eventSequence is null when GetNextEventId()");
                return null;
            }

            if (_currentEventIndex >= 0 && _currentEventIndex < _eventSequence.Count)
                return _eventSequence[_currentEventIndex];
            return null;
        }

        #endregion


        #region === Các hàm nâng cao (Nếu muốn mở ra các quest phụ thì dùng 3 hàm dưới) ===
        ///<summary>
        ///  1. Sau khi hoàn thành một event nào đó cụ thể (không nằm chính trong thứ tự tuần tự),
        ///  gọi UnlockProcess("Quest_X") để làm SubProcess.Status = InProgress.
        ///
        ///  2. CheckProcessCompletion(string id, object data): Dùng trong trường hợp “SubProcess”
        ///  có kèm điều kiện phức tạp (ví dụ phải thu thập đủ item, phải giết đủ quái, kết hợp nhiều sự kiện).
        ///
        ///  3. Update ProcessStatus: Tính năng “mở khóa” (unlock) hay “đổi trạng thái” theo bất kỳ điều kiện
        /// nào ngoài việc event finished, thì gọi sẵn một hàm chung sẽ hạn chế viết code trùng lặp.
        /// 
        ///  Gọi “update trạng thái” (Lock, InProgress, Completed) của một process từ một chỗ khác
        /// (ví dụ UI cho phép tự set status, hoặc một luồng logic xảy ra không thông qua event manager),
        /// thì hàm helper giống UpdateProcessStatus_Internal(string id, ProcessStatus newStatus) khá hữu ích để tái sử dụng.
        /// </summary>
        

        /// <summary>
        /// Trả về dữ liệu tiến trình theo ID.
        /// </summary>
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

            Debug.LogWarning($"Process with ID '{id}' not found!");
            return null;
        }

        /// <summary>
        /// Mở khóa tiến trình theo ID.
        /// trả về true nếu mở khóa thành công.
        /// </summary>
        public bool UnlockProcess(string id)
        {
            var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Locked)
            {
                mainProcess.Status = MainProcess.ProcessStatus.InProgress;
                SaveGameManager.Instance?.SaveNow();
                Debug.Log($"[ProgressionManager] Unlocked MainProcess '{id}'");
                return true;
            }

            foreach (var main in progression.MainProcesses)
            {
                var subProcess = main.SubProcesses.Find(s => s.Id == id);
                if (subProcess != null && subProcess.Status == MainProcess.ProcessStatus.Locked)
                {
                    subProcess.Status = MainProcess.ProcessStatus.InProgress;
                    SaveGameManager.Instance?.SaveNow();
                    Debug.Log($"[ProgressionManager] Unlocked SubProcess '{id}'");
                    return true;
                }
            }

            Debug.LogWarning($"[ProgressionManager] Cannot unlock process '{id}': Not found or not locked!");
            return false;
        }

        /// <summary>
        /// Kiểm tra điều kiện hoàn thành tiến trình con và cấp phần thưởng nếu hoàn thành.
        /// trả về true nếu tiến trình con đã hoàn thành.
        /// </summary>
        public bool CheckProcessCompletion(string id, object data)
        {
            foreach (var main in progression.MainProcesses)
            {
                var subProcess = main.SubProcesses.Find(s => s.Id == id);
                if (subProcess != null && subProcess.Status != MainProcess.ProcessStatus.Completed)
                {
                    bool allConditionsMet = true;
                    if (subProcess.Conditions != null && subProcess.Conditions.Count > 0)
                    {
                        allConditionsMet = subProcess.Conditions.All(c => c.IsSatisfied(data));
                    }

                    if (allConditionsMet)
                    {
                        subProcess.Status = MainProcess.ProcessStatus.Completed;
                        GrantRewards_List(subProcess.Rewards);
                        Debug.Log($"[ProgressionManager] SubProcess '{id}' completed by conditions.");

                        if (main.SubProcesses.All(s2 => s2.Status == MainProcess.ProcessStatus.Completed))
                        {
                            main.Status = MainProcess.ProcessStatus.Completed;
                            GrantRewards_List(main.Rewards);
                            Debug.Log($"[ProgressionManager] MainProcess '{main.Id}' completed by sub-conditions.");
                        }

                        SaveGameManager.Instance?.SaveNow();
                        return true;
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Cho phép update trạng thái SubProcess hoặc MainProcess bằng enum string
        /// </summary>
        private bool UpdateProcessStatus_Internal(string id, MainProcess.ProcessStatus newStatus)
        {
            // Tìm MainProcess.Id == id
            var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null)
            {
                mainProcess.Status = newStatus;
                Debug.Log($"[ProgressionManager] Updated MainProcess {id} → {newStatus}");
                return true;
            }

            // Nếu không, tìm SubProcess.Id == id
            foreach (var main in progression.MainProcesses)
            {
                var subProcess = main.SubProcesses.Find(s => s.Id == id);
                if (subProcess != null)
                {
                    subProcess.Status = newStatus;
                    Debug.Log($"[ProgressionManager] Updated SubProcess {id} → {newStatus}");
                    return true;
                }
            }

            return false;
        }

        #endregion

    }
}
