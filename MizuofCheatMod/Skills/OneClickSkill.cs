using UnityEngine;

namespace MizuofCheatMod
{
    /// <summary>一键功能 — 全部拉满</summary>
    public class OneClickSkill : ICheatSkill
    {
        public string Name => "一键功能";
        public string Prefix => "oc_";
        public bool IsMainMenuSkill => false;

        public (string id, string name) GetMainMenuItem() => ("m_oneclick", "一键功能");

        public bool Handle(string action)
        {
            if (!GameReflect.Ok()) return true;
            switch (action)
            {
                case "nature": MaxAllNature(); break;
                case "innate": MaxAllInAttri(); break;
                case "res": MaxResources(); break;
                case "course": LearnAllCourses(); break;
                case "skills": UnlockAllSkills(); break;
                case "items": AddAllItems(); break;
                case "costume": UnlockAllCostumes(); break;
                case "achieve": UnlockAllAchievements(); break;
                case "cg": UnlockAllCG(); break;
                case "map": UnlockAllMaps(); break;
                case "npcfav": MaxAllNPCFavor(); break;
                case "npclove": MaxAllNPCLover(); break;
                case "horse": UnlockAllHorses(); break;
                case "talent": UnlockAllTalents(); break;
                case "trueend": UnlockTrueEnding(); break;
                default: return false;
            }
            return true;
        }

        public static void Show() => ModMenu.OpenSub("oneclick", new[]{
            "oc_nature","oc_innate","oc_res","oc_course","oc_skills",
            "oc_items","oc_costume","oc_achieve","oc_cg","oc_map",
            "oc_npcfav","oc_npclove","oc_horse","oc_talent","oc_trueend"
        }, new[]{
            "基础属性→9999","三维属性→999","金钱/能量/心情最大","全部课程修完","全部技能解锁",
            "全部物品添加","全部服装解锁","全部成就解锁","CG图鉴全解锁","全地图解锁",
            "NPC好感全满","NPC恋爱全满","马匹全解锁","天赋树全点亮","真结局解锁"
        });

        // ===================== 实现函数 =====================
        static void MaxAllNature() { for (int i = 0; i < 4; i++) DauSys.AddNature(i, 9999, true); GameReflect.Tip("基础属性→9999"); }
        static void MaxAllInAttri() { for (int i = 0; i < 3; i++) DauSys.AddInAttri(i, 999, true); GameReflect.Tip("三维→999"); }
        static void MaxResources() { DataSys.AddCoin(BillType.otherIncome, 999999 - DataSys.Coin(), true, false); DataSys.AddMaxEnergy(999); DauSys.AddMood(999); DauSys.AddMotivation(999); DauSys.AddInspiration(999); GameReflect.Tip("资源最大"); }
        static void LearnAllCourses() { var cs = GameReflect.Inst(typeof(CourseSys)); foreach (var kv in DataSys.Instan.dataCfg.courseDic) { var c = kv.Value; if (c == null) continue; var own = GameReflect.Gf<System.Collections.Generic.List<string>>(cs, "ownCourses"); if (own != null && !own.Contains(c.enName)) own.Add(c.enName); c.prg = 100; c.owned = true; var fin = GameReflect.Gf<System.Collections.Generic.List<string>>(cs, "finishedCourses"); if (fin != null && !fin.Contains(c.enName)) fin.Add(c.enName); } GameReflect.Tip("课程全修完"); }
        static void UnlockAllSkills() { var fn = FightSys.FightNpc("dau"); if (fn == null) return; foreach (Skill sk in DataSys.Instan.dataCfg.skills) if (sk != null) fn.AddSkill(sk.index); fn.level = Constant.maxFightLevel; GameReflect.Tip("技能全解锁"); }
        static void AddAllItems() { foreach (Item it in DataSys.Instan.dataCfg.items) { if (it == null) continue; if (it.type == 38) { GameReflect.Inst(typeof(ItemSys)).GetType().GetMethod("AddFurniture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(GameReflect.Inst(typeof(ItemSys)), new object[] { it }); } else { typeof(ItemSys).GetMethod("AddItem", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(int), typeof(int), typeof(bool), typeof(bool) }, null)?.Invoke(null, new object[] { it.index, 999, false, true }); } } GameReflect.Tip("物品全添加"); }
        static void UnlockAllCostumes() { var clo = GameReflect.Gf<bool[]>(GameReflect.Inst(typeof(CollectSys)), "clothesUnlocked"); if (clo != null) for (int i = 0; i < clo.Length; i++) clo[i] = true; GameReflect.Tip("服装全解锁"); }
        static void UnlockAllAchievements() { for (int i = 0; i < DataSys.Instan.dataCfg.achievements.Length; i++) CollectSys.Instan.DoneAchievement(i); GameReflect.Tip("成就全解锁"); }
        static void UnlockAllCG() { PlayerPrefs.SetInt("openTrueEnd", 1); GameReflect.Tip("CG全解锁"); }
        static void UnlockAllMaps() { foreach (var mp in DataSys.Instan.dataCfg.mapPoints) if (mp != null) mp.finished = true; GameReflect.Tip("地图全解锁"); }
        static void MaxAllNPCFavor() { foreach (var p in DataSys.Instan.dataCfg.people) { if (p != null && p.isNpc) { GameReflect.Sf(p, "favor", 100); GameReflect.Sf(p, "relationLevel", 5); } } GameReflect.Tip("NPC好感全满"); }
        static void MaxAllNPCLover() { foreach (var p in DataSys.Instan.dataCfg.people) if (p != null && p.isNpc) GameReflect.Sf(p, "lover", 9999); DauSys.AddLover(9999); GameReflect.Tip("恋爱全满"); }
        static void UnlockAllHorses() { var hs = GameReflect.Inst(typeof(HorseSys)); var hh = GameReflect.Gf<System.Collections.Generic.List<Horse>>(hs, "ownHorse"); if (hh == null) return; foreach (HorseData hd in DataSys.Instan.dataCfg.horses) { if (hd == null) continue; bool exists = false; foreach (var h in hh) if (h.index == hd.index) { exists = true; break; } if (exists) continue; var nh = typeof(HorseSys).GetMethod("NewHorse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.Invoke(null, new object[] { hd, false }) as Horse; if (nh != null) { hh.Add(nh); } } GameReflect.Tip("马全解锁"); }
        static void UnlockAllTalents() { var ds = GameReflect.GetDauSys(); var o = GameReflect.Gf<bool[]>(ds, "talentLevelOpen"); if (o != null) for (int i = 0; i < o.Length; i++) o[i] = true; foreach (var t in DataSys.Instan.dataCfg.talents) if (t != null) t.level = t.maxLevel; DauSys.AddTalentPoint(999); GameReflect.Tip("天赋全点亮"); }
        static void UnlockTrueEnding() { PlayerPrefs.SetInt("openTrueEnd", 1); GameReflect.Tip("真结局解锁"); }
    }
}
