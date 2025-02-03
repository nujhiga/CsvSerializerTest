using CsvFileHandler.IO;

using System.Reflection;

namespace CsvSerializer;

public sealed class CsvCollection<T> where T : class, new()
{
    private readonly List<T> _list;
    private readonly int _maxCapacity;
    private readonly Lock _lock = new Lock();
    public CsvCollection()
    {
        _list = [];
    }
    public CsvCollection(int maxCapacity)
    {
        if (maxCapacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(maxCapacity));

        _maxCapacity = maxCapacity;
        _list = new List<T>(maxCapacity);
    }
    public CsvCollection(T[] objs)
    {
        ArgumentNullException.ThrowIfNull(objs);
        int maxCapacity = objs.Length;

        if (maxCapacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(maxCapacity));

        _maxCapacity = maxCapacity;
        _list = new List<T>(objs);
        _list.EnsureCapacity(maxCapacity);
    }
    public CsvCollection(IEnumerable<T> objs)
    {
        ArgumentNullException.ThrowIfNull(objs);
        int maxCapacity = 0;
        _list = [];

        foreach (var item in objs)
        {
            _list.Add(item);
            maxCapacity++;
        }

        if (maxCapacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(maxCapacity));

        _maxCapacity = maxCapacity;
        _list.EnsureCapacity(maxCapacity);
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _list.Count;
            }
        }
    }
    public bool TryAdd(T item)
    {
        lock (_lock)
        {
            if (_list.Count >= _maxCapacity) return false;

            _list.Add(item);
            return true;

            //if (_list.Count < _maxCapacity)
            //{
            //    _list.Add(item);
            //    return true;
            //}
            //return false;  
        }
    }
    public void Remove()
    {
        //lock (_lock)
        //{
        //    if (_list.Count == 0) return;

        //    _list.RemoveAt(_list.Count - 1);
        //    _list.TrimExcess();
        //}
        Remove(Count - 1);
    }
    public void Remove(int index)
    {
        lock (_lock)
        {
            if (_list.Count == 0) return;
            if (index < 0 || index > _list.Count - 1) return;

            _list.RemoveAt(index);
            _list.TrimExcess();
        }
    }
    public bool TryRemove(out T? item)
    {
        //lock (_lock)
        //{
        //    item = null;
        //    if (_list.Count == 0) return false;

        //    item = _list[_list.Count - 1];
        //    _list.RemoveAt(_list.Count - 1);
        //    _list.TrimExcess();
        //    return true;
        //}
        return TryRemove(out item, Count - 1);
    }
    public bool TryRemove(out T? item, int index)
    {
        lock (_lock)
        {
            item = null;
            if (_list.Count == 0) return false;
            if (index < 0 || index > _list.Count - 1) return false;

            item = _list[index];
            _list.RemoveAt(index);
            _list.TrimExcess();
            return true;
        }
    }
    public T? Find(Predicate<T> predicate)
    {
        if (_list.Count == 0) return null;

        lock (_lock)
        {
            T? obj = _list.Find(predicate);
            return obj;
        }
    }
    public bool TryFind(Predicate<T> predicate, out T? obj)
    {
        obj = Find(predicate);
        return obj is not null;
    }
    public T? Peek(Func<T, bool> predicate)
    {
        if (_list.Count == 0) return null;
        lock (_lock)
        {
            T? obj = _list.FirstOrDefault(predicate);
            return obj;
        }
    }
    public bool TryPeek(Func<T, bool> predicate, out T? obj)
    {
        obj = Peek(predicate);
        return obj is not null;
    }
    public T? Take(Func<T, bool> predicate)
    {
        if (_list.Count == 0) return null;
        lock (_lock)
        {
            T? obj = _list.FirstOrDefault(predicate);
            if (obj is null) return null;

            _list.Remove(obj);
            _list.TrimExcess();
            return obj;
        }
    }
    public bool TryTake(Func<T, bool> predicate, out T? obj)
    {
        obj = Take(predicate);
        return obj is not null;
    }
    public bool TryReplace(T item, Predicate<T> predicate)
    {
        if (_list.Count == 0) return false;
        lock (_lock)
        {
            int idx = _list.FindIndex(predicate);
            if (idx == -1) return false;

            _list.RemoveAt(idx);
            _list.Insert(idx, item);
            return true;
        }
    }
    public bool TryReplace(T item, int index)
    {
        if (_list.Count == 0) return false;
        if (index >= _maxCapacity || index < 0) return false;
        lock (_lock)
        {
            _list.RemoveAt(index);
            _list.Insert(index, item);
            return true;
        }
    }
    public bool TryInsert(T item, int index)
    {
        if (_list.Count == 0) return false;
        if (index >= _maxCapacity || index < 0) return false;
        lock (_lock)
        {
            _list.EnsureCapacity(_list.Capacity + 1);
            _list.Insert(index, item);
            return true;
        }
    }
    public bool TryUpdate(Predicate<T> predicate, string updateProperty, object updateValue)
    {
        if (Count == 0) return false;
        if (string.IsNullOrWhiteSpace(updateProperty)) return false;
        lock (_lock)
        {
            foreach (var item in _list)
            {
                if (item is null) continue;
                if (!predicate(item)) continue;

                PropertyInfo pinfo = item.GetType().GetProperty(updateProperty)!;
                if (pinfo is null) return false;

                if (updateValue is null && Nullable.GetUnderlyingType(pinfo.PropertyType) is null) return false;
                pinfo.SetValue(item, updateValue);
                return true;
            }

            return false;
        }
    }
    public bool TryUpdate(int index, string updateProperty, object updateValue)
    {
        if (Count == 0) return false;
        if (string.IsNullOrWhiteSpace(updateProperty)) return false;
        lock (_lock)
        {
            var item = _list[index];
            if (item is null) return false;

            PropertyInfo pinfo = item.GetType().GetProperty(updateProperty)!;
            if (pinfo is null) return false;

            if (updateValue is null && Nullable.GetUnderlyingType(pinfo.PropertyType) is null) return false;
            pinfo.SetValue(item, updateValue);
            return true;
        }
    }
    public string GetString(int index, char delimiter, bool getHeaders = false)
    {
        if (_list.Count == 0) return string.Empty;
        if (index >= _maxCapacity || index < 0) return string.Empty;

        lock (_lock)
        {
            T? obj = _list[index];
            if (obj is null) return string.Empty;
            return obj.ToCsvString(delimiter, getHeaders);
        }
    }
    public string GetString(Predicate<T> predicate, char delimiter, bool getHeaders = false)
    {
        if (_list.Count == 0) return string.Empty;

        lock (_lock)
        {
            T? obj = _list.Find(predicate);
            if (obj is null) return string.Empty;
            return obj.ToCsvString(delimiter, getHeaders);
        }
    }
    public void Clear(bool trimExcess = false)
    {
        if (Count == 0) return;
        lock (_lock)
        {
            _list.Clear();
            if (trimExcess)
                _list.TrimExcess();
        }
    }
    public void EnsureCapacity(int capacity)
    {
        if (Count == 0) return;
        lock (_lock)
        {
            _list.EnsureCapacity(capacity);
            _list.TrimExcess();
        }
    }
    public void Sort(Func<T, IComparable> keySelector)
    {
        if (Count == 0) return;

        lock (_lock)
        {
            CsvCollectionComparer comparer = new(keySelector);
            _list.Sort(comparer);
        }
    }
    public int CountAs(Func<T, bool> predicate)
    {
        if (Count == 0) return 0;
        lock (_lock)
        {
            return _list.Count(predicate);
        }
    }
    public IEnumerable<T> AsEnumerable()
    {
        if (Count == 0) yield break;

        foreach (var item in _list)
            yield return item;
    }
    public IEnumerable<T> AsEnumerable(Func<T, bool> predicate)
    {
        if (Count == 0) yield break;

        foreach (var item in _list.Where(predicate))
            yield return item;
    }
    public T[] ToArray()
    {
        lock (_lock)
        {
            return [.. _list];
        }
    }
    public T[] ToArray(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            return [.. _list.Where(predicate)];
        }
    }
    public override string ToString()
    {
        return $"Count = {Count}";
    }
    private class CsvCollectionComparer(Func<T, IComparable> keySelector) : IComparer<T>
    {
        private readonly Func<T, IComparable> _keySelector = keySelector;

        public int Compare(T? x, T? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var keyX = _keySelector(x);
            var keyY = _keySelector(y);

            return keyX.CompareTo(keyY);
        }
    }
}