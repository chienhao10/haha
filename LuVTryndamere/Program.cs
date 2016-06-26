﻿namespace LuVTryndamere
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;
    using EloBuddy.SDK.Spells;
    using SharpDX;
    /*todo:smite,smite combo, dmg indicator, r buff tick count, ks with flash e, DrawKillable();*/
    class Program
    {
        public static readonly Item Cutlass = new Item((int)ItemId.Bilgewater_Cutlass, 550);

        public static readonly Item Botrk = new Item((int)ItemId.Blade_of_the_Ruined_King, 550);

        public static readonly Item Youmuu = new Item((int)ItemId.Youmuus_Ghostblade);

        public static readonly Item Tiamat = new Item((int)ItemId.Tiamat);

        public static readonly Item Hydra = new Item((int)ItemId.Ravenous_Hydra);

        public static readonly Item Potion = new Item((int)ItemId.Health_Potion), Biscuit = new Item(2010), Refillable = new Item(2031), Hunter = new Item(2032), Corrupting = new Item(2033);

        private static AIHeroClient User = Player.Instance;

        private static Spell.Active Q;

        private static Spell.Active W;

        private static Spell.Skillshot E;

        private static Spell.Active R;

        private static Menu MwMenu, ComboMenu, HarassMenu, JungleMenu, LaneMenu, FleeMenu, ItemMenu, DrawingsMenu, MiscMenu;

        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (User.ChampionName != "Tryndamere")
            {
                return;
            }

            Chat.Print("LuV Tryndmere BY ChienHao Has Loaded!", System.Drawing.Color.Purple);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W, 850);
            E = new Spell.Skillshot(SpellSlot.E, 660, SkillShotType.Linear, 250, 700, (int)92.5);
            R = new Spell.Active(SpellSlot.R);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            MwMenu = MainMenu.AddMenu("LuV Tryndamere", "LuV Tryndamere");
            ComboMenu = MwMenu.AddSubMenu("Combo");
            HarassMenu = MwMenu.AddSubMenu("Harass");
            JungleMenu = MwMenu.AddSubMenu("Jungle Clear");
            LaneMenu = MwMenu.AddSubMenu("Lane Clear");
            FleeMenu = MwMenu.AddSubMenu("Flee");
            MiscMenu = MwMenu.AddSubMenu("Misc");
            ItemMenu = MwMenu.AddSubMenu("Items");
            DrawingsMenu = MwMenu.AddSubMenu("Drawing");

            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            ComboMenu.AddSeparator();
            ComboMenu.AddLabel("Combo Sumoner Spells");
            ComboMenu.Add("ign", new CheckBox("Use Combo Ignite"));
            ComboMenu.Add("exh", new CheckBox("Use Combo Exhaust"));

            HarassMenu.Add("useW", new CheckBox("Use W"));
            HarassMenu.Add("useE", new CheckBox("Use E"));
            HarassMenu.AddSeparator();
            
            JungleMenu.Add("useE", new CheckBox("Use E"));
            JungleMenu.AddSeparator();

            LaneMenu.Add("useE", new CheckBox("Use E"));
            LaneMenu.AddSeparator();

            FleeMenu.Add("useW", new CheckBox("Use W"));
            FleeMenu.Add("useE", new CheckBox("Use E"));
            FleeMenu.AddSeparator();

            ItemMenu.AddGroupLabel("Global Settings");
            ItemMenu.Add("Items", new CheckBox("Use Items In Combo"));
            ItemMenu.Add("pots", new CheckBox("Use Auto Potion"));
            ItemMenu.AddGroupLabel("Items Settings");
            ItemMenu.Add("cut", new CheckBox("Use Cutlass"));
            ItemMenu.Add("bork", new CheckBox("Use Botrk"));
            ItemMenu.Add("yo", new CheckBox("Use Youmuu"));
            ItemMenu.Add("tia", new CheckBox("Use Timat"));
            ItemMenu.Add("hyd", new CheckBox("Use Hydra"));
            ItemMenu.Add("qss", new CheckBox("Use QSS"));
            ItemMenu.Add("eL", new Slider("Use On Enemy health", 65, 0, 100));
            ItemMenu.Add("oL", new Slider("Use On My health", 65, 0, 100));
            ItemMenu.AddGroupLabel("Potions Settings");
            ItemMenu.Add("potion", new CheckBox("Use Health Potion"));
            ItemMenu.Add("ref", new CheckBox("Use Refillable Potion"));
            ItemMenu.Add("hunt", new CheckBox("Use Hunter Potion"));
            ItemMenu.Add("cour", new CheckBox("Use Corrupting Potion"));
            ItemMenu.Add("bis", new CheckBox("Use Biscuit"));
            ItemMenu.Add("mh", new Slider("Hp% Use Potion", 75, 0, 100));
            ItemMenu.AddSeparator();

            MiscMenu.AddGroupLabel("Global Settings");
            MiscMenu.Add("qhp", new Slider("Hp% Use Q", 30, 0, 95));
            MiscMenu.Add("rhp", new Slider("Hp% Use R", 25, 0, 95));
            MiscMenu.AddLabel("Killsteal");
            MiscMenu.Add("kse", new CheckBox("KS E"));
            MiscMenu.Add("ksfe", new CheckBox("KS Flash + E"));
            MiscMenu.AddSeparator();

            DrawingsMenu.Add("nocds", new CheckBox("Only Draw Spells Are Ready"));
            DrawingsMenu.Add("text", new CheckBox("Show R Text"));
            DrawingsMenu.Add("drawekill", new CheckBox("Draw E Killable"));
            DrawingsMenu.Add("Track", new CheckBox("Track"));
            DrawingsMenu.Add("aacount", new CheckBox("Show AA To Kill"));

            foreach (var Spell in SpellList)
            {
                if (!User.IsDead)
                {
                    DrawingsMenu.Add(Spell.Slot.ToString(), new CheckBox("Draw " + Spell.Slot));
                }
            }

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            ////DMG INDICATOR
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                var Spell in SpellList.Where(spell => DrawingsMenu[spell.Slot.ToString()].Cast<CheckBox>().CurrentValue))
            {
                Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, User);
            }

            if (DrawingsMenu["nocds"].Cast<CheckBox>().CurrentValue)
            {
                if (E.IsReady())
                {
                    Circle.Draw(Color.Green, E.Range, User);
                }
            }

            if (DrawingsMenu["drawekill"].Cast<CheckBox>().CurrentValue || E.IsReady())
            {
                foreach (var etarget in EntityManager.Heroes.Enemies.Where(
                hero => hero.IsValidTarget(E.Range) && !hero.IsDead && !hero.IsZombie)
                .Where(etarget => User.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health))
                {
                    {
                        Drawing.DrawText(
                            etarget.HPBarPosition.X + 50, User.HPBarPosition.Y + 215,
                            System.Drawing.Color.Purple,
                            "E KILL!");
                    }
                }
            }

            if (DrawingsMenu["text"].Cast<CheckBox>().CurrentValue)
            {
                if (!R.IsReady() && R.Level > 0 && !User.HasBuff("UndyingRage") && !User.IsDead)
                {
                    Drawing.DrawText(
                        User.HPBarPosition.X + 50, User.HPBarPosition.Y + 215,
                        System.Drawing.Color.Red,
                        "R Not Ready");
                }

                if (User.HasBuff("UndyingRage"))
                {
                    Drawing.DrawText(
                        User.HPBarPosition.X + 50, User.HPBarPosition.Y + 215,
                        System.Drawing.Color.Purple,
                        "R Actavited");
                }
                else
                if (R.IsReady() && R.IsLearned)
                {
                    Drawing.DrawText(
                        User.HPBarPosition.X + 50, User.HPBarPosition.Y + 215,
                        System.Drawing.Color.White,
                        "R Ready");
                }

                if (User.IsDead)
                {
                    return;
                }
            }

            if (DrawingsMenu.Get<CheckBox>("Track").CurrentValue)
            {
                DrawEnemyHealth();
            }

            if (DrawingsMenu.Get<CheckBox>("aacount").CurrentValue)
            { 
                foreach (var enemy in EntityManager.Heroes.Enemies.Where((e => e.IsValid && e.IsHPBarRendered)))
                {
                    {
                        var AADmg = User.GetAutoAttackDamage((enemy));
                        var aacount = (int)(enemy.Health / AADmg);
                        Drawing.DrawText(enemy.HPBarPosition.X + 15, enemy.HPBarPosition.Y + 200, System.Drawing.Color.NavajoWhite, string.Format("HP: {0} ({1})", enemy.Health, string.Concat(aacount > 0 ? "AA:" : "", aacount)), 10);
                    }
                }
            }

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (User.IsDead)
            {
                return;
            }

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }

            Killsteal();

            if (ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var autoQ = ComboMenu["useQ"].Cast<CheckBox>().CurrentValue;
                var healthQ = MiscMenu["qhp"].Cast<Slider>().CurrentValue;
                if (autoQ && !User.HasBuff("UndyingRage") && User.HealthPercent < healthQ)
                {
                    Q.Cast();
                }
            }

            if (ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                var rhp = MiscMenu["rhp"].Cast<Slider>().CurrentValue;
                if (User.HealthPercent < rhp)
                {
                    R.Cast();
                }
            }
            
            if (ItemMenu["pots"].Cast<CheckBox>().CurrentValue)
            {
                Potions();
            }
            
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (ComboMenu["ign"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(600) && SummonerSpells.Ignite.IsReady())
                {
                    SummonerSpells.Ignite.Cast(target);
                }
            }

            if (ComboMenu["exh"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(650) && SummonerSpells.Exhaust.IsReady())
                {
                    SummonerSpells.Exhaust.Cast(target);
                }
            }

            if (ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var pred = E.GetPrediction(target);
                if (target.IsValidTarget(E.Range) && E.IsReady() && pred.HitChance >= HitChance.High)
                {
                    E.Cast(target);
                }
            }

            if (ComboMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(W.Range) && W.IsReady())
                {
                    W.Cast();
                }
            }

            if (ItemMenu["Items"].Cast<CheckBox>().CurrentValue)
            {
                Items();
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (HarassMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var pred = E.GetPrediction(target);
                if (target.IsValidTarget(E.Range) && E.IsReady() && pred.HitChance >= HitChance.High && !User.IsDead)
                {
                    E.Cast(target);
                }
            }

            if (HarassMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(W.Range) && W.IsReady())
                {
                    W.Cast();
                }
            }
        }
        
        private static void JungleClear()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            {
                if (JungleMenu["useE"].Cast<CheckBox>().CurrentValue)
                {
                    var pred = E.GetPrediction(target);
                    if (target.IsValidTarget(E.Range) && E.IsReady() && pred.HitChance >= HitChance.High && !User.IsDead)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            {
                if (LaneMenu["useE"].Cast<CheckBox>().CurrentValue)
                {
                    var pred = E.GetPrediction(target);
                    if (target.IsValidTarget(E.Range) && E.IsReady() && pred.HitChance >= HitChance.High && !User.IsDead)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        private static void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (HarassMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(W.Range) && W.IsReady())
                {
                    W.Cast();
                }
            }

            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(Game.CursorPos);
        }

        private static void Killsteal()
        {
            if (!MiscMenu["kse"].Cast<CheckBox>().CurrentValue || !E.IsReady()) return;
            if (MiscMenu["kse"].Cast<CheckBox>().CurrentValue || E.IsReady())
            {
                    foreach (var etarget in EntityManager.Heroes.Enemies.Where(
                    hero => hero.IsValidTarget(E.Range) && !hero.IsDead && !hero.IsZombie)
                    .Where(etarget => User.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health))
                {
                    {
                        E.Cast(etarget.ServerPosition);
                    }
                }
            }
            if (MiscMenu["ksfe"].Cast<CheckBox>().CurrentValue || E.IsReady())
                                    foreach (var etarget in EntityManager.Heroes.Enemies.Where(
                    hero => hero.IsValidTarget(1070) && !hero.IsDead && !hero.IsZombie && SummonerSpells.Flash.IsReady())
                    .Where(etarget => User.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health))
                {
                    {
                        SummonerSpells.Flash.Cast((etarget.Position));
                        E.Cast(etarget.ServerPosition);
                    }
                }
        }

        private static void Items()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (Botrk.IsReady() && Botrk.IsOwned(User) && Botrk.IsInRange(target)
                && target.HealthPercent <= ItemMenu["eL"].Cast<Slider>().CurrentValue && ItemMenu["bork"].Cast<CheckBox>().CurrentValue)
            {
                Botrk.Cast(target);
            }

            if (Botrk.IsReady() && Botrk.IsOwned(User) && Botrk.IsInRange(target)
                && target.HealthPercent <= ItemMenu["oL"].Cast<Slider>().CurrentValue && ItemMenu["bork"].Cast<CheckBox>().CurrentValue)
            {
                Botrk.Cast(target);
            }

            if (Cutlass.IsReady() && Cutlass.IsOwned(User) && Cutlass.IsInRange(target)
                && target.HealthPercent <= ItemMenu["eL"].Cast<Slider>().CurrentValue && ItemMenu["cut"].Cast<CheckBox>().CurrentValue)
            {
                Cutlass.Cast(target);
            }

            if (Youmuu.IsReady() && Youmuu.IsOwned(User) && target.IsValidTarget(Q.Range)
                && ItemMenu["yo"].Cast<CheckBox>().CurrentValue)
            {
                Youmuu.Cast();
            }
            if (Hydra.IsReady() && Hydra.IsOwned(User) && target.IsValidTarget(Q.Range)
                && ItemMenu["hyd"].Cast<CheckBox>().CurrentValue)
            {
                Hydra.Cast();
            }
            if (Tiamat.IsReady() && Tiamat.IsOwned(User) && target.IsValidTarget(Q.Range)
                && ItemMenu["tia"].Cast<CheckBox>().CurrentValue)
            {
                Tiamat.Cast();
            }
        }
        
        private static void Potions()
        {
            if (User.IsInShopRange() || User.IsRecalling() || User.HasBuff("RegenerationPotion")
                || User.HasBuff("ItemMiniRegenPotion") || User.HasBuff("ItemCrystalFlask")
                || User.HasBuff("ItemCrystalFlaskJungle") || User.HasBuff("ItemDarkCrystalFlask"))
                return;

            if (ItemMenu["pots"].Cast<CheckBox>().CurrentValue)
            {
                var Health = ItemMenu["mh"].Cast<Slider>().CurrentValue;
                if (User.HealthPercent < Health && Potion.IsOwned() && Refillable.IsOwned() && Corrupting.IsOwned() && Biscuit.IsOwned() && Hunter.IsOwned()
                    && !User.HasBuff("ItemCrystalFlask")
                && !User.HasBuff("ItemCrystalFlaskJungle") && !User.HasBuff("ItemDarkCrystalFlask") && !User.HasBuff("ItemMiniRegenPotion") && User.HasBuff("RegenerationPotion"))
                {
                    if (Hunter.IsReady())
                    {
                        Hunter.Cast();
                    }
                    else if (Corrupting.IsReady())
                    {
                        Corrupting.Cast();
                    }
                    else if (Refillable.IsReady())
                    {
                        Refillable.Cast();
                    }
                    else if (Potion.IsReady())
                    {
                        Potion.Cast();
                    }
                    else if (Biscuit.IsReady())
                    {
                        Biscuit.Cast();
                    }
                }
            }
                
        }
        
        private static void DrawEnemyHealth()
        {
            {
                float i = 0;
                foreach (var hero in
                    EntityManager.Heroes.Enemies.Where(
                        hero => hero != null && hero.IsEnemy && !hero.IsMe && !hero.IsDead))
                {
                    var champion = hero.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }

                    var percent = (int)hero.HealthPercent;
                    var color = System.Drawing.Color.Red;
                    if (percent > 25)
                    {
                        color = System.Drawing.Color.Orange;
                    }

                    if (percent > 50)
                    {
                        color = System.Drawing.Color.Yellow;
                    }

                    if (percent > 75)
                    {
                        color = System.Drawing.Color.LimeGreen;
                    }

                    Drawing.DrawText(Drawing.Width * 0.01f, Drawing.Height * 0.1f + i, color, champion);
                    Drawing.DrawText(
                        Drawing.Width * 0.06f,
                        Drawing.Height * 0.1f + i,
                        color,
                        (" ( " + (int)hero.TotalShieldHealth()) + " / " + (int)hero.MaxHealth + " | " + percent + "% ) ");

                    if (hero.IsVisible)
                    {
                        Drawing.DrawText(Drawing.Width * 0.13f, Drawing.Height * 0.1f + i, color, "");
                    }

                    i += 20f;
                }
            }
        }

        /*
        private static void DrawKillable()
        {
            throw new NotImplementedException();
        }
        */
    }
}