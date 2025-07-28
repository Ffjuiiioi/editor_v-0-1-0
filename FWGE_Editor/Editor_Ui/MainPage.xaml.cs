using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using FWGE_Editor.Engine;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using WpfColor = System.Windows.Media.Color;
using WpfRectangle = System.Windows.Shapes.Rectangle;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfPath = System.IO.Path;

namespace FWGE_Editor
{
    public partial class MainPage : System.Windows.Controls.UserControl
    {
        private string _currentProjectPath = string.Empty;
        private string _currentProjectName = string.Empty;
        private GameScene? _currentScene;
        private GameObject? _selectedObject;
        private ToolType _currentTool = ToolType.Select;
        private bool _isDragging = false;
        private System.Windows.Point _dragStart;
        private System.Windows.Point _lastMousePosition;

        // Game state
        private bool _isPlaying = false;
        private bool _isPaused = false;

        public enum ToolType
        {
            Select,
            Move,
            Rotate,
            Scale
        }

        public MainPage()
        {
            InitializeComponent();
            InitializeEditor();
            SetupToolButtons();
            CreateDefaultScene();
        }

        private void InitializeEditor()
        {
            UpdateProjectInfo();
            RefreshHierarchy();
            RefreshSceneView();
            UpdateStatus("Editor initialized");
        }

        private void SetupToolButtons()
        {
            SetCurrentTool(ToolType.Select);
        }

        public void LoadProject(string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    System.Windows.MessageBox.Show("Путь к проекту не указан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(projectPath))
                {
                    System.Windows.MessageBox.Show($"Проект не найден: {projectPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем, что это действительно директория проекта
                string projectSettingsPath = WpfPath.Combine(projectPath, "ProjectSettings");
                if (!Directory.Exists(projectSettingsPath))
                {
                    System.Windows.MessageBox.Show($"Директория не является проектом FWGE: {projectPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentProjectPath = projectPath;
                _currentProjectName = WpfPath.GetFileName(projectPath);
                
                // Очищаем поврежденные файлы перед загрузкой
                CleanupCorruptedFiles(projectPath);
                
                UpdateProjectInfo();
                LoadSceneFromProject();
                
                System.Windows.MessageBox.Show($"Проект '{_currentProjectName}' успешно загружен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки проекта: {ex.Message}\nПопробуйте открыть проект снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Сбрасываем состояние редактора
                _currentProjectPath = string.Empty;
                _currentProjectName = string.Empty;
                CreateDefaultScene();
                
                UpdateProjectInfo();
                RefreshHierarchy();
                RefreshSceneView();
            }
        }

        private void LoadSceneFromProject()
        {
            try
            {
                // Load scene data from project
                string scenePath = WpfPath.Combine(_currentProjectPath, "Assets", "Scenes", "SampleScene.unity");
                string backupPath = scenePath + ".backup";
                
                if (File.Exists(scenePath))
                {
                    try
                    {
                        // Load scene data
                        _currentScene = GameScene.LoadFromFile(scenePath);
                    }
                    catch
                    {
                        // Если основной файл поврежден, пробуем восстановить из резервной копии
                        if (File.Exists(backupPath))
                        {
                            try
                            {
                                _currentScene = GameScene.LoadFromFile(backupPath);
                                // Восстанавливаем основной файл из резервной копии
                                File.Copy(backupPath, scenePath, true);
                                System.Windows.MessageBox.Show("Сцена восстановлена из резервной копии.", "Восстановление", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch
                            {
                                // Если и резервная копия повреждена, создаем новую сцену
                                CreateDefaultScene();
                            }
                        }
                        else
                        {
                            CreateDefaultScene();
                        }
                    }
                }
                else
                {
                    // Create default scene
                    CreateDefaultScene();
                }

                RefreshHierarchy();
                RefreshSceneView();
            }
            catch (Exception ex)
            {
                // Если загрузка сцены не удалась, создаем новую
                System.Windows.MessageBox.Show($"Ошибка загрузки сцены: {ex.Message}\nСоздана новая сцена.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                CreateDefaultScene();
                RefreshHierarchy();
                RefreshSceneView();
            }
        }

        private void CreateDefaultScene()
        {
            _currentScene = new GameScene();
            _currentScene.AddObject(new GameObject("Main Camera", GameObjectType.Camera));
        }

        private void CleanupCorruptedFiles(string projectPath)
        {
            try
            {
                string scenesPath = WpfPath.Combine(projectPath, "Assets", "Scenes");
                if (Directory.Exists(scenesPath))
                {
                    string scenePath = WpfPath.Combine(scenesPath, "SampleScene.unity");
                    string backupPath = scenePath + ".backup";
                    
                    // Если основной файл поврежден, а резервная копия существует, восстанавливаем
                    if (File.Exists(scenePath) && File.Exists(backupPath))
                    {
                        try
                        {
                            // Проверяем основной файл
                            string content = File.ReadAllText(scenePath);
                            if (string.IsNullOrWhiteSpace(content) || !content.TrimStart().StartsWith("{"))
                            {
                                File.Copy(backupPath, scenePath, true);
                                System.Diagnostics.Debug.WriteLine("Восстановлен поврежденный файл сцены из резервной копии");
                            }
                        }
                        catch
                        {
                            // Если основной файл поврежден, восстанавливаем из резервной копии
                            File.Copy(backupPath, scenePath, true);
                            System.Diagnostics.Debug.WriteLine("Восстановлен поврежденный файл сцены из резервной копии");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при очистке поврежденных файлов: {ex.Message}");
            }
        }

        private void UpdateProjectInfo()
        {
            if (string.IsNullOrEmpty(_currentProjectName))
            {
                ProjectInfoText.Text = "No Project";
            }
            else
            {
                ProjectInfoText.Text = $"{_currentProjectName}";
            }
        }

        private void RefreshHierarchy()
        {
            HierarchyTreeView.Items.Clear();
            
            if (_currentScene?.Objects == null) return;
            
            foreach (var gameObject in _currentScene.Objects)
            {
                var treeItem = new TreeViewItem
                {
                    Header = gameObject.Name,
                    Tag = gameObject,
                    IsExpanded = true
                };
                HierarchyTreeView.Items.Add(treeItem);
            }
            
            UpdateObjectsCount();
        }

        private void RefreshSceneView()
        {
            SceneCanvas.Children.Clear();
            
            if (_currentScene?.Objects == null) return;
            
            foreach (var gameObject in _currentScene.Objects)
            {
                if (gameObject.Type == GameObjectType.Sprite)
                {
                    var sprite = CreateSpriteVisual(gameObject);
                    SceneCanvas.Children.Add(sprite);
                }
                else if (gameObject.Type == GameObjectType.Camera)
                {
                    var camera = CreateCameraVisual(gameObject);
                    SceneCanvas.Children.Add(camera);
                }
            }
            
            UpdateObjectsCount();
        }

        private void UpdateObjectsCount()
        {
            int count = _currentScene?.Objects?.Count ?? 0;
            ObjectsCountText.Text = $"Objects: {count}";
        }

        private UIElement CreateSpriteVisual(GameObject gameObject)
        {
            var rectangle = new WpfRectangle
            {
                Width = gameObject.Transform.Scale.X * 50,
                Height = gameObject.Transform.Scale.Y * 50,
                Fill = new SolidColorBrush(WpfColor.FromRgb(0x4C, 0xAF, 0x50)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Tag = gameObject
            };

            // Add glow effect for selected objects
            if (_selectedObject == gameObject)
            {
                rectangle.Effect = new DropShadowEffect
                {
                    Color = WpfColor.FromRgb(0x00, 0x7A, 0xCC),
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
                rectangle.Stroke = new SolidColorBrush(WpfColor.FromRgb(0x00, 0x7A, 0xCC));
                rectangle.StrokeThickness = 3;
            }

            Canvas.SetLeft(rectangle, gameObject.Transform.Position.X);
            Canvas.SetTop(rectangle, gameObject.Transform.Position.Y);
            
            var transform = new RotateTransform(gameObject.Transform.Rotation.Z);
            rectangle.RenderTransform = transform;

            return rectangle;
        }

        private UIElement CreateCameraVisual(GameObject gameObject)
        {
            var ellipse = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = new SolidColorBrush(WpfColor.FromRgb(0xFF, 0x98, 0x00)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Tag = gameObject
            };

            // Add glow effect for selected objects
            if (_selectedObject == gameObject)
            {
                ellipse.Effect = new DropShadowEffect
                {
                    Color = WpfColor.FromRgb(0xFF, 0x98, 0x00),
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
                ellipse.Stroke = new SolidColorBrush(WpfColor.FromRgb(0xFF, 0x98, 0x00));
                ellipse.StrokeThickness = 3;
            }

            Canvas.SetLeft(ellipse, gameObject.Transform.Position.X);
            Canvas.SetTop(ellipse, gameObject.Transform.Position.Y);

            return ellipse;
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        private void UpdateMousePosition(System.Windows.Point position)
        {
            MousePositionText.Text = $"Mouse: ({(int)position.X}, {(int)position.Y})";
        }

        private void UpdateSelectedObject()
        {
            if (_selectedObject != null)
            {
                SelectedObjectText.Text = $"Selected: {_selectedObject.Name}";
                RefreshInspector();
            }
            else
            {
                SelectedObjectText.Text = "Selected: None";
                ClearInspector();
            }
        }

        private void RefreshInspector()
        {
            InspectorPanel.Children.Clear();

            if (_selectedObject == null) return;

            // GameObject Info
            var gameObjectSection = CreateInspectorSection("GameObject");
            var namePanel = CreateStringPropertyPanel("Name", _selectedObject.Name, (value) => _selectedObject.Name = value);
            gameObjectSection.Child = namePanel;
            InspectorPanel.Children.Add(gameObjectSection);

            // Transform Section
            var transformSection = CreateInspectorSection("Transform");
            
            // Position
            var positionPanel = CreatePropertyPanel("Position", _selectedObject.Transform.Position);
            transformSection.Child = positionPanel;
            InspectorPanel.Children.Add(transformSection);

            // Rotation
            var rotationSection = CreateInspectorSection("Rotation");
            var rotationPanel = CreatePropertyPanel("Rotation", _selectedObject.Transform.Rotation);
            rotationSection.Child = rotationPanel;
            InspectorPanel.Children.Add(rotationSection);

            // Scale
            var scaleSection = CreateInspectorSection("Scale");
            var scalePanel = CreatePropertyPanel("Scale", _selectedObject.Transform.Scale);
            scaleSection.Child = scalePanel;
            InspectorPanel.Children.Add(scaleSection);

            // Components
            if (_selectedObject.Components.Count > 0)
            {
                var componentsSection = CreateInspectorSection("Components");
                var componentsPanel = new StackPanel();
                
                foreach (var component in _selectedObject.Components)
                {
                    var componentText = new TextBlock
                    {
                        Text = component.GetType().Name,
                        Foreground = new SolidColorBrush(WpfColor.FromRgb(0x4C, 0xAF, 0x50)),
                        Margin = new Thickness(0, 2, 0, 2),
                        FontWeight = FontWeights.Medium
                    };
                    componentsPanel.Children.Add(componentText);
                }
                
                componentsSection.Child = componentsPanel;
                InspectorPanel.Children.Add(componentsSection);
            }
        }

        private Border CreateInspectorSection(string title)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(WpfColor.FromRgb(0x2D, 0x2D, 0x30)),
                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x3F, 0x3F, 0x46)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 4, 0, 4)
            };

            var header = new TextBlock
            {
                Text = title,
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0xCC, 0xCC, 0xCC)),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 6, 8, 6)
            };

            border.Child = header;
            return border;
        }

        private StackPanel CreatePropertyPanel(string propertyName, Vector3D value)
        {
            var panel = new StackPanel { Orientation = WpfOrientation.Vertical };
            
            var header = new TextBlock
            {
                Text = propertyName,
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0xCC, 0xCC, 0xCC)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 4, 0, 4)
            };
            panel.Children.Add(header);

            var inputPanel = new StackPanel { Orientation = WpfOrientation.Horizontal };
            
            inputPanel.Children.Add(CreateNumberInput("X", value.X, (v) => UpdateTransformProperty(propertyName, "X", v)));
            inputPanel.Children.Add(CreateNumberInput("Y", value.Y, (v) => UpdateTransformProperty(propertyName, "Y", v)));
            inputPanel.Children.Add(CreateNumberInput("Z", value.Z, (v) => UpdateTransformProperty(propertyName, "Z", v)));
            
            panel.Children.Add(inputPanel);
            return panel;
        }

        private StackPanel CreateStringPropertyPanel(string propertyName, string value, Action<string> onChanged)
        {
            var panel = new StackPanel { Orientation = WpfOrientation.Vertical };
            
            var header = new TextBlock
            {
                Text = propertyName,
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0xCC, 0xCC, 0xCC)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 4, 0, 4)
            };
            panel.Children.Add(header);

            var textBox = new WpfTextBox
            {
                Text = value,
                Background = new SolidColorBrush(WpfColor.FromRgb(0x1E, 0x1E, 0x1E)),
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x3F, 0x3F, 0x46)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Margin = new Thickness(0, 2, 0, 4)
            };
            
            textBox.TextChanged += (s, e) => onChanged(textBox.Text);
            panel.Children.Add(textBox);
            
            return panel;
        }

        private StackPanel CreateNumberInput(string axis, double value, Action<double> onChanged)
        {
            var panel = new StackPanel { Orientation = WpfOrientation.Vertical };
            
            var label = new TextBlock
            {
                Text = axis,
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0x99, 0x99, 0x99)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontSize = 10
            };
            panel.Children.Add(label);

            var textBox = new WpfTextBox
            {
                Text = value.ToString("F2"),
                Background = new SolidColorBrush(WpfColor.FromRgb(0x1E, 0x1E, 0x1E)),
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x3F, 0x3F, 0x46)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Margin = new Thickness(2, 2, 2, 4),
                Width = 60,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            
            textBox.TextChanged += (s, e) => 
            {
                if (double.TryParse(textBox.Text, out double result))
                {
                    onChanged(result);
                }
            };
            panel.Children.Add(textBox);
            
            return panel;
        }

        private void UpdateTransformProperty(string property, string axis, double value)
        {
            if (_selectedObject?.Transform == null) return;

            switch (property)
            {
                case "Position":
                    var currentPos = _selectedObject.Transform.Position;
                    switch (axis)
                    {
                        case "X": _selectedObject.Transform.Position = new Vector3D((float)value, currentPos.Y, currentPos.Z); break;
                        case "Y": _selectedObject.Transform.Position = new Vector3D(currentPos.X, (float)value, currentPos.Z); break;
                        case "Z": _selectedObject.Transform.Position = new Vector3D(currentPos.X, currentPos.Y, (float)value); break;
                    }
                    break;
                case "Rotation":
                    var currentRot = _selectedObject.Transform.Rotation;
                    switch (axis)
                    {
                        case "X": _selectedObject.Transform.Rotation = new Vector3D((float)value, currentRot.Y, currentRot.Z); break;
                        case "Y": _selectedObject.Transform.Rotation = new Vector3D(currentRot.X, (float)value, currentRot.Z); break;
                        case "Z": _selectedObject.Transform.Rotation = new Vector3D(currentRot.X, currentRot.Y, (float)value); break;
                    }
                    break;
                case "Scale":
                    var currentScale = _selectedObject.Transform.Scale;
                    switch (axis)
                    {
                        case "X": _selectedObject.Transform.Scale = new Vector3D((float)value, currentScale.Y, currentScale.Z); break;
                        case "Y": _selectedObject.Transform.Scale = new Vector3D(currentScale.X, (float)value, currentScale.Z); break;
                        case "Z": _selectedObject.Transform.Scale = new Vector3D(currentScale.X, currentScale.Y, (float)value); break;
                    }
                    break;
            }

            RefreshSceneView();
        }

        private void ClearInspector()
        {
            InspectorPanel.Children.Clear();
            
            var message = new TextBlock
            {
                Text = "Select an object to edit its properties",
                Foreground = new SolidColorBrush(WpfColor.FromRgb(0x99, 0x99, 0x99)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
            
            InspectorPanel.Children.Add(message);
        }

        // Tool selection handlers
        private void SelectTool_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentTool(ToolType.Select);
        }

        private void MoveTool_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentTool(ToolType.Move);
        }

        private void RotateTool_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentTool(ToolType.Rotate);
        }

        private void ScaleTool_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentTool(ToolType.Scale);
        }

        private void SetCurrentTool(ToolType tool)
        {
            _currentTool = tool;
            
            // Update button styles
            SelectToolButton.Background = tool == ToolType.Select ? new SolidColorBrush(WpfColor.FromRgb(0x09, 0x47, 0x71)) : null;
            MoveToolButton.Background = tool == ToolType.Move ? new SolidColorBrush(WpfColor.FromRgb(0x09, 0x47, 0x71)) : null;
            RotateToolButton.Background = tool == ToolType.Rotate ? new SolidColorBrush(WpfColor.FromRgb(0x09, 0x47, 0x71)) : null;
            ScaleToolButton.Background = tool == ToolType.Scale ? new SolidColorBrush(WpfColor.FromRgb(0x09, 0x47, 0x71)) : null;
            
            UpdateStatus($"Tool: {tool}");
        }

        // Object creation handlers
        private void CreateSprite_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null) return;
            
            var sprite = new GameObject("Sprite", GameObjectType.Sprite);
            sprite.Transform.Position = new Vector3D(100, 100, 0);
            _currentScene.AddObject(sprite);
            RefreshHierarchy();
            RefreshSceneView();
            UpdateStatus("Created new sprite");
        }

        private void CreateGameObject_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null) return;
            
            var gameObject = new GameObject("GameObject", GameObjectType.Empty);
            gameObject.Transform.Position = new Vector3D(150, 150, 0);
            _currentScene.AddObject(gameObject);
            RefreshHierarchy();
            RefreshSceneView();
            UpdateStatus("Created new object");
        }

        private void CreateCamera_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null) return;
            
            var camera = new GameObject("Camera", GameObjectType.Camera);
            camera.Transform.Position = new Vector3D(200, 200, 0);
            _currentScene.AddObject(camera);
            RefreshHierarchy();
            RefreshSceneView();
            UpdateStatus("Created new camera");
        }

        // Play controls
        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                _isPaused = false;
                PlayButton.Content = "⏸️ Pause";
                UpdateStatus("Game started");
            }
            else if (_isPaused)
            {
                _isPaused = false;
                PlayButton.Content = "⏸️ Pause";
                UpdateStatus("Game resumed");
            }
        }

        private void PauseGame_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
                PlayButton.Content = "▶️ Play";
                UpdateStatus("Game paused");
            }
        }

        private void StopGame_Click(object sender, RoutedEventArgs e)
        {
            _isPlaying = false;
            _isPaused = false;
            PlayButton.Content = "▶️ Play";
            UpdateStatus("Game stopped");
        }

        // File operations
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select project folder"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadProject(dialog.SelectedPath);
            }
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentProjectPath) || _currentScene == null)
            {
                System.Windows.MessageBox.Show("Please open a project first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveSceneToProject();
                System.Windows.MessageBox.Show("Project saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatus("Project saved");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSceneToProject()
        {
            try
            {
                if (_currentScene == null) return;
                
                string scenesPath = WpfPath.Combine(_currentProjectPath, "Assets", "Scenes");
                Directory.CreateDirectory(scenesPath);
                
                string scenePath = WpfPath.Combine(scenesPath, "SampleScene.unity");
                
                // Создаем резервную копию, если файл существует
                if (File.Exists(scenePath))
                {
                    string backupPath = scenePath + ".backup";
                    File.Copy(scenePath, backupPath, true);
                }
                
                _currentScene.SaveToFile(scenePath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения сцены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildGame_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentProjectPath) || _currentScene == null)
            {
                System.Windows.MessageBox.Show("Please open a project first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Build game logic here
                System.Windows.MessageBox.Show("Game built!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatus("Game built");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Build error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hierarchy events
        private void HierarchyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem && selectedItem?.Tag is GameObject gameObject)
            {
                _selectedObject = gameObject;
                UpdateSelectedObject();
            }
        }

        private void AddToHierarchy_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null) return;
            
            var name = Interaction.InputBox("Enter object name:", "Create Object", "NewObject") ?? "NewObject";
            if (!string.IsNullOrEmpty(name))
            {
                var gameObject = new GameObject(name, GameObjectType.Empty);
                _currentScene.AddObject(gameObject);
                RefreshHierarchy();
                RefreshSceneView();
                UpdateStatus($"Created object: {name}");
            }
        }

        // Scene canvas events
        private void SceneCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(SceneCanvas);
            _lastMousePosition = _dragStart;
            
            // Check if clicking on an object
            var hit = VisualTreeHelper.HitTest(SceneCanvas, e.GetPosition(SceneCanvas));
            if (hit?.VisualHit is FrameworkElement element && element?.Tag is GameObject gameObject)
            {
                _selectedObject = gameObject;
                UpdateSelectedObject();
            }
            else
            {
                _selectedObject = null;
                UpdateSelectedObject();
            }
        }

        private void SceneCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(SceneCanvas);
            UpdateMousePosition(position);

            if (_isDragging && _selectedObject?.Transform != null && _currentTool != ToolType.Select)
            {
                var delta = position - _lastMousePosition;
                
                switch (_currentTool)
                {
                    case ToolType.Move:
                        var currentPos = _selectedObject.Transform.Position;
                        _selectedObject.Transform.Position = new Vector3D(currentPos.X + (float)delta.X, currentPos.Y + (float)delta.Y, currentPos.Z);
                        break;
                    case ToolType.Rotate:
                        var currentRot = _selectedObject.Transform.Rotation;
                        _selectedObject.Transform.Rotation = new Vector3D(currentRot.X, currentRot.Y, currentRot.Z + (float)delta.X);
                        break;
                    case ToolType.Scale:
                        var currentScale = _selectedObject.Transform.Scale;
                        var scaleFactor = 1.0f + (float)delta.X * 0.01f;
                        _selectedObject.Transform.Scale = new Vector3D(currentScale.X * scaleFactor, currentScale.Y * scaleFactor, currentScale.Z);
                        break;
                }
                
                RefreshSceneView();
                RefreshInspector();
            }
            
            _lastMousePosition = position;
        }

        private void SceneCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }
    }
}