using System.Collections.Generic;

namespace MizuofCheatMod
{
    public class TimeSkill : ICheatSkill
    {
        public string Name => "时间管理";
        public string Prefix => "t_";
        public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_time", "时间管理");
        public bool Handle(string action)
        {
            switch (action) {
                case "reset": CheatFunctions.ResetTurn(); return true;
                case "stage1": CheatFunctions.JumpToStage(1); return true;
                case "stage2": CheatFunctions.JumpToStage(2); return true;
                case "actions10": CheatFunctions.SetMaxActions(10); return true;
                case "energy": CheatFunctions.SetMaxEnergy999(); return true;
                case "turn_detail": ShowTurnDetail(); return true;
                case "turn_s6": CheatFunctions.SetTurnExact(6); return true;
                case "turn_s18": CheatFunctions.SetTurnExact(18); return true;
                case "turn_s30": CheatFunctions.SetTurnExact(30); return true;
                case "turn_s50": CheatFunctions.SetTurnExact(50); return true;
                case "turn_s100": CheatFunctions.SetTurnExact(100); return true;
                case "turn_s200": CheatFunctions.SetTurnExact(200); return true;
            }
            return false;
        }
        public static void Show() => ModMenu.OpenSub("time", new[]{"t_reset","t_stage1","t_stage2","t_actions10","t_energy","t_turn_detail"}, new[]{"回到第1回合","跳到少年期(7)","跳到青年期(19)","每日行动→10","最大能量→999","回合→详细设置"});
        static void ShowTurnDetail() => ModMenu.OpenSub("detail", new[]{"t_turn_s6","t_turn_s18","t_turn_s30","t_turn_s50","t_turn_s100","t_turn_s200"}, new[]{"→6(幼年结束)","→18(少年结束)","→30(中期)","→50","→100","→200(终盘)"});
    }

    public class BattleSkill : ICheatSkill
    {
        public string Name => "战斗编辑";
        public string Prefix => "b_btlhp_btlatk_btldef_btlavd_btlcri_";
        public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_battle", "战斗编辑");
        public bool Handle(string action)
        {
            if (action.StartsWith("btlhp_") || action.StartsWith("btlatk_") || action.StartsWith("btldef_") || action.StartsWith("btlavd_") || action.StartsWith("btlcri_"))
                return HandleBattleDetail(action);
            switch (action) {
                case "max": CheatFunctions.MaxBattleStats(); return true;
                case "onehit": CheatFunctions.ToggleOneHitKill(); return true;
                case "skip": CheatFunctions.ToggleBattleSkip(); return true;
                case "heal": CheatFunctions.HealBattleFull(); return true;
                case "level": CheatFunctions.MaxBattleLevel(); return true;
                case "exp": CheatFunctions.AddBattleExp(99999); return true;
                case "skills": CheatFunctions.UnlockAllSkills(); return true;
                case "weapon": CheatFunctions.SwitchWeapon(); return true;
                case "hp_detail": ShowHpDetail(); return true;
                case "atk_detail": ShowAtkDetail(); return true;
                case "def_detail": ShowDefDetail(); return true;
                case "avd_detail": ShowAvdDetail(); return true;
                case "cri_detail": ShowCriDetail(); return true;
            }
            return false;
        }

        bool HandleBattleDetail(string a)
        {
            string[] parts = a.Split('_');
            string type = parts[0]; // btlhp, btlatk, etc.
            int idx = type == "btlhp" ? 0 : type == "btlatk" ? 1 : type == "btldef" ? 2 : type == "btlavd" ? 3 : 4;
            string op = parts[1];
            if (op.StartsWith("p")) { int[] v = { 500, 5000, -500, -5000 }; int oi = int.Parse(op.Substring(1)); SetFightAttri(idx, v[oi - 1]); }
            else if (op.StartsWith("s")) { int v = int.Parse(op.Substring(1)); SetFightAttri(idx, v); }
            else if (op == "cust") { ShowBattleCust(idx); }
            return true;
        }

        public static void SetFightAttri(int idx, int v) {
            var fn = FightSys.FightNpc("dau"); if (fn == null) return;
            fn.AddFightAttri((FightAttri)idx, v - fn.fightAttri[idx], true);
            GameReflect.Tip($"{GameReflect.FightStatCN[idx]}→{v}");
        }

        public static void Show()
        {
            ModMenu.OpenSub("battle", new[]{
                "b_max","b_onehit","b_skip","b_heal","b_level",
                "b_exp","b_skills","b_weapon",
                "b_hp_detail","b_atk_detail","b_def_detail","b_avd_detail","b_cri_detail"
            }, new[]{"战斗属性全满",
                ModConfig.IsOneHitKill ? "✓一击必杀":"一击必杀",
                ModConfig.IsBattleSkip ? "✓跳过":"跳过",
                "完全恢复","等级→29","经验+99999","技能解锁","武器切换",
                "HP→详细","攻击→详细","防御→详细","闪避→详细","暴击→详细"
            });
        }

        static void ShowHpDetail() => ShowBattleDetail("btlhp", new[]{500,5000,-500,-5000,0,5000,50000,99999});
        static void ShowAtkDetail() => ShowBattleDetail("btlatk", new[]{500,1000,-500,-1000,0,1000,5000,9999});
        static void ShowDefDetail() => ShowBattleDetail("btldef", new[]{500,1000,-500,-1000,0,1000,5000,9999});
        static void ShowAvdDetail() => ShowBattleDetail("btlavd", new[]{10,20,-10,-20,0,30,50,80});
        static void ShowCriDetail() => ShowBattleDetail("btlcri", new[]{10,20,-10,-20,0,30,50,80});
        static void ShowBattleDetail(string prefix, int[] vals)
        {
            var ids = new System.Collections.Generic.List<string>();
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 4; i++) { ids.Add($"{prefix}_p{i+1}"); names.Add((vals[i] > 0 ? "+" : "") + vals[i]); }
            ids.Add($"{prefix}_s0"); names.Add("→0");
            ids.Add($"{prefix}_s{vals[5]}"); names.Add($"→{vals[5]}");
            ids.Add($"{prefix}_s{vals[6]}"); names.Add($"→{vals[6]}");
            ids.Add($"{prefix}_s{vals[7]}"); names.Add($"→{vals[7]}");
            ids.Add($"{prefix}_cust"); names.Add("→自定义");
            ModMenu.OpenSub("detail", ids.ToArray(), names.ToArray());
        }

        static void ShowBattleCust(int idx)
        {
            string[] prefixes = {"bhp","batk","bdef","bavd","bcri"};
            string p = prefixes[idx];
            int[] vals = idx < 3 ? new[]{100,300,500,800,1000,2000,3000,5000} : new[]{5,10,15,20,25,30,35,40,45,50};
            var ids = new System.Collections.Generic.List<string>();
            var names = new System.Collections.Generic.List<string>();
            foreach (var v in vals) { ids.Add($"cv_{p}_{v}"); names.Add($"→{v}"); }
            ids.Add($"cv_{p}_more"); names.Add("→更多...");
            ModMenu.OpenSub("detail", ids.ToArray(), names.ToArray());
        }
    }

    public class NpcSkill : ICheatSkill
    {
        public string Name => "NPC编辑";
        public string Prefix => "n_";
        public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_npc", "NPC编辑");
        public bool Handle(string action)
        {
            switch (action) {
                case "allfav": CheatFunctions.MaxAllNPCFavor(); return true;
                case "alllove": CheatFunctions.MaxAllNPCLover(); return true;
                case "allstory": CheatFunctions.UnlockAllNPCStories(); return true;
                case "resetgift": CheatFunctions.ResetGiftRecords(); return true;
                case "allequip": CheatFunctions.MaxAllNPCEquip(); return true;
                case "pick": ShowPickList(); return true;
                case "view": CheatFunctions.ShowAllNPCData(); return true;
            }
            return false;
        }
        public static void Show() => ModMenu.OpenSub("npc", new[]{"n_allfav","n_alllove","n_allstory","n_resetgift","n_allequip","n_pick","n_view"}, new[]{"所有NPC好感→100","所有NPC恋爱→9999","NPC剧情全开","送礼记录重置","NPC装备全重置","选择单个NPC编辑","查看全部NPC数据"});
        static void ShowPickList() {
            var list = DataSys.Instan?.dataCfg?.people; if (list == null) return;
            var ids = new System.Collections.Generic.List<string>(); var names = new System.Collections.Generic.List<string>();
            foreach (var p in list) if (p != null && p.isNpc) { ids.Add("npc_"+p.enName); names.Add(p.chName+" (好感:"+CheatFunctions.GetFav(p)+")"); }
            ModMenu.OpenSub("npclist", ids.ToArray(), names.ToArray());
        }
        public static void ShowEdit(Person p) => ModMenu.OpenSub("npcedit", new[]{"ne_fav_"+p.enName,"ne_love_"+p.enName,"ne_story_"+p.enName,"ne_event_"+p.enName,"ne_reset_"+p.enName,"ne_view_"+p.enName}, new[]{"好感→100","恋爱→9999","剧情→最终","触发下段剧情","送礼重置","查看数据"});
        public static void ShowDetail(Person p) {
            var eq = GameReflect.Gf<int[]>(p, "equips");
            string t = $"<b>【{p.chName}】</b>\n好感:{GameReflect.Gf<int>(p,"favor")} 恋爱:{GameReflect.Gf<int>(p,"lover")}\n名望:{GameReflect.Gf<int>(p,"fame")} 关系:{GameReflect.Gf<int>(p,"relationLevel")}\n剧情:{GameReflect.Gf<int>(p,"eveStage")}";
            if(eq != null) t += $"\n装备:{string.Join("/",eq)}";
            GameReflect.Alert(t);
        }
        public static void TriggerEvent(Person p) {
            if (p == null) return;
            var metList = NpcSys.Instan.MetPersons();
            if (metList != null && !metList.Contains(p.enName)) metList.Add(p.enName);
            GameReflect.Sf(p, "refuseInvitation", false);
            int stage = GameReflect.Gf<int>(p, "eveStage");
            var events = GameReflect.Gf<System.Collections.Generic.List<Person.FavorEvent>>(p, "events");
            if (events == null || stage >= events.Count) { GameReflect.Tip(p.chName+"已无更多剧情"); return; }
            GameReflect.Sf(p, "relationLevel", stage + 2);
            NpcSys.Instan.favorEve = events[stage];
            NpcSys.tempNpc = p;
            NpcSys.Instan.SetEvePer(events[stage]);
            ChatSys.StartNpcChat(p.enName, "date"+stage, 0);
            GameReflect.Sf(p, "eveStage", stage + 1);
        }
    }

    public class HorseSkill : ICheatSkill
    {
        public string Name => "马匹编辑";
        public string Prefix => "h_";
        public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_horse", "马匹编辑");
        public bool Handle(string action)
        {
            switch (action) {
                case "stat_detail": ShowStatDetail(); return true;
                case "favor": CheatFunctions.MaxHorseFavor(); return true;
                case "all": CheatFunctions.UnlockAllHorses(); return true;
                case "racewin": CheatFunctions.MaxHorseRaceWin(); return true;
                case "point": CheatFunctions.SetHorsePoint(9999); return true;
                case "appetency": CheatFunctions.MaxHorseAppetency(); return true;
                case "stable": CheatFunctions.RenameStable(); return true;
            }
            if (action.StartsWith("hspd") || action.StartsWith("happr") || action.StartsWith("hacc"))
                return HandleStatDetail(action);
            return false;
        }

        public static void SetHorseStat(int idx, int val) {
            var hh = GameReflect.Gf<System.Collections.Generic.List<Horse>>(GameReflect.GetHorseSys(), "ownHorses");
            if (hh != null) foreach (var h in hh) if (h != null && h.natures != null && idx < h.natures.Length) h.natures[idx] = val;
            GameReflect.Tip($"马{GameReflect.HorseStatCN[idx]}→{val}");
        }

        bool HandleStatDetail(string a) {
            string[] parts = a.Split('_');
            int idx = parts[0] == "hspd" ? 0 : parts[0] == "happr" ? 1 : 2;
            string op = parts[1];
            if (op.StartsWith("p")) { int[] v = {50,100,-50,-100}; int oi=int.Parse(op.Substring(1)); SetHorseStat(idx, v[oi-1]); }
            else if (op.StartsWith("s")) { int v=int.Parse(op.Substring(1)); SetHorseStat(idx, v); }
            else if (op == "cust") { ShowCust(idx); }
            return true;
        }

        public static void Show() => ModMenu.OpenSub("horse", new[]{
            "h_stat_detail","h_favor","h_all","h_racewin","h_point",
            "h_appetency","h_stable"
        }, new[]{"马属性→详细","好感最大","解锁全马","比赛胜利","点数→9999","亲密度→最大","马厩改名"});

        static void ShowStatDetail()
        {
            ModMenu.OpenSub("detail", new[]{
                "hspd_p1","hspd_p2","hspd_p3","hspd_p4","hspd_s0","hspd_s500","hspd_s999","hspd_cust",
                "happr_p1","happr_p2","happr_p3","happr_p4","happr_s0","happr_s500","happr_s999","happr_cust",
                "hacc_p1","hacc_p2","hacc_p3","hacc_p4","hacc_s0","hacc_s500","hacc_s999","hacc_cust",
                "haccn_s1","haccn_s3","haccn_s5"
            }, new[]{"速度+50","+100","-50","-100","→0","→500","→999","→自定义",
                "外貌+50","+100","-50","-100","→0","→500","→999","→自定义",
                "加速+50","+100","-50","-100","→0","→500","→999","→自定义",
                "加次→1","→3","→5"});
        }

        static void ShowCust(int idx) {
            string[] prefixes = {"hspd","happr","hacc"};
            string p = prefixes[idx];
            ModMenu.OpenSub("detail", new[]{
                $"cv_{p}_10","cv_{p}_30","cv_{p}_50","cv_{p}_80","cv_{p}_100","cv_{p}_150","cv_{p}_200",
                $"cv_{p}_300","cv_{p}_400","cv_{p}_500","cv_{p}_more"
            }, new[]{"→10","→30","→50","→80","→100","→150","→200","→300","→400","→500","→更多..."});
        }
    }

    public class ItemSkill : ICheatSkill
    {
        public string Name => "物品商店";
        public string Prefix => "s_";
        public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_shop", "物品商店");
        public bool Handle(string action) {
            switch (action) {
                case "cheatshop": CheatFunctions.OpenCheatShop(); return true;
                case "closeshop": CloseShop(); return true;
                case "items": CheatFunctions.AddAllItemsToBag(); return true;
                case "costume": CheatFunctions.UnlockAllCostumes(); return true;
                case "farm": CheatFunctions.MaxFarm(); return true;
                case "cook": CheatFunctions.MaxCooking(); return true;
                case "alchemy": CheatFunctions.MaxAlchemy(); return true;
                case "books": CheatFunctions.MaxAllBooks(); return true;
                case "furniture": CheatFunctions.UnlockAllFurniture(); return true;
                case "magicshop": CheatFunctions.ResetMagicShop(); return true;
                case "lost": CheatFunctions.RecoverLostItems(); return true;
            }
            return false;
        }
        static void CloseShop() {
            var isys = GameReflect.Inst(typeof(ItemSys));
            if(isys != null) {
                // 直接调用 OpenShop(false) 关闭当前商店窗口
                var m = isys.GetType().GetMethod("OpenShop", new System.Type[]{typeof(bool), typeof(string)});
                if(m != null) m.Invoke(isys, new object[]{false, "weaponShop"});
                // 同时关闭服装店
                m?.Invoke(isys, new object[]{false, "cosShop"});
            }
            CheatFunctions.RestoreShop();
            GameReflect.Tip("商店已关闭");
        }
        public static void Show() => ModMenu.OpenSub("shop", new[]{
            "s_cheatshop","s_closeshop","s_items","s_costume",
            "s_farm","s_cook","s_alchemy","s_books",
            "s_furniture","s_magicshop","s_lost"
        }, new[]{
            "★打开作弊商店","■关闭商店","全部物品×999","全部服装解锁",
            "农场满级","烹饪全解锁","炼金全满","读书全满",
            "家具全解锁","魔法商店重置","失物找回"
        });
    }

    public class MapSkill : ICheatSkill
    {
        public string Name => "地图数据"; public string Prefix => "map_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_map", "地图数据");
        public bool Handle(string action) {
            switch (action) {
                case "unlock": CheatFunctions.UnlockAllMaps(); return true;
                case "points": CheatFunctions.CompleteAllMapPoints(); return true;
                case "level_detail": ShowLevelDetail(); return true;
                case "cnt_detail": ShowCountDetail(); return true;
                case "lev_s0": GameReflect.Sf(GameReflect.GetDataSys(), "countDic", null); var cd = GameReflect.Gf<System.Collections.Generic.Dictionary<StateType,int>>(GameReflect.GetDataSys(), "countDic"); if(cd != null) cd[StateType.exploLv]=0; GameReflect.Tip("探索等级→0"); return true;
                case "lev_s10": SetExplore(StateType.exploLv, 10); return true;
                case "lev_s50": SetExplore(StateType.exploLv, 50); return true;
                case "lev_s100": SetExplore(StateType.exploLv, 100); return true;
                case "lev_s500": SetExplore(StateType.exploLv, 500); return true;
                case "lev_s999": SetExplore(StateType.exploLv, 999); return true;
                case "cnt_s0": SetExplore(StateType.exploCount, 0); return true;
                case "cnt_s100": SetExplore(StateType.exploCount, 100); return true;
                case "cnt_s500": SetExplore(StateType.exploCount, 500); return true;
                case "cnt_s999": SetExplore(StateType.exploCount, 999); return true;
            }
            return false;
        }
        static void SetExplore(StateType t, int v) {
            var cd = GameReflect.Gf<System.Collections.Generic.Dictionary<StateType,int>>(GameReflect.GetDataSys(), "countDic");
            if(cd != null) cd[t] = v;
            GameReflect.Tip((t==StateType.exploLv?"探索等级":"探索次数")+"→"+v);
        }
        public static void Show() => ModMenu.OpenSub("map", new[]{"map_unlock","map_points","map_level_detail","map_cnt_detail"}, new[]{"全地图解锁","所有探索点完成","探索等级→详细","探索次数→详细"});
        static void ShowLevelDetail() => ModMenu.OpenSub("detail", new[]{"map_lev_s0","map_lev_s10","map_lev_s50","map_lev_s100","map_lev_s500","map_lev_s999"}, new[]{"→0","→10","→50","→100","→500","→最大999"});
        static void ShowCountDetail() => ModMenu.OpenSub("detail", new[]{"map_cnt_s0","map_cnt_s100","map_cnt_s500","map_cnt_s999"}, new[]{"→0","→100","→500","→999"});
    }

    public class EndingSkill : ICheatSkill
    {
        private static bool _endingPlaying = false;

        public string Name => "结局设定"; public string Prefix => "ed_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_ending", "结局设定");
        public bool Handle(string action) {
            switch (action) {
                case "open": CheatFunctions.OpenEndingSelection(); return true;
                case "true": CheatFunctions.UnlockTrueEnding(); return true;
                case "list": ShowList(); return true;
                case "score_max": CheatFunctions.MaxEndingScore(); return true;
            }
            // ed_trig_5 → trigger ending index 5
            if (action.StartsWith("trig_")) {
                int idx = int.Parse(action.Substring(5));
                TriggerEnding(idx);
                return true;
            }
            return false;
        }
        static void TriggerEnding(int idx) {
            if (!GameReflect.Ok() || _endingPlaying) return;
            _endingPlaying = true;
            var ends = DataSys.Instan?.dataCfg?.endings;
            if (ends == null || idx < 0 || idx >= ends.Count || ends[idx] == null) return;
            var ending = ends[idx];
            // 直接播放结局剧情（使用游戏原生结局播放 API）
            GameReflect.Sf(EndingSys.Instan, "ending", ending);
            EndingSys.Instan.StartEndingShowChat(0, ending, "", false);
            GameReflect.Tip("正在播放结局: "+ending.chName);
            _endingPlaying = false; // 立即释放锁，但 HarmonyPatch 的 Guard 防止重复
        }
        public static void Show() => ModMenu.OpenSub("ending", new[]{"ed_open","ed_true","ed_list","ed_score_max"}, new[]{"打开结局选择界面","解锁真结局(二周目)","结局列表(可点击触发)","结局评分→最大"});
        static void ShowList() {
            var ends = DataSys.Instan?.dataCfg?.endings; if (ends == null) return;
            var ids = new System.Collections.Generic.List<string>();
            var names = new System.Collections.Generic.List<string>();
            foreach (var e in ends) {
                if (e == null) continue;
                ids.Add("ed_trig_"+e.index);
                names.Add("#"+e.index+" "+e.chName+"(Lv."+e.level+")");
            }
            ModMenu.OpenSub("detail", ids.ToArray(), names.ToArray());
        }
    }

    public class AchieveSkill : ICheatSkill
    {
        public string Name => "成就设定"; public string Prefix => "co_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_achieve", "成就设定");
        public bool Handle(string action) {
            switch (action) {
                case "achieve": CheatFunctions.UnlockAllAchievements(); return true;
                case "cg": CheatFunctions.UnlockAllCG(); return true;
                case "inherit": CheatFunctions.MaxInheritPoints(); return true;
                case "costume": CheatFunctions.UnlockAllCostumes(); return true;
                case "score": CheatFunctions.MaxCollectionScore(); return true;
            }
            return false;
        }
        public static void Show() => ModMenu.OpenSub("achieve", new[]{"co_achieve","co_cg","co_inherit","co_costume","co_score"}, new[]{"全部成就解锁","CG/图鉴全解锁","继承点数满","服装全解锁","收藏室评分→最高"});
    }

    public class LordSkill : ICheatSkill
    {
        public string Name => "领主修改"; public string Prefix => "lord_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_lord", "领主修改");
        public bool Handle(string action) {
            switch (action) {
                case "knightlv_detail": ShowKnightDetail(); return true;
                case "knightexp": CheatFunctions.AddKnightExp(99999); return true;
                case "tasks": CheatFunctions.CompleteAllLordTasks(); return true;
                case "wishes_detail": ShowWishDetail(); return true;
                case "klv_s1": SetKnight(1); return true;
                case "klv_s2": SetKnight(2); return true;
                case "klv_s3": SetKnight(3); return true;
                case "klv_s4": SetKnight(4); return true;
                case "klv_s5": SetKnight(5); return true;
                case "klv_max": SetKnight(5); GameReflect.Tip("骑士等级→最大"); return true;
                case "wsh_all": CheatFunctions.CompleteAllWishes(); return true;
                case "wsh_s50": SetWish(50); return true;
                case "wsh_s100": SetWish(100); return true;
            }
            return false;
        }
        static void SetKnight(int v) {
            var kexp = DataSys.Instan.dataCfg.knightLevelUpExp;
            if(kexp != null) for(int i=0;i<kexp.Length;i++) kexp[i] = 0;
            GameReflect.Tip("骑士等级→Lv."+v);
        }
        static void SetWish(int v) {
            var ts = GameReflect.Inst(typeof(TaskSys));
            if(ts != null) { var ws = GameReflect.Gf<int[]>(ts, "dauWishScore"); if(ws != null) for(int i=0;i<ws.Length;i++) ws[i] = UnityEngine.Mathf.Min(v, Constant.maxDauWishScore); }
            GameReflect.Tip("心愿评分→"+v);
        }
        public static void Show() => ModMenu.OpenSub("lord", new[]{"lord_knightlv_detail","lord_knightexp","lord_tasks","lord_wishes_detail"}, new[]{"骑士等级→详细","骑士经验+99999","领主任务全部完成","女儿心愿→详细"});
        static void ShowKnightDetail() => ModMenu.OpenSub("detail", new[]{"lord_klv_s1","lord_klv_s2","lord_klv_s3","lord_klv_s4","lord_klv_s5","lord_klv_max"}, new[]{"→Lv.1见习","→Lv.2初级","→Lv.3中级","→Lv.4高级","→Lv.5终极","→满级"});
        static void ShowWishDetail() => ModMenu.OpenSub("detail", new[]{"lord_wsh_all","lord_wsh_s50","lord_wsh_s100"}, new[]{"全部达成","评分→50","评分→100(最大)"});
    }

    public class ActivitySkill : ICheatSkill
    {
        public string Name => "活动修改"; public string Prefix => "act_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_activity", "活动修改");
        public bool Handle(string action) {
            switch (action) {
                case "drama": CheatFunctions.UnlockAllDramas(); return true;
                case "drama_detail": ShowDramaDetail(); return true;
                case "date": CheatFunctions.UnlockAllDates(); return true;
                case "fawork": CheatFunctions.MaxFaWork(); return true;
                case "race": CheatFunctions.MaxAllRaces(); return true;
                case "dice": CheatFunctions.SetDiceAlwaysWin(); return true;
                case "dice_detail": ShowDiceDetail(); return true;
                case "letter": CheatFunctions.UnlockAllLetters(); return true;
                case "penfriend": CheatFunctions.UnlockAllPenfriend(); return true;
                case "dance": CheatFunctions.UnlockAllDances(); return true;
                case "painting": CheatFunctions.MaxPaintingScore(); return true;
                case "hunt": CheatFunctions.AddHuntCount(999); return true;
                case "debate": CheatFunctions.AddDebateWin(999); return true;
                case "drdone": CheatFunctions.MaxDramaIncome(); return true;
                case "dract": GameReflect.Sf(GameReflect.GetDauSys(), "actingLevel", 5); GameReflect.Tip("演技等级→5"); return true;
                case "dicewin": CheatFunctions.SetDiceAlwaysWin(); return true;
                case "dicelose": CheatFunctions.SetDiceAlwaysLose(); return true;
                case "dicept6": CheatFunctions.SetDicePoint(6); return true;
                case "dicecnt99": CheatFunctions.SetDiceCount(99); return true;
            }
            return false;
        }
        public static void Show() => ModMenu.OpenSub("activity", new[]{
            "act_drama_detail","act_date","act_fawork","act_race","act_dice_detail",
            "act_letter","act_penfriend","act_dance","act_painting","act_hunt","act_debate"
        }, new[]{"戏剧→详细","约会全解锁","父亲工作","赛马全胜利","骰子→详细",
            "信件全解锁","笔友信全解锁","舞蹈/节庆","画作评价最高","狩猎+999","辩论+999"});
        static void ShowDramaDetail() => ModMenu.OpenSub("detail", new[]{"act_drama","act_drdone","act_dract"}, new[]{"戏剧全解锁","戏剧收入最大(演技5)","演技等级→5"});
        static void ShowDiceDetail() => ModMenu.OpenSub("detail", new[]{"act_dicewin","act_dicelose","act_dicept6","act_dicecnt99"}, new[]{"骰子必胜","骰子必输","点数→6","次数→99"});
    }

    public class OtherSkill : ICheatSkill
    {
        public string Name => "其它修改"; public string Prefix => "o_"; public bool IsMainMenuSkill => false;
        public (string id, string name) GetMainMenuItem() => ("m_other", "其它修改");
        public bool Handle(string action) {
            switch (action) {
                case "worry_detail": ShowWorryDetail(); return true;
                case "patient_detail": ShowPatientDetail(); return true;
                case "mom": CheatFunctions.TriggerMomStory(); return true;
                case "name": CheatFunctions.ShowNameEditor(); return true;
                case "birth": CheatFunctions.SetBirthday(); return true;
                case "blood": CheatFunctions.CycleBloodType(); return true;
                case "tutorial": CheatFunctions.ResetTutorial(); return true;
                case "lang": CheatFunctions.CycleLanguage(); return true;
                case "worry_all": CheatFunctions.ClearAllWorries(); return true;
                case "worry_view": CheatFunctions.ShowStateList(); return true;
                case "patient_all": CheatFunctions.CureAllPatient(); return true;
                case "patient_view": {
                    var states = GameReflect.Gf<System.Collections.Generic.List<int>>(GameReflect.GetDauSys(), "ownState");
                    if(states == null || states.Count == 0) GameReflect.Alert("当前没有伤病。");
                    else GameReflect.Alert($"<b>【伤病列表({states.Count}个)】</b>\n可在消除单个状态中逐一移除。");
                    return true;
                }
            }
            return false;
        }
        public static void Show() => ModMenu.OpenSub("other", new[]{
            "o_worry_detail","o_patient_detail","o_mom","o_name","o_birth","o_blood","o_tutorial","o_lang"
        }, new[]{"烦恼→详细","伤病→详细","触发妈妈剧情","查看姓名","查看生日","血型切换ABC","教程重置","语言切换"});
        static void ShowWorryDetail() => ModMenu.OpenSub("detail", new[]{"o_worry_all","o_worry_view"}, new[]{"消除所有烦恼","查看烦恼列表"});
        static void ShowPatientDetail() => ModMenu.OpenSub("detail", new[]{"o_patient_all","o_patient_view"}, new[]{"治疗所有伤病","查看伤病列表"});
    }
}
