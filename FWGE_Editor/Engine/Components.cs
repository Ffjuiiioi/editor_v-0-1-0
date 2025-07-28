using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FWGE_Editor.Engine
{
    // Component interfaces
    public interface IComponent
    {
        GameObject GameObject { get; set; }
        bool Enabled { get; set; }
        Dictionary<string, object> Serialize();
        void Deserialize(Dictionary<string, object> data);
    }

    public interface IUpdateable
    {
        void Update(float deltaTime);
    }

    public interface IStartable
    {
        void Start();
    }

    public interface IDestroyable
    {
        void Destroy();
    }

    public interface IRenderable
    {
        void Render();
    }

    // Base component class
    public abstract class Component : IComponent
    {
        private GameObject? _gameObject;
        private bool _enabled = true;

        public GameObject GameObject
        {
            get => _gameObject ?? throw new InvalidOperationException("GameObject is not set");
            set
            {
                if (_gameObject != value)
                {
                    _gameObject = value;
                    OnGameObjectChanged();
                }
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        protected virtual void OnGameObjectChanged() { }
        protected virtual void OnEnabledChanged() { }

        public virtual Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["Enabled"] = Enabled
            };
        }

        public virtual void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("Enabled"))
                Enabled = Convert.ToBoolean(data["Enabled"]);
        }
    }

    // Sprite Renderer Component
    public class SpriteRenderer : Component, IRenderable
    {
        private string _spritePath = string.Empty;
        private Vector2D _size;
        private Color _color;
        private bool _flipX;
        private bool _flipY;

        public string SpritePath
        {
            get => _spritePath;
            set
            {
                if (_spritePath != value)
                {
                    _spritePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector2D Size
        {
            get => _size;
            set
            {
                if (_size.X != value.X || _size.Y != value.Y)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool FlipX
        {
            get => _flipX;
            set
            {
                if (_flipX != value)
                {
                    _flipX = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool FlipY
        {
            get => _flipY;
            set
            {
                if (_flipY != value)
                {
                    _flipY = value;
                    OnPropertyChanged();
                }
            }
        }

        public SpriteRenderer()
        {
            _size = new Vector2D(100, 100);
            _color = new Color(255, 255, 255, 255);
        }

        public void Render()
        {
            if (!Enabled || GameObject == null) return;

            // Render sprite logic here
            // In a real implementation, this would draw to the graphics context
        }

        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["SpritePath"] = SpritePath;
            data["Size"] = Size;
            data["Color"] = Color;
            data["FlipX"] = FlipX;
            data["FlipY"] = FlipY;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("SpritePath"))
                SpritePath = data["SpritePath"].ToString();
            if (data.ContainsKey("Size"))
                Size = (Vector2D)data["Size"];
            if (data.ContainsKey("Color"))
                Color = (Color)data["Color"];
            if (data.ContainsKey("FlipX"))
                FlipX = Convert.ToBoolean(data["FlipX"]);
            if (data.ContainsKey("FlipY"))
                FlipY = Convert.ToBoolean(data["FlipY"]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Camera Component
    public class Camera : Component
    {
        private float _fieldOfView = 60f;
        private float _nearClipPlane = 0.1f;
        private float _farClipPlane = 1000f;
        private bool _orthographic = true;
        private float _orthographicSize = 5f;

        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView != value)
                {
                    _fieldOfView = value;
                    OnPropertyChanged();
                }
            }
        }

        public float NearClipPlane
        {
            get => _nearClipPlane;
            set
            {
                if (_nearClipPlane != value)
                {
                    _nearClipPlane = value;
                    OnPropertyChanged();
                }
            }
        }

        public float FarClipPlane
        {
            get => _farClipPlane;
            set
            {
                if (_farClipPlane != value)
                {
                    _farClipPlane = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Orthographic
        {
            get => _orthographic;
            set
            {
                if (_orthographic != value)
                {
                    _orthographic = value;
                    OnPropertyChanged();
                }
            }
        }

        public float OrthographicSize
        {
            get => _orthographicSize;
            set
            {
                if (_orthographicSize != value)
                {
                    _orthographicSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["FieldOfView"] = FieldOfView;
            data["NearClipPlane"] = NearClipPlane;
            data["FarClipPlane"] = FarClipPlane;
            data["Orthographic"] = Orthographic;
            data["OrthographicSize"] = OrthographicSize;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("FieldOfView"))
                FieldOfView = Convert.ToSingle(data["FieldOfView"]);
            if (data.ContainsKey("NearClipPlane"))
                NearClipPlane = Convert.ToSingle(data["NearClipPlane"]);
            if (data.ContainsKey("FarClipPlane"))
                FarClipPlane = Convert.ToSingle(data["FarClipPlane"]);
            if (data.ContainsKey("Orthographic"))
                Orthographic = Convert.ToBoolean(data["Orthographic"]);
            if (data.ContainsKey("OrthographicSize"))
                OrthographicSize = Convert.ToSingle(data["OrthographicSize"]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Rigidbody Component
    public class Rigidbody : Component, IUpdateable
    {
        private Vector3D _velocity;
        private Vector3D _angularVelocity;
        private float _mass = 1f;
        private float _drag = 0f;
        private float _angularDrag = 0.05f;
        private bool _useGravity = true;
        private bool _isKinematic = false;

        public Vector3D Velocity
        {
            get => _velocity;
            set
            {
                if (_velocity.X != value.X || _velocity.Y != value.Y || _velocity.Z != value.Z)
                {
                    _velocity = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector3D AngularVelocity
        {
            get => _angularVelocity;
            set
            {
                if (_angularVelocity.X != value.X || _angularVelocity.Y != value.Y || _angularVelocity.Z != value.Z)
                {
                    _angularVelocity = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Mass
        {
            get => _mass;
            set
            {
                if (_mass != value)
                {
                    _mass = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Drag
        {
            get => _drag;
            set
            {
                if (_drag != value)
                {
                    _drag = value;
                    OnPropertyChanged();
                }
            }
        }

        public float AngularDrag
        {
            get => _angularDrag;
            set
            {
                if (_angularDrag != value)
                {
                    _angularDrag = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseGravity
        {
            get => _useGravity;
            set
            {
                if (_useGravity != value)
                {
                    _useGravity = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsKinematic
        {
            get => _isKinematic;
            set
            {
                if (_isKinematic != value)
                {
                    _isKinematic = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Update(float deltaTime)
        {
            if (!Enabled || GameObject == null || IsKinematic) return;

            // Apply gravity
            if (UseGravity)
            {
                Velocity = new Vector3D(Velocity.X, Velocity.Y - 9.81f * deltaTime, Velocity.Z);
            }

            // Apply drag
            if (Drag > 0)
            {
                Velocity = new Vector3D(
                    Velocity.X * (1 - Drag * deltaTime),
                    Velocity.Y * (1 - Drag * deltaTime),
                    Velocity.Z * (1 - Drag * deltaTime)
                );
            }

            if (AngularDrag > 0)
            {
                AngularVelocity = new Vector3D(
                    AngularVelocity.X * (1 - AngularDrag * deltaTime),
                    AngularVelocity.Y * (1 - AngularDrag * deltaTime),
                    AngularVelocity.Z * (1 - AngularDrag * deltaTime)
                );
            }

            // Update transform
            GameObject.Transform.Translate(Velocity * deltaTime);
            GameObject.Transform.Rotate(AngularVelocity * deltaTime);
        }

        public void AddForce(Vector3D force)
        {
            if (!IsKinematic)
            {
                Velocity += force / Mass;
            }
        }

        public void AddTorque(Vector3D torque)
        {
            if (!IsKinematic)
            {
                AngularVelocity += torque / Mass;
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["Velocity"] = Velocity;
            data["AngularVelocity"] = AngularVelocity;
            data["Mass"] = Mass;
            data["Drag"] = Drag;
            data["AngularDrag"] = AngularDrag;
            data["UseGravity"] = UseGravity;
            data["IsKinematic"] = IsKinematic;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("Velocity"))
                Velocity = (Vector3D)data["Velocity"];
            if (data.ContainsKey("AngularVelocity"))
                AngularVelocity = (Vector3D)data["AngularVelocity"];
            if (data.ContainsKey("Mass"))
                Mass = Convert.ToSingle(data["Mass"]);
            if (data.ContainsKey("Drag"))
                Drag = Convert.ToSingle(data["Drag"]);
            if (data.ContainsKey("AngularDrag"))
                AngularDrag = Convert.ToSingle(data["AngularDrag"]);
            if (data.ContainsKey("UseGravity"))
                UseGravity = Convert.ToBoolean(data["UseGravity"]);
            if (data.ContainsKey("IsKinematic"))
                IsKinematic = Convert.ToBoolean(data["IsKinematic"]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Collider Component
    public class Collider : Component
    {
        private ColliderType _type = ColliderType.Box;
        private Vector3D _size = new Vector3D(1, 1, 1);
        private float _radius = 0.5f;
        private bool _isTrigger = false;

        public ColliderType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector3D Size
        {
            get => _size;
            set
            {
                if (_size.X != value.X || _size.Y != value.Y || _size.Z != value.Z)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTrigger
        {
            get => _isTrigger;
            set
            {
                if (_isTrigger != value)
                {
                    _isTrigger = value;
                    OnPropertyChanged();
                }
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["Type"] = Type.ToString();
            data["Size"] = Size;
            data["Radius"] = Radius;
            data["IsTrigger"] = IsTrigger;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("Type"))
                Type = Enum.Parse<ColliderType>(data["Type"]?.ToString() ?? "Box");
            if (data.ContainsKey("Size"))
                Size = (Vector3D)data["Size"];
            if (data.ContainsKey("Radius"))
                Radius = Convert.ToSingle(data["Radius"]);
            if (data.ContainsKey("IsTrigger"))
                IsTrigger = Convert.ToBoolean(data["IsTrigger"]);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ColliderType
    {
        Box,
        Circle,
        Polygon
    }

    // Color struct
    public struct Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color White => new Color(255, 255, 255);
        public static Color Black => new Color(0, 0, 0);
        public static Color Red => new Color(255, 0, 0);
        public static Color Green => new Color(0, 255, 0);
        public static Color Blue => new Color(0, 0, 255);
        
        // Comparison operators
        public static bool operator ==(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        public static bool operator !=(Color a, Color b) => !(a == b);
        
        public override bool Equals(object? obj)
        {
            if (obj is Color other)
                return this == other;
            return false;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }
    }

    // Component Factory
    public static class ComponentFactory
    {
        private static readonly Dictionary<string, Type> _componentTypes = new Dictionary<string, Type>
        {
            ["SpriteRenderer"] = typeof(SpriteRenderer),
            ["Camera"] = typeof(Camera),
            ["Rigidbody"] = typeof(Rigidbody),
            ["Collider"] = typeof(Collider)
        };

        public static IComponent? CreateComponent(string typeName, Dictionary<string, object>? data = null)
        {
            if (_componentTypes.TryGetValue(typeName, out Type? type))
            {
                var component = (IComponent?)Activator.CreateInstance(type);
                if (data != null && component != null)
                {
                    component.Deserialize(data);
                }
                return component;
            }
            return null;
        }

        public static void RegisterComponentType<T>(string name) where T : IComponent
        {
            _componentTypes[name] = typeof(T);
        }
    }
} 