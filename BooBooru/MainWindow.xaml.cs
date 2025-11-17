using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BooBooru;

namespace BooBooru
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            // Configurar User-Agent para Danbooru
            client.DefaultRequestHeaders.Add("User-Agent", "BooBooru/1.0 (by g3ck0)");
        }

        // Evento click del botón Buscar
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string tags = SearchTextBox.Text.Trim().Replace(' ', '+');
            if (string.IsNullOrEmpty(tags)) return;

            await LoadImages(tags);
        }

        // Cargar imágenes desde la API
        private async Task LoadImages(string tags)
        {
            try
            {
                ImagesWrapPanel.Children.Clear(); // Limpiar miniaturas anteriores

                string url = $"https://danbooru.donmai.us/posts.json?tags={tags}&limit=100";
                var response = await client.GetStringAsync(url);

                var posts = JsonSerializer.Deserialize<Post[]>(response);

                foreach (var post in posts)
                {
                    // Usar preview_file_url primero, luego file_url
                    var imageUrl = post.preview_file_url ?? post.file_url;
                    if (string.IsNullOrEmpty(imageUrl))
                        continue;

                    // Crear control de imagen
                    Image imgControl = new Image();
                    imgControl.Width = 150;
                    imgControl.Height = 150;
                    imgControl.Margin = new Thickness(5);

                    try
                    {
                        BitmapImage bitmap = await LoadImageAsync(imageUrl);
                        imgControl.Source = bitmap;

                        // Click para abrir visor con la imagen original y tags
                        imgControl.MouseLeftButtonUp += (s, e) =>
                        {
                            // Intentar abrir la mejor URL disponible
                            string fullImageUrl = post.file_url ?? post.large_file_url ?? post.jpeg_url;

                            if (string.IsNullOrEmpty(fullImageUrl) ||
                                !(fullImageUrl.EndsWith(".jpg") || fullImageUrl.EndsWith(".jpeg") || fullImageUrl.EndsWith(".png")))
                            {
                                MessageBox.Show("No hay imagen en alta resolución disponible o no es compatible con el visor.");
                                return;
                            }

                            var viewer = new ImageViewerWindow(fullImageUrl, post.tag_string);
                            viewer.Show();
                        };

                        ImagesWrapPanel.Children.Add(imgControl);
                    }
                    catch
                    {
                        // Ignorar imágenes que no se puedan procesar
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar imágenes: " + ex.Message);
            }
        }

        // Descargar imagen y convertir a BitmapImage
        private async Task<BitmapImage> LoadImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            byte[] imageData = await client.GetByteArrayAsync(url);
            BitmapImage bitmap = new BitmapImage();

            using (var stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Permite usar la imagen en threads distintos
            }

            return bitmap;
        }

        private void SearchTextBox_TextChanged()
        {

        }
    }

    // Clase para mapear JSON de Danbooru
    public class Post
    {
        public string file_url { get; set; }          // Original
        public string preview_file_url { get; set; }  // Miniatura segura
        public string large_file_url { get; set; }    // JPG/PNG grande
        public string jpeg_url { get; set; }          // Otra alternativa segura
        public string tag_string { get; set; }
    }

}
