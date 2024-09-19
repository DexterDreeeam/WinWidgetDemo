using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Tips
{
    public class ImageInfoProvider
    {


        public async Task<string> GetImageInfo(string imagePath)
        {
            // Check if the image exists
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image not found at the given path.");
            }


            return "mock summary";
        }

    }

}
