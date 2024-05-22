using LillyScan.Backend.Imaging;
using LillyScan.Backend.Utils;
using LillyScan.FrontendXamarin.Models;
using LillyScan.FrontendXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Repository
{
    public class ImageStorage
    {
        private readonly string DirectoryPath;
        public ImageStorage(string path)
        {
            DirectoryPath = path;
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
        }


        public async Task<ImageSource> FetchImage(ImageRef imageRef)
        {
            if (!File.Exists(imageRef.Path))
                throw new FileNotFoundException("ImageRef path not found", imageRef.Path);
            var bytes = await File.ReadAllBytesAsync(imageRef.Path);
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        public void RemoveImage(ImageRef imageRef)
        {
            File.Delete(imageRef.Path);
        }

        public async Task<ImageRef> MakeImageRef(ImageSource imageSource)
        {
            if (!(imageSource is StreamImageSource sis))
                throw new ArgumentException($"StreamImageSource was expected, got {imageSource?.GetType()?.ToString() ?? "null"}");
            var filename = GetUniqueImagePath();
            int iconSize = 128, width, height;
            var thumbnail = new RawBitmap(iconSize, iconSize, 3);
            thumbnail.Clear();
            var bmp = await imageSource.ToRawBitmap();
            (width, height) = (bmp.Width, bmp.Height);
            (var thumbWidth, var thumbHeight) = bmp.Width >= bmp.Height
                ? (iconSize, iconSize * bmp.Height / bmp.Width)
                : (iconSize * bmp.Width / bmp.Height, iconSize);            

            using (var resized = bmp.Resize(thumbWidth, thumbHeight, disposeOriginal: true))            
                thumbnail.DrawImage(resized, (iconSize - thumbWidth) / 2, (iconSize - thumbHeight) / 2, inPlace: true);                                        

            var thumbnailSource = await thumbnail.ToImageSource();            
            thumbnail.Dispose();            

            using (var stream = await sis.Stream(CancellationToken.None))            
            using (var f = File.Create(filename))
            {
                stream.CopyTo(f);
            }            

            return new ImageRef
            {
                Width = width,
                Height = height,
                Path = filename,
                Thumbnail = await thumbnailSource.ToBytes()
            };
        }

        private string GetUniqueImagePath()
        {            
            var path = Path.Combine(DirectoryPath, GetCurrentTimestamp() + ".jpg");
            while(File.Exists(path))
            {
                Thread.Sleep(2);
                path = Path.Combine(DirectoryPath, GetCurrentTimestamp() + ".jpg");
            }          
            return path;
        }

        private static string GetCurrentTimestamp() => ((ulong)DateTime.Now.ToBinary()).ToString();

    }
}
