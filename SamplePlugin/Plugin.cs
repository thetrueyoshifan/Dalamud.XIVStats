using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace SamplePlugin
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "XIVStats";

        private const string commandName = "/xivstats";

        private DalamudPluginInterface pi;
        private Configuration configuration;
        private PluginUI ui;
        private Thread classCheckThread;

        private int cachedClass = 0;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            
            this.configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.configuration.Initialize(this.pi);

            // you might normally want to embed resources and load them from the manifest stream
            this.ui = new PluginUI(this.configuration);

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Shows the logged stats for your current class"
            });

            this.pi.UiBuilder.OnBuildUi += DrawUI;
            this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
            classCheckThread = new Thread(Loop)
            {
                Name = "XIVStats Worker Thread",
                IsBackground = true
            };
            classCheckThread.Start();
            pi.Framework.Gui.Chat.OnChatMessage += OnChatMessage;
        }

        public void OnChatMessage(Dalamud.Game.Chat.XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled){
            if (message.TextValue == "You received a player commendation!" && sender.TextValue == "")
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).CommendationsReceived++;
            }
            else if (message.TextValue == "You are defeated." && sender.TextValue == "")
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).Deaths++;
            }
        }
        
        public void Loop()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (pi.ClientState.LocalPlayer != null)
                {
                    if (configuration.classes.FindIndex(a => a.ClassID == cachedClass && a.AssociatedCharacterName != pi.ClientState.LocalPlayer.Name) == -1)
                    {
                        configuration.classes.Add(new ClassInfo { ClassID = cachedClass, AssociatedCharacterName = pi.ClientState.LocalPlayer.Name });
                    }
                    ClassInfo inf = configuration.classes.Find(a => a.ClassID == cachedClass);
                    inf.TimeActive++;
                    configuration.Save();
                }
            }
        }

        public void Dispose()
        {
            this.ui.Dispose();
            classCheckThread.Abort();

            this.pi.CommandManager.RemoveHandler(commandName);
            this.pi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.ui.Visible = true;
        }

        private void DrawUI()
        {
            if (pi.ClientState.LocalPlayer != null)
                cachedClass = pi.ClientState.LocalPlayer.ClassJob.Id;
            if (pi.ClientState.LocalPlayer != null) {
                ui.PlayerHealth = pi.ClientState.LocalPlayer.CurrentHp;
                ui.PlayerMP = pi.ClientState.LocalPlayer.CurrentMp;
            }
            ui.currentClass = configuration.classes.Find(a => a.ClassID == cachedClass);
            this.ui.Draw();
        }

        private void DrawConfigUI()
        {
            this.ui.SettingsVisible = true;
        }
    }
}
