using System;
using System.Collections.Generic;

namespace Sparkling.DeepClicker
{
    internal class SimplePool<T> where T : class, new()
    {
        private readonly Stack<T> _objects;
        private readonly int _maxSize;

        public int Count => _objects.Count;

        internal SimplePool(int maxSize)
        {
            _maxSize = maxSize;
            _objects = new Stack<T>(maxSize);
        }

        public void Prepopulate(int count)
        {
            lock (_objects)
            {
                int amountToCreate = Math.Min(count, _maxSize - _objects.Count);

                for (int i = 0; i < amountToCreate; i++)
                {
                    _objects.Push(Activator.CreateInstance<T>());
                }
            }
        }

        public void Resize(int targetCount)
        {
            lock (_objects)
            {
                int finalTarget = Math.Min(targetCount, _maxSize);

                if (finalTarget > _objects.Count)
                {
                    int toAdd = finalTarget - _objects.Count;
                    for (int i = 0; i < toAdd; i++)
                    {
                        _objects.Push(Activator.CreateInstance<T>());
                    }
                }
                else if (finalTarget < _objects.Count)
                {
                    int toRemove = _objects.Count - finalTarget;
                    for (int i = 0; i < toRemove; i++)
                    {
                        _objects.Pop();
                    }
                }
            }
        }

        public T Rent()
        {
            lock (_objects)
            {
                if (_objects.Count > 0)
                {
                    return _objects.Pop();
                }
            }

            return Activator.CreateInstance<T>();
        }

        public void ReturnRange(IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                Return(item);
            }
        }

        public void Return(T item)
        {
            if (item == null)
            {
                return;
            }

            lock (_objects)
            {
                if (_objects.Count < _maxSize)
                {
                    _objects.Push(item);
                }
            }
        }
    }
}