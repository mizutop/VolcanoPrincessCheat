using System.Collections.Generic;

namespace MizuofCheatMod
{
    /// <summary>
    /// 技能管理器 — 注册/路由分发
    /// </summary>
    public static class SkillManager
    {
        private static List<ICheatSkill> _skills = new List<ICheatSkill>();
        private static bool _initialized = false;

        /// <summary>注册一个技能</summary>
        public static void Register(ICheatSkill skill)
        {
            _skills.Add(skill);
        }

        /// <summary>初始化所有技能</summary>
        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // 按主菜单顺序注册
            Register(new OneClickSkill());
            Register(new AttrSkill());
            Register(new TimeSkill());
            Register(new ItemSkill());
            Register(new NpcSkill());
            Register(new BattleSkill());
            Register(new HorseSkill());
            Register(new MapSkill());
            Register(new EndingSkill());
            Register(new AchieveSkill());
            Register(new LordSkill());
            Register(new ActivitySkill());
            Register(new OtherSkill());
            Register(new GameConfigSkill());
        }

        /// <summary>路由分发 — 找到匹配 Prefix 的技能并交给它处理</summary>
        public static bool Handle(string action)
        {
            foreach (var skill in _skills)
            {
                if (action.StartsWith(skill.Prefix))
                {
                    string subAction = action.Substring(skill.Prefix.Length);
                    return skill.Handle(subAction);
                }
            }
            return false;
        }

        /// <summary>获取主菜单项列表</summary>
        public static (string[] ids, string[] names) GetMainItems()
        {
            var ids = new List<string>();
            var names = new List<string>();
            foreach (var skill in _skills)
            {
                if (skill.IsMainMenuSkill)
                {
                    var item = skill.GetMainMenuItem();
                    ids.Add(item.id);
                    names.Add(item.name);
                }
            }
            return (ids.ToArray(), names.ToArray());
        }
    }
}
