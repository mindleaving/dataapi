using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SharedViewModels.Repositories
{
    public class ConcurrentObservableCollection<T> : INotifyCollectionChanged, ICollection<T>
    {
        private readonly ObservableCollection<T> backendCollection;
        private readonly object modificationLock = new object();

        public ConcurrentObservableCollection()
        {
            backendCollection = new ObservableCollection<T>();
        }

        public ConcurrentObservableCollection(IEnumerable<T> items)
        {
            backendCollection = new ObservableCollection<T>(items);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { backendCollection.CollectionChanged += value; }
            remove { backendCollection.CollectionChanged -= value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<T> copy;
            lock (modificationLock)
            {
                copy = backendCollection.ToList();
            }
            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            lock (modificationLock)
            {
                backendCollection.Add(item);
            }
        }

        public void Clear()
        {
            lock (modificationLock)
            {
                backendCollection.Clear();
            }
        }

        public bool Contains(T item)
        {
            return backendCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            backendCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            lock (modificationLock)
            {
                return backendCollection.Remove(item);
            }
        }

        public int Count => backendCollection.Count;
        public bool IsReadOnly { get; } = false;

        public void Insert(int insertIndex, T item)
        {
            lock (modificationLock)
            {
                backendCollection.Insert(insertIndex, item);
            }
        }
    }
}
