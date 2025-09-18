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
using Newtonsoft.Json;


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
    private const string Key = "MyDict";

    // Create default dict {0:false, 1:false, ..., 12:false}
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
            return CreateDefault();  // no save yet → default

        var dict = CreateDefault(); // start with default (so missing keys still exist)
        string encoded = PlayerPrefs.GetString(Key);

        foreach (var entry in encoded.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            var kv = entry.Split('=');
            if (kv.Length != 2)
                continue; // skip malformed

            // trim whitespace
            var kStr = kv[0].Trim();
            var vStr = kv[1].Trim();

            if (!int.TryParse(kStr, out int key))
                continue; // skip if key not int
            if (!int.TryParse(vStr, out int val))
                continue; // skip if value not int

            // only accept keys in 0..MaxKey
            if (key < 0 || key > Main.Achievements.Count)
                continue;

            dict[key] = val != 0; // any non-zero → true
            //Main.Achievements[key].unlocked = val != 0; Does not work
            //Main.modLogger.LogMessage("Did some stuff "+key+(val != 0).ToString());
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
            name = "How did we get here?",
            description = "Have a unit with all four status effects at once (frozen, poisoned, boosted, veteran).",
            category = 2
        },

        new Achievement(){
            idx = 3,
            name = "Yoink!",
            description = "Mindbend a giant.",
            category = 1
        },

        new Achievement(){
            idx = 4,
            name = "Green City",
            description = "Have 5 parks in a city you own.",
            category = 1
        },

        new Achievement(){
            idx = 5,
            name = "Blood!",
            description = "Eradicate a tribe."
        },

        new Achievement(){
            idx = 6,
            name = "The Hoodrick Hood",
            hiddenDesc = true,
            description = Decipher("Hpg fhkx matg " + DecryptInt(214748354).ToString() + " ngbml, tee tkvaxkl.", 19),
            category = 1
        },

        new Achievement(){
            idx = 7,
            name = "Serious Dedication",
            description = "Research Aquatism and Navigation on a Drylands map by turn 10.",
            category = 1
        },

        new Achievement(){
            idx = 8,
            name = "Super Team",
            description = "Own a Giant, a Crab, a Dragon, a Gaami and a Centipede.",
            category = 2
        },

        new Achievement(){
            idx = 9,
            name = "Master Genius",
            description = "Build the Tower of Wisdom on at least Normal Difficulty.",
            category = 0
        },

        new Achievement(){
            idx = 10,
            name = "The Diplomat",
            description = "Have an embassy in at least 5 tribes' capitals on Crazy difficulty.",
            category = 1
        },

        new Achievement(){
            idx = 11,
            name = "The Great Wall",
            //Playing as Xin-Xi, put a defender on all of your border tiles, while having at least 3 cities, having chosen Walls in each of your cities.
            description = Decipher("Ietrbgz tl Qbg-qb, inm t wxyxgwxk hg tee hy rhnk uhkwxk mbexl, pabex atobgz tm extlm "+ DecryptInt(214748361).ToString() + " vbmbxl, atobgz vahlxg 'Pteel' bg xtva hy maxf.", 19),
            hiddenDesc = true,
            category = 2
        },

        new Achievement(){
            idx = 12,
            name = "First Win!",
            description = "Win a game."
        },

        new Achievement(){
            idx = 13,
            name = "Crazy good!",
            description = "Win a Crazy difficulty game.",
            category = 1
        },

        new Achievement(){
            idx = 14,
            name = "Vengir Waterworld",
            description = "Win a Crazy difficulty Waterworld game with Vengir, playing against 15 bots.",
            category = 2
        },

        new Achievement(){
            idx = 15,
            name = "A welcomed neighbor",
            description = "Eradicate a tribe by turn 5.",
            category = 2
        },

        new Achievement(){
            idx = 16,
            name = "Bug Math",
            hiddenDesc = true,
            // Own a centipede with 1 segment, one with 2 segments, one with 3 segments, one with 4 segments and one with 5 segments strictly.
            description = Decipher("Hpg t vxgmbixwx pbma 1 lxzfxgm, hgx pbma 2 lxzfxgml, hgx pbma 3 lxzfxgml, hgx pbma 4 lxzfxgml tgw hgx pbma 5 lxzfxgml lmkbvmer.", 19),
            category = 2
        },

        new Achievement(){
            idx = 17,
            name = "Economist",
            description = "Have 50 income before reaching turn 20 with at least 1 crazy AI opponent.",
            category = 2
        },
    };

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

    static int farmingidx = GetAchievementLocation(0);
    static int veteranidx = GetAchievementLocation(1);
    static int howdidweidx = GetAchievementLocation(2);
    static int yoinkidx = GetAchievementLocation(3);
    static int parkidx =GetAchievementLocation(4);
    static int wipeidx = GetAchievementLocation(5);
    static int secret1idx = GetAchievementLocation(6);
    static int sdachidx = GetAchievementLocation(7); //s(erious) d(edication) ach(ievement) i(n)d(e)x
    static int stachidx = GetAchievementLocation(8); //super team achievement idx
    static int geniusidx = GetAchievementLocation(9);
    
    
    
    
    
    
    


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

        if (Achievements.Count < -1)
        {
            modLogger.LogMessage("IMPORTANT!!!");
            modLogger.LogMessage("Dear People who watch this code!");
            modLogger.LogMessage("This log will not appear in the game, but can be viewed by you hopefully!");
            modLogger.LogMessage("Some achievements are secret, and not wanting to spoil the fun for y'all if y'all want to play this, I decided to try hiding these.");
            modLogger.LogMessage("Of course, you can find the hidden achievements' descriptions if you want to, but hopefully you won't accidentally.");
            modLogger.LogMessage("Main.Decipher decodes encrypted strings thus.");
        }

    }

    public static char cipher(char ch, int key)
    {
        if (!char.IsLetter(ch))
        {

            return ch;
        }

        char d = char.IsUpper(ch) ? 'A' : 'a';
        return (char)((((ch + key) - d) % 26) + d);


    }


    public static string Encipher(string input, int key)
    {
        string output = string.Empty;

        foreach (char ch in input)
            output += cipher(ch, key);

        return output;
    }

    public static string Decipher(string input, int key)
    {
        return Encipher(input, 26 - key);
    }

    public static int DecryptInt(int input)
    {
        return 214748364 % input;
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

    public static void AchievementPopup(Achievement achievement)
    {
        NotificationManager.Notify(achievement.description, achievement.name, PolyMod.Registry.GetSprite("achievement"));
    }

    // Generously stolen from klipi
    internal static void AddUiButtonToArray(UIRoundButton prefabButton, HudScreen hudScreen, UIButtonBase.ButtonAction action, UIRoundButton[] buttonArray, string? description = null)
    {
        UIRoundButton button = UnityEngine.GameObject.Instantiate(prefabButton, prefabButton.transform);
        button.transform.parent = hudScreen.buttonBar.transform;
        button.OnClicked += action;
        List<UIRoundButton> list = buttonArray.ToList();
        list.Add(button);
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
            if (!(achi.hiddenDesc && !achi.unlocked)) popup.Description = achi.description + "\n\n"+unlockedis;
            else popup.Description = "???\n\nUnlocked: " + achi.unlocked.ToString();
            List<PopupBase.PopupButtonData> popupButtons = new()
            {
                new("buttons.ok")
            };
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
            // __instance.selectViewmodePopup.Header = Localization.Get("replay.viewmode.header", new Il2CppSystem.Object[] { });
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

    #region AchGranting

    public static void GrantAchievement(Achievement achievement)
    {
        if (achievement.unlocked)
        {
            //modLogger.LogInfo("Achievement already unlocked " + achievement.name + " " + achievement.idx);
            return;
        }
        AchievementPopup(achievement);
        AudioManager.PlaySFX(SFXTypes.Achievement, PolytopiaBackendBase.Common.SkinType.Default, 1, 1, 0);
        achievement.unlocked = true;
        unlockedDict[achievement.idx] = true;
        PrefsHelper.SaveDict(unlockedDict);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildAction), nameof(BuildAction.ExecuteDefault))]
    public static void BuildAchievements(BuildAction __instance, GameState gameState)
    {
        
        if (__instance.PlayerId != GameManager.LocalPlayer.Id)
        {
            return;
        }
        if (__instance.Type == ImprovementData.Type.Monument2 && gameState.Settings.Difficulty != BotDifficulty.Easy)
        {
            Main.GrantAchievement(Achievements[geniusidx]);
        }
        if (__instance.Type == ImprovementData.Type.Farm)
        {
            Main.GrantAchievement(Achievements[farmingidx]);
        }
    }

    // Some achievements are checked at the start of each turn

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartTurnAction), nameof(StartTurnAction.ExecuteDefault))]
    public static void StartTurnCheck(StartTurnAction __instance, GameState gameState)
    {
        PlayerState player;
        gameState.TryGetPlayer(__instance.PlayerId, out player);
        if (player.AutoPlay)
        {
            return;
        }


        
        int veterancounter = 0;
        int SECRETcounter1 = 0;

        
        List<UnitData.Type> units = new List<UnitData.Type>();

        foreach (var tile in gameState.Map.Tiles)
        {
            if (tile.unit != null)
            {
                if (tile.unit.owner == player.Id)
                {
                    units.Add(tile.unit.type);
                    if (tile.unit.promotionLevel == 1)
                    {
                        veterancounter++;
                    }
                    if (player.tribe == EnumCache<PolytopiaBackendBase.Common.TribeType>.GetType(Main.Decipher("jqqftkem", 2)) && gameState.Settings.OpponentCount > 1)
                    {
                        if (tile.unit.type == EnumCache<UnitData.Type>.GetType(Main.Decipher("ctejgt", 2)))
                        {
                            SECRETcounter1++;
                        }
                        else SECRETcounter1 = int.MinValue;
                    }
                    if (tile.unit.HasEffect(UnitEffect.Boosted) && tile.unit.HasEffect(UnitEffect.Poisoned) && tile.unit.HasEffect(UnitEffect.Frozen) && tile.unit.promotionLevel == 1 && !Achievements[howdidweidx].unlocked)
                    {
                        Main.GrantAchievement(Achievements[howdidweidx]);
                    }
                }
            }
            if (tile.improvement != null)
            {
                if (tile.improvement.owner == player.Id)
                {
                    if (tile.improvement.type == ImprovementData.Type.City)
                    {
                        if (tile.improvement.RewardCount(CityReward.Park) >= 5)
                        {
                            Main.GrantAchievement(Achievements[parkidx]);
                        }
                    }
                }
            }
        }
        if (veterancounter >= 3 && !Achievements[veteranidx].unlocked)
        {
            Main.GrantAchievement(Achievements[veteranidx]);
        }

        if (SECRETcounter1 >= DecryptInt(214748354) && !Achievements[secret1idx].unlocked)
        {
            Main.GrantAchievement(Achievements[secret1idx]);
        }

        if (gameState.CurrentTurn <= 10 && gameState.Settings.mapPreset == MapPreset.Dryland)
        {
            if (player.HasTech(TechData.Type.Aquatism) && player.HasTech(TechData.Type.Navigation))
            {
                Main.GrantAchievement(Achievements[sdachidx]);
            }
        }

        if (units.Contains(UnitData.Type.Giant) && units.Contains(UnitData.Type.Crab) && units.Contains(UnitData.Type.FireDragon) && units.Contains(UnitData.Type.Gaami) && units.Contains(UnitData.Type.Centipede))
        {
            Main.GrantAchievement(Achievements[stachidx]);
        }
    }

    //Mindbend a giant

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConvertAction), nameof(ConvertAction.ExecuteDefault))]
    public static void Yoinkers(ConvertAction __instance, GameState gameState)
    {
        TileData tile = gameState.Map.GetTile(__instance.Origin);
        TileData tile2 = gameState.Map.GetTile(__instance.Target);
        UnitState unit = tile.unit;
        UnitState unit2 = tile2.unit;
        if (unit == null || unit2 == null || unit.owner != GameManager.LocalPlayer.Id)
        {
            return;
        }

        
        if (unit2.type == UnitData.Type.Giant)
        {
            Main.GrantAchievement(Achievements[yoinkidx]);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WipePlayerAction), nameof(WipePlayerAction.ExecuteDefault))]
    public static void WipePlayerAction_ExecuteDefault(WipePlayerAction __instance, GameState state)
    {
        
        if (__instance.PlayerId == GameManager.LocalPlayer.Id)
        {
            Main.GrantAchievement(Achievements[wipeidx]);
        }
    }

    #endregion
}
