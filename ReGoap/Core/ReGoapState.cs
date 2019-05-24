using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ReGoap.Core
{
    public class ReGoapState<T, W>
    {
        // can change to object
        private ConcurrentDictionary<T, W> values;
        private readonly ConcurrentDictionary<T, W> bufferA;
        private readonly ConcurrentDictionary<T, W> bufferB;

        public static int DefaultSize = 20;

        private ReGoapState()
        {
            int concurrencyLevel = 5; // No idea.
            bufferA = new ConcurrentDictionary<T, W>(concurrencyLevel, DefaultSize);
            bufferB = new ConcurrentDictionary<T, W>(concurrencyLevel, DefaultSize);
            values = bufferA;
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

        public void AddFromState(ReGoapState<T, W> b)
        {
            lock (values) lock (b.values)
            {
                foreach (var pair in b.values)
                    values[pair.Key] = pair.Value;
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
                        var otherValue = pair.Value;

                        // not here, ignore this check
                        W thisValue;
                        if (!values.TryGetValue(pair.Key, out thisValue))
                            continue;
                        if (!Equals(otherValue, thisValue))
                            return true;
                    }
                    return false;
                }
        }

        // this method is more relaxed than the other, also accepts conflits that are fixed by "changes"
        public bool HasAnyConflict(ReGoapState<T, W> changes, ReGoapState<T, W> other)
        {
            lock (values) lock (other.values)
                {
                    foreach (var pair in other.values)
                    {
                        var otherValue = pair.Value;

                        // not here, ignore this check
                        W thisValue;
                        if (!values.TryGetValue(pair.Key, out thisValue))
                            continue;
                        W effectValue;
                        changes.values.TryGetValue(pair.Key, out effectValue);
                        if (!Equals(otherValue, thisValue) && !Equals(effectValue, thisValue))
                            return true;
                    }
                    return false;
                }
        }

        public int MissingDifference(ReGoapState<T, W> other, int stopAt = int.MaxValue)
        {
            lock (values)
            {
                var count = 0;
                foreach (var pair in values)
                {
                    W otherValue;
                    other.values.TryGetValue(pair.Key, out otherValue);
                    if (!Equals(pair.Value, otherValue))
                    {
                        count++;
                        if (count >= stopAt)
                            break;
                    }
                }
                return count;
            }
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

        // keep only missing differences in values
        public int ReplaceWithMissingDifference(ReGoapState<T, W> other, int stopAt = int.MaxValue, Func<KeyValuePair<T, W>, W, bool> predicate = null, bool test = false)
        {
            lock (values)
            {
                var count = 0;
                var buffer = values;
                values = values == bufferA ? bufferB : bufferA;
                values.Clear();
                foreach (var pair in buffer)
                {
                    W otherValue;
                    other.values.TryGetValue(pair.Key, out otherValue);
                    if (!Equals(pair.Value, otherValue) && (predicate == null || predicate(pair, otherValue)))
                    {
                        count++;
                        values[pair.Key] = pair.Value;
                        if (count >= stopAt)
                            break;
                    }
                }
                return count;
            }
        }

        public ReGoapState<T, W> Clone()
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
            lock (cachedStates)
            {
                cachedStates.Push(this);
            }
        }

        public static ReGoapState<T, W> Instantiate(ReGoapState<T, W> old = null)
        {
            ReGoapState<T, W> state;
            if (cachedStates == null)
            {
                cachedStates = new Stack<ReGoapState<T, W>>();
            }
            lock (cachedStates)
            {
                state = cachedStates.Count > 0 ? cachedStates.Pop() : new ReGoapState<T, W>();
            }
            state.Init(old);
            return state;
        }
        #endregion

        public override string ToString()
        {
            lock (values)
            {
                var result = "";
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
            values.TryRemove(key, out _);
        }

        public ConcurrentDictionary<T, W> GetValues()
        {
            lock (values)
                return values;
        }

        public bool TryGetValue(T key, out W value) {
            return values.TryGetValue(key, out value);
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
}
