using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers.Syntax
{
    public struct Prediction1<T>
    {
        public T Value { get; }
        public bool IsValue { get; }
        public bool IsEndOfWord { get; }
        public bool IsEmpty { get; }

        private Prediction1(T value, bool isValue, bool isEndOfWord, bool isEmpty)
        {
            Value = value;
            IsValue = isValue;
            IsEndOfWord = isEndOfWord;
            IsEmpty = isEmpty;
        }

        public static Prediction1<T> Of(T value) => new Prediction1<T>(value, true, false, false);
        public static Prediction1<T> Of(Terminal<T> r) => Of(r.Value);

        public static Prediction1<T> EndOfWord() => new Prediction1<T>(default, false, true, false);
        public static Prediction1<T> Empty() => new Prediction1<T>(default, false, false, true);

        public override string ToString() => IsEndOfWord ? "$" : IsEmpty ? "''" : $"'{Value}'";

        public override bool Equals(object obj)
        {
            return obj is Prediction1<T> prediction &&
                   EqualityComparer<T>.Default.Equals(Value, prediction.Value) &&
                   IsValue == prediction.IsValue &&
                   IsEndOfWord == prediction.IsEndOfWord &&
                   IsEmpty == prediction.IsEmpty;
        }

        public override int GetHashCode()
        {
            int hashCode = 1172480897;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + IsValue.GetHashCode();
            hashCode = hashCode * -1521134295 + IsEndOfWord.GetHashCode();
            hashCode = hashCode * -1521134295 + IsEmpty.GetHashCode();
            return hashCode;
        }
    }
}
