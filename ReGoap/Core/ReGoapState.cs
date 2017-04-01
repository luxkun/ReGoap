using System;
using System.Collections.Generic;
using UnityEngine;

public class ReGoapState<T, W> : ICloneable
{
    // can change to object
    private volatile Dictionary<T, W> values;

    public static int DefaultSize = 20;

    private ReGoapState()
    {
        values = new Dictionary<T, W>(DefaultSize);
    }

    private void Init(ReGoapState<T, W> old)
    {
        values.Clear();
        if (old != null)
        {
            lock (old.values)
            {
                foreach (var pair in old.values)
                {
                    values[pair.Key] = pair.Value;
                }
            }
        }
    }

    public static ReGoapState<T, W> operator +(ReGoapState<T, W> a, ReGoapState<T, W> b)
    {
        ReGoapState<T, W> result;
        lock (a.values)
        {
            result = Instantiate(a);
        }
        lock (b.values)
        {
            foreach (var pair in b.values)
                result.values[pair.Key] = pair.Value;
            return result;
        }
    }

    public int Count
    {
        get { return values.Count; }
    }
    public bool HasAny(ReGoapState<T, W> other)
    {
        lock (values) lock (other.values)
            {
                foreach (var pair in other.values)
                {
                    W thisValue;
                    values.TryGetValue(pair.Key, out thisValue);
                    if (Equals(thisValue, pair.Value))
                        return true;
                }
                return false;
            }
    }
    public bool HasAnyConflict(ReGoapState<T, W> other) // used only in backward for now
    {
        lock (values) lock (other.values)
            {
                foreach (var pair in other.values)
                {
                    W thisValue;
                    values.TryGetValue(pair.Key, out thisValue);
                    var otherValue = pair.Value;
                    if (otherValue == null || Equals(otherValue, false))
                        continue;
                    if (thisValue != null && !Equals(otherValue, thisValue))
                        return true;
                }
                return false;
            }
    }

    public int MissingDifference(ReGoapState<T, W> other, int stopAt = int.MaxValue)
    {
        ReGoapState<T, W> nullGoap = null;
        return MissingDifference(other, ref nullGoap, stopAt);
    }

    // write differences in "difference"
    public int MissingDifference(ReGoapState<T, W> other, ref ReGoapState<T, W> difference, int stopAt = int.MaxValue, Func<KeyValuePair<T, W>, W, bool> predicate = null, bool test = false)
    {
        lock (values)
        {
            var count = 0;
            foreach (var pair in values)
            {
                W otherValue;
                other.values.TryGetValue(pair.Key, out otherValue);
                if (!Equals(pair.Value, otherValue) && (predicate == null || predicate(pair, otherValue)))
                {
                    count++;
                    if (difference != null)
                        difference.values[pair.Key] = pair.Value;
                    if (count >= stopAt)
                        break;
                }
            }
            return count;
        }
    }

    public object Clone()
    {
        return Instantiate(this);
    }


    #region StateFactory
    private static Stack<ReGoapState<T, W>> cachedStates;

    public static void Warmup(int count)
    {
        cachedStates = new Stack<ReGoapState<T, W>>(count);
        for (int i = 0; i < count; i++)
        {
            cachedStates.Push(new ReGoapState<T, W>());
        }
    }

    public void Recycle()
    {
        cachedStates.Push(this);
    }

    public static ReGoapState<T, W> Instantiate(ReGoapState<T, W> old = null)
    {
        if (cachedStates == null)
        {
            cachedStates = new Stack<ReGoapState<T, W>>();
        }
        ReGoapState<T, W> state = cachedStates.Count > 0 ? cachedStates.Pop() : new ReGoapState<T, W>();
        state.Init(old);
        return state;
    }
    #endregion

    public override string ToString()
    {
        lock (values)
        {
            var result = "GoapState: ";
            foreach (var pair in values)
                result += string.Format("'{0}': {1}, ", pair.Key, pair.Value);
            return result;
        }
    }

    public W Get(T key)
    {
        lock (values)
        {
            if (!values.ContainsKey(key))
                return default(W);
            return values[key];
        }
    }

    public void Set(T key, W value)
    {
        lock (values)
        {
            values[key] = value;
        }
    }

    public void Remove(T key)
    {
        lock (values)
        {
            values.Remove(key);
        }
    }

    public Dictionary<T, W> GetValues()
    {
        lock (values)
            return values;
    }

    public bool HasKey(T key)
    {
        lock (values)
            return values.ContainsKey(key);
    }

    public void Clear()
    {
        lock (values)
            values.Clear();
    }
}