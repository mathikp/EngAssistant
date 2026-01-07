using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace EngAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private readonly HttpClient _client = new();
        private const string ApiKey = "<<YOUR_API_KEY>>";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputText.Text))
            {
                StatusMessage.Text = "Please paste some text first.";
                StatusMessage.Foreground = Brushes.Red;
                return;
            }

            // Build the prompt
            string input = $"Please correct the grammar and improve the clarity of the following text. " +
                           $"Return only the corrected text without any explanations or additional comments:\n\n{InputText.Text}";

            try
            {
                // Disable button + show loader
                ProcessButton.IsEnabled = false;
                Loader.Visibility = Visibility.Visible;
                StatusMessage.Text = "Processing...";
                StatusMessage.Foreground = Brushes.DarkBlue;

                var requestBody = new
                {
                    contents = new[]
                    {
                new { parts = new[] { new { text = input } } }
            }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={ApiKey}",
                    content);

                string result = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(result);
                var corrected = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                OutputText.Text = corrected;
                StatusMessage.Text = "✓ Rephrased text ready!";
                StatusMessage.Foreground = Brushes.DarkGreen;
            }
            catch (Exception ex)
            {
                StatusMessage.Text = "Error: " + ex.Message;
                StatusMessage.Foreground = Brushes.Red;
            }
            finally
            {
                // Re-enable button + hide loader
                ProcessButton.IsEnabled = true;
                Loader.Visibility = Visibility.Collapsed;
            }
        }


        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OutputText.Text))
            {
                Clipboard.SetText(OutputText.Text);
                StatusMessage.Text = "Corrected text copied to clipboard!";
                StatusMessage.Foreground = Brushes.DarkGreen;
            }
        }
    }
}