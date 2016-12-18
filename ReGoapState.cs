using System;
using System.Collections.Generic;
using UnityEngine;

public class ReGoapState : ICloneable
{
    // can change to object
    private volatile Dictionary<string, object> values;

    public ReGoapState(ReGoapState old)
    {
        lock (old.values)
            values = new Dictionary<string, object>(old.values);
    }

    public ReGoapState()
    {
        values = new Dictionary<string, object>();
    }

    public static ReGoapState operator +(ReGoapState a, ReGoapState b)
    {
        ReGoapState result;
        lock (a.values)
        {
            result = new ReGoapState(a);
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
    public bool HasAny(ReGoapState other)
    {
        lock (values) lock (other.values)
        {
            foreach (var pair in other.values)
            {
                object thisValue;
                values.TryGetValue(pair.Key, out thisValue);
                var otherValue = pair.Value;
                if (thisValue == otherValue || (thisValue != null && thisValue.Equals(pair.Value)))
                    return true;
            }
            return false;
        }
    }
    public bool HasAnyConflict(ReGoapState other) // used only in backward for now
    {
        lock (values) lock (other.values)
        {
            foreach (var pair in other.values)
            {
                object thisValue;
                values.TryGetValue(pair.Key, out thisValue);
                var otherValue = pair.Value;
                if (otherValue == null || otherValue.Equals(false)) // backward search does NOT support false preconditions
                    continue;
                if (thisValue != null && !otherValue.Equals(thisValue))
                    return true;
            }
            return false;
        }
    }

    public int MissingDifference(ReGoapState other, int stopAt = int.MaxValue)
    {
        ReGoapState nullGoap = null;
        return MissingDifference(other, ref nullGoap, stopAt);
    }

    // write differences in "difference"
    public int MissingDifference(ReGoapState other, ref ReGoapState difference, int stopAt = int.MaxValue, Func<KeyValuePair<string, object>, object, bool> predicate = null, bool test = false)
    {
        lock (values)
        {
            var count = 0;
            foreach (var pair in values)
            {
                var add = false;
                var valueBool = pair.Value as bool?;
                object otherValue;
                other.values.TryGetValue(pair.Key, out otherValue);
                if (valueBool.HasValue)
                {
                    // we don't need to check otherValue type since every key is supposed to always have same value type
                    var otherValueBool = otherValue == null ? false : (bool)otherValue;
                    if (valueBool.Value != otherValueBool)
                        add = true;
                }
                else // generic version
                {
                    var valueVector3 = pair.Value as Vector3?;
                    if (valueVector3 != null && test)
                    {
                        add = true;
                    }
                    else if ((pair.Value == null && otherValue != null) || (!pair.Value.Equals(otherValue)))
                    {
                        add = true;
                    }
                }
                if (add && (predicate == null || predicate(pair, otherValue)))
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
        var clone = new ReGoapState(this);
        return clone;
    }

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

    public T Get<T>(string key)
    {
        lock (values)
        {
            if (!values.ContainsKey(key))
                return default(T);
            return (T)values[key];
        }
    }

    public void Set<T>(string key, T value)
    {
        lock (values)
        {
            values[key] = value;
        }
    }

    public void Remove(string key)
    {
        lock (values)
        {
            values.Remove(key);
        }
    }

    public Dictionary<string, object> GetValues()
    {
        lock (values)
            return values;
    }

    public bool HasKey(string key)
    {
        lock (values)
            return values.ContainsKey(key);
    }

    public void Clear()
    {
        values.Clear();
    }
}