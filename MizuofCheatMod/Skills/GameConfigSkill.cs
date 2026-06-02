using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MizuofCheatMod
{
    /// <summary>
    /// 游戏规则修改 — 在线修改 Constant 全部游戏常量
    /// 炫技点: 动态改游戏规则（行动次数/战斗等级上限/经济/课程等）
    /// </summary>
    public class GameConfigSkill : ICheatSkill
    {
        public string Name => "游戏规则";
        public string Prefix => "cfg_";
        public bool IsMainMenuSkill => true;
        public (string id, string name) GetMainMenuItem() => ("m_gamecfg", "游戏规则");

        public bool Handle(string action)
        {
            // cfg_time_maxchat_5 → Constant.maxChatNum = 5
            string[] parts = action.Split('_');
            if (parts.Length < 3) return false;
            string category = parts[0]; // "time", "action", "economy", "battle", "favor", "explo"
            string field = parts[1];    // e.g. "maxchat"
            int val;
            if (parts.Length >= 3 && int.TryParse(parts[2], out val))
            {
                return SetConstant(category, field, val);
            }
            return false;
        }

        static bool SetConstant(string cat, string field, int val)
        {
            var t = typeof(Constant);
            string fname = "";
            // Map shorthand to field name
            var map = GetFieldMap();
            foreach (var kv in map)
            {
                if (kv.Key == cat + "_" + field || kv.Key.EndsWith("_" + field))
                {
                    fname = kv.Value;
                    break;
                }
            }
            if (fname == "") return false;
            var fi = t.GetField(fname, BindingFlags.Public | BindingFlags.Static);
            if (fi == null) return false;
            fi.SetValue(null, val);
            GameReflect.Tip("规则已修改: " + fname + " = " + val);
            return true;
        }

        static Dictionary<string, string> GetFieldMap()
        {
            return new Dictionary<string, string>
            {
                // 时间系统
                {"time_totalturn", "totalTurn"}, {"time_stage1turn", "stage1Turn"},
                {"time_stageturn", "stageTurn"}, {"time_halfturn", "halfTurn"},
                {"time_newsinside", "newsInsideTurn"},
                // 行动系统
                {"action_maxchat", "maxChatNum"}, {"action_maxwalk", "maxWalkNum"},
                {"action_explocost", "exploCost"}, {"action_dancingcost", "dancingCost"},
                {"action_dramacost", "dramaCost"}, {"action_watercost", "waterCost"},
                {"action_catchhorse", "catchHorseCost"},
                // 经济系统
                {"eco_defaultcoin", "defaultCoin"}, {"eco_defaultenergy", "defaultEnergy"},
                {"eco_allowance", "allowance"}, {"eco_foodcost", "foodCost"},
                {"eco_fathersalary", "fatherRetiredSalary"},
                // 战斗系统
                {"battle_maxlevel", "maxFightLevel"}, {"battle_maxauto", "maxAutoLevel"},
                {"battle_maxskill", "maxSkillNum"}, {"battle_teammate", "teammateNum"},
                {"battle_defenergy", "defaultFightEnergy"},
                {"battle_weakratio", "weakRatio"}, {"battle_hurtratio", "hurtRatio"},
                {"battle_criratio", "criRatio"},
                // 好感系统
                {"favor_chatprob", "chatFavorProb"}, {"favor_giftprob", "giftFavorProb"},
                {"favor_battleprob", "battleFavorProb"},
                // 课程系统
                {"course_minprg", "minCoursePrg"}, {"course_maxprg", "maxCoursePrg"},
                {"course_break", "breakNum"}, {"course_choose", "chooseMasterCourseNum"},
                // 天赋系统
                {"talent_cost", "talentCost"}, {"talent_need", "needTalent"},
                {"talent_levelnum", "talentLevelNum"},
                // 马系统
                {"horse_openturn", "horseOpenTurn"}, {"horse_maxnum", "maxHorseNum"},
                // 骰子系统
                {"dice_cost", "diceCost"}, {"dice_hp", "diceHp"},
                // 心情系统
                {"mood_upsetline", "upsetLine"}, {"mood_default", "defaultMood"},
                // 探索系统
                {"explo_maxhole", "maxHoleNum"}, {"explo_maxlevel", "maxLevelNum"},
                {"explo_fogalley", "maxFogAlleyEnter"},
            };
        }

        public static void Show()
        {
            ModMenu.OpenSub("gamecfg", new[]{
                "cfg_time", "cfg_action", "cfg_economy", "cfg_battle",
                "cfg_favor", "cfg_course", "cfg_talent", "cfg_horse",
                "cfg_dice", "cfg_mood", "cfg_explo"
            }, new[]{
                "时间系统(回合/阶段)","行动系统(次数/花费)","经济系统(金币/能量)","战斗系统(等级/倍率)",
                "好感系统(概率)", "课程系统", "天赋系统", "马系统",
                "骰子系统", "心情系统", "探索系统"
            });
        }

        // ---- 子面板: 各分类具体值 ----
        public static Dictionary<string, int[]> GetCategoryVals(string cat)
        {
            var map = new Dictionary<string, int[]>();
            switch (cat)
            {
                case "time":
                    map["totalTurn"] = new[]{30,42,60,99,200,999};
                    map["stage1Turn"] = new[]{3,6,12,20,30};
                    map["stageTurn"] = new[]{6,12,24,36,50};
                    map["halfTurn"] = new[]{3,6,12,24};
                    break;
                case "action":
                    map["maxChatNum"] = new[]{1,3,5,10,20,99};
                    map["maxWalkNum"] = new[]{1,2,3,5,10,20};
                    map["exploCost"] = new[]{0,2,4,8,12};
                    break;
                case "battle":
                    map["maxFightLevel"] = new[]{10,20,29,50,99};
                    map["maxAutoLevel"] = new[]{10,20,50,99};
                    map["teammateNum"] = new[]{1,2,3,5,10};
                    map["maxSkillNum"] = new[]{1,3,5,10,20};
                    break;
                case "economy":
                    map["defaultCoin"] = new[]{100,1000,9999,99999};
                    map["defaultEnergy"] = new[]{5,10,20,50,99};
                    map["allowance"] = new[]{0,20,50,100,500,999};
                    break;
                default:
                    map["value"] = new[]{0,1,5,10,50,100};
                    break;
            }
            return map;
        }

        public static void ShowCategory(string cat)
        {
            var vals = GetCategoryVals(cat);
            var ids = new List<string>();
            var names = new List<string>();
            foreach (var kv in vals)
            {
                foreach (int v in kv.Value)
                {
                    ids.Add("cfg_"+cat+"_"+kv.Key.ToLower()+"_"+v);
                    names.Add(kv.Key+" = "+v);
                }
            }
            ModMenu.OpenSub("detail", ids.ToArray(), names.ToArray());
        }
    }
}
