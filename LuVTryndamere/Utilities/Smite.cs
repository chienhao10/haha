using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace LuVTryndamere.Utilities
{
    internal class Smite
    {
        public static double TotalDamage = 0;
        public static AIHeroClient User = Player.Instance;

        public static Spell.Targeted SmiteSpell;

        public static Obj_AI_Base Monster;


        public static readonly string[] SmiteableUnits =
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak",
            "SRU_Red", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_RiftHerald",
            "SRU_Dragon_Elder", "SRU_Dragon_Air", "SRU_Dragon_Earth",
            "SRU_Dragon_Fire", "SRU_Dragon_Water"
        };

        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714, 3931, 1415, 1419, 1401 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707, 3930, 1416 };
        public static Menu SmiteMenu;

        public static void Smitemethod()
        {
            SmiteMenu = MainMenu.AddMenu("Smite","test");
            SmiteMenu.AddSeparator();
            SmiteMenu.Add("smiteActive",
                   new KeyBind("Smite Active (toggle)", true, KeyBind.BindTypes.PressToggle, 'M'));
            SmiteMenu.AddSeparator();
            SmiteMenu.Add("useSlowSmite", new CheckBox("KS with Blue Smite"));
            SmiteMenu.Add("comboWithDuelSmite", new CheckBox("Combo with Red Smite"));
            SmiteMenu.AddSeparator();
            SmiteMenu.AddGroupLabel("Camps");
            SmiteMenu.AddLabel("Epics");
            if (Game.MapId == GameMapId.TwistedTreeline)
            {
                SmiteMenu.AddGroupLabel("Mobs Settings");
                SmiteMenu.Add("TT_Spiderboss", new CheckBox("Vilemaw Enabled"));
                SmiteMenu.Add("TT_NGolem", new CheckBox("Golem Enabled"));
                SmiteMenu.Add("TT_NWolf", new CheckBox("Wolf Enabled"));
                SmiteMenu.Add("TT_NWraith", new CheckBox("Wraith Enabled"));
            }
            if (Game.MapId == GameMapId.SummonersRift)
            {
                SmiteMenu.AddGroupLabel("Mobs Settings");
                SmiteMenu.Add("SRU_Baron", new CheckBox("Baron Enabled"));
                SmiteMenu.Add("SRU_RiftHerald", new CheckBox("RiftHerald Enabled"));
                SmiteMenu.Add("SRU_Blue", new CheckBox("Blue Enabled"));
                SmiteMenu.Add("SRU_Red", new CheckBox("Red Enabled"));
                SmiteMenu.Add("SRU_Gromp", new CheckBox("Gromp Enabled"));
                SmiteMenu.Add("SRU_Murkwolf", new CheckBox("Murkwolf Enabled"));
                SmiteMenu.Add("SRU_Krug", new CheckBox("Krug Enabled"));
                SmiteMenu.Add("SRU_Razorbeak", new CheckBox("Razorbeak Enabled"));
                SmiteMenu.Add("Sru_Crab", new CheckBox("Crab Enabled"));
                SmiteMenu.Add("SRU_Dragon_Elder", new CheckBox("Dragon Elder Enabled"));
                SmiteMenu.Add("SRU_Dragon_Air", new CheckBox("Dragon Air Enabled"));
                SmiteMenu.Add("SRU_Dragon_Earth", new CheckBox("Dragon Earth Enabled"));
                SmiteMenu.Add("SRU_Dragon_Fire", new CheckBox("Dragon Fire Enabled"));
                SmiteMenu.Add("SRU_Dragon_Water", new CheckBox("Dragon Water Enabled"));
            }

            Game.OnUpdate += SmiteEvent;
        }

        public static void SetSmiteSlot()
        {
            SpellSlot smiteSlot;
            if (SmiteBlue.Any(x => User.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = User.GetSpellSlotFromName("s5_summonersmiteplayerganker");
            else if (
                SmiteRed.Any(
                    x => User.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = User.GetSpellSlotFromName("s5_summonersmiteduel");
            else
                smiteSlot = User.GetSpellSlotFromName("summonersmite");
            SmiteSpell = new Spell.Targeted(smiteSlot, 500);
        }

        public static int GetSmiteDamage()
        {
            var level = User.Level;
            int[] smitedamage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return smitedamage.Max();
        }

        private static void SmiteEvent(EventArgs args)
        {
            SetSmiteSlot();
            if (!SmiteSpell.IsReady() || User.IsDead) return;
            if (SmiteMenu["smiteActive"].Cast<KeyBind>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.Monsters
                        .Where(
                            a =>
                                SmiteableUnits.Contains(a.BaseSkinName) && a.Health < GetSmiteDamage() &&
                                SmiteMenu[a.BaseSkinName].Cast<CheckBox>().CurrentValue)
                        .OrderByDescending(a => a.MaxHealth)
                        .FirstOrDefault();

                if (unit != null)
                {
                    SmiteSpell.Cast(unit);
                    return;
                }
            }
            if (SmiteMenu["useSlowSmite"].Cast<CheckBox>().CurrentValue &&
                SmiteSpell.Handle.Name == "s5_summonersmiteplayerganker")
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(SmiteSpell.Range) && h.Health <= 20 + 8 * User.Level))
                {
                    SmiteSpell.Cast(target);
                    return;
                }
            }
            if (SmiteMenu["comboWithDuelSmite"].Cast<CheckBox>().CurrentValue &&
                SmiteSpell.Handle.Name == "s5_summonersmiteduel" &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(SmiteSpell.Range)).OrderByDescending(TargetSelector.GetPriority)
                    )
                {
                    SmiteSpell.Cast(target);
                    return;
                }
            }
        }
    }
}