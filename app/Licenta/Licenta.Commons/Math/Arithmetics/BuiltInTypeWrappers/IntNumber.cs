namespace Licenta.Commons.Math.Arithmetics
{
    public struct IntNumber : INumber
    {
        public int Value { get; }
        public IntNumber(int value)
        {
            Value = value;
        }

        public static explicit operator int(IntNumber x) => x.Value;        

        public IntNumber Add(INumber x)
        {
            var intX = OperativeConverter.Convert<IntNumber>(x);
            return new IntNumber(Value + intX.Value);
        }

        public IntNumber Divide(INumber x)
        {
            var intX = OperativeConverter.Convert<IntNumber>(x);
            return new IntNumber(Value / intX.Value);
        }

        public IntNumber Multiply(INumber x)
        {
            var intX = OperativeConverter.Convert<IntNumber>(x);
            return new IntNumber(Value * intX.Value);
        }

        public IntNumber Subtract(INumber x)
        {
            var intX = OperativeConverter.Convert<IntNumber>(x);
            return new IntNumber(Value - intX.Value);
        }

        INumber INumber.Add(INumber x) => Add(x);
        INumber INumber.Subtract(INumber x) => Subtract(x);
        INumber INumber.Multiply(INumber x) => Multiply(x);
        INumber INumber.Divide(INumber x) => Divide(x);
        IOperative IAdditive<INumber>.Add(INumber x) => Add(x);
        IOperative IDivisive<INumber>.Divide(INumber x) => Subtract(x);
        IOperative IMultiplicative<INumber>.Multiply(INumber x) => Multiply(x);
        IOperative ISubtrative<INumber>.Subtract(INumber x) => Subtract(x);

        public IOperative Clone() => new IntNumber(Value);
    }
}
