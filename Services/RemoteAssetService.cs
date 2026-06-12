using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicOGK_OIV_Builder.Services
{
    public static class RemoteAssetService
    {
        private static readonly HttpClient Http = new HttpClient();

        private const string BaseUrl =
            "https://raw.githubusercontent.com/Mjc-g3/MagicOGK-OIV-Builder/master/remote-assets/";

        private static readonly string CacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MagicOGK",
            "RemoteAssets"
        );

        public static async Task<Image?> LoadPreviewAsync(string category, string fileName)
        {
            Directory.CreateDirectory(CacheDir);

            string safeCategory = category.Replace("\\", "/").Trim('/');
            string safeFileName = Path.GetFileName(fileName);

            string cachePath = Path.Combine(CacheDir, safeCategory, safeFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

            if (!File.Exists(cachePath))
            {
                string url = $"{BaseUrl}{safeCategory}/{safeFileName}";

                byte[] data = await Http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(cachePath, data);
            }

            return Image.FromFile(cachePath);
        }
    }
}