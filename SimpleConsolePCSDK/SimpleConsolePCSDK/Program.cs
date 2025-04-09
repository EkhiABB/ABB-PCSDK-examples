// See https://aka.ms/new-console-template for more information


using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using System.Reflection;

Console.WriteLine("PCSDK test app");
Console.WriteLine("");


Controller controller = null;
NetworkScanner scanner = new NetworkScanner();
Console.WriteLine("Scanning controllers");

scanner.Scan();
ControllerInfoCollection controllers = scanner.Controllers;
if (controllers.Count >= 1)
{
    try
    {
        Console.WriteLine("Connecting to controller on MGMT");
        controller = Controller.Connect(
            controllers.First(e => e.IPAddress.ToString() == "192.168.125.1"),
            ConnectionType.Standalone,
            true); // Need TRUE to validate certificate

        Console.WriteLine(@"Connected to controller {0}",controller.Name);
    }
    catch
    {
        Console.WriteLine("Couldn't connect to controller");
    }
}
else
{
    Console.WriteLine("No controller found");
    return;
}


Console.WriteLine("Logon to controller");
controller.Logon(UserInfo.DefaultUser);

string currentFolder=Path.GetFullPath(
     Path.GetDirectoryName(
         Assembly.GetExecutingAssembly()
         .Location));

Console.WriteLine(@"put file to {0}",controller.FileSystem.RemoteDirectory);
controller.FileSystem.PutFile(currentFolder + @"/testmodule.modx", @"/testmodule.modx",true);


Console.WriteLine(@"loading module in RAPID Task");

using (Mastership.Ensure(controller))
{
    controller.Rapid.GetTask("T_ROB1").LoadModuleFromFile(@"/testmodule.modx", RapidLoadMode.Replace);
    Console.WriteLine(@"Set Program Pointer to PROC");
    controller.Rapid.GetTask("T_ROB1").SetProgramPointer("testmodule", "TestProc");
    Console.WriteLine(@"Run PROC");
    controller.Rapid.Start(RegainMode.Clear, ExecutionMode.StepIn);
}
Console.WriteLine(@"Done");

