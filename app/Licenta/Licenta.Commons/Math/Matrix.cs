using System;
using System.Linq;

namespace Licenta.Commons.Math
{
    public interface IReadMatrix<out T>
    {
        int RowsCount { get; }
        int ColumnsCount { get; }
        T this[int row, int column] { get; }
        IReadMatrix<T> GetRow(int index);
        IReadMatrix<T> GetColumn(int index);
        IReadMatrix<S> Cast<S>();        
    }

    public interface IWriteMatrix<in T>
    {
        int RowsCount { get; }
        int ColumnsCount { get; }
        T this[int row, int column] { set; }
        IReadMatrix<S> Cast<S>();
    }

    public interface IMatrix<T> : IReadMatrix<T>, IWriteMatrix<T> { }

    public class Matrix<T> : IMatrix<T>
    {
        public int RowsCount { get; }
        public int ColumnsCount { get; }
        public T[] Items { get; }
        public Matrix(int rowsCount, int columnsCount)
        {
            RowsCount = rowsCount;
            ColumnsCount = columnsCount;
            Items = new T[RowsCount * ColumnsCount];
        }
        public Matrix(int rowsCount, int columnsCount, params T[] items) : this(rowsCount, columnsCount)
        {
            Array.Copy(items, Items, System.Math.Min(items.Length, Items.Length));
        }

        public T this[int row, int column]
        {
            get => Items[row * ColumnsCount + column];
            set => Items[row * ColumnsCount + column] = value;
        }

        public Matrix<T> GetRow(int index)
        {
            if (index < 0 || index >= RowsCount)
                throw new IndexOutOfRangeException($"Invalid row index: {index}");
            return new Matrix<T>(1, ColumnsCount, Items.Skip(index * ColumnsCount).Take(ColumnsCount).ToArray());
        }
        public Matrix<T> GetColumn(int index)
        {
            if (index < 0 || index >= ColumnsCount)
                throw new IndexOutOfRangeException($"Invalid column index: {index}");
            var col = new T[RowsCount];
            for (int i = 0; i < RowsCount; i++)
                col[i] = Items[i * ColumnsCount + index];
            return new Matrix<T>(RowsCount, 1, col);
        }

        IReadMatrix<T> IReadMatrix<T>.GetRow(int index) => GetRow(index);
        IReadMatrix<T> IReadMatrix<T>.GetColumn(int index) => GetColumn(index);
        public IReadMatrix<S> Cast<S>() => new Matrix<S>(RowsCount, ColumnsCount, Items.Cast<S>().ToArray());
    }
}
