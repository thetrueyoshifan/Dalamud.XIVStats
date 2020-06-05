using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVStats
{
    public class ClassInfo
    {
        //public static List<string> ClassNames = new List<string> { "Adventurer", "Gladiator", "Pugilist", "Marauder", "Lancer", "Archer", "Conjurer", "Thaumaturge", "Carpenter", "Blacksmith", "Armorer", "Goldsmith", "Leatherworker", "Weaver", "Alchemist", "Culinarian", "Miner", "Botanist", "Fisher", "Paladin", "Monk", "Warrior", "Dragoon", "Bard", "White Mage", "Black Mage", "Arcanist", "Summoner", "Scholar", "Rogue", "Ninja", "Machinist", "Dark Knight", "Astrologian", "Samurai", "Red Mage", "Blue Mage", "Gunbreaker", "Dancer" };
        //public static List<string> AbbreviatedClassNames = new List<string> { "ADV", "GLA", "PGL", "MRD", "LNC", "ARC", "CNJ", "THM", "CRP", "BSM", "ARM", "GSM", "LTW", "WVR", "ALC", "CUL", "MIN", "BTN", "FSH", "PLD", "MNK", "WAR", "DRG", "BRD", "WHM", "BLM", "ACN", "SMN", "SCH", "ROG", "NIN", "MCH", "DRK", "AST", "SAM", "RDM", "BLU", "GNB", "DNC" };
        public static string ClassName(int classID)
        {       if (Plugin.Instance.pi.Data.IsDataReady)
                {
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Plugin.Instance.pi.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().GetRow(classID).Name);
                }
                else
                    return "";
        }
        public static string ClassAbbreviatedName(int classID)
        {
            if (Plugin.Instance.pi.Data.IsDataReady)
            {
                return Plugin.Instance.pi.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().GetRow(classID).Abbreviation;
            }
            else
                return "";
        }
        public string AssociatedCharacterName = "";
        public int ClassID = 0;
        public long TimeActive = 0;
        public long DamageDealt = 0;
        public long HealthHealed = 0;
        public int CommendationsReceived = 0;
        public int Deaths = 0;

    }
}
