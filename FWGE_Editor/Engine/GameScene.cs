using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic; // Added for List and Dictionary

namespace FWGE_Editor.Engine
{
    public class GameScene
    {
        public ObservableCollection<GameObject> Objects { get; set; } = new ObservableCollection<GameObject>();
        public string Name { get; set; } = "SampleScene";
        public Vector2D CameraPosition { get; set; } = new Vector2D(0, 0);
        public float CameraZoom { get; set; } = 1.0f;

        public GameScene()
        {
            // Add default camera
            var camera = new GameObject("Main Camera", GameObjectType.Camera);
            camera.Transform.Position = new Vector3D(0, 0, -10);
            Objects.Add(camera);
        }

        public void AddObject(GameObject gameObject)
        {
            Objects.Add(gameObject);
        }

        public void RemoveObject(GameObject gameObject)
        {
            Objects.Remove(gameObject);
        }

        public GameObject? FindObjectByName(string name)
        {
            return Objects.FirstOrDefault(obj => obj.Name == name);
        }

        public void SaveToFile(string path)
        {
            try
            {
                var sceneData = new SceneData
                {
                    Name = this.Name,
                    CameraPosition = this.CameraPosition,
                    CameraZoom = this.CameraZoom,
                    Objects = Objects.Select(obj => new GameObjectData
                    {
                        Name = obj.Name,
                        Type = obj.Type.ToString(),
                        Transform = new TransformData
                        {
                            Position = obj.Transform.Position,
                            Rotation = obj.Transform.Rotation,
                            Scale = obj.Transform.Scale
                        },
                        Components = obj.Components.Select(c => new ComponentData
                        {
                            Type = c.GetType().Name,
                            Data = c.Serialize()
                        }).ToList()
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(sceneData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения сцены: {ex.Message}");
            }
        }

        public static GameScene LoadFromFile(string path)
        {
            string? json = null;
            try
            {
                if (!File.Exists(path))
                {
                    return new GameScene();
                }

                json = File.ReadAllText(path);
                
                // Проверяем, что JSON не пустой и начинается с правильного символа
                if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                {
                    System.Diagnostics.Debug.WriteLine("JSON файл пустой или имеет неправильный формат");
                    return new GameScene();
                }

                // Дополнительная проверка на валидность JSON
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        System.Diagnostics.Debug.WriteLine("JSON не является объектом");
                        return new GameScene();
                    }
                }
                catch (JsonException)
                {
                    System.Diagnostics.Debug.WriteLine("JSON файл поврежден");
                    return new GameScene();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var sceneData = JsonSerializer.Deserialize<SceneData>(json, options);

                if (sceneData == null)
                {
                    return new GameScene();
                }

                var scene = new GameScene
                {
                    Name = sceneData.Name,
                    CameraPosition = sceneData.CameraPosition,
                    CameraZoom = sceneData.CameraZoom
                };

                scene.Objects.Clear();

                if (sceneData.Objects != null)
                {
                    foreach (var objData in sceneData.Objects)
                    {
                        if (objData == null) continue;

                        try
                        {
                            var gameObject = new GameObject(objData.Name, 
                                Enum.Parse<GameObjectType>(objData.Type))
                            {
                                Transform = new Transform
                                {
                                    Position = objData.Transform?.Position ?? new Vector3D(0, 0, 0),
                                    Rotation = objData.Transform?.Rotation ?? new Vector3D(0, 0, 0),
                                    Scale = objData.Transform?.Scale ?? new Vector3D(1, 1, 1)
                                }
                            };

                            // Load components
                            if (objData.Components != null)
                            {
                                foreach (var compData in objData.Components)
                                {
                                    if (compData == null) continue;

                                    try
                                    {
                                        var component = ComponentFactory.CreateComponent(compData.Type, compData.Data);
                                        if (component != null)
                                        {
                                            gameObject.AddComponent(component);
                                        }
                                    }
                                    catch (Exception compEx)
                                    {
                                        // Логируем ошибку компонента, но продолжаем загрузку
                                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки компонента: {compEx.Message}");
                                    }
                                }
                            }

                            scene.Objects.Add(gameObject);
                        }
                        catch (Exception objEx)
                        {
                            // Логируем ошибку объекта, но продолжаем загрузку
                            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки объекта: {objEx.Message}");
                        }
                    }
                }

                return scene;
            }
            catch (JsonException jsonEx)
            {
                // Если файл поврежден, возвращаем новую сцену
                System.Diagnostics.Debug.WriteLine($"JSON ошибка при загрузке сцены: {jsonEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Путь к файлу: {path}");
                System.Diagnostics.Debug.WriteLine($"Содержимое файла: {json?.Substring(0, Math.Min(100, json?.Length ?? 0))}");
                return new GameScene();
            }
            catch (Exception ex)
            {
                // Для других ошибок также возвращаем новую сцену
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сцены: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Путь к файлу: {path}");
                return new GameScene();
            }
        }
    }

    // Data classes for serialization
    public class SceneData
    {
        public string Name { get; set; } = string.Empty;
        public Vector2D CameraPosition { get; set; }
        public float CameraZoom { get; set; }
        public List<GameObjectData> Objects { get; set; } = new List<GameObjectData>();
    }

    public class GameObjectData
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public TransformData Transform { get; set; } = new TransformData();
        public List<ComponentData> Components { get; set; } = new List<ComponentData>();
    }

    public class TransformData
    {
        public Vector3D Position { get; set; }
        public Vector3D Rotation { get; set; }
        public Vector3D Scale { get; set; }
    }

    public class ComponentData
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
} 