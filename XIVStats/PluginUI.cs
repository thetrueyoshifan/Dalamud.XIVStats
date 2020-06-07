using ImGuiNET;
using System;
using System.Numerics;

namespace XIVStats
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public ClassInfo currentClass;
        public bool InDuty = false;
        public bool wasInDuty = false;
        public long DutyTime = 0;
        public int DutyDeaths = 0;

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(180, 160), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(180, 160), new Vector2(float.MaxValue, float.MaxValue));
            string WindowTitle = "XIVStats";
            if (currentClass != null)
            {
                WindowTitle += " | ";
                if (configuration.ShowAbbreviatedNames)
                    WindowTitle += ClassInfo.ClassAbbreviatedName(currentClass.ClassID);
                else
                    WindowTitle += ClassInfo.ClassName(currentClass.ClassID);
                TimeSpan classPlaytime = (new DateTime().AddSeconds(currentClass.TimeActive) - new DateTime());
                WindowTitle +=  " ["+classPlaytime.ToString("g")+"]";
            }
            WindowTitle += "###XIVStats";
            if (ImGui.Begin(WindowTitle, ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (currentClass != null)
                {
                    ImGui.Text("Class: " + (configuration.ShowAbbreviatedNames ? ClassInfo.ClassAbbreviatedName(currentClass.ClassID) : ClassInfo.ClassName(currentClass.ClassID)));
                    TimeSpan classPlaytime = (new DateTime().AddSeconds(currentClass.TimeActive) - new DateTime());
                    ImGui.Text("Time played: " + classPlaytime.ToString("g"));
                    ImGui.Text("Commendations received: " + currentClass.CommendationsReceived);
                    ImGui.Text("Deaths: " + currentClass.Deaths);
                    ImGui.Text("Damage Dealt: " + currentClass.DamageDealt);
                    ImGui.Text("Health Healed: " + currentClass.HealthHealed);
                }
                if (ImGui.Button("Configuration"))
                {
                    settingsVisible = true;
                }

            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("XIVStats Configuration", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = this.configuration.ShowAbbreviatedNames;
                if (ImGui.Checkbox("Show abbreviated class names", ref configValue)){
                    this.configuration.ShowAbbreviatedNames = configValue;
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
