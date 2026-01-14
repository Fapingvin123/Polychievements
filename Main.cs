using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Polychievements;

public static class Main
{
    private static Dictionary<string, bool> unlockedDict;
    public static int AchViewMode = 0;

    public static List<Achievement> Achievements = new List<Achievement>()
    {
        /////////////////////////
        /// EASY
        /////////////////////////
        new Achievement(){
            idx = "agriculture",
            name = "ach.agriculture", // Implemented!
            description = "ach.agriculture.desc" // Build a farm
        },

        new Achievement(){
            idx = "veterans",
            name = "ach.veterans", // Implemented?
            description = "ach.veterans.desc" // Own 3 veterans
        },

        new Achievement(){
            idx = "blood",
            name = "ach.blood", // Implemented?
            description = "ach.blood.desc" // Eradicate a tribe.
        },

        new Achievement(){
            idx = "unseen",
            name = "ach.unseen", // Implemented?
            description = "ach.unseen.desc" // Have three unrevealed cloaks in enemy territory.
        },

        new Achievement(){
            idx = "win",
            name = "ach.win",
            description = "ach.win.desc" // Win a game.
        },

        new Achievement(){
            idx = "cartography",
            name = "Cartography",
            description = "Reveal all tiles in a Large map or bigger."
        },

        new Achievement(){
            idx = "cow",
            name = "City of Wonders", // Implemented!
            description = "Build 7 Monuments in a single city."
        },

        new Achievement(){
            idx = "dopilus",
            name = "All roads lead to Dopilus",
            hiddenDesc = true,
            description = "Playing as Imperius, have half of the worlds' cities connected to your capital."
        },

        new Achievement()
        {
            idx = "colony",
            name = "Colonizer",
            description = "Have at least 1 city from all other tribes on a Large map or bigger."
        },

        ////////////////////////////////////////
        /// MEDIUM
        ////////////////////////////////////////

        

        new Achievement(){
            idx = "yoink",
            name = "Yoink!",
            description = "Mindbend a giant.",
            category = 1
        },

        new Achievement(){
            idx = "green",
            name = "Green City",
            description = "Have 5 parks in a city you own.",
            category = 1
        },

        new Achievement(){
            idx = "hypno",
            name = "Hypnotist",
            hiddenDesc = true,
            description = "Convert a mindbender with a mindbender.",
            category = 1
        },

        new Achievement(){
            idx = "serious",
            name = "Serious Dedication",
            description = "Research Aquatism and Navigation on a Drylands map by turn 10.",
            category = 1
        },

        new Achievement(){
            idx = "crazy",
            name = "Crazy good!",
            description = "Win a Crazy difficulty game.",
            category = 1
        },

        new Achievement(){
            idx = "diplomat",
            name = "The Diplomat",
            description = "Have an embassy in at least 4 tribes' capitals on at least Normal difficulty.",
            category = 1
        },

        new Achievement(){
            idx = "terrorknight",
            name = "Knight of Terror",
            description = "Own a knight that has killed more than 10 units.",
            category = 1
        },

        new Achievement(){
            idx = "lifeashore",
            name = "Life Ashore",
            description = "Win a Waterworld or Archipelago map without researching Aquatism or Navigation on Normal or harder difficulty.",
            category = 1
        },

        /////////////////////////
        /// HARD
        /////////////////////////

        new Achievement(){
            idx = "howdidwe",
            name = "How did we get here?",
            description = "Have a Poisoned, Frozen, Speedy, Veteran unit.",
            category = 2
        },

        new Achievement(){
            idx = "superteam",
            name = "Super Team",
            description = "Own a Giant, a Crab, a Dragon, a Gaami and a Centipede.",
            category = 2
        },

        new Achievement(){
            idx = "houdini",
            name = "Houdini",
            description = "Kill a unit sieging your city by spawning a Super Unit in the city, with no available tiles for the attacker to move.",
            hiddenDesc = true,
            category = 2
        },

        new Achievement(){
            idx = "vengirwater",
            name = "Vengir Waterworld",
            description = "Win a Crazy difficulty Waterworld game with Vengir, playing against 15 bots.",
            category = 2
        },

        new Achievement(){
            idx = "welcomeneigh",
            name = "A welcomed neighbour",
            description = "Eradicate a tribe before turn 5.",
            category = 2
        },

        new Achievement(){
            idx = "atlantis",
            name = "Atlantis",
            hiddenDesc = true,
            description = "Have an island city reach level 10.",
            category = 2
        },

        new Achievement(){
            idx = "economist",
            name = "Economist",
            description = "Have 50 income before reaching turn 20 with at least 1 crazy AI opponent.",
            category = 2
        },

        new Achievement(){
            idx = "fakediplomat",
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
        if (GetAchievement(achievement.idx) != null)
        {
            modLogger.LogFatal("Error adding achievement at runtime! Index already exists, change description or name of achievement " + achievement.name);
            return false;
        }
        if (unlockedDict.TryGetValue(achievement.idx, out bool result)) achievement.unlocked = result;
        else achievement.unlocked = false;
        Achievements.Add(achievement);
        return true;
    }

    public static string GetAchievementIdx(string title)
    {
        foreach (var ach in Achievements)
        {
            if (ach.name == title)
            {
                return ach.idx;
            }
        }
        return string.Empty;
    }

    public static Achievement GetAchievement(string idx)
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

    public static int GetAchievementLocation(string idx)
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


    public static List<T> ToSystemList<T>(Il2CppSystem.Collections.Generic.List<T> il2cppList)
    {
        var sysList = new List<T>(il2cppList.Count);
        for (int i = 0; i < il2cppList.Count; i++)
        {
            sysList.Add(il2cppList[i]);
        }
        return sysList;
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
            ach.unlocked = unlockedDict[ach.idx];
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
        list.Insert(list.Count - 2, button);
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
            popup.Header = Localization.Get(achi.name);
            string unlockedis = achi.unlocked ? Localization.Get("ach.ui.unlocked") : Localization.Get("ach.ui.notunlocked");
            if (!(achi.hiddenDesc && !achi.unlocked)) popup.Description = Localization.Get(achi.description) + "\n\n" + unlockedis;
            else popup.Description = "???\n\n" + unlockedis;
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
                pop1.Header = Localization.Get("ach.ui.usure");
                pop1.Description = Localization.Get("ach.ui.usure2");
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
                    CreatePlayerButton(__instance, gameState, Localization.Get(ach.name), "achievement", GetAchievementLocation(ach.idx), Main.GetColor(ach), ref num);
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
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.easy");
            }
            else if (AchViewMode == 1)
            {
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.medium"); ;
            }
            else __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.hard"); ;
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
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.easy");
                __instance.Update();
            }
            void medium(int id, BaseEventData eventData)
            {
                Main.AchViewMode = 1;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.medium");
                __instance.Update();
            }
            void hard(int id, BaseEventData eventData)
            {
                Main.AchViewMode = 2;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.hard");
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PromoteAction), nameof(PromoteAction.ExecuteDefault))]
    public static void PromoteAction_ExecuteDefault(PromoteAction __instance, GameState state)
    {
        VeteranAcquired(state, __instance.Coordinates, __instance.PlayerId, "promote");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConvertAction), nameof(ConvertAction.ExecuteDefault))]
    public static void ConvertAction_ExecuteDefault(ConvertAction __instance, GameState gameState)
    {
        TileData tile = gameState.Map.GetTile(__instance.Target);
        if (TileHasVeteran(tile) == 1) VeteranAcquired(gameState, tile.coordinates, __instance.PlayerId, "convert");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExamineRuinsAction), nameof(ExamineRuinsAction.ExecuteDefault))]
    public static void ExamineRuinsAction_E(ExamineRuinsAction __instance, GameState gameState)
    {
        TileData tile = gameState.Map.GetTile(__instance.Coordinates);
        if (TileHasVeteran(tile) == 1) VeteranAcquired(gameState, tile.coordinates, __instance.PlayerId, "ruin");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WipePlayerAction), nameof(WipePlayerAction.ExecuteDefault))]
    public static void WipePlayerAction_ExecuteDefault(WipePlayerAction __instance, GameState state)
    {
        if (__instance.PlayerId == GameManager.LocalPlayer.Id) GrantAchievement(GetAchievement("blood"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HideAction), nameof(HideAction.Execute))]
    public static void HideAction_Execute(HideAction __instance, GameState state)
    {
        if (state.Map.GetTile(__instance.Coordinates).unit != null)
        {
            UnitHid(state, state.Map.GetTile(__instance.Coordinates).unit, __instance.PlayerId, "hide");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.ExecuteDefault))]
    public static void MoveAction_ExecuteDefault(MoveAction __instance, GameState gameState)
    {
        var success = gameState.TryGetUnit(__instance.UnitId, out UnitState unit);
        if (success)
        {
            if (unit.HasAbility(UnitAbility.Type.Hide)) UnitHid(gameState, unit, __instance.PlayerId, "hide");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameOverAction), nameof(GameOverAction.Execute))]
    public static void GameOverAction_Execute(GameOverAction __instance, GameState state)
    {
        if (__instance.WinningPlayerId == GameManager.LocalPlayer.Id)
        {
            Main.GrantAchievement(GetAchievement("win"));
            /*if (state.Settings.Difficulty == BotDifficulty.Crazy)
            {
                Main.GrantAchievement(GetAchievement("")); // crazy win index here
                if (state.Settings.mapPreset == MapPreset.WaterWorld && state.Settings.OpponentCount >= 15)
                {
                    Main.GrantAchievement(GetAchievement("")); // VengirWaterWorld here? Also implement Vengir check
                }
            }*/
        }
    }


    #endregion

    #region Common Events

    public static void UnitHid(GameState gameState, UnitState unit, byte player, string reason)
    {
        TileData tile = gameState.Map.GetTile(unit.coordinates);
        if (reason == "hide" && player == GameManager.LocalPlayer.Id)
        {
            if (!GetAchievement("unseen").unlocked)
            {
                int counter = 0;
                foreach (var tile1 in gameState.Map.Tiles)
                {
                    if(tile1.unit != null && (tile1.unit.type == UnitData.Type.Cloak || tile1.unit.type == UnitData.Type.MermaidCloak) && tile1.unit.HasEffect(UnitEffect.Invisible) && tile1.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES && tile1.owner != player)
                    {
                        counter++;
                    }
                }
                if (counter > 2) GrantAchievement(GetAchievement("unseen"));
            }
        }
    }

    public static void VeteranAcquired(GameState gameState, WorldCoordinates coordinates, byte player, string reason)
    {
        TileData tile = gameState.Map.GetTile(coordinates);
        List<TileData> maptiles = new();
        foreach (var tile1 in gameState.Map.Tiles)
        {
            maptiles.Add(tile1);
        }

        if (player == GameManager.LocalPlayer.Id)
        {
            if (!GetAchievement("veterans").unlocked && CounterThroughTiles(maptiles, TileHasVeteran) > 2)
            {
                GrantAchievement(GetAchievement("veterans"));
            }
        }
    }

    public static void ImprovementAcquired(GameState gameState, WorldCoordinates coordinates, byte player, ImprovementData.Type type, string reason)
    {
        TileData tile = gameState.Map.GetTile(coordinates);
        TileData citytile = gameState.Map.GetTile(tile.rulingCityCoordinates);

        if (player == GameManager.LocalPlayer.Id)
        {
            if (reason == "build")
            {
                if (type == ImprovementData.Type.Farm) GrantAchievement(GetAchievement("agriculture"));
                if (type.IsMonument() && CounterThroughTiles(ToSystemList(ActionUtils.GetCityArea(gameState, citytile)), TileHasMonument) > 6)
                {
                    GrantAchievement(GetAchievement("cow"));
                }
            }
        }
    }

    #endregion

    #region Event Utils

    public static int CounterThroughTiles(List<TileData> tiles, Func<TileData, int> filter)
    {
        int sum = 0;
        foreach (var tile in tiles)
        {
            sum += filter(tile);
        }
        return sum;
    }

    public static int CounterThroughTiles(List<WorldCoordinates> tiles, Func<TileData, int> filter)
    {
        List<TileData> tileList = new();
        foreach (var tile in tiles)
        {
            tileList.Add(GameManager.GameState.Map.GetTile(tile));
        }
        return CounterThroughTiles(tileList, filter);
    }

    public static int TileHasMonument(TileData tile)
    {
        if (tile.improvement != null && tile.improvement.IsMonument()) return 1;
        return 0;
    }

    public static int TileHasVeteran(TileData tile)
    {
        if (tile.unit != null && tile.unit.promotionLevel >= 1) return 1;
        return 0;
    }


    #endregion

    #region AchGranting
    public static void AchievementPopup(Achievement achievement)
    {
        NotificationManager.Notify(Localization.Get(achievement.description), Localization.Get(achievement.name), PolyMod.Registry.GetSprite("achievement"));
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
