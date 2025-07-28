using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace FWGE_Editor.Engine
{
    public class GameObject : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private GameObjectType _type;
        private Transform _transform = new Transform();
        private bool _isActive = true;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameObjectType Type
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

        public Transform Transform
        {
            get => _transform;
            set
            {
                if (_transform != value)
                {
                    _transform = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<IComponent> Components { get; set; } = new ObservableCollection<IComponent>();

        public GameObject(string name, GameObjectType type)
        {
            Name = name;
            Type = type;
            Transform = new Transform();
        }

        public void AddComponent(IComponent component)
        {
            if (component != null && !Components.Contains(component))
            {
                Components.Add(component);
                component.GameObject = this;
            }
        }

        public void RemoveComponent(IComponent component)
        {
            if (component != null && Components.Contains(component))
            {
                Components.Remove(component);
                component.GameObject = null!;
            }
        }

        public T? GetComponent<T>() where T : class, IComponent
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        public List<T> GetComponents<T>() where T : class, IComponent
        {
            return Components.OfType<T>().ToList();
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return Components.OfType<T>().Any();
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            foreach (var component in Components)
            {
                if (component is IUpdateable updateable)
                {
                    updateable.Update(deltaTime);
                }
            }
        }

        public void Start()
        {
            if (!IsActive) return;

            foreach (var component in Components)
            {
                if (component is IStartable startable)
                {
                    startable.Start();
                }
            }
        }

        public void Destroy()
        {
            foreach (var component in Components.ToList())
            {
                if (component is IDestroyable destroyable)
                {
                    destroyable.Destroy();
                }
            }
            Components.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum GameObjectType
    {
        Empty,
        Sprite,
        Camera,
        Light,
        UI,
        ParticleSystem,
        AudioSource,
        Collider
    }
} 