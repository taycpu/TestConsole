// This file must live in an Editor folder to access UnityEditor APIs.
// Move it to Assets/DebugPanel/Editor/ if needed.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DebugPanel;
using DebugPanel.Tabs;

namespace DebugPanel.Editor
{
    public static class DebugPanelPrefabBuilder
    {
        private const string ResourcesDir = "Assets/DebugPanel/Resources";

        [MenuItem("Tools/Debug Panel/Build Prefab")]
        public static void BuildPrefab()
        {
            if (!System.IO.Directory.Exists(ResourcesDir))
                System.IO.Directory.CreateDirectory(ResourcesDir);

            // ── Save sub-prefabs as separate assets first ─────────────────
            var catHeaderPrefab   = SaveSubPrefab(BuildCategoryHeader(),  "DebugPanel_CategoryHeader");
            var optRowPrefab      = SaveSubPrefab(BuildOptionRow(),        "DebugPanel_OptionRow");
            var conEntryPrefab    = SaveSubPrefab(BuildConsoleEntry(),     "DebugPanel_ConsoleEntry");
            var sysRowPrefab      = SaveSubPrefab(BuildSystemInfoRow(),    "DebugPanel_SystemInfoRow");
            var sysSectionPrefab  = SaveSubPrefab(BuildSectionHeader(),   "DebugPanel_SectionHeader");

            // ── Build and save main prefab ────────────────────────────────
            var root = BuildHierarchy(catHeaderPrefab, optRowPrefab, conEntryPrefab, sysRowPrefab, sysSectionPrefab);

            string path = $"{ResourcesDir}/DebugPanel.prefab";
            bool success;
            PrefabUtility.SaveAsPrefabAsset(root, path, out success);
            Object.DestroyImmediate(root);

            if (success)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[DebugPanel] Prefab saved to {path}");
                EditorUtility.DisplayDialog("Debug Panel", $"Prefab built successfully!\n{path}", "OK");
            }
            else
            {
                Debug.LogError("[DebugPanel] Failed to save prefab.");
            }
        }

        private static GameObject SaveSubPrefab(GameObject go, string assetName)
        {
            string path = $"{ResourcesDir}/{assetName}.prefab";
            bool success;
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path, out success);
            Object.DestroyImmediate(go);
            if (!success) Debug.LogError($"[DebugPanel] Failed to save sub-prefab {assetName}");
            return saved;
        }

        private static GameObject BuildHierarchy(
            GameObject catHeaderPrefab,
            GameObject optRowPrefab,
            GameObject conEntryPrefab,
            GameObject sysRowPrefab,
            GameObject sysSectionPrefab)
        {
            // ── Root ──────────────────────────────────────────────────────
            var root = new GameObject("DebugPanel");
            var manager = root.AddComponent<DebugPanelManager>();

            // ── Trigger Canvas (always visible) ───────────────────────────
            var triggerCanvasGo = new GameObject("TriggerCanvas");
            triggerCanvasGo.transform.SetParent(root.transform, false);
            var triggerCanvas = triggerCanvasGo.AddComponent<Canvas>();
            triggerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            triggerCanvas.sortingOrder = 99;
            triggerCanvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            triggerCanvasGo.AddComponent<GraphicRaycaster>();

            // Trigger button
            var triggerBtnGo = CreateButton(triggerCanvasGo.transform, "TriggerButton", "D", 14, new Color(0.1f, 0.1f, 0.1f, 0.6f));
            var triggerRect = triggerBtnGo.GetComponent<RectTransform>();
            triggerRect.anchorMin = triggerRect.anchorMax = triggerRect.pivot = Vector2.zero;
            triggerRect.anchoredPosition = new Vector2(10, 10);
            triggerRect.sizeDelta = new Vector2(44, 44);
            triggerBtnGo.AddComponent<DebugPanelTrigger>();

            // ── Panel Canvas (hidden by default) ──────────────────────────
            var panelCanvasGo = new GameObject("PanelCanvas");
            panelCanvasGo.transform.SetParent(root.transform, false);
            var panelCanvas = panelCanvasGo.AddComponent<Canvas>();
            panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            panelCanvas.sortingOrder = 100;
            var scaler = panelCanvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            panelCanvasGo.AddComponent<GraphicRaycaster>();
            var panelCanvasComp = panelCanvasGo.AddComponent<DebugPanelCanvas>();

            // Panel root (full screen)
            var panelRoot = CreateFullScreenRect(panelCanvasGo.transform, "PanelRoot");
            var bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.97f);

            // ── Header bar ────────────────────────────────────────────────
            var header = CreateRect(panelRoot.transform, "Header");
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = Vector2.one;
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 80);
            header.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 1f);

            // Tab buttons row
            var tabBar = CreateRect(header.transform, "TabBar");
            var tabBarRect = tabBar.GetComponent<RectTransform>();
            tabBarRect.anchorMin = Vector2.zero;
            tabBarRect.anchorMax = Vector2.one;
            tabBarRect.offsetMin = new Vector2(4, 4);
            tabBarRect.offsetMax = new Vector2(-50, -4);
            var tabBarHLG = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabBarHLG.spacing = 4;
            tabBarHLG.childForceExpandWidth = true;
            tabBarHLG.childForceExpandHeight = true;
            tabBarHLG.padding = new RectOffset(0, 0, 0, 0);

            var optTabBtn = CreateButton(tabBar.transform, "OptionsTabBtn", "Options", 16, new Color(0.2f, 0.6f, 1f));
            var conTabBtn = CreateButton(tabBar.transform, "ConsoleTabBtn", "Console", 16, new Color(0.15f, 0.15f, 0.15f));
            var sysTabBtn = CreateButton(tabBar.transform, "SystemInfoTabBtn", "System", 16, new Color(0.15f, 0.15f, 0.15f));

            // Close button
            var closeBtn = CreateButton(header.transform, "CloseButton", "X", 18, new Color(0.7f, 0.15f, 0.15f));
            var closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 0);
            closeBtnRect.anchorMax = Vector2.one;
            closeBtnRect.pivot = new Vector2(1, 0.5f);
            closeBtnRect.anchoredPosition = new Vector2(-4, 0);
            closeBtnRect.sizeDelta = new Vector2(44, -8);

            // ── Content area ──────────────────────────────────────────────
            var contentArea = CreateRect(panelRoot.transform, "ContentArea");
            var contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -80);

            // ── Options Tab ───────────────────────────────────────────────
            var optTabPanel = BuildScrollableTabPanel(contentArea.transform, "OptionsTabPanel", out var optContent);
            var optTabComp = optTabPanel.AddComponent<OptionsTab>();

            SetPrivate(optTabComp, "contentRoot", optContent.transform);
            SetPrivate(optTabComp, "categoryHeaderPrefab", catHeaderPrefab);
            SetPrivate(optTabComp, "optionRowPrefab", optRowPrefab);

            // ── Console Tab ───────────────────────────────────────────────
            var conTabPanel = BuildScrollableTabPanel(contentArea.transform, "ConsoleTabPanel", out var conContent);
            var conTabComp = conTabPanel.AddComponent<ConsoleTab>();

            // Console toolbar
            var conToolbar = CreateRect(conTabPanel.transform, "Toolbar");
            var conToolbarRect = conToolbar.GetComponent<RectTransform>();
            conToolbarRect.anchorMin = new Vector2(0, 1);
            conToolbarRect.anchorMax = Vector2.one;
            conToolbarRect.pivot = new Vector2(0.5f, 1);
            conToolbarRect.sizeDelta = new Vector2(0, 44);
            conToolbarRect.anchoredPosition = Vector2.zero;
            conToolbar.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f);
            var conToolbarHLG = conToolbar.AddComponent<HorizontalLayoutGroup>();
            conToolbarHLG.childForceExpandWidth = false;
            conToolbarHLG.childForceExpandHeight = true;
            conToolbarHLG.spacing = 4;
            conToolbarHLG.padding = new RectOffset(4, 4, 4, 4);

            var clearBtn = CreateButton(conToolbar.transform, "ClearButton", "Clear", 14, new Color(0.3f, 0.3f, 0.3f));
            clearBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 0);
            var clearBtnLE = clearBtn.AddComponent<LayoutElement>();
            clearBtnLE.preferredWidth = 70;
            clearBtnLE.minWidth = 70;

            var logToggle = CreateToggleButton(conToolbar.transform, "LogToggle", "Log", Color.white);
            var warnToggle = CreateToggleButton(conToolbar.transform, "WarnToggle", "Warn", new Color(1f, 0.85f, 0f));
            var errToggle = CreateToggleButton(conToolbar.transform, "ErrToggle", "Error", new Color(1f, 0.3f, 0.3f));

            SetPrivate(conTabComp, "contentRoot", conContent.transform);
            SetPrivate(conTabComp, "consoleEntryPrefab", conEntryPrefab);
            SetPrivate(conTabComp, "clearButton", clearBtn.GetComponent<Button>());
            SetPrivate(conTabComp, "showLogsToggle", logToggle.GetComponent<Toggle>());
            SetPrivate(conTabComp, "showWarningsToggle", warnToggle.GetComponent<Toggle>());
            SetPrivate(conTabComp, "showErrorsToggle", errToggle.GetComponent<Toggle>());

            // ── System Info Tab ───────────────────────────────────────────
            var sysTabPanel = BuildScrollableTabPanel(contentArea.transform, "SystemInfoTabPanel", out var sysContent);
            var sysTabComp = sysTabPanel.AddComponent<SystemInfoTab>();

            SetPrivate(sysTabComp, "contentRoot", sysContent.transform);
            SetPrivate(sysTabComp, "sectionHeaderPrefab", sysSectionPrefab);
            SetPrivate(sysTabComp, "infoRowPrefab", sysRowPrefab);

            // ── Wire up DebugPanelCanvas ───────────────────────────────────
            SetPrivate(panelCanvasComp, "panelRoot", panelRoot);
            SetPrivate(panelCanvasComp, "optionsTabButton", optTabBtn.GetComponent<Button>());
            SetPrivate(panelCanvasComp, "consoleTabButton", conTabBtn.GetComponent<Button>());
            SetPrivate(panelCanvasComp, "systemInfoTabButton", sysTabBtn.GetComponent<Button>());
            SetPrivate(panelCanvasComp, "closeButton", closeBtn.GetComponent<Button>());
            SetPrivate(panelCanvasComp, "optionsTabPanel", optTabPanel);
            SetPrivate(panelCanvasComp, "consoleTabPanel", conTabPanel);
            SetPrivate(panelCanvasComp, "systemInfoTabPanel", sysTabPanel);

            // ── Wire up DebugPanelManager ──────────────────────────────────
            SetPrivate(manager, "panelCanvas", panelCanvasComp);

            return root;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static GameObject BuildScrollableTabPanel(Transform parent, string name, out RectTransform contentTransform)
        {
            var panel = CreateFullScreenRect(parent, name);

            var scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            var viewport = CreateFullScreenRect(panel.transform, "Viewport");
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateRect(viewport.transform, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;

            contentTransform = contentRect;
            return panel;
        }

        private static GameObject BuildCategoryHeader()
        {
            var go = new GameObject("CategoryHeader");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);
            go.AddComponent<LayoutElement>().preferredHeight = 30;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.12f);

            var label = CreateTMPText(go.transform, "Label", "", 13, FontStyles.Bold);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8, 2);
            labelRect.offsetMax = new Vector2(-8, -2);
            label.GetComponent<TMP_Text>().color = new Color(0.5f, 0.7f, 1f);

            return go;
        }

        private static GameObject BuildSectionHeader()
        {
            var go = BuildCategoryHeader();
            go.name = "SectionHeader";
            return go;
        }

        private static GameObject BuildOptionRow()
        {
            var go = new GameObject("OptionRow");
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 48);
            go.AddComponent<LayoutElement>().preferredHeight = 48;
            go.AddComponent<Image>().color = new Color(0.11f, 0.11f, 0.11f);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 8;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;

            // Label
            var labelGo = CreateTMPText(go.transform, "Label", "Option Name", 14);
            labelGo.AddComponent<LayoutElement>().flexibleWidth = 1;

            // Toggle
            var toggleGo = CreateRect(go.transform, "ToggleControl");
            var toggleLE = toggleGo.AddComponent<LayoutElement>();
            toggleLE.preferredWidth = 40;
            var toggle = toggleGo.AddComponent<Toggle>();
            var toggleBg = CreateRect(toggleGo.transform, "Background");
            var toggleBgImg = toggleBg.AddComponent<Image>();
            toggleBgImg.color = new Color(0.3f, 0.3f, 0.3f);
            var toggleBgRect = toggleBg.GetComponent<RectTransform>();
            toggleBgRect.sizeDelta = new Vector2(36, 20);
            var checkmark = CreateRect(toggleBg.transform, "Checkmark");
            var checkmarkImg = checkmark.AddComponent<Image>();
            checkmarkImg.color = new Color(0.2f, 0.6f, 1f);
            var checkRect = checkmark.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(18, 18);
            checkRect.anchoredPosition = new Vector2(8, 0);
            toggle.targetGraphic = toggleBgImg;
            toggle.graphic = checkmarkImg;

            // Slider + value label
            var sliderGo = CreateRect(go.transform, "SliderControl");
            var sliderLE = sliderGo.AddComponent<LayoutElement>();
            sliderLE.flexibleWidth = 1.5f;
            sliderLE.preferredHeight = 30;
            var slider = sliderGo.AddComponent<Slider>();
            var sliderBg = CreateRect(sliderGo.transform, "Background");
            sliderBg.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
            StretchFull(sliderBg.GetComponent<RectTransform>());
            var fillArea = CreateRect(sliderGo.transform, "FillArea");
            StretchFull(fillArea.GetComponent<RectTransform>());
            var fill = CreateRect(fillArea.transform, "Fill");
            fill.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f);
            StretchFull(fill.GetComponent<RectTransform>());
            var handleArea = CreateRect(sliderGo.transform, "HandleArea");
            StretchFull(handleArea.GetComponent<RectTransform>());
            var handle = CreateRect(handleArea.transform, "Handle");
            handle.AddComponent<Image>().color = Color.white;
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            var sliderValLabel = CreateTMPText(sliderGo.transform, "ValueLabel", "0.00", 12);
            sliderValLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineRight;
            var sliderValRect = sliderValLabel.GetComponent<RectTransform>();
            sliderValRect.anchorMin = new Vector2(1, 0);
            sliderValRect.anchorMax = Vector2.one;
            sliderValRect.pivot = new Vector2(1, 0.5f);
            sliderValRect.anchoredPosition = new Vector2(-4, 0);
            sliderValRect.sizeDelta = new Vector2(50, 0);

            // Input field
            var inputGo = CreateRect(go.transform, "InputFieldControl");
            inputGo.AddComponent<LayoutElement>().preferredWidth = 120;
            var inputImg = inputGo.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.2f);
            var inputField = inputGo.AddComponent<TMP_InputField>();
            var inputTextArea = CreateRect(inputGo.transform, "Text Area");
            StretchFull(inputTextArea.GetComponent<RectTransform>(), new Vector2(4, 4), new Vector2(-4, -4));
            inputTextArea.AddComponent<RectMask2D>();
            var inputText = CreateTMPText(inputTextArea.transform, "Text", "", 14);
            StretchFull(inputText.GetComponent<RectTransform>());
            inputField.textComponent = inputText.GetComponent<TMP_Text>();
            inputField.textViewport = inputTextArea.GetComponent<RectTransform>();

            // Dropdown
            var dropdownGo = CreateRect(go.transform, "DropdownControl");
            dropdownGo.AddComponent<LayoutElement>().preferredWidth = 140;
            var dropdownImg = dropdownGo.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.2f);
            var dropdown = dropdownGo.AddComponent<TMP_Dropdown>();
            var dropLabel = CreateTMPText(dropdownGo.transform, "Label", "Option", 13);
            StretchFull(dropLabel.GetComponent<RectTransform>(), new Vector2(8, 4), new Vector2(-8, -4));
            dropdown.captionText = dropLabel.GetComponent<TMP_Text>();

            // Button (for method options)
            var btnGo = CreateButton(go.transform, "ButtonControl", "Invoke", 14, new Color(0.2f, 0.45f, 0.8f));
            btnGo.AddComponent<LayoutElement>().preferredWidth = 140;
            var btnLabel = btnGo.GetComponentInChildren<TMP_Text>();

            // Read-only label
            var roLabel = CreateTMPText(go.transform, "ReadOnlyLabel", "—", 14);
            roLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineRight;
            roLabel.GetComponent<TMP_Text>().color = new Color(0.6f, 0.6f, 0.6f);
            roLabel.AddComponent<LayoutElement>().preferredWidth = 140;

            // Wire OptionRowRenderer
            var renderer = go.AddComponent<OptionRowRenderer>();
            SetPrivate(renderer, "labelText", labelGo.GetComponent<TMP_Text>());
            SetPrivate(renderer, "toggleControl", toggle);
            SetPrivate(renderer, "sliderControl", slider);
            SetPrivate(renderer, "sliderValueLabel", sliderValLabel.GetComponent<TMP_Text>());
            SetPrivate(renderer, "inputFieldControl", inputField);
            SetPrivate(renderer, "dropdownControl", dropdown);
            SetPrivate(renderer, "buttonControl", btnGo.GetComponent<Button>());
            SetPrivate(renderer, "buttonLabel", btnLabel);
            SetPrivate(renderer, "readonlyLabel", roLabel.GetComponent<TMP_Text>());

            return go;
        }

        private static GameObject BuildConsoleEntry()
        {
            var go = new GameObject("ConsoleEntry");
            go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 40;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 4, 4);
            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var btn = go.AddComponent<Button>();
            var msgText = CreateTMPText(go.transform, "Message", "Log message", 13);
            msgText.GetComponent<TMP_Text>().color = Color.white;
            msgText.AddComponent<LayoutElement>().preferredHeight = 30;

            var stackText = CreateTMPText(go.transform, "StackTrace", "", 11);
            stackText.GetComponent<TMP_Text>().color = new Color(0.7f, 0.7f, 0.7f);
            stackText.AddComponent<LayoutElement>();

            var row = go.AddComponent<ConsoleEntryRow>();
            SetPrivate(row, "messageText", msgText.GetComponent<TMP_Text>());
            SetPrivate(row, "stackTraceText", stackText.GetComponent<TMP_Text>());
            SetPrivate(row, "expandButton", btn);
            SetPrivate(row, "backgroundImage", img);

            return go;
        }

        private static GameObject BuildSystemInfoRow()
        {
            var go = new GameObject("SystemInfoRow");
            go.AddComponent<RectTransform>();
            go.AddComponent<LayoutElement>().preferredHeight = 36;
            go.AddComponent<Image>().color = new Color(0.11f, 0.11f, 0.11f);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 8;
            hlg.childForceExpandHeight = true;

            var labelGo = CreateTMPText(go.transform, "Label", "Label", 13);
            labelGo.GetComponent<TMP_Text>().color = new Color(0.7f, 0.7f, 0.7f);
            labelGo.AddComponent<LayoutElement>().flexibleWidth = 1;

            var valGo = CreateTMPText(go.transform, "Value", "Value", 13);
            valGo.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineRight;
            valGo.AddComponent<LayoutElement>().flexibleWidth = 1;

            return go;
        }

        // ── Small UI factory helpers ──────────────────────────────────────

        private static GameObject CreateButton(Transform parent, string name, string label, int fontSize, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.3f;
            colors.pressedColor = bgColor * 0.7f;
            btn.colors = colors;
            var txtGo = CreateTMPText(go.transform, "Text", label, fontSize);
            var txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateToggleButton(Transform parent, string name, string label, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(60, 0);
            go.AddComponent<LayoutElement>().preferredWidth = 60;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f);
            var toggle = go.AddComponent<Toggle>();
            toggle.isOn = true;
            toggle.targetGraphic = img;
            var onColors = toggle.colors;
            onColors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            toggle.colors = onColors;
            var txtGo = CreateTMPText(go.transform, "Text", label, 13);
            txtGo.GetComponent<TMP_Text>().color = color;
            var txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return go;
        }

        private static GameObject CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateFullScreenRect(Transform parent, string name)
        {
            var go = CreateRect(parent, name);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return go;
        }

        private static void StretchFull(RectTransform rect, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }

        private static void SetPrivate(UnityEngine.Object obj, string fieldName, object value)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[DebugPanelBuilder] Field '{fieldName}' not found on {obj.GetType().Name}");
                return;
            }

            if (value is UnityEngine.Object uObj)
                prop.objectReferenceValue = uObj;
            else if (value is bool b)
                prop.boolValue = b;
            else if (value is int i)
                prop.intValue = i;
            else if (value is float f)
                prop.floatValue = f;
            else if (value is string s)
                prop.stringValue = s;
            else if (value is Color c)
                prop.colorValue = c;
            else if (value is Vector2 v2)
                prop.vector2Value = v2;
            else if (value is Vector3 v3)
                prop.vector3Value = v3;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
