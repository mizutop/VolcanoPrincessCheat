namespace MizuofCheatMod
{
    /// <summary>
    /// 技能接口 — 每个子系统实现此接口，自动注册到菜单系统
    /// </summary>
    public interface ICheatSkill
    {
        /// <summary>技能显示名称（如"女儿属性"）</summary>
        string Name { get; }
        /// <summary>路由前缀（如"oc_"）— 所有以此开头的 action 由本技能处理</summary>
        string Prefix { get; }
        /// <summary>是否在主菜单显示入口</summary>
        bool IsMainMenuSkill { get; }

        /// <summary>主菜单项 (id, name) — 仅 IsMainMenuSkill=true 时调用</summary>
        (string id, string name) GetMainMenuItem();

        /// <summary>处理路由 — action 已去除 Prefix</summary>
        /// <returns>true=已处理, false=不属于本技能</returns>
        bool Handle(string action);
    }
}
