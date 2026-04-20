using System;
using System.Collections;
using System.Collections.Generic;

namespace ReGoap.Core
{
    /// <summary>
    /// Supported comparison operators for GOAP state conditions.
    /// </summary>
    public enum ReGoapConditionOperator
    {
        Equal,
        NotEqual,
        GreaterOrEqual,
        LessOrEqual
    }

    /// <summary>
    /// Represents a typed state condition (for example: chestOreCount >= 1).
    /// </summary>
    public sealed class ReGoapCondition : IEquatable<ReGoapCondition>
    {
        public ReGoapConditionOperator Operator { get; }
        public object Value { get; }

        /// <summary>
        /// Creates a condition with a specific operator and reference value.
        /// </summary>
        private ReGoapCondition(ReGoapConditionOperator op, object value)
        {
            Operator = op;
            Value = value;
        }

        /// <summary>
        /// Builds an equality condition (==).
        /// </summary>
        public static ReGoapCondition Equal(object value)
        {
            return new ReGoapCondition(ReGoapConditionOperator.Equal, value);
        }

        /// <summary>
        /// Builds an inequality condition (!=).
        /// </summary>
        public static ReGoapCondition NotEqual(object value)
        {
            return new ReGoapCondition(ReGoapConditionOperator.NotEqual, value);
        }

        /// <summary>
        /// Builds a greater-or-equal condition.
        /// </summary>
        public static ReGoapCondition GreaterOrEqual(object value)
        {
            return new ReGoapCondition(ReGoapConditionOperator.GreaterOrEqual, value);
        }

        /// <summary>
        /// Builds a less-or-equal condition.
        /// </summary>
        public static ReGoapCondition LessOrEqual(object value)
        {
            return new ReGoapCondition(ReGoapConditionOperator.LessOrEqual, value);
        }

        /// <summary>
        /// Returns true when <paramref name="candidate"/> satisfies this condition.
        /// </summary>
        public bool IsSatisfiedBy(object candidate)
        {
            return IsMatch(this, candidate);
        }

        /// <summary>
        /// Generic matcher used by planner state checks.
        /// If one side is a condition it is evaluated against the other side;
        /// if both are plain values, equality is used.
        /// </summary>
        public static bool IsMatch(object required, object candidate)
        {
            if (required is ReGoapCondition requiredCondition)
                return requiredCondition.IsSatisfiedByRaw(candidate);
            if (candidate is ReGoapCondition candidateCondition)
                return candidateCondition.IsSatisfiedByRaw(required);
            return Equals(required, candidate);
        }

        /// <summary>
        /// Returns true if two constraints can coexist.
        /// Used to detect conflicts between preconditions/effects/world state.
        /// </summary>
        public static bool AreCompatible(object left, object right)
        {
            if (left is ReGoapCondition leftCondition && right is ReGoapCondition rightCondition)
                return leftCondition.IsCompatibleWith(rightCondition);
            if (left is ReGoapCondition leftOnly)
                return leftOnly.IsSatisfiedByRaw(right);
            if (right is ReGoapCondition rightOnly)
                return rightOnly.IsSatisfiedByRaw(left);
            return Equals(left, right);
        }

        /// <summary>
        /// Evaluates this condition against a raw candidate value.
        /// </summary>
        private bool IsSatisfiedByRaw(object candidate)
        {
            switch (Operator)
            {
                case ReGoapConditionOperator.Equal:
                    return Equals(candidate, Value);
                case ReGoapConditionOperator.NotEqual:
                    return !Equals(candidate, Value);
                case ReGoapConditionOperator.GreaterOrEqual:
                    return Compare(candidate, Value, out var result) && result >= 0;
                case ReGoapConditionOperator.LessOrEqual:
                    return Compare(candidate, Value, out var lessResult) && lessResult <= 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true when this condition and <paramref name="other"/> overlap
        /// (i.e. at least one value can satisfy both).
        /// </summary>
        private bool IsCompatibleWith(ReGoapCondition other)
        {
            // Equality is compatible if the fixed value satisfies the opposite condition.
            if (Operator == ReGoapConditionOperator.Equal)
                return other.IsSatisfiedByRaw(Value);
            if (other.Operator == ReGoapConditionOperator.Equal)
                return IsSatisfiedByRaw(other.Value);

            // != x is compatible with everything except a condition that forces x.
            if (Operator == ReGoapConditionOperator.NotEqual)
            {
                if (other.Operator == ReGoapConditionOperator.NotEqual)
                    return true;
                return !Equals(Value, other.Value);
            }

            if (other.Operator == ReGoapConditionOperator.NotEqual)
                return other.IsCompatibleWith(this);

            // Range/range compatibility check: verify the two bounds overlap.
            if ((Operator == ReGoapConditionOperator.GreaterOrEqual || Operator == ReGoapConditionOperator.LessOrEqual) &&
                (other.Operator == ReGoapConditionOperator.GreaterOrEqual || other.Operator == ReGoapConditionOperator.LessOrEqual))
            {
                if (!Compare(Value, other.Value, out var cmp))
                    return false;

                if (Operator == ReGoapConditionOperator.GreaterOrEqual && other.Operator == ReGoapConditionOperator.LessOrEqual)
                    return cmp <= 0;
                if (Operator == ReGoapConditionOperator.LessOrEqual && other.Operator == ReGoapConditionOperator.GreaterOrEqual)
                    return cmp >= 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to compare two objects.
        /// Returns false when values are not comparable.
        /// </summary>
        private static bool Compare(object left, object right, out int result)
        {
            result = 0;
            if (left == null || right == null)
                return false;

            if (left is IComparable comparable)
            {
                try
                {
                    result = comparable.CompareTo(right);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (TryConvertToDecimal(left, out var leftNumber) && TryConvertToDecimal(right, out var rightNumber))
            {
                result = decimal.Compare(leftNumber, rightNumber);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts supported numeric types to decimal for safe cross-type comparison.
        /// </summary>
        private static bool TryConvertToDecimal(object value, out decimal number)
        {
            switch (value)
            {
                case byte b:
                    number = b;
                    return true;
                case sbyte sb:
                    number = sb;
                    return true;
                case short s:
                    number = s;
                    return true;
                case ushort us:
                    number = us;
                    return true;
                case int i:
                    number = i;
                    return true;
                case uint ui:
                    number = ui;
                    return true;
                case long l:
                    number = l;
                    return true;
                case ulong ul:
                    number = ul;
                    return true;
                case float f:
                    number = (decimal)f;
                    return true;
                case double d:
                    number = (decimal)d;
                    return true;
                case decimal dec:
                    number = dec;
                    return true;
                default:
                    number = 0;
                    return false;
            }
        }

        /// <summary>
        /// Human-readable representation (for example: ">= 1").
        /// </summary>
        public override string ToString()
        {
            var op = Operator switch
            {
                ReGoapConditionOperator.Equal => "==",
                ReGoapConditionOperator.NotEqual => "!=",
                ReGoapConditionOperator.GreaterOrEqual => ">=",
                ReGoapConditionOperator.LessOrEqual => "<=",
                _ => "?"
            };
            return op + " " + Value;
        }

        /// <summary>
        /// Value equality for condition objects.
        /// </summary>
        public bool Equals(ReGoapCondition other)
        {
            return other != null && Operator == other.Operator && Equals(Value, other.Value);
        }

        /// <summary>
        /// Value equality for condition objects.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ReGoapCondition);
        }

        /// <summary>
        /// Hash based on operator and value.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Operator * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
