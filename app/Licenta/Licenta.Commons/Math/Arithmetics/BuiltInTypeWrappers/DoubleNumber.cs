namespace Licenta.Commons.Math.Arithmetics
{
    public struct DoubleNumber : INumber
    {
        public double Value { get; }
        public DoubleNumber(double value)
        {
            Value = value;
        }

        public static explicit operator double(DoubleNumber x) => x.Value;

        public DoubleNumber Add(INumber x)
        {
            var doubleX = OperativeConverter.Convert<DoubleNumber>(x);
            return new DoubleNumber(Value + doubleX.Value);
        }

        public DoubleNumber Divide(INumber x)
        {
            var doubleX = OperativeConverter.Convert<DoubleNumber>(x);
            return new DoubleNumber(Value / doubleX.Value);
        }

        public DoubleNumber Multiply(INumber x)
        {
            var doubleX = OperativeConverter.Convert<DoubleNumber>(x);
            return new DoubleNumber(Value * doubleX.Value);
        }

        public DoubleNumber Subtract(INumber x)
        {
            var doubleX = OperativeConverter.Convert<DoubleNumber>(x);
            return new DoubleNumber(Value - doubleX.Value);
        }

        INumber INumber.Add(INumber x) => Add(x);
        INumber INumber.Subtract(INumber x) => Subtract(x);
        INumber INumber.Multiply(INumber x) => Multiply(x);
        INumber INumber.Divide(INumber x) => Divide(x);
        IOperative IAdditive<INumber>.Add(INumber x) => Add(x);
        IOperative IDivisive<INumber>.Divide(INumber x) => Subtract(x);
        IOperative IMultiplicative<INumber>.Multiply(INumber x) => Multiply(x);
        IOperative ISubtrative<INumber>.Subtract(INumber x) => Subtract(x);

        public IOperative Clone() => new DoubleNumber(Value);

    }
}
