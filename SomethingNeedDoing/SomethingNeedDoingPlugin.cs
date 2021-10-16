using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs;
using Dalamud.Logging;
namespace SomethingNeedDoing
{
    /// <summary>
    /// Main plugin implementation.
    /// </summary>
    public sealed class SomethingNeedDoingPlugin : IDalamudPlugin
    {
        private const string Command = "/pcraft";

        private readonly WindowSystem windowSystem;
        private readonly MacroWindow macroWindow;
        private EquipmentScanner? equipmentScanner;
        private readonly EventHandler eventHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SomethingNeedDoingPlugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">Dalamud plugin interface.</param>
        public SomethingNeedDoingPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            ClickLib.Click.Initialize(pluginInterface);

            Service.Configuration = SomethingNeedDoingConfiguration.Load(pluginInterface.ConfigDirectory);
            Service.Address = new PluginAddressResolver();
            Service.Address.Setup();
            Resolver.Initialize();
            equipmentScanner = new EquipmentScanner();
            eventHandler = new EventHandler(equipmentScanner);
            eventHandler.Start();
            Service.ChatManager = new ChatManager();
            Service.MacroManager = new MacroManager(eventHandler);

            this.macroWindow = new();
            this.windowSystem = new("SomethingNeedDoing");
            this.windowSystem.AddWindow(this.macroWindow);
            float a = eventHandler.EquipmentScannerLastEquipmentData.LowestConditionPercent;
            PluginLog.Log(a.ToString());
            Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
            Service.CommandManager.AddHandler(Command, new CommandInfo(this.OnChatCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true,
            });
        }

        /// <inheritdoc/>
        public string Name => "Something Need Doing";

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.CommandManager.RemoveHandler(Command);
            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
            eventHandler?.Dispose();
            this.windowSystem.RemoveAllWindows();

            Service.MacroManager.Dispose();
            Service.ChatManager.Dispose();
        }

        /// <summary>
        /// Save the plugin configuration.
        /// </summary>
        internal void SaveConfiguration() => Service.Interface.SavePluginConfig(Service.Configuration);

        private void OnOpenConfigUi()
        {
            this.macroWindow.Toggle();
        }

        private void OnChatCommand(string command, string arguments)
        {
            this.macroWindow.Toggle();
        }
    }
}
