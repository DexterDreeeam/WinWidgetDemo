using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Tips
{
    public class ImageInfoProvider
    {
        private readonly string azureEndpoint = "AZURE_ENDPOINT";
        private readonly string azureApiKey = "AZURE_API_KEY";



        public string GetImageInfo(string imageUrl)
        {
            if (!File.Exists(imageUrl))
            {
                throw new Exception("Image not found.");
            }

            string summary = GetImageSummarybyName(imageUrl);
            return summary;

        }

        private string GetImageSummarybyName(string imageUrl)
        {
            string fileName = Path.GetFileName(imageUrl);
            string prompt = GeneratePrompt(fileName);
            string summary = CallAiModel(prompt);
            return summary;
        }

        private string GeneratePrompt(string name) {
            return $"Based on the image name provided, generate a brief description of the image in less than 10 words." +
                $" Only return the description without any additional text. Here are some examples:\n\n" +
                $"1. For 'dog.jpg': An adorable, playful pup with bright, expressive eyes.\n" +
                $"2. For 'cat.png': A fluffy kitten with curious green eyes.\n" +
                $"3. For 'beach.jpg': A serene beach with golden sands and gentle waves.\n" +
                $"4. For 'flowers.jpeg': A vibrant bouquet of colorful blossoms.\n" +
                $"5. For 'sunset.jpg': A breathtaking sunset with hues of orange and pink.\n\n" +
                $"Image name: {name}";
        }

        /**
        private string GetImageSummarybyContent(string imageUrl) {
            Image image = Image.FromFile(imageUrl);
            // Step 1: Get and compress the image from local storage
            byte[] imageData = LoadAndCompressImageFromLocalStorage(image);


            // Step 2: Generate a prompt with the image data
            string prompt = GeneratePrompt(imageData);

            // Step 3: Call the AI model to get the summary
            string summary = CallAiModel(prompt);
            return summary;
        }
        private byte[] LoadAndCompressImageFromLocalStorage(Image image)
        {
            using MemoryStream ms = new MemoryStream();
            // Compress the image to reduce size
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 25L); // Adjust quality (0-100)

            image.Save(ms, jpgEncoder, encoderParameters);
            return ms.ToArray();

        }

        private ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private string GeneratePrompt(byte[] imageData)
        {
            // Convert the image bytes to a base64 string
            string base64Image = Convert.ToBase64String(imageData);

            // Create a prompt that includes the base64 image
            return $"tell me what's in this image in 10 words" +
                $"https://www.hdwallpaper.nu/wp-content/uploads/2015/06/1843513.jpg";
        }**/



        private string CallAiModel(string prompt)
        {
           // Console.WriteLine(prompt);
            AzureOpenAIClient azureClient = new(new Uri(azureEndpoint), new AzureKeyCredential(azureApiKey));
            ChatClient chatClient = azureClient.GetChatClient("dexdep");

            ChatCompletion completion = chatClient.CompleteChat(
                [
                new UserChatMessage(prompt),
                ]);

            return completion.Content[0].Text;
        }

    }
}
