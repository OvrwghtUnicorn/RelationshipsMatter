using MelonLoader;
using UnityEngine;
using Il2CppScheduleOne;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.UI.Phone;
using UnityEngine.UI;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Map;
using Il2CppScheduleOne.Economy;
using UnityEngine.Events;
using Il2CppInterop.Runtime;
using UnityEngine.EventSystems;
using Il2CppFishNet;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppSystem.Runtime.InteropServices;

namespace RelationshipsMatter {
    public struct LocationData {
        public DeliveryLocation Location;
        public GameObject ButtonGo;
        public string Title;
        public Vector3 Position;
    }

    [RegisterTypeInIl2Cpp]
    public class DealLocationSelector : MonoBehaviour {

        public GameObject playerRef;
        public GameObject ButtonSelectorPrefab;
        public GameObject contentHolder;
        public InputField searchInput;
        public int totalElements;
        public List<LocationData> deliveryLocations = new List<LocationData>();
        public HashSet<string> buttonListSet = new HashSet<string>();
        public bool IsOpen = false;

        // Not using start cause it wouldn't get called when the gameobject was instantiated
        public void Initialize() {
            playerRef = Player.GetPlayer(InstanceFinder.ClientManager.Connection).gameObject;
            InitializeButtons();
            InitializeContentHolder();
            InitializeSearchField();
            this.gameObject.SetActive(false);
            this.IsOpen = false;
        }

        public void InitializeExitListener() {
            try {
                Action<ExitAction> della = this.Exit;
                var controlledDelegate = DelegateSupport.ConvertDelegate<GameInput.ExitDelegate>(della);
                GameInput.RegisterExitListener(controlledDelegate, 4);
            } catch (Exception e) {
                MelonLogger.Msg("Failed to register delegate");
            }
        }

        private void InitializeSearchField() {
            searchInput = this.transform.GetComponentInChildren<InputField>();
            if (searchInput != null) {
                searchInput.onValueChanged.AddListener((UnityAction<string>)Search);
                searchInput.caretWidth = 2;
            }
        }

        private void InitializeContentHolder() {
            contentHolder = this.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
        }

        private void InitializeButtons() {
            Button[] buttons = this.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons) {
                if (btn.name == "CloseButton") {
                    btn.onClick.AddListener((UnityAction)Close);
                    continue;
                }

                if (btn.name == "SortAZ") {
                    btn.onClick.AddListener((UnityAction)SortChildrenAz);
                    continue;
                }

                if (btn.name == "SortZA") {
                    btn.onClick.AddListener((UnityAction)SortChildrenZa);
                    continue;
                }

                if (btn.name == "Closest") {
                    btn.onClick.AddListener((UnityAction)SortChildrenDistDesc);
                    continue;
                }
            }
        }

        public void Open() {
            this.IsOpen = true;
            this.gameObject.SetActive(true);
            this.searchInput.ActivateInputField();
            this.searchInput.text = "";
            GameInput.IsTyping = true;
            foreach (LocationData ld in deliveryLocations){
                DeliveryLocation loc = ld.Location;
                if (loc != null) { 
                    if(loc.ScheduledContracts.Count == 0) {
                        ld.ButtonGo.SetActive(true);
                    } else {
                        ld.ButtonGo.SetActive(false);
                    }
                }
            }
        }

        public void Close() {
            this.IsOpen = false;
            this.gameObject.SetActive(false);
            GameInput.IsTyping = false;
        }

        public void AddButton(DeliveryLocation location, Vector3 locationPosition, UnityAction handleClick) {
            if (buttonListSet.Contains(location.LocationName)) {
                MelonLogger.Msg($"{location.LocationName} already exists in the list");
                return;
            }

            if (this.ButtonSelectorPrefab != null) {
                GameObject listButton = GameObject.Instantiate(this.ButtonSelectorPrefab);
                listButton.transform.SetParent(contentHolder.transform, false);
                Button listBtn = listButton.GetComponent<Button>();
                listBtn.onClick.AddListener(handleClick);
                Text btnText = listButton.GetComponentInChildren<Text>();
                btnText.text = location.LocationName;
                LocationData ld = new LocationData {
                    Location = location,
                    ButtonGo = listButton,
                    Title = location.LocationName,
                    Position = locationPosition,
                };
                deliveryLocations.Add(ld);
            }
        }
        void Search(string text) {
            foreach (LocationData ld in deliveryLocations) {
                if (ld.Title != null && ld.Title.ToLower().Contains(text) && ld.Location.ScheduledContracts.Count == 0) {
                    ld.ButtonGo.SetActive(true);
                } else {
                    ld.ButtonGo.SetActive(false);
                }
            }
        }

        void SortChildrenAz() {
            Comparison<LocationData> sortAscending = (a, b) => {
                return string.Compare(a.Title, b.Title);
            };
            Utility.MergeSortUI(deliveryLocations, sortAscending);
        }

        void SortChildrenZa() {
            Comparison<LocationData> sortDescending = (a, b) => {
                return string.Compare(b.Title, a.Title);
            };
            Utility.MergeSortUI(deliveryLocations, sortDescending);
        }

        void SortChildrenDistDesc() {
            Comparison<LocationData> sortDescending = (a, b) => {
                Vector3 playerPos = playerRef.transform.position;
                float aDistToPlayer = Vector3.Distance(a.Position,playerPos);
                float bDistToPlayer = Vector3.Distance(b.Position, playerPos);

                return Mathf.RoundToInt(aDistToPlayer - bDistToPlayer);
            };
            Utility.MergeSortUI(deliveryLocations, sortDescending);
        }

        void SortChildrenDistAsc() {
            Comparison<LocationData> sortAscending = (a, b) => {
                Vector3 playerPos = playerRef.transform.position;
                float aDistToPlayer = Vector3.Distance(a.Position, playerPos);
                float bDistToPlayer = Vector3.Distance(b.Position, playerPos);

                return Mathf.RoundToInt(bDistToPlayer - aDistToPlayer);
            };
            Utility.MergeSortUI(deliveryLocations, sortAscending);
        }

        public void Exit(ExitAction action) {
            if (action.Used) {
                return;
            }
            if (!this.IsOpen) {
                return;
            }
            action.Used = true;
            this.Close();
        }
    }
}
