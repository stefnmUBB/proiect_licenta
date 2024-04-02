using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
		public static Tensor Conv2D(this Tensor t, Tensor kernel)
		{
			var validate_shapes = new Func<bool>(() => t.Rank == 4 && kernel.Rank == 4 && t.Shape[3] == kernel.Shape[2]);
			if (!validate_shapes())
				throw new ArgumentException($"Invalid input shapes for Conv2D operation: {t.Shape}, {kernel.Shape}");

			(int B, int n, int m) = (t.Shape[0], t.Shape[1], t.Shape[2]);
			(int K1, int K2) = (kernel.Shape[0], kernel.Shape[1]);
			(int f1, int f2) = (t.Shape[3], kernel.Shape[3]);

			var cells = new List<float>();

			int k1Start = -K1 / 2 + (1 - K1 % 2);
			int k2Start = -K2 / 2 + (1 - K2 % 2);
			var channelsList = new float[K1 * K2 * f1];

			Stopwatch sw1 = new Stopwatch();
			Stopwatch sw2 = new Stopwatch();
			Stopwatch sw3 = new Stopwatch();
			Stopwatch sw4 = new Stopwatch();
			long t1 = 0;
			long t2 = 0;
			long t3 = 0;
			long t4 = 0;

			for (int b = 0; b < B; b++)
			{
				int offset = b * n * m * f1;
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
					{
						sw1.Reset();
						sw2.Reset();
						sw3.Reset();
						sw4.Reset();
						sw1.Start();
						for (int k1 = k1Start; k1 <= K1 / 2; k1++)
						{
							for (int k2 = k2Start; k2 <= K2 / 2; k2++)
							{
								var ii = (i + k1).Clamp(0, n - 1);
								var jj = (j + k2).Clamp(0, m - 1);
								t.BuffAccessor.CopyTo(offset + ii * m * f1 + jj * f1, channelsList, (k1 - k1Start) * K2 + k2 - k2Start, f1);
							}
						}
						sw1.Stop();
						t1 += sw1.ElapsedTicks;
						sw2.Start();						
						var convSrc = new Tensor((K1, K2, 1, f1), channelsList);
						sw2.Stop();
						t2 += sw2.ElapsedTicks;
						sw3.Start();
						var x = convSrc.MatMul(kernel);
						sw3.Stop();
						t3 += sw3.ElapsedTicks;

						sw4.Start();
						x = x.ReduceSum(new[] { 0, 1, 2 });
						cells.AddRange(x.BuffAccessor.GetSlice(0, x.ElementsCount));
						sw4.Stop();
						t4 += sw4.ElapsedTicks;
					}
				}
			}
			Console.WriteLine($"t1 = {t1}");
			Console.WriteLine($"t2 = {t2}");
			Console.WriteLine($"t3 = {t3}");
			Console.WriteLine($"t4 = {t4}");
			return new Tensor((B, n, m, f2), cells.ToArray());
		}
	}
}
