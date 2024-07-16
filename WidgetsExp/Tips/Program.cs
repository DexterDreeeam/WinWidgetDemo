using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Microsoft.Windows.Widgets;
using System;
using Tips;

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("ole32.dll")]

static extern int CoRegisterClassObject(
    [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
    [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
    uint dwClsContext,
    uint flags,
    out uint lpdwRegister);

[DllImport("ole32.dll")]
static extern int CoRevokeClassObject(uint dwRegister);


uint cookie;
Guid CLSID_Factory = Guid.Parse("B9D27651-E4BE-4712-823A-C1D68E6F4FB3");

Console.WriteLine("Registering Widget Provider");
CoRegisterClassObject(CLSID_Factory, new WidgetProviderFactory<WidgetProvider>(), 0x4, 0x1, out cookie);
Console.WriteLine("Registered Successfully");

if (GetConsoleWindow() != IntPtr.Zero)
{
    Console.WriteLine("Press ENTER to exit.");
    Console.ReadLine();
}
else
{
    using (var emptyWidgetListEvent = WidgetProvider.GetEmptyWidgetEvent())
    {
        emptyWidgetListEvent.WaitOne();
    }
    CoRevokeClassObject(cookie);
}
