using EloBuddy;
using EloBuddy.SDK;

namespace LuVTryndamere.Utilities
{
    public static class SpellDamage
    {
        public static float GetTotalDamage(AIHeroClient target)
        {

            var damage = Program.User.GetAutoAttackDamage(target);
            if (Program.R.IsReady())
                damage = Program.User.GetSpellDamage(target, SpellSlot.R);
            if (Program.E.IsReady())
                damage = Program.User.GetSpellDamage(target, SpellSlot.E);
            if (Program.W.IsReady())
                damage = Program.User.GetSpellDamage(target, SpellSlot.W);
            if (Program.Q.IsReady())
                damage = Program.User.GetSpellDamage(target, SpellSlot.Q);

            return damage;
        }
    }
}
