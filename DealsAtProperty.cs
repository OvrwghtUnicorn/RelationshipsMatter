using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.UI.Phone.Messages;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ScheduleOneGame = Il2CppScheduleOne;

[assembly: MelonInfo(typeof(RelationshipsMatter.DealsAtProperty), RelationshipsMatter.BuildInfo.Name, RelationshipsMatter.BuildInfo.Version, RelationshipsMatter.BuildInfo.Author, RelationshipsMatter.BuildInfo.DownloadLink)]
[assembly: MelonColor(255, 191, 0, 255)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace RelationshipsMatter {

    public static class BuildInfo {
        public const string Name = "Deals at Property";
        public const string Description = "Properties become a location for drug deals";
        public const string Author = "OverweightUnicorn";
        public const string Company = "UnicornsCanMod";
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class DealsAtProperty : MelonMod {
        public static Il2CppAssetBundle ab;
        public static GameObject dealLocationSelectorPrefab = null;
        public static GameObject dealLocationSelectorButtonPrefab = null;

        public static Button button = null;
        public static DealLocationSelector dealLocationSelector = null;

        public static PlayerCamera cameraController = null;
        public static Customer currCustom = null;
        public static Dictionary<string, DeliveryLocation> LocationGuids = new Dictionary<string, DeliveryLocation>();

        public static Vector3 newBarbershopPosition = new Vector3(-75.1477f, 0.9662f, 110.768f);
        public static Quaternion newBarbershopRotation = new Quaternion(-0, 0.4201f, -0, 0.9075f);


        public override void OnInitializeMelon() {
            MelonLogger.Msg(System.ConsoleColor.DarkMagenta, "Loading Assets");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if(ab == null) {
                ab = Il2CppAssetBundleManager.LoadFromMemory(Assets.deallocatorassets);
            }
        }

        public override void OnLateInitializeMelon() {
            ScheduleOneGame.Persistence.LoadManager.Instance.onLoadComplete.AddListener((UnityAction)MakeDeliveryLocationsDict);
        }

        public static void InitializePlayerCamera() {
            //cameraController = GameObject.Find();
        }

        public static void MakeDeliveryLocationsDict() {
            GameObject messagesGo = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/Messages/Container");
            if (messagesGo != null) {
                MelonLogger.Msg("MessagesContainer is Loaded");

                try {
                    if (dealLocationSelectorPrefab == null) {
                        dealLocationSelectorPrefab = ab.LoadAsset<GameObject>("DealLocationSelector.prefab");
                        dealLocationSelectorPrefab.AddComponent<DealLocationSelector>();
                    } else {
                        throw new Exception("Failed to load dealLocationSelectorPrefab");
                    }

                    if (dealLocationSelectorButtonPrefab == null) {
                        dealLocationSelectorButtonPrefab = ab.LoadAsset<GameObject>("DealLocationButton.prefab");
                    } else {
                        throw new Exception("Failed to load dealLocationSelectorButtonPrefab");
                    }

                    GameObject locationSelector = GameObject.Instantiate(dealLocationSelectorPrefab);
                    locationSelector.transform.SetParent(messagesGo.transform, false);
                    if (dealLocationSelector == null) {
                        dealLocationSelector = locationSelector.GetComponent<DealLocationSelector>();
                    }
                    dealLocationSelector.ButtonSelectorPrefab = dealLocationSelectorButtonPrefab;
                    dealLocationSelector.Initialize();

                } catch (Exception e) {
                    MelonLogger.Msg(System.ConsoleColor.Red,e.Source);
                    MelonLogger.Msg(System.ConsoleColor.DarkRed,e.Message);
                }

                if (dealLocationSelector == null) return;

                dealLocationSelector.InitializeExitListener();

                GameObject deliveryLocations = null;
                try {
                    deliveryLocations = GameObject.Find("Delivery Locations");
                    MelonLogger.Msg("Got deliveryLocations");
                } catch (Exception exception) {
                    MelonLogger.Msg("Could not get deliveryLocations: " + exception);
                    return;
                }

                for (int i = 0; i < deliveryLocations.transform.childCount; i++) {
                    Transform child = deliveryLocations.transform.GetChild(i);
                    string locName = child.name;

                    DeliveryLocation location = child.GetComponent<DeliveryLocation>();
                    if (locName == "Next to barbershop") {
                        child.position = newBarbershopPosition;
                        child.rotation = newBarbershopRotation;
                        location.LocationDescription = "at the north end of the motel";
                        location.name = "North End of Motel";
                        location.LocationName = "North End of Motel";
                    }

                    //if (locName == "Next to barbershop") {
                    //    child.position = newBarbershopPosition;
                    //    child.rotation = newBarbershopRotation;
                    //    location.name = "North End of Motel";
                    //    location.LocationName = "North End of Motel";
                    //}

                    if (location != null) {
                        string name = location.LocationName;
                        string guid = location.GUID.ToString();

                        if (!LocationGuids.ContainsKey(guid)) {
                            LocationGuids.Add(guid, location);
                            dealLocationSelector.AddButton(location, child.position, DelegateSupport.ConvertDelegate<UnityAction>(() => ListItemClicked(guid)));
                        }
                    }
                }
            }
        }

        //public void CreateNewDealLocations() {
        //    GameObject dealLocationsGO = GameObject.Find("Delivery Locations");
        //    if (dealLocationsGO != null) {
        //        MelonLogger.Msg(System.ConsoleColor.DarkMagenta, "Found Delivery Locations");
        //        Transform dealLocTrans = dealLocationsGO.transform;

        //        GameObject cubeSpawnPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        cubeSpawnPoint.name = "Spawn Location";
        //        cubeSpawnPoint.transform.position = player.transform.position;
        //        cubeSpawnPoint.transform.SetParent(dealLocTrans, false);
        //        MelonLogger.Msg(System.ConsoleColor.Magenta, $"Cube {cubeSpawnPoint.name} created at {cubeSpawnPoint.transform.position.ToString()}");
        //    }
        //}

        [HarmonyPatch(typeof(MessagesApp), nameof(MessagesApp.Exit))]
        public static class MessagesApp_Exit_Patch {
            public static bool Prefix(ref ExitAction exit) {
                dealLocationSelector.Close();
                return true;
            }
        }

        //[HarmonyPatch(typeof(Player), nameof(Player.PlayerLoaded))]
        //public static class Player_PlayerLoaded_Patch {
        //    public static void Postfix() {
        //        MelonLogger.Msg("Player Loaded? Lets Check");
        //        //CreateNewDealLocations();
        //        GameObject messagesGo = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/Messages/Container");
        //        if (messagesGo != null) {
        //            MelonLogger.Msg("MessagesContainer is Loaded");
        //            var ab = Il2CppAssetBundleManager.LoadFromMemory(Assets.deallocatorassets);

        //            if (dealLocationSelectorPrefab == null) {
        //                dealLocationSelectorPrefab = ab.LoadAsset<GameObject>("DealLocationSelector.prefab");
        //            }

        //            if (dealLocationSelectorButtonPrefab == null) {
        //                dealLocationSelectorButtonPrefab = ab.LoadAsset<GameObject>("DealLocationButton.prefab");
        //            }         

        //            if (dealLocationSelector == null) {
        //                dealLocationSelector = GameObject.Instantiate(dealLocationSelectorPrefab);
        //                MelonLogger.Msg("Deal Location Instantiated");
        //                dealLocationSelector.transform.SetParent(messagesGo.transform,false);
        //                dealLocationSelector.SetActive(false);

        //                Transform scrollContent = dealLocationSelector.transform.Find("Shade/Content/ScrollContainer/ScrollArea");
        //                GameObject scrollAreaGo = (scrollContent != null) ? scrollContent.gameObject : null;

        //                if (scrollAreaGo != null) {
        //                    GameObject locationButton = GameObject.Instantiate(dealLocationSelectorButtonPrefab);
        //                    locationButton.transform.SetParent(scrollContent, false);
        //                    button = locationButton.GetComponent<Button>();
        //                    Text buttonText = locationButton.GetComponentInChildren<Text>();
        //                    buttonText.text = "Clicking Enabled";

        //                    for (int i = 0; i < 11; i++) {
        //                        GameObject temp = GameObject.Instantiate(dealLocationSelectorButtonPrefab);
        //                        temp.transform.SetParent(scrollContent, false);
        //                        Text tempText = temp.GetComponentInChildren<Text>();
        //                        tempText.text = $"Location {i + 1}";
        //                    }
        //                }

        //            }

        //        }
        //    }
        //}

        public static void ListItemClicked(string guid) {
            DeliveryLocation selectedLoc = LocationGuids[guid];
            if (selectedLoc != null) {
                MelonLogger.Msg($"Location selected! {selectedLoc.LocationName}");
                ContractInfo contractInfo = currCustom.OfferedContractInfo;
                if (contractInfo != null) { 
                    contractInfo.DeliveryLocation = selectedLoc;
                    contractInfo.DeliveryLocationGUID = guid;
                }
            }
            dealLocationSelector.Close();
            PlayerSingleton<MessagesApp>.Instance.DealWindowSelector.SetIsOpen(true, currCustom.NPC.MSGConversation, new Action<EDealWindow>(currCustom.PlayerAcceptedContract));
        }

        [HarmonyPatch(typeof(Customer), nameof(Customer.AcceptContractClicked))]
        public static class Customer_AcceptContractClicked_Patch {
            public static bool Prefix(Customer __instance) {
                currCustom = __instance;

                if (__instance.OfferedContractInfo == null) {
                    MelonLogger.Msg("Offered contract is null!");
                    return true;
                }

                if (dealLocationSelector == null) {
                    return true;
                }

                dealLocationSelector.Open();

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), nameof(PlayerCamera.AddActiveUIElement))]
        public static class PlayerCamera_AddActiveUIElement_Patch {
            public static bool Prefix(string name) {
                MelonLogger.Msg($"{name}");
                return true;
            }
        }

        [HarmonyPatch(typeof(MessagesApp), nameof(MessagesApp.SetCurrentConversation))]
        public static class MessagesApp_SetCurrentConversation_Patch {
            public static bool Prefix(MessagesApp __instance, ref MSGConversation conversation) {

                if (conversation != null) {
                    if (conversation.HasChanged) {
                        MelonLogger.Msg("Conversation changed");
                    }

                    if (conversation.isOpen) {
                        MelonLogger.Msg("Conversation has been opened");
                    } else {
                        MelonLogger.Msg("Conversation has been closed");
                    }
                }
                return true;
            }
        }

    }
}
