using SharpHook;
using SharpHook.Data;

namespace CysterApp.Utilities;

internal static class InputSimulator
{
    private static readonly EventSimulator Simulator = new();

    public static void PressKey(KeyCode key, int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            Simulator.SimulateKeyPress(key);
            Simulator.SimulateKeyRelease(key);
        }
    }

    public static void Hotkey(KeyCode mod1, KeyCode mod2, KeyCode key)
    {
        Simulator.SimulateKeyPress(mod1);
        Simulator.SimulateKeyPress(mod2);
        Simulator.SimulateKeyPress(key);
        Simulator.SimulateKeyRelease(key);
        Simulator.SimulateKeyRelease(mod2);
        Simulator.SimulateKeyRelease(mod1);
    }
}