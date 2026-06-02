using System.Collections.Generic;
using UnityEngine;

namespace MizuofCheatMod
{
    /// <summary>
    /// 菜单渲染引擎 — 5按钮分页/导航
    /// </summary>
    public static class ModMenu
    {
        public static bool MenuOpen = false, MenuWasHidden = false;
        public static int MenuPage = 0;
        public static string MenuContext = "main";
        public static string[] CachedIds, CachedNames;

        /// <summary>打开主菜单</summary>
        public static void ShowMain()
        {
            if (UiSys.Instan == null) return;
            MenuOpen = true; MenuContext = "main"; MenuPage = 0; MenuWasHidden = false;
            RenderMain(0);
        }

        /// <summary>关闭菜单</summary>
        public static void Close() { MenuWasHidden = true; MenuOpen = false; ChatSys.EndMenu(); }

        /// <summary>打开子菜单</summary>
        public static void OpenSub(string ctx, string[] ids, string[] names)
        { MenuOpen = true; MenuContext = ctx; MenuPage = 0; CachedIds = ids; CachedNames = names; RenderSub(); }

        // ===================== 主菜单渲染 =====================
        public static void RenderMainPage(int page) => RenderMain(page);
        static void RenderMain(int page)
        {
            MenuPage = page; MenuOpen = true; MenuContext = "main";
            string[] ids, names;
            switch (page)
            {
                case 0: ids = new[] { "m_oneclick", "m_attr", "m_time", "m_shop", "_pg1" }; names = new[] { "一键功能", "女儿属性", "时间管理", "物品商店", "→下一页" }; break;
                case 1: ids = new[] { "_pg0", "m_npc", "m_battle", "m_horse", "_pg2" }; names = new[] { "←上一页", "NPC编辑", "战斗编辑", "马匹编辑", "→下一页" }; break;
                case 2: ids = new[] { "_pg1", "m_map", "m_ending", "m_achieve", "_pg3" }; names = new[] { "←上一页", "地图数据", "结局设定", "成就设定", "→下一页" }; break;
                case 3: ids = new[] { "_prev", "m_gamecfg", "m_activity", "m_other", "_first" }; names = new[] { "←上一页", "★游戏规则", "活动修改", "其它修改", "≡返回首页" }; break;
                default: return;
            }
            ChatSys.Instan.ChatMenu(ids, names, ChatSys.MenuState.normal, null);
        }

        // ===================== 子菜单渲染（5按钮限制） =====================
        /// <summary>自适应：每页必有←返回，按需加←上页/→下页，内容数自动调整</summary>
        public static void RenderSubPage() => RenderSub();
        static void RenderSub()
        {
            int tot = CachedIds.Length, perPage = 3;
            int tp = Mathf.Max(1, (int)Mathf.Ceil((float)tot / perPage));
            int st = MenuPage * perPage;
            bool hp = MenuPage > 0, hn = MenuPage < tp - 1;

            // 计算导航占位：返回必占1，上页/下页按需
            int navCount = 1 + (hp ? 1 : 0) + (hn ? 1 : 0);
            int maxContent = 5 - navCount; // 留给内容的槽位数（1~3）

            var pi = new List<string>(); var pn = new List<string>();
            if (hp) { pi.Add("_prev_sub"); pn.Add("←上页"); }
            // 内容（自适应数量，最多maxContent个）
            int end = Mathf.Min(st + perPage, tot);
            int actual = Mathf.Min(end - st, maxContent);
            for (int i = st; i < st + actual; i++) { pi.Add(CachedIds[i]); pn.Add(CachedNames[i]); }
            if (hn) { pi.Add("_next_sub"); pn.Add("→下页"); }
            pi.Add("_back"); pn.Add("←返回"); // 每页必带
            ChatSys.Instan.ChatMenu(pi.ToArray(), pn.ToArray(), ChatSys.MenuState.normal, null);
        }

        // ===================== 全局路由分发 =====================
        public static void HandleRoute(string name)
        {
            // 导航
            if (name == "_pg0") { ChatSys.EndMenu(); MenuPage = 0; RenderMain(0); return; }
            if (name == "_pg1") { ChatSys.EndMenu(); MenuPage = 1; RenderMain(1); return; }
            if (name == "_pg2") { ChatSys.EndMenu(); MenuPage = 2; RenderMain(2); return; }
            if (name == "_pg3") { ChatSys.EndMenu(); MenuPage = 3; RenderMain(3); return; }
            if (name == "_close") { ChatSys.EndMenu(); MenuOpen = false; MenuWasHidden = false; return; }
            if (name == "_back") { ChatSys.EndMenu(); ShowMain(); return; }
            if (name == "_next_sub") { ChatSys.EndMenu(); MenuPage++; RenderSub(); return; }
            if (name == "_prev_sub") { ChatSys.EndMenu(); MenuPage--; RenderSub(); return; }
            if (name == "_first") { ChatSys.EndMenu(); MenuPage = 0; RenderMain(0); return; }

            // 自定义值路由 (cv_ / cvm_)
            if (HandleCustomValue(name)) return;

            // cfg_ 路由
            if (name.StartsWith("cfg_")) {
                string[] parts = name.Split('_');
                if (parts.Length == 2) {
                    ChatSys.EndMenu();
                    string cat = parts[1]; // "time", "action", etc.
                    // Show the category submenu
                    var vals = GameConfigSkill.GetCategoryVals(cat);
                    var ids = new System.Collections.Generic.List<string>();
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var kv in vals) {
                        foreach (int v in kv.Value) {
                            ids.Add("cfg_"+cat+"_"+kv.Key.ToLower()+"_"+v);
                            names.Add(kv.Key+" = "+v);
                        }
                    }
                    ModMenu.OpenSub("detail", ids.ToArray(), names.ToArray());
                }
                return;
            }

            // 主菜单路由
            if (name.StartsWith("m_"))
            {
                ChatSys.EndMenu();
                string key = name.Substring(2);
                switch (key)
                {
                    case "oneclick": OneClickSkill.Show(); break;
                    case "attr": AttrSkill.Show(); break;
                    case "time": TimeSkill.Show(); break;
                    case "shop": ItemSkill.Show(); break;
                    case "npc": NpcSkill.Show(); break;
                    case "battle": BattleSkill.Show(); break;
                    case "horse": HorseSkill.Show(); break;
                    case "map": MapSkill.Show(); break;
                    case "ending": EndingSkill.Show(); break;
                    case "achieve": AchieveSkill.Show(); break;
                    case "lord": LordSkill.Show(); break;
                    case "activity": ActivitySkill.Show(); break;
                    case "other": OtherSkill.Show(); break;
                    case "gamecfg": GameConfigSkill.Show(); break;
                }
                return;
            }

            ChatSys.EndMenu();
            ModMenu.MenuOpen = false;

            // 属性路由（AttrSkill 多前缀，直接分发）
            if (name.StartsWith("an_") || name.StartsWith("ai_") ||
                name.StartsWith("dn") || name.StartsWith("di") ||
                name.StartsWith("d_") || name.StartsWith("mj_") ||
                name.StartsWith("dfame_") || name.StartsWith("dmood_") || name.StartsWith("ddark_"))
            {
                AttrSkill.HandleAttrAction(name);
                ModMenu.MenuWasHidden = false;
                return;
            }

            // 委托给技能管理器
            if (SkillManager.Handle(name)) { ModMenu.MenuWasHidden = false; return; }

            // NPC 特殊路由（npc_ / ne_）
            if (name.StartsWith("npc_")) { HandleNpcSelect(name); return; }
            if (name.StartsWith("ne_")) { HandleNpcEdit(name); return; }
            if (name.StartsWith("st_remove_")) { HandleStateRemove(name); return; }
        }

        // ===== 自定义值路由（子子子面板/子子子子面板） =====
        static bool HandleCustomValue(string name)
        {
            if (!name.StartsWith("cv_")) return false;
            ChatSys.EndMenu();
            ModMenu.MenuOpen = false;
            // cv_n0_100 → nature 0 = 100
            // cv_bhp_5000 → battle HP = 5000
            // cv_n0_more → show more for nature 0
            // cv_n0m_8000 → more page, nature 0 = 8000
            string rest = name.Substring(3);
            bool isMore = name.Contains("_more");
            bool isMorePage = name.Contains("m_") && !isMore;

            if (isMore)
            {
                // 子子子 → 子子子子
                // cv_n0_more → ShowNatureCustMore(0)
                string target = rest.Replace("_more", "");
                // TODO: dispatch to correct skill's Show*More
                return false;
            }

            if (isMorePage)
            {
                // cv_n0m_8000 → the 'm' separates target from value
                int idx = rest.IndexOf('m');
                string target = rest.Substring(0, idx); // "n0"
                int val = int.Parse(rest.Substring(idx + 1)); // "8000"
                return DispatchCustomValue(target, val);
            }

            // cv_n0_100 → target="n0", val=100
            int lastUnder = rest.LastIndexOf('_');
            string tgt = rest.Substring(0, lastUnder);
            int v = int.Parse(rest.Substring(lastUnder + 1));
            return DispatchCustomValue(tgt, v);
        }

        static bool DispatchCustomValue(string target, int val)
        {
            if (!GameReflect.Ok()) return true;
            if (target.StartsWith("n"))
            {
                int idx = int.Parse(target.Substring(1));
                var n = GameReflect.Gf<int[]>(GameReflect.GetDauSys(), "nature");
                if (n != null && idx < n.Length) n[idx] = val;
                GameReflect.Tip($"{GameReflect.NatureCN[idx]} → {val}");
            }
            else if (target.StartsWith("i"))
            {
                int idx = int.Parse(target.Substring(1));
                var ia = GameReflect.Gf<int[]>(GameReflect.GetDauSys(), "inAttri");
                if (ia != null && idx < ia.Length) ia[idx] = val;
                GameReflect.Tip($"{GameReflect.InAttriCN[idx]} → {val}");
            }
            else if (target == "bhp") { BattleSkill.SetFightAttri(0, val); }
            else if (target == "batk") { BattleSkill.SetFightAttri(1, val); }
            else if (target == "bdef") { BattleSkill.SetFightAttri(2, val); }
            else if (target == "bavd") { BattleSkill.SetFightAttri(3, val); }
            else if (target == "bcri") { BattleSkill.SetFightAttri(4, val); }
            else if (target == "fame") { GameReflect.Sf(GameReflect.GetDauSys(), "fame", val); GameReflect.Tip($"名望→{val}"); }
            else if (target == "mood") { var ds = GameReflect.GetDauSys(); int cur = GameReflect.Gf<int>(ds, "mood"); for (int i = 0; i < Mathf.Abs(val - cur); i++) { if (val > cur) DauSys.AddMood(1); else DauSys.AddMood(-1); } GameReflect.Tip($"心情→{val}"); }
            else if (target.StartsWith("hspd")) { HorseSkill.SetHorseStat(0, val); }
            else if (target.StartsWith("happr")) { HorseSkill.SetHorseStat(1, val); }
            else if (target.StartsWith("hacc")) { HorseSkill.SetHorseStat(2, val); }
            return true;
        }

        // ===== NPC 路由 =====
        static void HandleNpcSelect(string name)
        {
            string en = name.Substring(4);
            foreach (var p in DataSys.Instan.dataCfg.people)
                if (p != null && p.enName == en) { NpcSkill.ShowEdit(p); return; }
        }
        static void HandleNpcEdit(string name)
        {
            string[] parts = name.Split('_');
            if (parts.Length < 3) return;
            string en = parts[2];
            foreach (var p in DataSys.Instan.dataCfg.people) if (p != null && p.enName == en)
                {
                    if (name.StartsWith("ne_fav_")) { GameReflect.Sf(p, "favor", 100); GameReflect.Sf(p, "relationLevel", 5); GameReflect.Tip($"{p.chName}好感→100"); }
                    else if (name.StartsWith("ne_love_")) { GameReflect.Sf(p, "lover", 9999); GameReflect.Tip($"{p.chName}恋爱→9999"); }
                    else if (name.StartsWith("ne_story_")) { GameReflect.Sf(p, "eveStage", 999); var rb = GameReflect.Gf<bool[]>(p, "relationBools"); if (rb != null) for (int i = 0; i < rb.Length; i++) rb[i] = true; GameReflect.Tip($"{p.chName}剧情全开"); }
                    else if (name.StartsWith("ne_event_")) { NpcSkill.TriggerEvent(p); }
                    else if (name.StartsWith("ne_reset_")) { GameReflect.Sf(p, "sendGiftBools", new bool[10]); GameReflect.Sf(p, "haveSentFavItems", new bool[20]); GameReflect.Tip($"{p.chName}送礼重置"); }
                    else if (name.StartsWith("ne_view_")) { NpcSkill.ShowDetail(p); }
                    return;
                }
        }
        static void HandleStateRemove(string name)
        {
            int idx = int.Parse(name.Substring("st_remove_".Length));
            var states = GameReflect.Gf<List<int>>(GameReflect.GetDauSys(), "ownState");
            if (states != null && idx < states.Count) { states.RemoveAt(idx); GameReflect.Tip("状态已移除"); }
        }
    }
}
