namespace Licenta.Imaging
{
    public struct Color24
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Color24(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color24(int color)
        {
            B = (byte)(color & 0xFF);
            G = (byte)((color >> 8) & 0xFF);
            R = (byte)((color >> 16) & 0xFF);
        }

        public static Color24 FromRGB(int color) => new Color24(color);
    }
}
