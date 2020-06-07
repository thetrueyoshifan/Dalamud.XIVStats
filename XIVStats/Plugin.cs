using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace XIVStats
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "XIVStats";

        private const string commandName = "/xivstats";

        internal static Plugin Instance;
        internal DalamudPluginInterface pi;
        private Configuration configuration;
        private PluginUI ui;
        private Thread classCheckThread;

        private string cachedCommendationString = "";

        private int cachedClass = 0;
        private string cachedName = "";

        private bool spellCasted = false;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Instance = this;
            this.pi = pluginInterface;
            
            this.configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.configuration.Initialize(this.pi);

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

        public void OnChatMessage(Dalamud.Game.Chat.XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (cachedCommendationString == "")
            {
                if (pi.Data.IsDataReady)
                {
                    cachedCommendationString = pi.Data.GetExcelSheet<LogMessage>().GetRow(926).Text;
                }
            }
            if (message.TextValue == cachedCommendationString && sender.TextValue == "")
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).CommendationsReceived++;
            }
            else if ((message.TextValue.Contains("You are defeated") || message.TextValue.Contains("Du brichst zusammen") || (message.TextValue.Contains("Du wurdest von") && message.TextValue.Contains("besiegt")) || message.TextValue.Contains("Vous vous effondrez") || message.TextValue.Contains("Vous avez été vaincue") || message.TextValue == "\u306f\u3001\u529b\u5c3d\u304d\u305f\u3002") && sender.TextValue == "")
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).Deaths++;
                if (ui.InDuty)
                    ui.DutyDeaths++;
            }
            else if (((message.TextValue.Contains("You use") || message.TextValue.Contains("You cast")) && sender.TextValue == ""))
            {
                spellCasted = true;
            }
            else if (message.TextValue.Contains("takes") && message.TextValue.Contains("damage") && spellCasted)
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).DamageDealt += long.Parse(Regex.Match(message.TextValue, @"\d+").Value);
            }
            else if (message.TextValue.Contains("recover") && message.TextValue.Contains("HP") && spellCasted)
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).HealthHealed += long.Parse(Regex.Match(message.TextValue, @"\d+").Value);
            }
            else if (spellCasted && !message.TextValue.Contains("recover") && (!message.TextValue.Contains("takes") && !message.TextValue.Contains("damage")) && !message.TextValue.Contains("defeat") && !message.TextValue.Contains("suffers") && !message.TextValue.Contains("misses"))
                spellCasted = false;
            else if (message.TextValue.Contains("You hit the") && sender.TextValue == "")
            {
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass) != -1)
                    configuration.classes.Find(a => a.ClassID == cachedClass).DamageDealt += long.Parse(Regex.Match(message.TextValue, @"\d+").Value);
            }
        }

        public void Loop()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (configuration.classes.FindIndex(a => a.ClassID == cachedClass && a.AssociatedCharacterName != cachedName) == -1)
                {
                    configuration.classes.Add(new ClassInfo { ClassID = cachedClass, AssociatedCharacterName = cachedName });
                }
                ClassInfo inf = configuration.classes.Find(a => a.ClassID == cachedClass);
                inf.TimeActive++;
                configuration.Save();
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
            this.ui.Visible = true;
        }

        private void DrawUI()
        {
            if (pi.ClientState.LocalPlayer != null)
            {
                cachedClass = pi.ClientState.LocalPlayer.ClassJob.Id;
                if (!string.IsNullOrEmpty(pi.ClientState.LocalPlayer.Name))
                cachedName = pi.ClientState.LocalPlayer.Name;

                if (ui.currentClass == null || ui.currentClass.ClassID != cachedClass)
                    ui.currentClass = configuration.classes.Find(a => a.ClassID == cachedClass);
            }
            this.ui.Draw();
        }

        private void DrawConfigUI()
        {
            this.ui.SettingsVisible = true;
        }
    }
}
