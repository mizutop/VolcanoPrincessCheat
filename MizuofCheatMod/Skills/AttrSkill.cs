using System.Collections.Generic;

namespace MizuofCheatMod
{
    /// <summary>女儿属性 — 详细修改（含子子子面板）</summary>
    public class AttrSkill : ICheatSkill
    {
        public string Name => "女儿属性";
        public string Prefix => "__attr_internal__"; // 由 ModMenu 直接路由
        public bool IsMainMenuSkill => true;
        public (string id, string name) GetMainMenuItem() => ("m_attr", "女儿属性");

        // 接收完整 action（不经过 Prefix 剥离）
        public bool Handle(string action)
        {
            return HandleAttrAction(action);
        }

        public static bool HandleAttrAction(string a)
        {
            if (!GameReflect.Ok()) return true;
            var ds = GameReflect.GetDauSys();

            // dn0_p1 → nature 0 +100
            if (a.StartsWith("dn") && a.Contains("_p")) {
                int idx = int.Parse(a[2].ToString()); int op = int.Parse(a[5].ToString());
                int[] vals = { 100, 1000, -100, -1000 };
                DauSys.AddNature(idx, (op >= 1 && op <= 4) ? vals[op - 1] : 0); return true;
            }
            if (a.StartsWith("dn") && a.Contains("_s")) {
                int idx = int.Parse(a[2].ToString()); int v = int.Parse(a.Substring(5));
                var n = GameReflect.Gf<int[]>(ds, "nature"); if (n != null && idx < n.Length) n[idx] = v;
                GameReflect.Tip($"{GameReflect.NatureCN[idx]}→{v}"); return true;
            }
            if (a.StartsWith("dn") && a.Contains("_cust")) {
                ShowNatureCust(int.Parse(a[2].ToString())); return true;
            }

            // di0_p1 → inattri 0 +100
            if (a.StartsWith("di") && a.Contains("_p")) {
                int idx = int.Parse(a[2].ToString()); int op = int.Parse(a[5].ToString());
                int[] vals = { 100, 500, -100, -500 };
                DauSys.AddInAttri(idx, (op >= 1 && op <= 4) ? vals[op - 1] : 0); return true;
            }
            if (a.StartsWith("di") && a.Contains("_s")) {
                int idx = int.Parse(a[2].ToString()); int v = int.Parse(a.Substring(5));
                var ia = GameReflect.Gf<int[]>(ds, "inAttri"); if (ia != null && idx < ia.Length) ia[idx] = v;
                GameReflect.Tip($"{GameReflect.InAttriCN[idx]}→{v}"); return true;
            }
            if (a.StartsWith("di") && a.Contains("_cust")) {
                ShowInAttriCust(int.Parse(a[2].ToString())); return true;
            }

            // an_0 → show nature detail
            if (a.StartsWith("an_")) { ShowNatureDetail(int.Parse(a[3].ToString())); return true; }
            if (a.StartsWith("ai_")) { ShowInAttriDetail(int.Parse(a[3].ToString())); return true; }

            // 其它属性
            switch (a) {
                case "d_other": ShowOther(); return true;
                case "d_fame": CheatFunctions.AddFameCustom(1000); return true;
                case "d_fame_detail": ShowFameDetail(); return true;
                case "d_mood": CheatFunctions.AddMoodCustom(200); return true;
                case "d_mood_detail": ShowMoodDetail(); return true;
                case "d_moti": CheatFunctions.AddMotivationCustom(200); return true;
                case "d_insp": CheatFunctions.AddInspirationCustom(200); return true;
                case "d_dark": CheatFunctions.SetDarkness(0); return true;
                case "d_dark_detail": ShowDarkDetail(); return true;
                case "d_talent": CheatFunctions.AddTalentPoints(100); return true;
                case "d_favor": CheatFunctions.MaxFatherFavor(); return true;
                case "d_lover": CheatFunctions.MaxAllNPCLover(); return true;
                case "d_loverlevel": GameReflect.Sf(ds, "loverLevel", 5); DauSys.AddLover(9999); return true;
                case "d_height": GameReflect.Sf(ds, "height", 170); return true;
                case "a_onemax": CheatFunctions.MaxAllNature(); CheatFunctions.MaxAllInAttri(); CheatFunctions.MaxResources(); return true;
                case "a_panel": CheatFunctions.ShowHiddenStats(); return true;

                // 名望详细
                case "dfame_a1000": CheatFunctions.AddFameCustom(1000); return true;
                case "dfame_s1000": CheatFunctions.AddFameCustom(-1000); return true;
                case "dfame_s0": GameReflect.Sf(ds, "fame", 0); return true;
                case "dfame_s500": GameReflect.Sf(ds, "fame", 500); return true;
                case "dfame_s9999": GameReflect.Sf(ds, "fame", 9999); return true;
                case "dfame_s99999": GameReflect.Sf(ds, "fame", 99999); return true;

                // 心情详细
                case "dmood_a100": CheatFunctions.AddMoodCustom(100); return true;
                case "dmood_s100": CheatFunctions.AddMoodCustom(-100); return true;
                case "dmood_a500": CheatFunctions.AddMoodCustom(500); return true;
                case "dmood_s500": CheatFunctions.AddMoodCustom(-500); return true;
                case "dmood_s0": int m0 = -DauSys.Mood(); while (m0 < 0) { DauSys.AddMood(-1); m0++; } return true;
                case "dmood_set100": int m1 = 100 - DauSys.Mood(); for (int i = 0; i < UnityEngine.Mathf.Abs(m1); i++) { DauSys.AddMood(m1 > 0 ? 1 : -1); } return true;
                case "dmood_set500": int m5 = 500 - DauSys.Mood(); for (int i = 0; i < UnityEngine.Mathf.Abs(m5); i++) { DauSys.AddMood(m5 > 0 ? 1 : -1); } return true;

                // 黑暗值
                case "ddark_s0": CheatFunctions.SetDarkness(0); return true;
                case "ddark_s500": CheatFunctions.SetDarkness(500); return true;
                case "ddark_s999": CheatFunctions.SetDarkness(999); return true;

                // 专业分
                case "mj_all": CheatFunctions.MaxAllMajorScore(); return true;
                case "mj_acting": GameReflect.Sf(ds, "actingLevel", 5); GameReflect.Sf(ds, "actingExp", 9999); return true;
            }
            if (a.StartsWith("mj_")) {
                int idx = int.Parse(a.Substring(3));
                var p = NpcSys.GetNpc("dau");
                if (p != null) { var ms = GameReflect.Gf<int[]>(p, "majorScore"); if (ms != null && idx < ms.Length) ms[idx] = 999; }
                GameReflect.Tip($"{GameReflect.MajorCN[idx]}→999"); return true;
            }
            return false;
        }

        // ===================== UI 展示 =====================
        public static void Show()
        {
            ModMenu.OpenSub("attr", new[]{
                "an_0","an_1","an_2","an_3",
                "ai_0","ai_1","ai_2",
                "a_other","a_major","a_onemax","a_panel"
            }, new[]{
                "体质→详细","智力→详细","情感→详细","想象→详细",
                "武力→详细","头脑→详细","魅力→详细",
                "其它属性(名望/心情/干劲)","专业分/演艺","★一键MAX","查看隐藏数值"
            });
        }

        // 基础属性详细
        static void ShowNatureDetail(int idx)
        {
            string p = "dn" + idx;
            ModMenu.OpenSub("detail", new[]{
                p+"_p1",p+"_p2",p+"_p3",p+"_p4",
                p+"_s0",p+"_s500",p+"_s9999",
                p+"_cust"
            }, new[]{ "+100","+1000","-100","-1000","→0","→500","→9999","→自定义值" });
        }

        // 三维属性详细
        static void ShowInAttriDetail(int idx)
        {
            string p = "di" + idx;
            ModMenu.OpenSub("detail", new[]{
                p+"_p1",p+"_p2",p+"_p3",p+"_p4",
                p+"_s0",p+"_s100",p+"_s999",
                p+"_cust"
            }, new[]{ "+100","+500","-100","-500","→0","→100","→999","→自定义值" });
        }

        // 子子子面板
        public static void ShowNatureCust(int idx)
        {
            string p = "n" + idx;
            ModMenu.OpenSub("detail", new[]{
                $"cv_{p}_10","cv_{p}_50","cv_{p}_100","cv_{p}_200","cv_{p}_300",
                $"cv_{p}_400","cv_{p}_500","cv_{p}_600","cv_{p}_700","cv_{p}_800",
                $"cv_{p}_900","cv_{p}_1000","cv_{p}_2000","cv_{p}_5000","cv_{p}_more"
            }, new[]{ "→10","→50","→100","→200","→300","→400","→500","→600","→700","→800","→900","→1000","→2000","→5000","→更多..." });
        }
        public static void ShowNatureCustMore(int idx)
        {
            string p = "n" + idx;
            ModMenu.OpenSub("detail", new[]{
                $"cv_{p}m_8000","cv_{p}m_9999","cv_{p}m_15000","cv_{p}m_30000","cv_{p}m_50000","cv_{p}m_99999"
            }, new[]{ "→8000","→9999","→15000","→30000","→50000","→99999" });
        }

        static void ShowInAttriCust(int idx)
        {
            string p = "i" + idx;
            ModMenu.OpenSub("detail", new[]{
                $"cv_{p}_10","cv_{p}_30","cv_{p}_50","cv_{p}_80","cv_{p}_100","cv_{p}_150","cv_{p}_200",
                $"cv_{p}_250","cv_{p}_300","cv_{p}_350","cv_{p}_400","cv_{p}_500","cv_{p}_600","cv_{p}_more"
            }, new[]{ "→10","→30","→50","→80","→100","→150","→200","→250","→300","→350","→400","→500","→600","→更多..." });
        }
        static void ShowInAttriCustMore(int idx)
        {
            string p = "i" + idx;
            ModMenu.OpenSub("detail", new[]{
                $"cv_{p}m_700","cv_{p}m_800","cv_{p}m_900","cv_{p}m_999","cv_{p}m_1500","cv_{p}m_3000","cv_{p}m_5000","cv_{p}m_9999"
            }, new[]{ "→700","→800","→900","→999","→1500","→3000","→5000","→9999" });
        }

        static void ShowOther()
        {
            ModMenu.OpenSub("attr", new[]{
                "d_fame","d_fame_detail","d_mood","d_mood_detail","d_moti",
                "d_insp","d_dark","d_dark_detail","d_talent","d_favor",
                "d_lover","d_loverlevel","d_height"
            }, new[]{ "名望+1000","名望→详细","心情+200","心情→详细","干劲+200",
                "灵感+200","黑暗值→0","黑暗值→详细","天赋点+100","父亲好感→100",
                "恋爱值→9999","恋爱等级→最大","身高→170" });
        }

        static void ShowFameDetail() => ModMenu.OpenSub("detail", new[]{
            "dfame_a1000","dfame_s1000","dfame_s0","dfame_s500","dfame_s9999","dfame_s99999",
            "cv_fame_100","cv_fame_300","cv_fame_500","cv_fame_800","cv_fame_1000","cv_fame_2000","cv_fame_3000","cv_fame_5000","cv_fame_more"
        }, new[]{ "+1000","-1000","→0","→500","→9999","→99999",
            "→100","→300","→500","→800","→1000","→2000","→3000","→5000","→更多..." });
        static void ShowMoodDetail() => ModMenu.OpenSub("detail", new[]{
            "dmood_a100","dmood_s100","dmood_a500","dmood_s500","dmood_s0","dmood_set100","dmood_set500",
            "cv_mood_10","cv_mood_30","cv_mood_50","cv_mood_80","cv_mood_100","cv_mood_150","cv_mood_200","cv_mood_300","cv_mood_more"
        }, new[]{ "+100","-100","+500","-500","→0","→100","→500",
            "→10","→30","→50","→80","→100","→150","→200","→300","→更多..." });
        static void ShowDarkDetail() => ModMenu.OpenSub("detail", new[]{
            "ddark_s0","ddark_s500","ddark_s999",
            "cv_dark_0","cv_dark_100","cv_dark_200","cv_dark_300","cv_dark_400","cv_dark_500","cv_dark_600","cv_dark_700","cv_dark_800","cv_dark_900","cv_dark_999"
        }, new[]{ "→0光明","→500","→999黑暗",
            "设0","100","200","300","400","500","600","700","800","900","999" });
    }
}
