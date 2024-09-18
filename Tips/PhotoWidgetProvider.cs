using Background;
using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        private string GetJsonData()
        {
            var ss = new SystemSensor();
            var files = ss.GetRecentPhotos();
            var data = new WidgetData()
            {
                i1 = "https://ichef.bbci.co.uk/ace/ws/800/cpsprodpb/1313B/production/_133293187_img_0598.jpg",
                t1 = "pic1",
                desc1 = "Breathtaking coastal sunset with a silhouette of rugged cliffs, a serene, peaceful ambiance",
                i2 = "https://ichef.bbci.co.uk/ace/ws/800/cpsprodpb/3673/production/_133293931_dee5870a-2305-4b60-9cc7-1faf5bd5cc0e.jpg",
                t2 = "pic2",
                desc2 = "Charming alleyway in a historic European village, lined with colorful flowers",
                i3 = "https://ichef.bbci.co.uk/ace/ws/800/cpsprodpb/C136/production/_131626494_grandpalace.jpg",
                t3 = "pic3",
                desc3 = "Majestic Grand Palace in Bangkok, with its ornate golden architecture",
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
