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
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75L); // Adjust quality (0-100)

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
            return $"Tell me the content of this image in 10 words:\n\n![image](data:image/jpeg;base64,{base64Image})";
        }



        private string CallAiModel(string prompt)
        {
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
