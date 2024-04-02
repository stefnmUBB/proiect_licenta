using System;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
		#region Generic operations (but slow)
		public static Tensor PerformElementWiseBinaryOperation(this Tensor t1, Tensor t2, Func<float, float, float> op)
		{
			if (t1.Rank == 0 && t2.Rank == 0)
			{
				return new Tensor(0, new[] { op(t1.BuffAccessor[0], t2.BuffAccessor[0]) });
			}
			if (t2.Rank == 0)
				return PerformElementWiseBinaryOperation(t2, t1, (x, y) => op(y, x));
			if (t1.Rank == 0)
			{
				var scalar = t1.BuffAccessor[0];
				var newBuffer = new float[t2.ElementsCount];
				for (int i = 0; i < newBuffer.Length; i++)
					newBuffer[i] = op(scalar, t2.BuffAccessor[i]);
				return new Tensor(t2.Shape, newBuffer);
			}

			Tensor? toDispose = null;

			if (t1.Rank < t2.Rank)
				toDispose = t1 = t1.Reshape(Shape.Ones(t2.Rank - t1.Rank) + t1.Shape);
			else if (t2.Rank < t1.Rank)
				toDispose = t2 = t2.Reshape(Shape.Ones(t1.Rank - t2.Rank) + t2.Shape);

			var newDims = new int[t1.Rank];
			for (int i = 0; i < t1.Rank; i++)
			{
				if (t1.Shape[i] != 1 && t2.Shape[i] != 1 && t2.Shape[i] != t1.Shape[i])
				{
					toDispose?.Dispose();
					throw new InvalidOperationException($"Cannot perform element-wise operations on tensors of shapes {t1.Shape} and {t2.Shape}");
				}
				newDims[i] = System.Math.Max(t1.Shape[i], t2.Shape[i]);
			}
			var shape = new Shape(newDims);

			if (shape.DimensionsCount == 4)
			{
				var result = PerformElementWiseBinaryOperation4(shape, t1, t2, op);
				toDispose?.Dispose();
				return result;
			}

			var buffer = new float[shape.ElementsCount];
			shape.IterateIndicesWithBufferCounter((it, i) =>
			{
				int indexA = 0, indexB = 0;
				for (int k = 0; k < it.Length; k++)
				{
					indexA += t1.Shape.DimMultipliers[k] * System.Math.Min(it[k], t1.Shape[k] - 1);
					indexB += t2.Shape.DimMultipliers[k] * System.Math.Min(it[k], t2.Shape[k] - 1);
				}
				var a = t1.BuffAccessor[indexA];
				var b = t1.BuffAccessor[indexB];
				buffer[i] = op(a, b);
			});			
			toDispose?.Dispose();
			return new Tensor(shape, buffer);
		}
		#endregion


		#region Specific Operations
		public static Tensor Add(this Tensor t1, Tensor t2)
		{
			if (t1.Rank == 0 && t2.Rank == 0)
			{
				return new Tensor(0, new[] { t1.BuffAccessor[0] + t2.BuffAccessor[0] });
			}
			if (t2.Rank == 0)
				return Add(t2, t1);
			if (t1.Rank == 0)
			{
				var scalar = t1.BuffAccessor[0];
				var newBuffer = new float[t2.ElementsCount];
				for (int i = 0; i < newBuffer.Length; i++)
					newBuffer[i] = scalar + t2.BuffAccessor[i];
				return new Tensor(t2.Shape, newBuffer);
			}

			Tensor? toDispose = null;

			if (t1.Rank < t2.Rank)
				toDispose = t1 = t1.Reshape(Shape.Ones(t2.Rank - t1.Rank) + t1.Shape);
			else if (t2.Rank < t1.Rank)
				toDispose = t2 = t2.Reshape(Shape.Ones(t1.Rank - t2.Rank) + t2.Shape);

			var newDims = new int[t1.Rank];
			for (int i = 0; i < t1.Rank; i++)
			{
				if (t1.Shape[i] != 1 && t2.Shape[i] != 1 && t2.Shape[i] != t1.Shape[i])
				{
					toDispose?.Dispose();
					throw new InvalidOperationException($"Cannot perform element-wise operations on tensors of shapes {t1.Shape} and {t2.Shape}");
				}
				newDims[i] = System.Math.Max(t1.Shape[i], t2.Shape[i]);
			}
			var shape = new Shape(newDims);

			if (shape.DimensionsCount == 4)
			{
				var result = Add4(shape, t1, t2);
				toDispose?.Dispose();
				return result;
			}

			var buffer = new float[shape.ElementsCount];
			shape.IterateIndicesWithBufferCounter((it, i) =>
			{
				int indexA = 0, indexB = 0;
				for (int k = 0; k < it.Length; k++)
				{
					indexA += t1.Shape.DimMultipliers[k] * System.Math.Min(it[k], t1.Shape[k] - 1);
					indexB += t2.Shape.DimMultipliers[k] * System.Math.Min(it[k], t2.Shape[k] - 1);
				}
				var a = t1.BuffAccessor[indexA];
				var b = t1.BuffAccessor[indexB];
				buffer[i] = a + b;
			});
			toDispose?.Dispose();
			return new Tensor(shape, buffer);
		}


		public static Tensor MatMul(this Tensor t1, Tensor t2)
		{
			if (t1.Rank < 2 || t2.Rank < 2)
				throw new ArgumentException("Ranks of matmul tensors must be at least 2");

			if (t1.Shape[-1] != t2.Shape[-2])
				throw new ArgumentException($"Cannot matmul tensors of shapes {t1.Shape} and {t2.Shape}");

			if (t1.Rank == 2 && t2.Rank == 2)
				return new Tensor((t1.Shape[0], t2.Shape[1]), MatMul2(t1, t2));

			return t1.SubDimBroadcast(t2, (x, y) => new Tensor((x.Shape[0], y.Shape[1]), MatMul2(x, y)), 2);
		}

		#endregion

		#region Known Rank Optimizations
		private static Tensor PerformElementWiseBinaryOperation4(Shape shape, Tensor t1, Tensor t2, Func<float, float, float> op)
		{
			var buffer = new float[shape.ElementsCount];
			int i = 0;
			for (int i0 = 0; i0 < shape[0]; i0++)
			{
				var a0 = t1.Shape.DimMultipliers[0] * System.Math.Min(i0, t1.Shape[0] - 1);
				var b0 = t2.Shape.DimMultipliers[0] * System.Math.Min(i0, t2.Shape[0] - 1);
				for (int i1 = 0; i1 < shape[1]; i1++)
				{
					var a1 = t1.Shape.DimMultipliers[1] * System.Math.Min(i1, t1.Shape[1] - 1) + a0;
					var b1 = t2.Shape.DimMultipliers[1] * System.Math.Min(i1, t2.Shape[1] - 1) + b0;
					for (int i2 = 0; i2 < shape[2]; i2++)
					{
						var a2 = t1.Shape.DimMultipliers[2] * System.Math.Min(i2, t1.Shape[2] - 1) + a1;
						var b2 = t2.Shape.DimMultipliers[2] * System.Math.Min(i2, t2.Shape[2] - 1) + b1;
						for (int i3 = 0; i3 < shape[3]; i3++)
						{
							var indexA = t1.Shape.DimMultipliers[3] * System.Math.Min(i3, t1.Shape[3] - 1) + a2;
							var indexB = t2.Shape.DimMultipliers[3] * System.Math.Min(i3, t2.Shape[3] - 1) + b2;

							var a = t1.BuffAccessor[indexA];
							var b = t1.BuffAccessor[indexB];
							buffer[i++] = op(a, b);
						}
					}
				}
			}
			return new Tensor(shape, buffer);
		}
		private static Tensor Add4(Shape shape, Tensor t1, Tensor t2)
		{
			var buffer = new float[shape.ElementsCount];
			int i = 0;
			for (int i0 = 0; i0 < shape[0]; i0++)
			{
				var a0 = t1.Shape.DimMultipliers[0] * System.Math.Min(i0, t1.Shape[0] - 1);
				var b0 = t2.Shape.DimMultipliers[0] * System.Math.Min(i0, t2.Shape[0] - 1);
				for (int i1 = 0; i1 < shape[1]; i1++)
				{
					var a1 = t1.Shape.DimMultipliers[1] * System.Math.Min(i1, t1.Shape[1] - 1) + a0;
					var b1 = t2.Shape.DimMultipliers[1] * System.Math.Min(i1, t2.Shape[1] - 1) + b0;
					for (int i2 = 0; i2 < shape[2]; i2++)
					{
						var a2 = t1.Shape.DimMultipliers[2] * System.Math.Min(i2, t1.Shape[2] - 1) + a1;
						var b2 = t2.Shape.DimMultipliers[2] * System.Math.Min(i2, t2.Shape[2] - 1) + b1;
						for (int i3 = 0; i3 < shape[3]; i3++)
						{
							var indexA = t1.Shape.DimMultipliers[3] * System.Math.Min(i3, t1.Shape[3] - 1) + a2;
							var indexB = t2.Shape.DimMultipliers[3] * System.Math.Min(i3, t2.Shape[3] - 1) + b2;

							var a = t1.BuffAccessor[indexA];
							var b = t1.BuffAccessor[indexB];
							buffer[i++] = a + b;
						}
					}
				}
			}
			return new Tensor(shape, buffer);
		}
		#endregion


		#region Helpers
		private static float[] MatMul2(this Tensor a, Tensor b)
		{
			int m = a.Shape[-2], n = a.Shape[-1], p = b.Shape[-1];
			var results = new float[m * p];
			for (int i = 0; i < m; i++)
				for (int j = 0; j < p; j++)
					for (int k = 0; k < n; k++)
						results[i * p + j] += a.BuffAccessor[i * n + k] * b.BuffAccessor[k * p + j];
			return results;
		}
		#endregion
	}
}
