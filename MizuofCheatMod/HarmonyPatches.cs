using System;
using System.Collections;
using MelonLoader;
using HarmonyLib;
using UnityEngine;

namespace MizuofCheatMod
{
    [HarmonyPatch(typeof(UiSys), "Start")]
    public static class Patch_Welcome
    {
        public static void Postfix()
        {
            if (!ModConfig.HasShownWelcome)
            {
                ModConfig.HasShownWelcome = true;
                UiSys.Instan.StartCoroutine(ShowWelcome());
            }
        }
        static IEnumerator ShowWelcome()
        {
            yield return new WaitForSeconds(2f);
            UiSys.SureUI(SureUi.none, true, "火山的女儿修改器\n本Mod由Mizuof制作");
        }
    }

    [HarmonyPatch(typeof(UiSys), "Update")]
    public static class Patch_CloseDlg
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var p = UiSys.Instan.uiTrans[16].Find("panel");
                if (p != null && p.parent.gameObject.activeSelf)
                    UiSys.SureUI(SureUi.none, false, "");
            }
        }
    }

    [HarmonyPatch(typeof(ChatSys), "ClickMenuChoice")]
    public static class Patch_MenuRoute
    {
        public static bool Prefix(string name)
        {
            // 关键守卫: 只有当 mod 菜单打开时才拦截，防止误截游戏自身菜单
            if (!ModMenu.MenuOpen && !ModMenu.MenuWasHidden) return true;
            // 全部 mod 路由前缀
            if (name == "_next" || name == "_prev" || name == "_close" || name == "_back" ||
                name == "_next_sub" || name == "_prev_sub" || name == "_first" ||
                name == "_pg0" || name == "_pg1" || name == "_pg2" || name == "_pg3" ||
                name.StartsWith("m_") || name.StartsWith("oc_") ||
                name.StartsWith("t_") || name.StartsWith("s_") || name.StartsWith("b_") ||
                name.StartsWith("n_") || name.StartsWith("ne_") || name.StartsWith("npc_") ||
                name.StartsWith("h_") || name.StartsWith("e_") || name.StartsWith("ed_") ||
                name.StartsWith("co_") || name.StartsWith("f_") || name.StartsWith("c_") ||
                name.StartsWith("st_") || name.StartsWith("st_remove_") ||
                name.StartsWith("dn") || name.StartsWith("di") || name.StartsWith("an_") ||
                name.StartsWith("ai_") || name.StartsWith("d_") || name.StartsWith("mj_") ||
                name.StartsWith("dfame_") || name.StartsWith("dmood_") || name.StartsWith("ddark_") ||
                name.StartsWith("map_") || name.StartsWith("lord_") || name.StartsWith("act_") ||
                name.StartsWith("o_") || name.StartsWith("btlhp_") || name.StartsWith("btlatk_") ||
                name.StartsWith("btldef_") || name.StartsWith("btlavd_") || name.StartsWith("btlcri_") ||
                name.StartsWith("hspd_") || name.StartsWith("happr_") || name.StartsWith("hacc_") ||
                name.StartsWith("haccn_") || name.StartsWith("turns_") || name.StartsWith("diced_") ||
                name.StartsWith("dramad_") || name.StartsWith("wr_") || name.StartsWith("pt_") ||
                name.StartsWith("explv_") || name.StartsWith("expcnt_") || name.StartsWith("kngh_") ||
                name.StartsWith("wishd_") || name.StartsWith("cv_") || name.StartsWith("cfg_"))
            {
                ModMenu.HandleRoute(name);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DataSys), "TransTurnBtn")]
    public static class Patch_Freeze
    {
        public static bool Prefix()
        {
            if (ModConfig.IsTimeFreeze) { UiSys.Instan.TransTurn(0); AudioSys.Instan.PlaySound(4); return false; }
            return true;
        }
    }

    [HarmonyPatch(typeof(DataSys), "Coin")]
    public static class Patch_Coin
    {
        public static bool Prefix(ref int __result)
        { if (ModConfig.IsUnlimitedMode) { __result = 999999; return false; } return true; }
    }

    [HarmonyPatch(typeof(DataSys), "MaxEnergy")]
    public static class Patch_Energy
    {
        public static bool Prefix(ref int __result)
        { if (ModConfig.IsUnlimitedMode) { __result = 999; return false; } return true; }
    }

    [HarmonyPatch(typeof(DataSys), "AddCoin")]
    public static class Patch_AddCoin
    {
        public static bool Prefix(ref int num)
        { if (ModConfig.IsUnlimitedMode && num < 0) num = 0; return true; }
    }

    [HarmonyPatch(typeof(DataSys), "AddEnergy")]
    public static class Patch_AddEnergy
    {
        public static bool Prefix(ref int num)
        { if (ModConfig.IsUnlimitedMode && num < 0) num = 0; return true; }
    }

    [HarmonyPatch(typeof(FightNpc), "CountSkillDamage")]
    public static class Patch_Dmg
    {
        public static void Postfix(FightNpc __instance, ref int __result, FightEnemy enemy = null)
        { if (ModConfig.IsOneHitKill && __instance.enName == "dau" && enemy != null) __result = 99999; }
    }

    [HarmonyPatch(typeof(FightEnemy), "BeAttacked")]
    public static class Patch_Slay
    {
        public static void Prefix(FightEnemy __instance)
        { if (ModConfig.IsOneHitKill) __instance.hp = 0; }
    }

    [HarmonyPatch(typeof(FightNpc), "AddHp")]
    public static class Patch_Heal
    {
        public static void Prefix(FightNpc __instance, ref int num)
        { if (ModConfig.IsUnlimitedMode && __instance.enName == "dau" && num < 0) num = 0; }
    }

    [HarmonyPatch(typeof(ItemSys), "CloseShop")]
    public static class Patch_RestoreShop
    {
        public static void Postfix() { CheatFunctions.RestoreShop(); }
    }
}
