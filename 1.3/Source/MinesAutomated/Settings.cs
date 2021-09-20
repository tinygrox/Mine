﻿using System.Linq;

namespace MinesAutomated {
    public class Settings : Verse.ModSettings {
        public bool disableLogging;
        //How much room each row of UI elements has.
        public float heightPerSetting = 25f;
        //The minimum and maximum values for the workamount and yield in %.
        public int minValue = 50;
        public int maxValue = 200;
        public System.Collections.Generic.List<SettingGlobalProperties> globalSettings = new System.Collections.Generic.List<SettingGlobalProperties>();
        public System.Collections.Generic.List<SettingindividualProperties> individualSettings = new System.Collections.Generic.List<SettingindividualProperties>();
        public override void ExposeData() {
            Verse.Scribe_Values.Look(ref disableLogging, "disableLogging", defaultValue: false);
            //Save the global settings
            foreach (SettingGlobalProperties sp in globalSettings)
                Verse.Scribe_Values.Look(ref sp.value, sp.Scribe_Values_String);
            //Save the individual settings
            foreach (SettingindividualProperties sp in individualSettings) {
                Verse.Scribe_Values.Look(ref sp.valueWorkamount, sp.Scribe_Values_Workamount, defaultValue: 100);
                Verse.Scribe_Values.Look(ref sp.valueYield, sp.Scribe_Values_Yield, defaultValue: 100);
            }
            base.ExposeData();
        }
        //Gets called whenever the Recipes should be updated.
        public void UpdateRecipeDefs() {
            System.Collections.Generic.List<string> recipes = CreateRecipeDefs.RecipeDefsNotToCreate();
            foreach (SettingindividualProperties sp in individualSettings) {
                if (!recipes.Contains(sp.recipeDef.defName)) {
                    Verse.RecipeDef rd = Verse.DefDatabase<Verse.RecipeDef>.GetNamed(sp.recipeDef.defName);
                    rd.products[0].count = (int)SettingsIndividual.CalculateValues(sp, this, false);
                    //Don't ask me where the 60 comes from, but it's needed for the calculation.
                    rd.workAmount = SettingsIndividual.CalculateValues(sp, this, true) * 60f;
                    rd.ResolveReferences();
                }
                Verse.DefDatabase<Verse.RecipeDef>.ResolveAllReferences();
            }
        }
        public Settings() {
            if (disableLogging)
                Verse.Log.Message("Mines 2.0: Logging has been disabled.");
            //The two global settings.
            globalSettings.Add(new SettingGlobalProperties("globalWorkamount", "Global workamount modifier"));
            globalSettings.Add(new SettingGlobalProperties("globalYield", "Global yield modifier"));

            foreach (Verse.ThingDef resourceBlock in Verse.DefDatabase<Verse.ThingDef>.AllDefs.Where(td => td.mineable && td.building?.mineableThing != null &&
            (td.building.isResourceRock || td.building.isNaturalRock))) {
                individualSettings.Add(new SettingindividualProperties(null, resourceBlock));
            }
        }
    }
    public class MinesAutomatedSettings : Verse.Mod {
        //Updates the RecipeDefs with the correct values after saving the settings.
        public override void WriteSettings() {
            Settings.UpdateRecipeDefs();
            base.WriteSettings();
        }
        public Settings Settings => GetSettings<Settings>();
        //Giving the Setting a name in the mod-setting window.
        public override string SettingsCategory() {
            return "Mines 2.0";
        }
        //The main method to draw the GUI.
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) {
            Verse.Listing_Standard listingStandard = new Verse.Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Disable logging", ref Settings.disableLogging);
            listingStandard.Label("All values are in %. Values can range between " + Settings.minValue + " and " + Settings.maxValue + ".");
            SettingsGlobal.DrawGlobalSettings(listingStandard, inRect.width, Settings);
            SettingsIndividual.DrawIndividualSettings(listingStandard, inRect.width, Settings, inRect);
            listingStandard.End();
        }
        public MinesAutomatedSettings(Verse.ModContentPack content) : base(content) { }
    }
}
