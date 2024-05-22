using System.IO;

namespace LillyScan.FrontendXamarin.Models
{
    public class ImageRef
    {
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Thumbnail { get; set; }

        public static ImageRef ReadBinary(BinaryReader br)
        {
            string path = br.ReadString();
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            int bytesCount = br.ReadInt32();
            byte[] thumbnail = br.ReadBytes(bytesCount);
            return new ImageRef
            {
                Path = path,
                Width = width,
                Height = height,
                Thumbnail = thumbnail
            };
        }

        public void WriteBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(Width);
            bw.Write(Height);
            bw.Write(Thumbnail.Length);
            bw.Write(Thumbnail);
        }

    }
}
