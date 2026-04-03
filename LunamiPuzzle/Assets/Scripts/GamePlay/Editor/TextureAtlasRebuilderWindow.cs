using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GamePlay.Editor
{
    public class TextureAtlasRebuilderWindow : EditorWindow
    {
        [Serializable]
        private class AtlasManifest
        {
            public string sourceDocument;
            public string sourceImage;
            public string atlasFile;
            public int atlasWidth;
            public int atlasHeight;
            public int atlasContentWidth;
            public int atlasContentHeight;
            public ManifestLayer[] layers;
            public HierarchyNode hierarchy;
        }

        [Serializable]
        private class ManifestLayer
        {
            public string name;
            public string type;
            public string parentPath;
            public string sourceRelativePath;
            public int sourceX;
            public int sourceY;
            public int sourceWidth;
            public int sourceHeight;
            public int x;
            public int y;
            public int width;
            public int height;
            public int left;
            public int top;
            public int w;
            public int h;
            public int atlasX;
            public int atlasY;
            public int atlasWidth;
            public int atlasHeight;
            public float uvX;
            public float uvY;
            public float uvWidth;
            public float uvHeight;
            public string text;
            public string color;
            public int fontSize;
            public string alignment;
        }

        [Serializable]
        private class HierarchyNode
        {
            public string name;
            public string type;
            public HierarchyNode[] children;
        }

        private class ParsedNode
        {
            public string Name;
            public string Type;
            public string Path;
            public ParsedNode Parent;
            public List<ParsedNode> Children = new List<ParsedNode>();
            public ManifestLayer Layer;
        }

        private struct LayoutBounds
        {
            public float MinX;
            public float MinY;
            public float MaxX;
            public float MaxY;

            public float Width => Mathf.Max(1f, MaxX - MinX);
            public float Height => Mathf.Max(1f, MaxY - MinY);
            public float CenterX => (MinX + MaxX) * 0.5f;
            public float CenterY => (MinY + MaxY) * 0.5f;
        }

        private const float LeftPanelWidth = 330f;

        private TextAsset _jsonAsset;
        private Texture2D _atlasTexture;

        private AtlasManifest _manifest;
        private ParsedNode _rootNode;
        private readonly List<ManifestLayer> _orderedLayers = new List<ManifestLayer>();
        private LayoutBounds _bounds;
        private bool _sourceYTopDown = true;
        private bool _atlasYTopDown = true;
        private int _duplicateLayerCount;

        private TreeView _treeView;
        private Label _statusLabel;
        private IMGUIContainer _previewContainer;

        [MenuItem("Tools/Texture Importer/Atlas Rebuilder")]
        private static void Open()
        {
            var window = GetWindow<TextureAtlasRebuilderWindow>("Atlas Rebuilder");
            window.minSize = new Vector2(1080f, 680f);
            window.Show();
        }

        private void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.paddingLeft = 8f;
            rootVisualElement.style.paddingRight = 8f;
            rootVisualElement.style.paddingTop = 8f;
            rootVisualElement.style.paddingBottom = 8f;

            BuildTopInputs(rootVisualElement);
            BuildMainPanels(rootVisualElement);
            BuildBottom(rootVisualElement);

            SetStatus("请先拖入 json 和 atlas 纹理。");
        }

        private void BuildTopInputs(VisualElement root)
        {
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.marginBottom = 8f;

            var jsonField = new ObjectField("Json")
            {
                objectType = typeof(TextAsset),
                allowSceneObjects = false,
                value = _jsonAsset
            };
            jsonField.style.flexGrow = 1f;
            jsonField.style.marginRight = 8f;
            jsonField.RegisterValueChangedCallback(evt =>
            {
                _jsonAsset = evt.newValue as TextAsset;
                ParseAndRefresh();
            });

            var textureField = new ObjectField("Texture")
            {
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
                value = _atlasTexture
            };
            textureField.style.flexGrow = 1f;
            textureField.style.marginRight = 8f;
            textureField.RegisterValueChangedCallback(evt =>
            {
                _atlasTexture = evt.newValue as Texture2D;
                ParseAndRefresh();
            });

            var parseButton = new Button(ParseAndRefresh)
            {
                text = "解析"
            };
            parseButton.style.width = 80f;
            parseButton.style.marginRight = 8f;

            var loadTestButton = new Button(TryLoadFromTestFolder)
            {
                text = "加载Test"
            };
            loadTestButton.style.width = 90f;

            topRow.Add(jsonField);
            topRow.Add(textureField);
            topRow.Add(parseButton);
            topRow.Add(loadTestButton);
            root.Add(topRow);

            var optionRow = new VisualElement();
            optionRow.style.flexDirection = FlexDirection.Row;
            optionRow.style.marginBottom = 8f;

            var sourceYToggle = new Toggle("SourceY Top-Down")
            {
                value = _sourceYTopDown
            };
            sourceYToggle.style.marginRight = 16f;
            sourceYToggle.RegisterValueChangedCallback(evt =>
            {
                _sourceYTopDown = evt.newValue;
                Repaint();
            });

            var atlasYToggle = new Toggle("AtlasY Top-Down")
            {
                value = _atlasYTopDown
            };
            atlasYToggle.RegisterValueChangedCallback(evt =>
            {
                _atlasYTopDown = evt.newValue;
                Repaint();
            });

            optionRow.Add(sourceYToggle);
            optionRow.Add(atlasYToggle);
            root.Add(optionRow);
        }

        private void BuildMainPanels(VisualElement root)
        {
            var body = new VisualElement();
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1f;
            body.style.minHeight = 440f;

            var left = new VisualElement();
            left.style.width = LeftPanelWidth;
            left.style.minWidth = 260f;
            left.style.maxWidth = 420f;
            left.style.marginRight = 8f;
            left.style.borderTopWidth = 1f;
            left.style.borderBottomWidth = 1f;
            left.style.borderLeftWidth = 1f;
            left.style.borderRightWidth = 1f;
            left.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
            left.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
            left.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
            left.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);

            var leftTitle = new Label("Layer Tree");
            leftTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            leftTitle.style.marginLeft = 6f;
            leftTitle.style.marginTop = 6f;
            left.Add(leftTitle);

            _treeView = new TreeView();
            _treeView.style.flexGrow = 1f;
            _treeView.style.marginTop = 4f;
            _treeView.makeItem = () => new Label();
            _treeView.bindItem = (element, index) =>
            {
                var item = _treeView.GetItemDataForIndex<ParsedNode>(index);
                var label = (Label)element;
                label.text = $"[{GetNodeTypeLabel(item)}] {item.Name}";
            };
            _treeView.selectionType = SelectionType.Single;
            left.Add(_treeView);

            var right = new VisualElement();
            right.style.flexGrow = 1f;
            right.style.borderTopWidth = 1f;
            right.style.borderBottomWidth = 1f;
            right.style.borderLeftWidth = 1f;
            right.style.borderRightWidth = 1f;
            right.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
            right.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
            right.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
            right.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);

            var rightTitle = new Label("Preview");
            rightTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            rightTitle.style.marginLeft = 6f;
            rightTitle.style.marginTop = 6f;
            right.Add(rightTitle);

            _previewContainer = new IMGUIContainer(DrawPreview);
            _previewContainer.style.flexGrow = 1f;
            _previewContainer.style.marginTop = 4f;
            right.Add(_previewContainer);

            body.Add(left);
            body.Add(right);
            root.Add(body);
        }

        private void BuildBottom(VisualElement root)
        {
            var bottom = new VisualElement();
            bottom.style.flexDirection = FlexDirection.Row;
            bottom.style.alignItems = Align.Center;
            bottom.style.marginTop = 8f;

            _statusLabel = new Label();
            _statusLabel.style.flexGrow = 1f;

            var generateButton = new Button(GenerateScene)
            {
                text = "Generate"
            };
            generateButton.style.width = 120f;
            generateButton.style.height = 30f;

            bottom.Add(_statusLabel);
            bottom.Add(generateButton);
            root.Add(bottom);
        }

        private void TryLoadFromTestFolder()
        {
            const string testFolder = "Assets/Scripts/GamePlay/Editor/Texture-Importer/test";
            string[] jsonGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { testFolder });
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { testFolder });

            if (jsonGuids.Length > 0)
            {
                _jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(jsonGuids[0]));
            }

            if (textureGuids.Length > 0)
            {
                _atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuids[0]));
            }

            ParseAndRefresh();
        }

        private void ParseAndRefresh()
        {
            _manifest = null;
            _rootNode = null;
            _orderedLayers.Clear();
            _duplicateLayerCount = 0;

            if (_jsonAsset == null || _atlasTexture == null)
            {
                RefreshTree();
                Repaint();
                SetStatus("请同时设置 json 和 texture。", true);
                return;
            }

            try
            {
                AtlasManifest manifest = JsonUtility.FromJson<AtlasManifest>(_jsonAsset.text);
                if (manifest == null)
                {
                    SetStatus("Json 解析失败。", true);
                    RefreshTree();
                    Repaint();
                    return;
                }

                if (manifest.layers == null || manifest.layers.Length == 0)
                {
                    SetStatus("Json 中没有 layers 数据。", true);
                    RefreshTree();
                    Repaint();
                    return;
                }

                _manifest = manifest;
                BuildTreeAndLayers();
                _bounds = CalculateBounds(_orderedLayers);
                RefreshTree();
                Repaint();
                SetStatus(_duplicateLayerCount > 0
                    ? $"解析成功：{_orderedLayers.Count} 个图层，跳过重复记录 {_duplicateLayerCount} 条"
                    : $"解析成功：{_orderedLayers.Count} 个图层");
            }
            catch (Exception ex)
            {
                RefreshTree();
                Repaint();
                SetStatus($"Json 解析异常: {ex.Message}", true);
            }
        }

        private void BuildTreeAndLayers()
        {
            _rootNode = new ParsedNode
            {
                Name = "root",
                Type = "group",
                Path = string.Empty
            };

            var pathToNode = new Dictionary<string, ParsedNode>(StringComparer.Ordinal)
            {
                [string.Empty] = _rootNode
            };

            if (_manifest.hierarchy != null)
            {
                BuildTreeFromHierarchy(_manifest.hierarchy, _rootNode, pathToNode);
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (ManifestLayer layer in _manifest.layers)
            {
                if (layer == null)
                {
                    continue;
                }

                NormalizeLayer(layer);
                string uniqueKey = BuildUniqueLayerKey(layer);
                if (!seen.Add(uniqueKey))
                {
                    _duplicateLayerCount++;
                    continue;
                }

                string layerPath = BuildLayerPath(layer);
                if (!pathToNode.TryGetValue(layerPath, out ParsedNode node))
                {
                    ParsedNode parent = GetOrCreateGroupPath(pathToNode, NormalizePath(layer.parentPath));
                    node = new ParsedNode
                    {
                        Name = string.IsNullOrEmpty(layer.name) ? "Layer" : layer.name,
                        Type = IsTextLayer(layer) ? "text" : "layer",
                        Path = layerPath,
                        Parent = parent,
                        Layer = layer
                    };
                    parent.Children.Add(node);
                    pathToNode[layerPath] = node;
                }

                node.Layer = layer;
                if (IsTextLayer(layer))
                {
                    node.Type = "text";
                }
                else if (string.IsNullOrEmpty(node.Type) || node.Type == "layer")
                {
                    node.Type = "layer";
                }

                _orderedLayers.Add(layer);
            }
        }

        private static string BuildUniqueLayerKey(ManifestLayer layer)
        {
            return string.Join("|", new[]
            {
                BuildLayerPath(layer),
                layer.sourceRelativePath ?? string.Empty,
                layer.sourceX.ToString(),
                layer.sourceY.ToString(),
                layer.sourceWidth.ToString(),
                layer.sourceHeight.ToString(),
                layer.atlasX.ToString(),
                layer.atlasY.ToString(),
                layer.atlasWidth.ToString(),
                layer.atlasHeight.ToString(),
                layer.text ?? string.Empty
            });
        }

        private static void NormalizeLayer(ManifestLayer layer)
        {
            layer.name = string.IsNullOrEmpty(layer.name) ? GuessNameFromSource(layer.sourceRelativePath) : layer.name;
            layer.parentPath = NormalizePath(layer.parentPath);

            int fallbackWidth = FirstPositive(layer.sourceWidth, layer.width, layer.w, layer.atlasWidth);
            int fallbackHeight = FirstPositive(layer.sourceHeight, layer.height, layer.h, layer.atlasHeight);
            layer.sourceWidth = Mathf.Max(1, fallbackWidth);
            layer.sourceHeight = Mathf.Max(1, fallbackHeight);

            int aliasX = FirstDefinedCoordinate(layer.x, layer.left);
            int aliasY = FirstDefinedCoordinate(layer.y, layer.top);

            bool sourceUnset = layer.sourceX == 0 && layer.sourceY == 0 && (aliasX != 0 || aliasY != 0);
            bool sourceLooksLikeAtlas = layer.sourceX == layer.atlasX && layer.sourceY == layer.atlasY &&
                                        (aliasX != layer.sourceX || aliasY != layer.sourceY);

            if (sourceUnset || sourceLooksLikeAtlas)
            {
                layer.sourceX = aliasX;
                layer.sourceY = aliasY;
            }

            if (layer.sourceWidth <= 0)
            {
                layer.sourceWidth = layer.atlasWidth;
            }

            if (layer.sourceHeight <= 0)
            {
                layer.sourceHeight = layer.atlasHeight;
            }
        }

        private static int FirstPositive(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0)
                {
                    return values[i];
                }
            }

            return 1;
        }

        private static int FirstDefinedCoordinate(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != 0)
                {
                    return values[i];
                }
            }

            return 0;
        }

        private static string GuessNameFromSource(string sourceRelativePath)
        {
            if (string.IsNullOrEmpty(sourceRelativePath))
            {
                return "Layer";
            }

            string file = Path.GetFileNameWithoutExtension(sourceRelativePath);
            return string.IsNullOrEmpty(file) ? "Layer" : file;
        }

        private static bool IsTextLayer(ManifestLayer layer)
        {
            if (layer == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(layer.type) && string.Equals(layer.type, "text", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrEmpty(layer.text);
        }

        private static string BuildLayerPath(ManifestLayer layer)
        {
            string parent = NormalizePath(layer.parentPath);
            string name = string.IsNullOrEmpty(layer.name) ? "Layer" : layer.name;
            return string.IsNullOrEmpty(parent) ? name : $"{parent}/{name}";
        }

        private static void BuildTreeFromHierarchy(HierarchyNode inputNode, ParsedNode outputParent, Dictionary<string, ParsedNode> pathToNode)
        {
            if (inputNode.children == null)
            {
                return;
            }

            for (int i = 0; i < inputNode.children.Length; i++)
            {
                HierarchyNode child = inputNode.children[i];
                if (child == null)
                {
                    continue;
                }

                string childName = string.IsNullOrEmpty(child.name) ? "Node" : child.name;
                string childPath = string.IsNullOrEmpty(outputParent.Path) ? childName : $"{outputParent.Path}/{childName}";

                var node = new ParsedNode
                {
                    Name = childName,
                    Type = string.IsNullOrEmpty(child.type) ? "group" : child.type,
                    Path = childPath,
                    Parent = outputParent
                };

                outputParent.Children.Add(node);
                pathToNode[childPath] = node;

                BuildTreeFromHierarchy(child, node, pathToNode);
            }
        }

        private static ParsedNode GetOrCreateGroupPath(Dictionary<string, ParsedNode> pathToNode, string groupPath)
        {
            if (pathToNode.TryGetValue(groupPath, out ParsedNode exists))
            {
                return exists;
            }

            string[] parts = groupPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = string.Empty;
            ParsedNode current = pathToNode[string.Empty];

            for (int i = 0; i < parts.Length; i++)
            {
                string segment = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";

                if (!pathToNode.TryGetValue(currentPath, out ParsedNode next))
                {
                    next = new ParsedNode
                    {
                        Name = segment,
                        Type = "group",
                        Path = currentPath,
                        Parent = current
                    };
                    current.Children.Add(next);
                    pathToNode[currentPath] = next;
                }

                current = next;
            }

            return current;
        }

        private static LayoutBounds CalculateBounds(List<ManifestLayer> layers)
        {
            bool initialized = false;
            var bounds = new LayoutBounds();

            for (int i = 0; i < layers.Count; i++)
            {
                ManifestLayer layer = layers[i];
                if (layer == null)
                {
                    continue;
                }

                int w = Mathf.Max(1, layer.sourceWidth);
                int h = Mathf.Max(1, layer.sourceHeight);
                float left = layer.sourceX;
                float top = layer.sourceY;
                float right = left + w;
                float bottom = top + h;

                if (!initialized)
                {
                    bounds.MinX = left;
                    bounds.MinY = top;
                    bounds.MaxX = right;
                    bounds.MaxY = bottom;
                    initialized = true;
                }
                else
                {
                    bounds.MinX = Mathf.Min(bounds.MinX, left);
                    bounds.MinY = Mathf.Min(bounds.MinY, top);
                    bounds.MaxX = Mathf.Max(bounds.MaxX, right);
                    bounds.MaxY = Mathf.Max(bounds.MaxY, bottom);
                }
            }

            if (!initialized)
            {
                bounds.MinX = 0f;
                bounds.MinY = 0f;
                bounds.MaxX = 1f;
                bounds.MaxY = 1f;
            }

            return bounds;
        }

        private void RefreshTree()
        {
            if (_treeView == null)
            {
                return;
            }

            if (_rootNode == null)
            {
                _treeView.SetRootItems(Array.Empty<TreeViewItemData<ParsedNode>>());
                _treeView.Rebuild();
                return;
            }

            int nextId = 1;
            TreeViewItemData<ParsedNode> rootItem = BuildTreeItem(_rootNode, ref nextId);
            _treeView.SetRootItems(new[] { rootItem });
            _treeView.ExpandRootItems();
            _treeView.Rebuild();
        }

        private static TreeViewItemData<ParsedNode> BuildTreeItem(ParsedNode node, ref int nextId)
        {
            int id = nextId++;
            var children = new List<TreeViewItemData<ParsedNode>>(node.Children.Count);
            for (int i = 0; i < node.Children.Count; i++)
            {
                children.Add(BuildTreeItem(node.Children[i], ref nextId));
            }

            return new TreeViewItemData<ParsedNode>(id, node, children);
        }

        private static string GetNodeTypeLabel(ParsedNode node)
        {
            if (node == null)
            {
                return "?";
            }

            if (string.Equals(node.Type, "group", StringComparison.OrdinalIgnoreCase))
            {
                return "G";
            }

            if (string.Equals(node.Type, "text", StringComparison.OrdinalIgnoreCase))
            {
                return "T";
            }

            return "L";
        }

        private void DrawPreview()
        {
            Rect full = GUILayoutUtility.GetRect(10f, 10000f, 10f, 10000f);
            EditorGUI.DrawRect(full, new Color(0.11f, 0.11f, 0.11f));

            if (_manifest == null || _atlasTexture == null || _orderedLayers.Count == 0)
            {
                GUI.Label(new Rect(full.x + 10f, full.y + 10f, 400f, 20f), "No parsed data.");
                return;
            }

            const float pad = 12f;
            float availableW = Mathf.Max(1f, full.width - pad * 2f);
            float availableH = Mathf.Max(1f, full.height - pad * 2f);
            float scale = Mathf.Min(availableW / _bounds.Width, availableH / _bounds.Height);
            scale = Mathf.Max(0.01f, scale);

            float drawW = _bounds.Width * scale;
            float drawH = _bounds.Height * scale;
            float startX = full.x + (full.width - drawW) * 0.5f;
            float startY = full.y + (full.height - drawH) * 0.5f;

            EditorGUI.DrawRect(new Rect(startX, startY, drawW, drawH), new Color(0.17f, 0.17f, 0.17f));

            for (int i = 0; i < _orderedLayers.Count; i++)
            {
                ManifestLayer layer = _orderedLayers[i];
                if (layer == null)
                {
                    continue;
                }

                float x = startX + (layer.sourceX - _bounds.MinX) * scale;
                float y = _sourceYTopDown
                    ? startY + (layer.sourceY - _bounds.MinY) * scale
                    : startY + (_bounds.MaxY - (layer.sourceY + layer.sourceHeight)) * scale;
                float w = Mathf.Max(1f, layer.sourceWidth * scale);
                float h = Mathf.Max(1f, layer.sourceHeight * scale);
                var rect = new Rect(x, y, w, h);

                if (IsTextLayer(layer))
                {
                    DrawPreviewText(layer, rect);
                }
                else
                {
                    DrawPreviewSprite(layer, rect);
                }
            }
        }

        private void DrawPreviewSprite(ManifestLayer layer, Rect rect)
        {
            if (layer.atlasWidth <= 0 || layer.atlasHeight <= 0)
            {
                EditorGUI.DrawRect(rect, new Color(0.4f, 0.2f, 0.2f, 0.8f));
                GUI.Label(rect, layer.name);
                return;
            }

            float texW = Mathf.Max(1f, _atlasTexture.width);
            float texH = Mathf.Max(1f, _atlasTexture.height);

            float u = layer.atlasX / texW;
            float v = _atlasYTopDown
                ? 1f - ((layer.atlasY + layer.atlasHeight) / texH)
                : layer.atlasY / texH;
            float uvW = layer.atlasWidth / texW;
            float uvH = layer.atlasHeight / texH;

            var uv = new Rect(u, v, uvW, uvH);
            GUI.DrawTextureWithTexCoords(rect, _atlasTexture, uv, true);
        }

        private static void DrawPreviewText(ManifestLayer layer, Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.2f, 0.35f, 0.85f));

            string content = string.IsNullOrEmpty(layer.text) ? layer.name : layer.text;
            var style = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(layer.fontSize > 0 ? layer.fontSize : 14, 8, 42),
                normal =
                {
                    textColor = ParseColor(layer.color, Color.white)
                }
            };

            GUI.Label(rect, content, style);
        }

        private void GenerateScene()
        {
            if (_manifest == null || _atlasTexture == null || _orderedLayers.Count == 0)
            {
                SetStatus("请先完成解析再生成。", true);
                return;
            }

            string atlasAssetPath = AssetDatabase.GetAssetPath(_atlasTexture);
            if (string.IsNullOrEmpty(atlasAssetPath))
            {
                SetStatus("Texture 必须是项目内资产。", true);
                return;
            }

            var imageLayers = new List<ManifestLayer>();
            bool hasText = false;
            for (int i = 0; i < _orderedLayers.Count; i++)
            {
                ManifestLayer layer = _orderedLayers[i];
                if (IsTextLayer(layer))
                {
                    hasText = true;
                }
                else
                {
                    imageLayers.Add(layer);
                }
            }

            Dictionary<string, Sprite> spriteMap = EnsureSpriteSubAssets(atlasAssetPath, imageLayers);
            if (spriteMap == null)
            {
                SetStatus("Sprite 切片失败。", true);
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("AtlasRebuildRoot");

            var spriteGroupMap = new Dictionary<string, Transform>(StringComparer.Ordinal)
            {
                [string.Empty] = root.transform
            };

            RectTransform canvasRoot = null;
            Dictionary<string, RectTransform> canvasGroupMap = null;
            if (hasText)
            {
                canvasRoot = CreateCanvasRoot(root.transform, _bounds);
                canvasGroupMap = new Dictionary<string, RectTransform>(StringComparer.Ordinal)
                {
                    [string.Empty] = canvasRoot
                };
                EnsureEventSystem();
            }

            int sortingOrder = 0;
            foreach (ManifestLayer layer in _orderedLayers)
            {
                string parentPath = NormalizePath(layer.parentPath);

                if (IsTextLayer(layer))
                {
                    if (canvasRoot == null)
                    {
                        continue;
                    }

                    RectTransform parentRect = GetOrCreateCanvasGroup(parentPath, canvasGroupMap, canvasRoot);
                    CreateTextObject(layer, parentRect, _bounds, _sourceYTopDown);
                    continue;
                }

                Transform parent = GetOrCreateSpriteGroup(parentPath, spriteGroupMap, root.transform);
                string spriteKey = BuildSpriteKey(layer);
                if (!spriteMap.TryGetValue(spriteKey, out Sprite sprite))
                {
                    Debug.LogWarning($"[AtlasRebuilder] Sprite not found for layer: {layer.name}, key: {spriteKey}");
                    continue;
                }

                CreateSpriteObject(layer, sprite, parent, _bounds, sortingOrder, _sourceYTopDown);
                sortingOrder++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeObject = root;
            SetStatus("Scene 生成完成。", false);
        }

        private Dictionary<string, Sprite> EnsureSpriteSubAssets(string atlasAssetPath, List<ManifestLayer> imageLayers)
        {
            var importer = AssetImporter.GetAtPath(atlasAssetPath) as UnityEditor.TextureImporter;
            if (importer == null)
            {
                return null;
            }

            var metas = new List<SpriteMetaData>(imageLayers.Count);
            int texHeight = 0;
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasAssetPath);
            if (tex != null)
            {
                texHeight = tex.height;
            }

            if (texHeight <= 0)
            {
                texHeight = 4096;
            }

            for (int i = 0; i < imageLayers.Count; i++)
            {
                ManifestLayer layer = imageLayers[i];
                int w = Mathf.Max(1, layer.atlasWidth);
                int h = Mathf.Max(1, layer.atlasHeight);

                float yBottom = _atlasYTopDown ? texHeight - layer.atlasY - h : layer.atlasY;
                yBottom = Mathf.Max(0f, yBottom);

                var meta = new SpriteMetaData
                {
                    name = BuildSpriteKey(layer),
                    rect = new Rect(layer.atlasX, yBottom, w, h),
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center
                };
                metas.Add(meta);
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();

            var result = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            UnityEngine.Object[] subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(atlasAssetPath);
            for (int i = 0; i < subs.Length; i++)
            {
                if (subs[i] is Sprite sprite)
                {
                    result[sprite.name] = sprite;
                }
            }

            return result;
        }

        private static void CreateSpriteObject(ManifestLayer layer, Sprite sprite, Transform parent, LayoutBounds bounds, int sortingOrder, bool sourceYTopDown)
        {
            var go = new GameObject(string.IsNullOrEmpty(layer.name) ? "Layer" : layer.name);
            go.transform.SetParent(parent, false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;

            float centerX = layer.sourceX + (layer.sourceWidth * 0.5f);
            float centerY = layer.sourceY + (layer.sourceHeight * 0.5f);

            float localX = centerX - bounds.CenterX;
            float localY = sourceYTopDown ? (bounds.CenterY - centerY) : (centerY - bounds.CenterY);

            float ppu = Mathf.Max(1f, sprite.pixelsPerUnit);
            go.transform.localPosition = new Vector3(localX / ppu, localY / ppu, 0f);
        }

        private static RectTransform CreateCanvasRoot(Transform parent, LayoutBounds bounds)
        {
            var canvasGo = new GameObject("Texts", typeof(RectTransform), typeof(Canvas));
            canvasGo.transform.SetParent(parent, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(bounds.Width, bounds.Height);
            rt.localScale = Vector3.one * 0.01f;

            canvasGo.transform.localPosition = Vector3.zero;
            return rt;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.hideFlags = HideFlags.None;
        }

        private static Transform GetOrCreateSpriteGroup(string path, Dictionary<string, Transform> cache, Transform root)
        {
            if (cache.TryGetValue(path, out Transform existing))
            {
                return existing;
            }

            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = string.Empty;
            Transform current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                if (!cache.TryGetValue(currentPath, out Transform child))
                {
                    var go = new GameObject(part);
                    go.transform.SetParent(current, false);
                    child = go.transform;
                    cache[currentPath] = child;
                }

                current = child;
            }

            return current;
        }

        private static RectTransform GetOrCreateCanvasGroup(string path, Dictionary<string, RectTransform> cache, RectTransform root)
        {
            if (cache.TryGetValue(path, out RectTransform existing))
            {
                return existing;
            }

            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = string.Empty;
            RectTransform current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                if (!cache.TryGetValue(currentPath, out RectTransform next))
                {
                    var go = new GameObject(part, typeof(RectTransform));
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(current, false);
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;

                    next = rt;
                    cache[currentPath] = next;
                }

                current = next;
            }

            return current;
        }

        private static void CreateTextObject(ManifestLayer layer, RectTransform parent, LayoutBounds bounds, bool sourceYTopDown)
        {
            var go = new GameObject(string.IsNullOrEmpty(layer.name) ? "Text" : layer.name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            float x = layer.sourceX - bounds.MinX;
            float y = sourceYTopDown
                ? (layer.sourceY - bounds.MinY)
                : (bounds.MaxY - (layer.sourceY + layer.sourceHeight));
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(Mathf.Max(1f, layer.sourceWidth), Mathf.Max(1f, layer.sourceHeight));

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = string.IsNullOrEmpty(layer.text) ? layer.name : layer.text;
            tmp.fontSize = layer.fontSize > 0 ? layer.fontSize : 24;
            tmp.color = ParseColor(layer.color, Color.white);
            tmp.alignment = ParseTmpAlignment(layer.alignment);
        }

        private static string BuildSpriteKey(ManifestLayer layer)
        {
            string path = BuildLayerPath(layer);
            return path.Replace("/", "__");
        }

        private static string NormalizePath(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return input.Replace('\\', '/').Trim('/');
        }

        private static Color ParseColor(string colorText, Color fallback)
        {
            if (string.IsNullOrEmpty(colorText))
            {
                return fallback;
            }

            if (ColorUtility.TryParseHtmlString(colorText, out Color result))
            {
                return result;
            }

            return fallback;
        }

        private static TextAlignmentOptions ParseTmpAlignment(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return TextAlignmentOptions.Midline;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "left":
                case "middleleft":
                    return TextAlignmentOptions.MidlineLeft;
                case "right":
                case "middleright":
                    return TextAlignmentOptions.MidlineRight;
                case "top":
                case "topcenter":
                    return TextAlignmentOptions.Top;
                case "bottom":
                case "bottomcenter":
                    return TextAlignmentOptions.Bottom;
                case "center":
                case "middle":
                case "midline":
                default:
                    return TextAlignmentOptions.Midline;
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (_statusLabel == null)
            {
                return;
            }

            _statusLabel.text = message;
            _statusLabel.style.color = isError
                ? new StyleColor(new Color(0.95f, 0.45f, 0.45f))
                : new StyleColor(new Color(0.78f, 0.9f, 0.78f));
        }
    }
}
