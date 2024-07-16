using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tips
{
    public class WidgetInfo
    {
        public string? widgetId { get; set; }
        public string? widgetName { get; set; }
        public string  customState = string.Empty;
        public bool    isActive = false;
        public int     random = 0;
    }

    internal class WidgetProvider : IWidgetProvider
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
            var id = actionInvokedArgs.WidgetContext.Id;
            if (Widgets.TryGetValue(id, out var widgetInfo))
            {
                var data = actionInvokedArgs.Data;
                var verb = actionInvokedArgs.Verb;
                if (verb == "rand")
                {
                    Random random = new Random();
                    int randomNumber = random.Next(1, 101);
                    widgetInfo.random = randomNumber;
                    UpdateWidget(widgetInfo);
                }
            }
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
            var uri = new Uri("ms-appx:///template.json");
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var text = await FileIO.ReadTextAsync(storageFile);
            return text;
        }

        private void UpdateWidget(WidgetInfo info)
        {
            var jsonTemplate = GetTemplateAsync().GetAwaiter().GetResult() ?? "{}";
            var jsonData = $"{{ \"number\": {info.random} }}";

            var options = new WidgetUpdateRequestOptions(info.widgetId);
            options.Template = jsonTemplate;
            options.Data = jsonData;
            options.CustomState = info.customState;

            WidgetManager.GetDefault().UpdateWidget(options);
        }
    }
}
