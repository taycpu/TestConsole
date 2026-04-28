using Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrototypeEditor
{
    public static class PrototypeSceneBuilder
    {
        private const string RootName = "PrototypeGame";
        private static readonly Vector2 ArenaSize = new Vector2(14f, 8f);

        [MenuItem("Tools/Prototype/Build Debug Console Playground")]
        public static void BuildDebugConsolePlayground()
        {
            RemoveExistingRoot();

            var root = new GameObject(RootName);
            var controller = root.AddComponent<PrototypeGameController>();

            var player = CreatePlayer(root.transform);
            var collectibleTemplate = CreateCollectibleTemplate(root.transform);
            var hazardTemplate = CreateHazardTemplate(root.transform);
            var statusText = CreateStatusText();

            CreateArenaBounds(root.transform);
            ConfigureController(controller, player, collectibleTemplate, hazardTemplate, statusText);
            ConfigureCamera();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            Debug.Log("[PrototypeSceneBuilder] Built Debug Console Playground in SampleScene.");
        }

        private static void RemoveExistingRoot()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null)
                Object.DestroyImmediate(existing);

            var existingStatus = GameObject.Find("Prototype Status");
            if (existingStatus != null)
                Object.DestroyImmediate(existingStatus);
        }

        private static PrototypePlayer CreatePlayer(Transform parent)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.transform.SetParent(parent);
            player.transform.position = Vector3.zero;
            player.transform.localScale = new Vector3(0.7f, 0.7f, 0.15f);
            ApplyColor(player, new Color(0.2f, 0.75f, 1f));
            Object.DestroyImmediate(player.GetComponent<BoxCollider>());

            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Rigidbody2D>();
            return player.AddComponent<PrototypePlayer>();
        }

        private static PrototypeCollectible CreateCollectibleTemplate(Transform parent)
        {
            var collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectible.name = "Collectible Template";
            collectible.transform.SetParent(parent);
            collectible.transform.position = new Vector3(0f, -20f, 0f);
            collectible.transform.localScale = new Vector3(0.45f, 0.45f, 0.15f);
            ApplyColor(collectible, new Color(1f, 0.85f, 0.2f));
            Object.DestroyImmediate(collectible.GetComponent<SphereCollider>());

            collectible.AddComponent<CircleCollider2D>();
            var behaviour = collectible.AddComponent<PrototypeCollectible>();
            collectible.SetActive(false);
            return behaviour;
        }

        private static PrototypeHazard CreateHazardTemplate(Transform parent)
        {
            var hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = "Hazard Template";
            hazard.transform.SetParent(parent);
            hazard.transform.position = new Vector3(0f, -22f, 0f);
            hazard.transform.localScale = new Vector3(0.8f, 0.8f, 0.15f);
            ApplyColor(hazard, new Color(1f, 0.25f, 0.25f));
            Object.DestroyImmediate(hazard.GetComponent<BoxCollider>());

            hazard.AddComponent<BoxCollider2D>();
            hazard.AddComponent<Rigidbody2D>();
            var behaviour = hazard.AddComponent<PrototypeHazard>();
            hazard.SetActive(false);
            return behaviour;
        }

        private static void CreateArenaBounds(Transform parent)
        {
            CreateBound(parent, "Top Bound", new Vector3(0f, ArenaSize.y * 0.5f, 0.1f), new Vector3(ArenaSize.x, 0.12f, 0.1f));
            CreateBound(parent, "Bottom Bound", new Vector3(0f, -ArenaSize.y * 0.5f, 0.1f), new Vector3(ArenaSize.x, 0.12f, 0.1f));
            CreateBound(parent, "Left Bound", new Vector3(-ArenaSize.x * 0.5f, 0f, 0.1f), new Vector3(0.12f, ArenaSize.y, 0.1f));
            CreateBound(parent, "Right Bound", new Vector3(ArenaSize.x * 0.5f, 0f, 0.1f), new Vector3(0.12f, ArenaSize.y, 0.1f));
        }

        private static void CreateBound(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var bound = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bound.name = name;
            bound.transform.SetParent(parent);
            bound.transform.position = position;
            bound.transform.localScale = scale;
            ApplyColor(bound, new Color(0.15f, 0.18f, 0.24f));
            Object.DestroyImmediate(bound.GetComponent<BoxCollider>());
        }

        private static TMP_Text CreateStatusText()
        {
            var canvas = GameObject.Find("Canvas") ?? CreateCanvas();
            var statusObject = new GameObject("Prototype Status", typeof(RectTransform));
            statusObject.transform.SetParent(canvas.transform, false);

            var rect = statusObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(18f, -18f);
            rect.sizeDelta = new Vector2(420f, 140f);

            var text = statusObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateCanvas()
        {
            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            return canvas;
        }

        private static void ConfigureController(
            PrototypeGameController controller,
            PrototypePlayer player,
            PrototypeCollectible collectibleTemplate,
            PrototypeHazard hazardTemplate,
            TMP_Text statusText)
        {
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("player").objectReferenceValue = player;
            serialized.FindProperty("collectiblePrefab").objectReferenceValue = collectibleTemplate;
            serialized.FindProperty("hazardPrefab").objectReferenceValue = hazardTemplate;
            serialized.FindProperty("statusText").objectReferenceValue = statusText;
            serialized.FindProperty("arenaSize").vector2Value = ArenaSize;
            serialized.FindProperty("startingHazards").intValue = 3;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        }

        private static void ApplyColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
                return;

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
