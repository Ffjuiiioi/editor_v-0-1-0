using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FWGE_Editor.Engine
{
    public class Transform : INotifyPropertyChanged
    {
        private Vector3D _position;
        private Vector3D _rotation;
        private Vector3D _scale;
        private Transform? _parent;

        public Vector3D Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                    OnTransformChanged();
                }
            }
        }

        public Vector3D Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged();
                    OnTransformChanged();
                }
            }
        }

        public Vector3D Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();
                    OnTransformChanged();
                }
            }
        }

        public Transform? Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged();
                    OnTransformChanged();
                }
            }
        }

        public Transform()
        {
            _position = new Vector3D(0, 0, 0);
            _rotation = new Vector3D(0, 0, 0);
            _scale = new Vector3D(1, 1, 1);
        }

        public Vector3D GetWorldPosition()
        {
            if (Parent == null)
                return Position;

            var parentWorldPos = Parent.GetWorldPosition();
            var parentWorldRot = Parent.GetWorldRotation();
            var parentWorldScale = Parent.GetWorldScale();

            // Apply parent transformation
            var worldPos = Position;
            worldPos = Vector3D.Multiply(worldPos, parentWorldScale);
            worldPos = Vector3D.Rotate(worldPos, parentWorldRot);
            worldPos = Vector3D.Add(worldPos, parentWorldPos);

            return worldPos;
        }

        public Vector3D GetWorldRotation()
        {
            if (Parent == null)
                return Rotation;

            var parentWorldRot = Parent.GetWorldRotation();
            return Vector3D.Add(Rotation, parentWorldRot);
        }

        public Vector3D GetWorldScale()
        {
            if (Parent == null)
                return Scale;

            var parentWorldScale = Parent.GetWorldScale();
            return Vector3D.Multiply(Scale, parentWorldScale);
        }

        public void Translate(Vector3D translation)
        {
            Position = Vector3D.Add(Position, translation);
        }

        public void Rotate(Vector3D rotation)
        {
            Rotation = Vector3D.Add(Rotation, rotation);
        }

        public void ScaleBy(Vector3D scale)
        {
            Scale = Vector3D.Multiply(Scale, scale);
        }

        public void LookAt(Vector3D target)
        {
            var direction = Vector3D.Subtract(target, Position);
            var angle = Math.Atan2(direction.Y, direction.X);
            Rotation = new Vector3D(0, 0, (float)(angle * 180 / Math.PI));
        }

        public Vector3D Forward
        {
            get
            {
                var radians = Rotation.Z * Math.PI / 180;
                return new Vector3D(
                    (float)Math.Cos(radians),
                    (float)Math.Sin(radians),
                    0
                );
            }
        }

        public Vector3D Right
        {
            get
            {
                var radians = (Rotation.Z + 90) * Math.PI / 180;
                return new Vector3D(
                    (float)Math.Cos(radians),
                    (float)Math.Sin(radians),
                    0
                );
            }
        }

        public Vector3D Up
        {
            get
            {
                var radians = (Rotation.Z + 90) * Math.PI / 180;
                return new Vector3D(
                    (float)Math.Cos(radians),
                    (float)Math.Sin(radians),
                    0
                );
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? TransformChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnTransformChanged()
        {
            TransformChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public struct Vector3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3D Add(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3D Subtract(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3D Multiply(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3D Divide(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b) => Add(a, b);
        public static Vector3D operator -(Vector3D a, Vector3D b) => Subtract(a, b);
        public static Vector3D operator *(Vector3D a, Vector3D b) => Multiply(a, b);
        public static Vector3D operator /(Vector3D a, Vector3D b) => Divide(a, b);
        
        // Scalar operations
        public static Vector3D operator *(Vector3D a, float scalar) => new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Vector3D operator /(Vector3D a, float scalar) => new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
        
        // Comparison operators
        public static bool operator ==(Vector3D a, Vector3D b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(Vector3D a, Vector3D b) => !(a == b);
        
        public override bool Equals(object? obj)
        {
            if (obj is Vector3D other)
                return this == other;
            return false;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3D Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag == 0) return new Vector3D(0, 0, 0);
                return new Vector3D(X / mag, Y / mag, Z / mag);
            }
        }

        public static Vector3D Rotate(Vector3D vector, Vector3D rotation)
        {
            // Simple 2D rotation around Z axis
            var radians = rotation.Z * Math.PI / 180;
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            return new Vector3D(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos,
                vector.Z
            );
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
    }

    public struct Vector2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D Add(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D Subtract(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b) => Add(a, b);
        public static Vector2D operator -(Vector2D a, Vector2D b) => Subtract(a, b);

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

        public Vector2D Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag == 0) return new Vector2D(0, 0);
                return new Vector2D(X / mag, Y / mag);
            }
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }
    }
} 