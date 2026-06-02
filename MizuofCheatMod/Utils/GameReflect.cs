using System;
using System.Collections.Generic;
using System.Reflection;

namespace MizuofCheatMod
{
    /// <summary>
    /// 反射辅助 — 安全读写游戏私有字段、获取单例
    /// </summary>
    public static class GameReflect
    {
        // 泛型读字段
        public static T Gf<T>(object o, string n)
        {
            var f = o?.GetType().GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return f != null ? (T)f.GetValue(o) : default;
        }

        // 泛型写字段
        public static void Sf(object o, string n, object v)
        {
            var f = o?.GetType().GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null) f.SetValue(o, v);
        }

        // 获取单例（通过静态字段 Instan）
        public static object Inst(Type t) =>
            t.GetField("Instan", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

        // 环境检查
        public static bool Ok() => UiSys.Instan != null && DataSys.Instan != null;

        // 数组安全取值
        public static int Gia(int[] a, int i) => (a != null && i < a.Length) ? a[i] : 0;

        // 快捷获取 DauSys
        public static object GetDauSys() => Inst(typeof(DauSys));
        public static object GetDataSys() => Inst(typeof(DataSys));
        public static object GetItemSys() => Inst(typeof(ItemSys));
        public static object GetHorseSys() => Inst(typeof(HorseSys));
        public static object GetDiceSys() => Inst(typeof(DiceSys));

        // 常量名称映射
        public static readonly string[] NatureCN = { "体质", "智力", "情感", "想象" };
        public static readonly string[] InAttriCN = { "武力", "头脑", "魅力" };
        public static readonly string[] MajorCN = { "剑术", "狩猎", "科学", "神学", "礼仪", "文学", "绘画", "音乐" };
        public static readonly string[] HorseStatCN = { "速度", "外貌", "加速", "加速次数" };
        public static readonly string[] FightStatCN = { "HP", "攻击", "防御", "闪避", "暴击" };

        // 显示提示
        public static void Tip(string msg) { UiSys.ShowTip(NatureTip.normal, 0, msg); }

        // 显示确认框
        public static void Alert(string msg) { UiSys.SureUI(SureUi.none, true, msg); }

        // 战斗 NPC
        public static FightNpc GetFightNpc() => FightSys.FightNpc("dau");

        // 读取读属性值
        public static int GetNature(int idx)
        {
            var n = Gf<int[]>(GetDauSys(), "nature");
            return (n != null && idx < n.Length) ? n[idx] : 0;
        }
        public static int GetInAttri(int idx)
        {
            var ia = Gf<int[]>(GetDauSys(), "inAttri");
            return (ia != null && idx < ia.Length) ? ia[idx] : 0;
        }
        public static int GetFightAttri(int idx)
        {
            var fn = GetFightNpc();
            return (fn != null && idx >= 0 && idx < fn.fightAttri.Length) ? fn.fightAttri[idx] : 0;
        }
        public static int GetHorseNature(int statIdx)
        {
            var hh = Gf<List<Horse>>(GetHorseSys(), "ownHorses");
            if (hh != null && hh.Count > 0)
            {
                var h = hh[0];
                if (h != null && h.natures != null && statIdx < h.natures.Length) return h.natures[statIdx];
            }
            return 0;
        }
    }
}
