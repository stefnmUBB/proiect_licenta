using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Math
{
    public static class Tensors
    {
        private static readonly HashSet<Type> ArithmeticTypes = new HashSet<Type>(new[]
        {
            typeof(int), typeof(uint), typeof(float), typeof(double)
        });

        public static Tensor<T> Constant<T>(Shape shape, T constant)
        {            
            T[] buffer = new T[shape.ElementsCount];
            for (int i = 0; i < buffer.Length; i++) buffer[i] = constant;
            return new Tensor<T>(shape, buffer);
        }

        public static Tensor<T> Ones<T>(Shape shape)
        {
            ValidateArithmeticType<T>();
            return Constant(shape, (T)Convert.ChangeType(1, typeof(T)));                       
        }

        public static Tensor<T> Zeros<T>(Shape shape)
        {
            ValidateArithmeticType<T>();
            return Constant(shape, (T)Convert.ChangeType(0, typeof(T)));
        }


        private static void ValidateArithmeticType<T>()
        {
            if (!ArithmeticTypes.Contains(typeof(T)))
                throw new InvalidOperationException("Invalid type for the specified operation");
        }

        public static Tensor<T> Stack<T>(IEnumerable<Tensor<T>> tensors)
        {
            var list = tensors.ToList();

            if (list.Count == 0)
                throw new ArgumentException("Cannot stack an empty collection of tensors");

            if (list.Any(_ => !object.Equals(_.Shape, list[0].Shape)))
                throw new InvalidOperationException("Cannot stack tensors with different shapes");

            return new Tensor<T>(new[] { list.Count }.Concat(list[0].Shape).ToArray(), list.SelectMany(_ => _.Buffer).ToArray());
        }
    }
}
