using Shellify.Core;

namespace Background
{
    internal class SystemSensor
    {
        public List<string> GetRecentFiles()
        {
            string[] fileExtensions = { ".txt", ".ps1", ".doc", ".docx", ".h", ".cpp" };
            string recentPath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            string[] recentFileLinks = Directory.GetFiles(recentPath);
            var recentFiles = new List<string>();
            foreach (var rf in recentFileLinks)
            {
                try
                {
                    var link = Shellify.ShellLinkFile.Load(rf);
                    var path = link?.LinkInfo?.LocalBasePath;
                    if (path == null)
                    {
                        continue;
                    }
                    if (fileExtensions.Any(path.EndsWith))
                    {
                        recentFiles.Add(path);
                        if (recentFiles.Count > 10)
                        {
                            break;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return recentFiles.OrderByDescending(file =>
                File.GetLastWriteTime(file))
                    .Take(10)
                    .ToList();
        }

        public List<string> GetRecentPhotos()
        {
            string picturesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            try
            {
                string[] filePaths = Directory.GetFiles(picturesFolderPath, "*.*", SearchOption.AllDirectories);
                string[] imageFileExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                return Array.FindAll(filePaths, file =>
                    Array.Exists(imageFileExtensions, ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
