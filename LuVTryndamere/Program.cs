using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuVTryndamere
{
    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;
    using SharpDX;

    class Program
    {

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static AIHeroClient User = Player.Instance;

        private static Spell.Active Q;

        private static Spell.Active W;

        private static Spell.Skillshot E;

        private static Spell.Active R;

        private static Menu MwMenu, ComboMenu, DrawingsMenu, MiscMenu;

        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (User.ChampionName != "Tryndamere")
            {
                return;
            }
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
            DrawingsMenu = MwMenu.AddSubMenu("Drawings");
            MiscMenu = MwMenu.AddSubMenu("Misc");

            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            ComboMenu.AddSeparator();

            MiscMenu.Add("qhp", new Slider("Hp% Use Q", 30, 0, 95));
            MiscMenu.Add("rhp", new Slider("Hp% Use R", 25, 0, 95));
            MiscMenu.AddSeparator();

            DrawingsMenu.Add("nocds", new CheckBox("Draw Only No CoolDown Spells"));
            DrawingsMenu.Add("text", new CheckBox("Show R Text"));
            DrawingsMenu.Add("Track", new CheckBox("Track"));
            DrawingsMenu.Add("aacount", new CheckBox("Show AA To Kill"));

            foreach (var Spell in SpellList)
            {
                DrawingsMenu.Add(Spell.Slot.ToString(), new CheckBox("Draw " + Spell.Slot));
            }

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                var Spell in SpellList.Where(spell => DrawingsMenu[spell.Slot.ToString()].Cast<CheckBox>().CurrentValue)
                )
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
                foreach (var enemy in EntityManager.Heroes.Enemies.Where((e => e.IsValid && e.IsHPBarRendered)))
            {
                var AADmg = User.GetAutoAttackDamage((enemy));
                var aacount = (int)(enemy.Health / AADmg);

                Drawing.DrawText(enemy.HPBarPosition.X + 15, enemy.HPBarPosition.Y + 200, System.Drawing.Color.NavajoWhite, string.Format("HP: {0} ({1})", enemy.Health, string.Concat(aacount > 0 ? "AA:" : "", aacount)), 10);
            }
            /*
            DrawKillable();
            */
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
            if (ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var autoQ = ComboMenu["useQ"].Cast<CheckBox>().CurrentValue;
                var healthQ = MiscMenu["qhp"].Cast<Slider>().CurrentValue;
                if (autoQ && !User.HasBuff("UndyingRage") && User.HealthPercent < healthQ)
                {
                    Q.Cast();
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
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
            if (ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                var autoR = ComboMenu["user"].Cast<CheckBox>().CurrentValue;
                var rhp = ComboMenu["rhp"].Cast<Slider>().CurrentValue;
                if (!autoR || (User.HealthPercent < rhp)) return;
                {
                    R.Cast();
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