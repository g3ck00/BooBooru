using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BooBooru
{
    public partial class ImageViewerWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public ImageViewerWindow(string imageUrl, string tags)
        {
            InitializeComponent();
            //TagsTextBlock.Text = tags;
            LoadFullImage(imageUrl);
        }

        private async void LoadFullImage(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "BooruBrowser/1.0 (by BryantEscalante)");
                request.Headers.Add("Referer", "https://danbooru.donmai.us/"); // Muy importante

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                BitmapImage bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                FullImage.Source = bitmap;
            }
            catch
            {
                MessageBox.Show("No se pudo cargar la imagen en alta resolución.");
            }
        }
    }
}
