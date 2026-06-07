using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Polychievements;

#nullable enable
public static class UI
{
    private static int AchViewMode = 0;
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
            var achi = Main.Achievements[id];
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
                    Main.Achievements[Main.GetAchievementLocation(achi.idx)].unlocked = false;
                    Main.unlockedDict[achi.idx] = false;
                    PrefsHelper.SaveDict(Main.unlockedDict);
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
            foreach (var ach in Main.Achievements)
            {
                if (ach.category == AchViewMode)
                {
                    CreatePlayerButton(__instance, gameState, Localization.Get(ach.name), "achievement", Main.GetAchievementLocation(ach.idx), GetColor(ach), ref num);
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
                AchViewMode = 0;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.easy");
                __instance.Update();
            }
            void medium(int id, BaseEventData eventData)
            {
                AchViewMode = 1;
                __instance.selectViewmodePopup.SetData(GameManager.GameState);
                __instance.selectViewmodePopup.Header = Localization.Get("ach.ui.medium");
                __instance.Update();
            }
            void hard(int id, BaseEventData eventData)
            {
                AchViewMode = 2;
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
}