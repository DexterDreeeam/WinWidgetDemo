using Background;
using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tips;
using Windows.Storage;

namespace PhotoWidget
{
    public class WidgetInfo
    {
        public string? widgetId { get; set; }
        public string? widgetName { get; set; }
        public string  customState = string.Empty;
        public bool    isActive = false;
        public int     random = 0;
    }

    public class WidgetData
    {
        public string i1 { get; set; }
        public string t1 { get; set; }
        public string desc1 { get; set; }
        public string i2 { get; set; }
        public string t2 { get; set; }
        public string desc2 { get; set; }
        public string i3 { get; set; }
        public string t3 { get; set; }
        public string desc3 { get; set; }
    }

    internal class PhotoWidgetProvider : IWidgetProvider
    {
        public static Dictionary<string, WidgetInfo> Widgets = new Dictionary<string, WidgetInfo>();
        private static ManualResetEvent emptyWidgetEvent = new ManualResetEvent(false);
        private const int showPicsNums = 3;

        // Called when Widget is viewed by user
        public void Activate(WidgetContext widgetContext)
        {
            var id = widgetContext.Id;
            if (Widgets.TryGetValue(id, out var widgetInfo))
            {
                widgetInfo.isActive = true;
                UpdateWidget(widgetInfo);
            }
        }

        // Called when Widget is no longer viewed
        public void Deactivate(string widgetId)
        {
            if (Widgets.TryGetValue(widgetId, out var widgetInfo))
            {
                widgetInfo.isActive = false;
            }
        }

        // Called when User Pinned a Widget
        public void CreateWidget(WidgetContext widgetContext)
        {
            var id = widgetContext.Id;
            var name = widgetContext.DefinitionId;
            WidgetInfo widgetInfo = new WidgetInfo() {
                widgetId = id,
                widgetName = name
            };
            Widgets[id] = widgetInfo;
            UpdateWidget(widgetInfo);
        }

        // Called when User Unpinned a Widget
        public void DeleteWidget(string widgetId, string customState)
        {
            Widgets.Remove(widgetId);
            if (Widgets.Count == 0)
            {
                emptyWidgetEvent.Set();
            }
        }

        // Called when User Interact with Widget
        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
        }

        // Called when Widget Size is changed
        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            var context = contextChangedArgs.WidgetContext;
            var id = context.Id;
            var size = context.Size;
            if (Widgets.TryGetValue(id, out var widgetInfo))
            {
                UpdateWidget(widgetInfo);
            }
        }

        public static ManualResetEvent GetEmptyWidgetEvent()
        {
            return emptyWidgetEvent;
        }

        private async Task<string?> GetTemplateAsync()
        {
            var uri = new Uri("ms-appx:///photoWidgetTemplate.json");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var text = await FileIO.ReadTextAsync(storageFile);
            return text;
        }

        public static string GetDataURL(string imgFile)
        {
            return "data:image/"
                        + Path.GetExtension(imgFile).Replace(".", "")
                        + ";base64,"
                        + Convert.ToBase64String(File.ReadAllBytes(imgFile));
        }

        private string[] GetLatestJpgFiles(int numberOfFiles)
        {

            string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var jpgFiles = Directory.GetFiles(picturesPath, "*.jpg");

            var latestFiles = jpgFiles
                .Select(file => new FileInfo(file))
                .OrderByDescending(fileInfo => fileInfo.LastWriteTime)
                .Take(numberOfFiles)
                .Select(fileInfo => fileInfo.FullName)
                .ToArray();

            return latestFiles;
        }
        private string GetJsonData()
        {
            var latestPics = GetLatestJpgFiles(showPicsNums);
            ImageInfoProvider imageInfoProvider = new ImageInfoProvider();
            var data = new WidgetData()
            {
                i1 = GetDataURL(latestPics[0]),
                t1 = Path.GetFileNameWithoutExtension(latestPics[0]),
                desc1 = Task.Run(() => imageInfoProvider.GetImageInfo(latestPics[0])).GetAwaiter().GetResult(),
                i2 = GetDataURL(latestPics[1]),
                t2 = Path.GetFileNameWithoutExtension(latestPics[1]),
                desc2 = Task.Run(() => imageInfoProvider.GetImageInfo(latestPics[1])).GetAwaiter().GetResult(),
                i3 = GetDataURL(latestPics[2]),
                t3 = Path.GetFileNameWithoutExtension(latestPics[2]),
                desc3 = Task.Run(() => imageInfoProvider.GetImageInfo(latestPics[2])).GetAwaiter().GetResult(),
            };
            return JsonSerializer.Serialize(data);
        }

        private void UpdateWidget(WidgetInfo info)
        {
            var jsonTemplate = GetTemplateAsync().GetAwaiter().GetResult() ?? "{}";
            var jsonData = GetJsonData();

            var options = new WidgetUpdateRequestOptions(info.widgetId);
            options.Template = jsonTemplate;
            options.Data = jsonData;
            options.CustomState = info.customState;

            WidgetManager.GetDefault().UpdateWidget(options);
        }
    }
}
