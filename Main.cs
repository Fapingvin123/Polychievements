//using System.ComponentModel;
//using System.Globalization;
//using System.Runtime.CompilerServices;
using BepInEx.Logging;
//using EnumsNET;
using HarmonyLib;
//using Il2CppInterop.Runtime;
//using Il2CppInterop.Runtime.InteropTypes.Arrays;
//using Il2CppSystem;
//using Il2CppSystem.Linq.Expressions.Interpreter;
//using JetBrains.Annotations;
using Polytopia.Data;
//using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
//using SevenZip.Compression.LZMA;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
using UnityEngine;
//using UnityEngine.Tilemaps;
//using UnityEngine.UIElements.UIR;
//using System.Reflection;
using UnityEngine.EventSystems;
//using Il2CppMicrosoft.Extensions.DependencyInjection;

namespace test;



public class Achievement
{
    public int idx;
    public string name;
    public string description;
    public bool unlocked = false;
    public bool hiddenDesc = false;
    public int category = 0;
}

public static class PrefsHelper
{
    private const string Key = "Polychievements";
    public static Dictionary<int, bool> CreateDefault()
    {
        var dict = new Dictionary<int, bool>();
        for (int i = 0; i <= Main.Achievements.Count; i++)
            dict[i] = false;
        return dict;
    }

    public static void SaveDict(Dictionary<int, bool> dict)
    {
        var parts = new List<string>();
        foreach (var kvp in dict)
            parts.Add($"{kvp.Key}={(kvp.Value ? 1 : 0)}");

        string encoded = string.Join(";", parts);
        PlayerPrefs.SetString(Key, encoded);
        PlayerPrefs.Save();
    }

    public static Dictionary<int, bool> LoadDict()
    {
        if (!PlayerPrefs.HasKey(Key))
            return CreateDefault();

        var dict = CreateDefault();
        string encoded = PlayerPrefs.GetString(Key);

        foreach (var entry in encoded.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            var kv = entry.Split('=');
            if (kv.Length != 2)
                continue;

            var kStr = kv[0].Trim();
            var vStr = kv[1].Trim();

            if (!int.TryParse(kStr, out int key))
                continue;
            if (!int.TryParse(vStr, out int val))
                continue;

            if (key < 0 || key > Main.Achievements.Count)
                continue;

            dict[key] = val != 0;
        }

        return dict;
    }
}


public static class Main
{
    private static Dictionary<int, bool> unlockedDict;
    public static int AchViewMode = 0;

    public static List<Achievement> Achievements = new List<Achievement>()
    {
        /////////////////////////
        /// EASY
        /////////////////////////
        new Achievement(){
            idx = 0,
            name = "Agriculture",
            description = "Build a farm."
        },

        new Achievement(){
            idx = 1,
            name = "Experienced Army",
            description = "Own 3 veteran units."
        },

        new Achievement(){
            idx = 2,
            name = "Blood!",
            description = "Eradicate a tribe."
        },

        new Achievement(){
            idx = 3,
            name = "We Move Unseen",
            description = "Have three unrevealed cloaks in enemy territory.",
            category = 0
        },

        new Achievement(){
            idx = 4,
            name = "First Win!",
            description = "Win a game."
        },

        new Achievement(){
            idx = 5,
            name = "Cartography",
            description = "Reveal all tiles in a Large map or bigger."
        },

        new Achievement(){
            idx = 6,
            name = "City of Wonders",
            description = "Build all Monuments in a single city."
        },

        new Achievement(){
            idx = 7,
            name = "All roads lead to Dopilus",
            hiddenDesc = true,
            description = "Playing as Imperius, have half of the worlds' cities connected to your capital."
        },

        new Achievement()
        {
            idx = 8,
            name = "Colonizer",
            description = "Have at least 1 city from all other tribes on a Large map or bigger."
        },

        ////////////////////////////////////////
        /// MEDIUM
        ////////////////////////////////////////

        

        new Achievement(){
            idx = 9,
            name = "Yoink!",
            description = "Mindbend a giant.",
            category = 1
        },

        new Achievement(){
            idx = 10,
            name = "Green City",
            description = "Have 5 parks in a city you own.",
            category = 1
        },

        new Achievement(){
            idx = 11,
            name = "Hypnotist",
            hiddenDesc = true,
            description = "Convert a mindbender with a mindbender.",
            category = 1
        },

        new Achievement(){
            idx = 12,
            name = "Serious Dedication",
            description = "Research Aquatism and Navigation on a Drylands map by turn 10.",
            category = 1
        },

        new Achievement(){
            idx = 13,
            name = "Crazy good!",
            description = "Win a Crazy difficulty game.",
            category = 1
        },

        new Achievement(){
            idx = 14,
            name = "The Diplomat",
            description = "Have an embassy in at least 4 tribes' capitals on at least Normal difficulty.",
            category = 1
        },

        new Achievement(){
            idx = 15,
            name = "Knight of Terror",
            description = "Own a knight that has killed more than 10 units.",
            category = 1
        },

        new Achievement(){
            idx = 16,
            name = "Life Ashore",
            description = "Win a Waterworld or Archipelago map without researching Aquatism or Navigation on Normal or harder difficulty.",
            category = 1
        },

        /////////////////////////
        /// HARD
        /////////////////////////

        new Achievement(){
            idx = 17,
            name = "How did we get here?",
            description = "Have a Poisoned, Frozen, Speedy, Veteran unit.",
            category = 2
        },

        new Achievement(){
            idx = 18,
            name = "Super Team",
            description = "Own a Giant, a Crab, a Dragon, a Gaami and a Centipede.",
            category = 2
        },

        new Achievement(){
            idx = 19,
            name = "Houdini",
            description = "Kill a unit sieging your city by spawning a Super Unit in the city, with no available tiles for the attacker to move.",
            hiddenDesc = true,
            category = 2
        },

        new Achievement(){
            idx = 20,
            name = "Vengir Waterworld",
            description = "Win a Crazy difficulty Waterworld game with Vengir, playing against 15 bots.",
            category = 2
        },

        new Achievement(){
            idx = 21,
            name = "A welcomed neighbor",
            description = "Eradicate a tribe before turn 5.",
            category = 2
        },

        new Achievement(){
            idx = 22,
            name = "Atlantis",
            hiddenDesc = true,
            description = "Have an island city reach level 10.",
            category = 2
        },

        new Achievement(){
            idx = 23,
            name = "Economist",
            description = "Have 50 income before reaching turn 20 with at least 1 crazy AI opponent.",
            category = 2
        },

        new Achievement(){
            idx = 24,
            name = "The \"Diplomat\"",
            description = "Gain 50 stars in turn from infiltrating enemy cities.",
            category = 2
        }
    };

    #region Utils

    /// <summary>
    /// Registers an achievement safely at runtime. Returns success.
    /// </summary>
    /// <param name="achievement"></param>
    public static bool AddAchievementRunTime(Achievement achievement)
    {
        achievement.idx = string.Concat(achievement.name, achievement.description).GetHashCode(); //done so that no matter the order, indexing stays consistent
        if(GetAchievement(achievement.idx) != null)
        {
            modLogger.LogFatal("Error adding achievement at runtime! Index already exists, change description or name of achievement "+achievement.name);
            return false;
        }
        if(unlockedDict.TryGetValue(achievement.idx, out bool result)) achievement.unlocked = result;
        else achievement.unlocked = false;
        Achievements.Add(achievement);
        return true;
    }

    public static int GetAchievementIdx(string title)
    {
        foreach (var ach in Achievements)
        {
            if (ach.name == title)
            {
                return ach.idx;
            }
        }
        return -1;
    }

    public static Achievement GetAchievement(int idx)
    {
        foreach (var ach in Achievements)
        {
            if (ach.idx == idx)
            {
                return ach;
            }
        }
        return null;
    }

    public static Achievement GetAchievement(string title)
    {
        return Achievements[GetAchievementLocation(GetAchievementIdx(title))];
    }
    public static int GetAchievementLocation(int idx) // Ideally return value is the same as input value but better be safe
    {
        for (int i = 0; i < Achievements.Count; i++)
        {
            if (Achievements[i].idx == idx)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion





    public static ManualLogSource modLogger;
    public static void Load(ManualLogSource logger)
    {

        Harmony.CreateAndPatchAll(typeof(Main));
        modLogger = logger;
        logger.LogMessage("Polychievements.dll loaded");
        unlockedDict = PrefsHelper.LoadDict();

        foreach (var ach in Achievements)
        {
            Achievements[ach.idx].unlocked = unlockedDict[ach.idx];
        }

    }
    #region UI

    public static int GetColor(Achievement achievement)
    {
        var gld = GameManager.GameState.GameLogicData;

        if (achievement.unlocked)
        {
            return gld.GetTribeColor(PolytopiaBackendBase.Common.TribeType.Kickoo, PolytopiaBackendBase.Common.SkinType.Default);
        }
        else return gld.GetTribeColor(PolytopiaBackendBase.Common.TribeType.Xinxi, PolytopiaBackendBase.Common.SkinType.Default);
    }



    // Generously stolen from klipi
    internal static void AddUiButtonToArray(UIRoundButton prefabButton, HudScreen hudScreen, UIButtonBase.ButtonAction action, UIRoundButton[] buttonArray, string? description = null)
    {
        UIRoundButton button = UnityEngine.GameObject.Instantiate(prefabButton, prefabButton.transform);
        button.transform.parent = hudScreen.buttonBar.transform;
        button.OnClicked += action;
        List<UIRoundButton> list = buttonArray.ToList();
        list.Insert(list.Count-2, button);
        list.ToArray();

        if (description != null)
        {
            Transform child = button.gameObject.transform.Find("DescriptionText");

            if (child != null)
            {
                TMPLocalizer localizer = child.gameObject.GetComponent<TMPLocalizer>();
                localizer.Text = description;
            }
            else
            {
            }
        }
    }



    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudButtonBar), nameof(HudButtonBar.Init))]
    internal static void HudButtonBar_Init(HudButtonBar __instance, HudScreen hudScreen)
    {
        if (true && !(GameManager.Instance.isLevelLoaded && GameManager.GameState.Settings.BaseGameMode == GameMode.Custom))
        {
            AddUiButtonToArray(__instance.menuButton, __instance.hudScreen, (UIButtonBase.ButtonAction)MenuButtonOnClicked, __instance.buttonArray, "Achievements");
            //__instance.nextTurnButton.gameObject.SetActive(false);
            //__instance.techTreeButton.gameObject.SetActive(false);
            //__instance.statsButton.gameObject.SetActive(false);
            __instance.Show();
            __instance.Update();
            // __instance.buttonBar.statsButton.BlockButton = true;
            void MenuButtonOnClicked(int id, BaseEventData eventdata)
            {

                hudScreen.replayInterface.SetData(GameManager.GameState);
                hudScreen.replayInterface.timeline.gameObject.SetActive(false);
                hudScreen.replayInterface.ShowViewModePopup();
            }
        }
    }

    internal static void CreatePlayerButton(SelectViewmodePopup viewmodePopup, GameState gameState, string header, string spriteName, int type, int color, ref float num)
    {
        UIRoundButton playerButton = GameObject.Instantiate<UIRoundButton>(viewmodePopup.buttonPrefab, viewmodePopup.gridLayout.transform);
        playerButton.id = (int)type;
        playerButton.rectTransform.sizeDelta = new Vector2(56f, 56f);
        playerButton.Outline.gameObject.SetActive(false);
        playerButton.BG.color = ColorUtil.SetAlphaOnColor(ColorUtil.ColorFromInt(color), 1f);
        playerButton.text = header[0].ToString().ToUpper() + header.Substring(1);
        playerButton.SetIconColor(Color.white);
        playerButton.ButtonEnabled = true;
        playerButton.OnClicked = (UIButtonBase.ButtonAction)OnClimateButtonClicked;
        void OnClimateButtonClicked(int id, BaseEventData eventData)
        {
            BasicPopup popup = PopupManager.GetBasicPopup();
            var achi = Achievements[id];
            popup.Header = achi.name;
            string unlockedis = achi.unlocked ? "Unlocked." : "Not unlocked.";
            if (!(achi.hiddenDesc && !achi.unlocked)) popup.Description = achi.description + "\n\n" + unlockedis;
            else popup.Description = "???\n\nUnlocked: " + achi.unlocked.ToString();
            List<PopupBase.PopupButtonData> popupButtons = new()
            {
                new("buttons.ok")
            };
            if (achi.unlocked)
            {
                popupButtons.Insert(0, new PopupBase.PopupButtonData("buttons.ungrant", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)ungrant, -1, false, customColorStates: ColorConstants.redButtonColorStates));
            }
            void ungrant(int id, BaseEventData eventData)
            {
                BasicPopup pop1 = PopupManager.GetBasicPopup();
                pop1.Header = "Are you sure?";
                pop1.Description = "Do you really want to reset this achievement? You would have to earn it again to have it unlocked!";
                List<PopupBase.PopupButtonData> popupButtons1 = new()
                {
                    new("buttons.nevermindachi"),
                    new PopupBase.PopupButtonData("buttons.absolutely", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)absolutely, -1, true, customColorStates: ColorConstants.redButtonColorStates)
                };

                void absolutely(int id, BaseEventData baseEventData)
                {
                    Achievements[Main.GetAchievementLocation(achi.idx)].unlocked = false;
                    unlockedDict[achi.idx] = false;
                    PrefsHelper.SaveDict(unlockedDict);
                    popup.Hide();
                    viewmodePopup.Hide();
                }

                pop1.Show();
                pop1.buttonData = popupButtons1.ToArray();
            }
            popup.buttonData = popupButtons.ToArray();
            popup.Show();
        }
        playerButton.SetSprite(PolyMod.Registry.GetSprite(spriteName));
        viewmodePopup.buttons.Add(playerButton);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SelectViewmodePopup), nameof(SelectViewmodePopup.SetData))]
    internal static bool SelectViewmodePopup_SetData(SelectViewmodePopup __instance, GameState gameState)
    {
        if (true)
        {
            __instance.ClearButtons();
            __instance.buttons = new Il2CppSystem.Collections.Generic.List<UIRoundButton>();
            float num = 0f;
            foreach (var ach in Achievements)
            {
                if (ach.category == AchViewMode)
                {
                    CreatePlayerButton(__instance, gameState, ach.name, "achievement", ach.idx, Main.GetColor(ach), ref num);
                }
            }
            __instance.gridLayout.spacing = new Vector2(__instance.gridLayout.spacing.x, num + 30f);
            __instance.gridLayout.padding.bottom = Mathf.RoundToInt(num + 30f);
            __instance.gridBottomSpacer.minHeight = num + 50f;
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SelectViewmodePopup), nameof(SelectViewmodePopup.OnPlayerButtonClicked))]
    private static bool SelectViewmodePopup_OnPlayerButtonClicked(SelectViewmodePopup __instance, int id, BaseEventData eventData)
    {
        __instance.SetSelectedButton(id);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReplayInterface), nameof(ReplayInterface.ShowViewModePopup))]
    private static bool ReplayInterface_ShowViewModePopup(ReplayInterface __instance)
    {
        if (true)
        {
            if (__instance.selectViewmodePopup != null && __instance.selectViewmodePopup.IsShowing())
            {
                return false;
            }
            __instance.selectViewmodePopup = PopupManager.GetSelectViewmodePopup();
            if (AchViewMode == 0)
            {
                __instance.selectViewmodePopup.Header = "Easy Achievements";
            }
            else if (AchViewMode == 1)
            {
                __instance.selectViewmodePopup.Header = "Medium Achievements";
            }
            else __instance.selectViewmodePopup.Header = "Hard Achievements";
            __instance.selectViewmodePopup.SetData(GameManager.GameState);
            __instance.selectViewmodePopup.buttonData = new PopupBase.PopupButtonData[]
            {
                new PopupBase.PopupButtonData("buttons.easyach", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)easy, -1, false, null),
                new PopupBase.PopupButtonData("buttons.mediumach", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)medium, -1, false, null),
                new PopupBase.PopupButtonData("buttons.hardach", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)hard, -1, false, null),
                new PopupBase.PopupButtonData("buttons.exit", PopupBase.PopupButtonData.States.None, (UIButtonBase.ButtonAction)exit, -1, true, null)
            };
            void exit(int id, BaseEventData eventData)
            {
                __instance.CloseViewModePopup();
            }
            void easy(int id, BaseEventData eventData)
            {
                Main.AchViewMode = 0;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = "Easy Achievements";
                __instance.Update();
            }
            void medium(int id, BaseEventData eventData)
            {
                Main.AchViewMode = 1;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = "Medium Achievements";
                __instance.Update();
            }
            void hard(int id, BaseEventData eventData)
            {
                Main.AchViewMode = 2;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = "Hard Achievements";
                __instance.Update();
            }
            __instance.selectViewmodePopup.Show(__instance.viewmodeSelectButton.rectTransform.position);
        }
        return false;
    }



    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReplayInterface), nameof(ReplayInterface.UpdateButton))]
    internal static bool ReplayInterface_UpdateButton(ReplayInterface __instance)
    {
        if (true)
        {
            __instance.viewmodeSelectButton.rectTransform.sizeDelta = new Vector2(75f, 75f);
            __instance.viewmodeSelectButton.iconSpriteHandle.SetCompletion((SpriteHandleCallback)TribeSpriteHandle);
            GameLogicData gameLogicData = GameManager.GameState.GameLogicData;
            void TribeSpriteHandle(SpriteHandle spriteHandleCallback)
            {
                __instance.viewmodeSelectButton.SetFaceIcon(spriteHandleCallback.sprite);
            }
            __instance.viewmodeSelectButton.Outline.gameObject.SetActive(false);
        }
        return false;
    }

    #endregion

    #region Common Triggers
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildAction), nameof(BuildAction.ExecuteDefault))]
    public static void BuildAction_ExecuteDefault(BuildAction __instance, GameState gameState)
    {
        ImprovementAcquired(gameState, __instance.Coordinates, __instance.PlayerId, __instance.Type, "build");
    }


    #endregion

    #region Common Events

    public static void ImprovementAcquired(GameState state, WorldCoordinates coordinates, byte player, ImprovementData.Type type, string reason)
    {
        if(player == GameManager.LocalPlayer.Id)
        {
            if(reason == "build")
            {
                if(type == ImprovementData.Type.Farm) GrantAchievement(GetAchievement("Agriculture"));
            }
        }
    }

    #endregion

    #region AchGranting
    public static void AchievementPopup(Achievement achievement)
    {
        NotificationManager.Notify(achievement.description, achievement.name, PolyMod.Registry.GetSprite("achievement"));
    }
    public static void GrantAchievement(Achievement achievement)
    {
        if (achievement.unlocked)
        {
            return;
        }
        AchievementPopup(achievement);
        AudioManager.PlaySFX(SFXTypes.Achievement, PolytopiaBackendBase.Common.SkinType.Default, 1f, 1f, 1f);

        achievement.unlocked = true;
        unlockedDict[achievement.idx] = true;
        PrefsHelper.SaveDict(unlockedDict);
    }
    #endregion
}
