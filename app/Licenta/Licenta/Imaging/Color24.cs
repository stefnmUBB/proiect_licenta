using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Utils;

namespace Licenta.Imaging
{
    public struct Color24 : ISetAddSubOperative<Color24>, ISetMultiplicative<DoubleNumber, Color24>
        , ISetDivisive<DoubleNumber, Color24>, ISetDivisive<IntNumber, Color24>
    {
        public DoubleNumber R { get; }
        public DoubleNumber G { get; }
        public DoubleNumber B { get; }

        public Color24(byte r, byte g, byte b)
        {
            R = r / 255.0;
            G = g / 255.0;
            B = b / 255.0;
        }

        public Color24(DoubleNumber r, DoubleNumber g, DoubleNumber b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color24(int color)
        {
            B = (color & 0xFF) / 255.0;
            G = ((color >> 8) & 0xFF) / 255.0;
            R = ((color >> 16) & 0xFF) / 255.0;
        }

        public IOperative Clone() => new Color24(R, G, B);

        public Color24 Clamp() => new Color24(R.Value.Clamp(0, 1), G.Value.Clamp(0, 1), B.Value.Clamp(0, 1));

        public static Color24 FromRGB(int color) => new Color24(color);

        public Color24 Add(Color24 x) => new Color24(R.Add(x.R), G.Add(x.G), B.Add(x.B));          
        public Color24 Subtract(Color24 x) => new Color24(R.Subtract(x.R), G.Subtract(x.G), B.Subtract(x.B));
        public Color24 Multiply(DoubleNumber x) => new Color24(R.Multiply(x), G.Multiply(x), B.Multiply(x));
        public Color24 Divide(DoubleNumber x) => new Color24(R.Divide(x), G.Divide(x), B.Divide(x));        

        Color24 ISetDivisive<DoubleNumber, Color24>.Divide(DoubleNumber x) => Divide(x);
        Color24 ISetMultiplicative<DoubleNumber, Color24>.Multiply(DoubleNumber x) => Multiply(x);
        IOperative IDivisive<DoubleNumber>.Divide(DoubleNumber x) => Divide(x);
        IOperative IMultiplicative<DoubleNumber>.Multiply(DoubleNumber x) => Multiply(x);

        IOperative IAdditive<Color24>.Add(Color24 x) => Add(x);
        IOperative ISubtrative<Color24>.Subtract(Color24 x) => Subtract(x);

        public Color24 Divide(IntNumber x)=> new Color24(R.Divide(x), G.Divide(x), B.Divide(x));
        IOperative IDivisive<IntNumber>.Divide(IntNumber x) => Divide(x);

        public override string ToString() => $"(R={R.Value};G={G.Value};B={B.Value})";
    }
}
