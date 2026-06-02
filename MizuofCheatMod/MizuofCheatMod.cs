using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(MizuofCheatMod.MizuofCheatMod), "老父亲关怀修改器", "4.0", "Mizuof")]
[assembly: MelonGame("", "")]

namespace MizuofCheatMod
{
    /// <summary>
    /// 入口点 — Skill 架构 v4.0
    /// 初始化技能 → 监听 F1 → 路由全权委托 ModMenu
    /// </summary>
    public class MizuofCheatMod : MelonMod
    {
        internal static MizuofCheatMod Instance;

        public override void OnInitializeMelon()
        {
            Instance = this;
            MelonLogger.Msg("  ███╗   ███╗██╗███████╗██╗   ██╗ ██████╗ ███████╗");
            MelonLogger.Msg("  ████╗ ████║██║╚══███╔╝██║   ██║██╔═══██╗██╔════╝");
            MelonLogger.Msg("  ██╔████╔██║██║  ███╔╝ ██║   ██║██║   ██║█████╗  ");
            MelonLogger.Msg("  ██║╚██╔╝██║██║ ███╔╝  ██║   ██║██║   ██║██╔══╝  ");
            MelonLogger.Msg("  ██║ ╚═╝ ██║██║███████╗╚██████╔╝╚██████╔╝██║     ");
            MelonLogger.Msg("  ╚═╝     ╚═╝╚═╝╚══════╝ ╚═════╝  ╚═════╝ ╚═╝     ");
            MelonLogger.Msg("  火山的女儿 老父亲关怀修改器  by Mizuof");
            SkillManager.Init();
            HarmonyInstance.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1) && UiSys.Instan != null)
            {
                if (ModMenu.MenuOpen) { ModMenu.MenuWasHidden = true; ModMenu.Close(); }
                else if (ModMenu.MenuWasHidden)
                {
                    ModMenu.MenuOpen = true; ModMenu.MenuWasHidden = false;
                    if (ModMenu.MenuContext == "main") ModMenu.RenderMainPage(ModMenu.MenuPage);
                    else ModMenu.RenderSubPage();
                }
                else ModMenu.ShowMain();
            }
        }
    }
}
