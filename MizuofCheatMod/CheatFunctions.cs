using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MizuofCheatMod
{
    public static class CheatFunctions
    {
        // 反射辅助（公开供 MizuofCheatMod.cs 使用）
        public static T Gf<T>(object o, string n) {
            var f = o?.GetType().GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            return f != null ? (T)f.GetValue(o) : default;
        }
        public static void Sf(object o, string n, object v) {
            var f = o?.GetType().GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if (f != null) f.SetValue(o, v);
        }
        public static object Inst(Type t) => t.GetField("Instan", BindingFlags.Public|BindingFlags.Static)?.GetValue(null);
        public static bool Ok() => UiSys.Instan != null && DataSys.Instan != null;
        public static int Gia(int[] a, int i) => a != null && i < a.Length ? a[i] : 0;

        // ============== DauSys 快捷获取 ==============
        public static object GetDauSys() => Inst(typeof(DauSys));

        // ============== NPC辅助 ==============
        public static int GetFav(Person p) => Gf<int>(p, "favor");

        // ============== 全物品商店(免费) ==============
        static Dictionary<int, int> _savedWS = null;
        static Dictionary<int, int> _savedCS = null;
        static int[] _savedPrices = null;

        public static void OpenCheatShop()
        {
            if (!Ok()) return;
            var isys = Inst(typeof(ItemSys));
            var shop = Gf<Dictionary<string, Dictionary<int, int>>>(isys, "shopItems");
            if (shop == null) return;

            // 备份两个商店
            string[] keys = { "weaponShop", "cosShop" };
            _savedWS = shop.ContainsKey(keys[0]) ? shop[keys[0]] : new Dictionary<int, int>();
            _savedCS = shop.ContainsKey(keys[1]) ? shop[keys[1]] : new Dictionary<int, int>();

            // 武器店：放武器/装备/道具类 (type!=38家具, uiType!=6服装)
            var wItems = new Dictionary<int, int>();
            // 服装店：放服装类 (uiType==6)
            var cItems = new Dictionary<int, int>();

            foreach (Item it in DataSys.Instan.dataCfg.items)
            {
                if (it == null) continue;
                if (it.uiType == 6) cItems[it.index] = 999; // 服装
                else if (it.type != 38) wItems[it.index] = 999; // 非家具
                // 家具(type=38)也加入武器店方便获取
                if (it.type == 38) wItems[it.index] = 1;
            }

            shop[keys[0]] = wItems;
            shop[keys[1]] = cItems;

            // 备份价格并设0
            _savedPrices = new int[DataSys.Instan.dataCfg.items.Length];
            for (int i = 0; i < DataSys.Instan.dataCfg.items.Length; i++)
            {
                var it = DataSys.Instan.dataCfg.items[i];
                if (it != null) { _savedPrices[i] = it.buyPrice; it.buyPrice = 0; }
            }

            // 先开武器店
            isys.GetType().GetMethod("OpenShop", new Type[] { typeof(bool), typeof(string) })
                ?.Invoke(isys, new object[] { true, "weaponShop" });
        }

        public static void RestoreShop()
        {
            if (_savedWS == null) return;
            var isys = Inst(typeof(ItemSys));
            var shop = Gf<Dictionary<string, Dictionary<int, int>>>(isys, "shopItems");
            if (shop != null) { shop["weaponShop"] = _savedWS; shop["cosShop"] = _savedCS; }
            for (int i = 0; i < DataSys.Instan.dataCfg.items.Length; i++)
                if (_savedPrices != null && i < _savedPrices.Length) {
                    var it = DataSys.Instan.dataCfg.items[i];
                    if (it != null) it.buyPrice = _savedPrices[i];
                }
            _savedWS = null; _savedCS = null; _savedPrices = null;
        }

        // ============== 女儿属性 — 详细精确设置 ==============
        public static void SetNatureExact(int idx, int val) {
            if(!Ok() || idx < 0 || idx >= 4) return;
            var ds = Inst(typeof(DauSys));
            var n = Gf<int[]>(ds, "nature");
            if(n != null) { n[idx] = val; }
            UiSys.ShowTip(NatureTip.normal, 0, $"{new[]{"体质","智力","情感","想象"}[idx]} → {val}");
        }
        public static void SetInAttriExact(int idx, int val) {
            if(!Ok() || idx < 0 || idx >= 3) return;
            var ds = Inst(typeof(DauSys));
            var ia = Gf<int[]>(ds, "inAttri");
            if(ia != null) { ia[idx] = val; }
            UiSys.ShowTip(NatureTip.normal, 0, $"{new[]{"武力","头脑","魅力"}[idx]} → {val}");
        }
        public static void SetFameValue(int val) {
            if(!Ok()) return;
            Sf(Inst(typeof(DauSys)), "fame", val);
            UiSys.ShowTip(NatureTip.normal, 0, $"名望 → {val}");
        }
        public static void SetMoodValue(int val) {
            if(!Ok()) return;
            DauSys.AddMood(val - DauSys.Mood());
            UiSys.ShowTip(NatureTip.normal, 0, $"心情 → {val}");
        }

        // ============== 战斗完全恢复 ==============
        public static void HealBattleFull() {
            if(!Ok()) return;
            var fn = FightSys.FightNpc("dau");
            if(fn != null) {
                fn.AddFightAttri(FightAttri.hp, 99999, true);
                Sf(fn, "shield", 9999);
            }
            UiSys.ShowTip(NatureTip.normal, 0, "战斗已完全恢复");
        }

        // ============== 状态列表查看 ==============
        public static void ShowStateList() {
            if(!Ok()) return;
            var states = Gf<List<int>>(Inst(typeof(DauSys)), "ownState");
            if(states == null || states.Count == 0) {
                UiSys.SureUI(SureUi.none, true, "当前没有负面状态。");
                return;
            }
            string txt = $"<b>【当前状态列表 ({states.Count}个)】</b>\n";
            for(int i = 0; i < states.Count; i++) {
                txt += $"\n{i+1}. 状态索引 #{states[i]}";
            }
            txt += "\n\n可在「消除单个状态」中逐一移除。";
            UiSys.SureUI(SureUi.none, true, txt);
        }

        // ============== 女儿属性 ==============
        public static void MaxAllNature() { if(!Ok())return; for(int i=0;i<4;i++) DauSys.AddNature(i,9999,true); UiSys.ShowTip(NatureTip.normal,0,"基础属性全满→9999"); }
        public static void AddAllNature(int val) { if(!Ok())return; for(int i=0;i<4;i++) DauSys.AddNature(i,val,true); UiSys.ShowTip(NatureTip.normal,0,$"基础属性+{val}"); }
        public static void MaxAllInAttri() { if(!Ok())return; for(int i=0;i<3;i++) DauSys.AddInAttri(i,999,true); UiSys.ShowTip(NatureTip.normal,0,"三维全满→999"); }
        public static void AddAllInAttri(int val) { if(!Ok())return; for(int i=0;i<3;i++) DauSys.AddInAttri(i,val,true); UiSys.ShowTip(NatureTip.normal,0,$"三维属性+{val}"); }
        public static void MaxResources() { if(!Ok())return; DataSys.AddCoin(BillType.otherIncome,999999-DataSys.Coin(),true,false); DataSys.AddMaxEnergy(999); DauSys.AddMood(999); DauSys.AddMotivation(999); DauSys.AddInspiration(999); UiSys.ShowTip(NatureTip.normal,0,"金钱/能量/心情最大"); }
        public static void SetDarkness(int v) { if(!Ok())return; DauSys.AddDarkness(v-DauSys.Darkness()); UiSys.ShowTip(NatureTip.normal,0,$"黑暗→{(v==0?"光明":"黑暗")}"); }
        public static void AddTalentPoints(int n) { if(!Ok())return; DauSys.AddTalentPoint(n); UiSys.ShowTip(NatureTip.normal,0,$"天赋点+{n}"); }
        public static void AddNature(int i, int v) { if(!Ok())return; DauSys.AddNature(i,v,true); UiSys.ShowTip(NatureTip.normal,0,$"{new[]{"体质","智力","情感","想象"}[i]}+{v}"); }
        public static void AddInAttri(int i, int v) { if(!Ok())return; DauSys.AddInAttri(i,v,true); UiSys.ShowTip(NatureTip.normal,0,$"{new[]{"武力","头脑","魅力"}[i]}+{v}"); }
        public static void AddFameCustom(int v) { if(!Ok())return; DauSys.AddFame(v,true); }
        public static void AddMoodCustom(int v) { if(!Ok())return; DauSys.AddMood(v); }
        public static void AddMotivationCustom(int v) { if(!Ok())return; DauSys.AddMotivation(v); }
        public static void AddInspirationCustom(int v) { if(!Ok())return; DauSys.AddInspiration(v); }
        public static void MaxFarm() { if(!Ok())return; var isys=Inst(typeof(ItemSys)); Sf(isys,"farmLevel",5); Sf(isys,"farmExp",9999); Sf(isys,"maxSeed",9); Sf(isys,"farmPrg",100); UiSys.ShowTip(NatureTip.normal,0,"农场已满级"); }
        public static void CureAllPatient() { if(!Ok())return; Gf<List<int>>(Inst(typeof(DauSys)),"ownState")?.Clear(); DauSys.AddPatient(999); UiSys.ShowTip(NatureTip.normal,0,"已治疗所有伤病"); }
        public static void MaxFatherFavor() { if(!Ok())return; Sf(Inst(typeof(DauSys)),"favor",100); UiSys.ShowTip(NatureTip.favor,100); }
        public static void MaxAllMajorScore() { if(!Ok())return; var p=NpcSys.GetNpc("dau"); if(p!=null){var m=Gf<int[]>(p,"majorScore");if(m!=null)for(int i=0;i<m.Length;i++)m[i]=999;} UiSys.ShowTip(NatureTip.normal,0,"专业分全满"); }
        public static void ShowHiddenStats() {
            if(!Ok())return; var ds=Inst(typeof(DauSys)); var n=Gf<int[]>(ds,"nature")??new int[0]; var ia=Gf<int[]>(ds,"inAttri")??new int[0];
            var nl=Gf<int[]>(ds,"natureLevel")??new int[0]; var ai=Gf<int[]>(ds,"addInAttri")??new int[0];
            string t=$"<b>【隐藏数值】</b>\n体质:{Gia(n,0)} 智力:{Gia(n,1)} 情感:{Gia(n,2)} 想象:{Gia(n,3)}\n等级:{string.Join("/",nl)}\n武力:{Gia(ia,0)} 头脑:{Gia(ia,1)} 魅力:{Gia(ia,2)}\n加成:{string.Join("/",ai)}\n\n金币:{DataSys.Coin()}G 能量:{DataSys.MaxEnergy()} 心情:{DauSys.Mood()}\n名望:{DauSys.Fame()} 恋爱:{DauSys.Lover()} 黑暗:{DauSys.Darkness()}\n好感:{Gf<int>(ds,"favor")} 天赋:{Gf<int>(ds,"talentPoint")} 演技Lv:{Gf<int>(ds,"actingLevel")} 回合:{DataSys.Turn()}/{Constant.totalTurn}";
            UiSys.SureUI(SureUi.none,true,t);
        }
        public static void UnlockAllTalents() { if(!Ok())return; var ds=Inst(typeof(DauSys)); var o=Gf<bool[]>(ds,"talentLevelOpen"); if(o!=null)for(int i=0;i<o.Length;i++)o[i]=true; foreach(var t in DataSys.Instan.dataCfg.talents)if(t!=null)t.level=t.maxLevel; DauSys.AddTalentPoint(999); UiSys.ShowTip(NatureTip.normal,0,"天赋已全点亮"); }

        // ============== 时间 ==============
        public static void ResetTurn() { if(!Ok())return; Sf(Inst(typeof(DataSys)),"turn",0); UiSys.UpdateTimeUI(); UiSys.ShowTip(NatureTip.normal,0,"已回到第1回合"); }
        public static void JumpToStage(int s) { if(!Ok())return; int t=s==1?Constant.stage1Turn:Constant.stage1Turn+Constant.stageTurn; Sf(Inst(typeof(DataSys)),"turn",t); UiSys.UpdateTimeUI(); UiSys.ShowTip(NatureTip.normal,0,$"跳到{(s==1?"少年期":"青年期")}"); }
        public static void SetMaxActions(int n) { Constant.maxChatNum=n; Constant.maxWalkNum=Mathf.Max(1,n/3); UiSys.ShowTip(NatureTip.normal,0,$"每日行动→{n}"); }

        // ============== 课程 ==============
        public static void LearnAllCourses() { if(!Ok())return; var cs=Inst(typeof(CourseSys)); foreach(var kv in DataSys.Instan.dataCfg.courseDic){var c=kv.Value;if(c==null)continue;var own=Gf<List<string>>(cs,"ownCourses");if(own!=null&&!own.Contains(c.enName))own.Add(c.enName);c.prg=100;c.owned=true;var fin=Gf<List<string>>(cs,"finishedCourses");if(fin!=null&&!fin.Contains(c.enName))fin.Add(c.enName);} cs.GetType().GetMethod("UpdatePlanUi",BindingFlags.NonPublic|BindingFlags.Instance)?.Invoke(cs,null); UiSys.ShowTip(NatureTip.normal,0,"全部课程已修完"); }
        public static void MaxAllBooks() { if(!Ok())return; foreach(var b in DataSys.Instan.dataCfg.books)if(b!=null)b.prg=100; UiSys.ShowTip(NatureTip.normal,0,"读书全满"); }
        public static void MaxAlchemy() { if(!Ok())return; var a=Gf<int[]>(Inst(typeof(ItemSys)),"alchemyNumByLevels"); if(a!=null)for(int i=0;i<a.Length;i++)a[i]=999; UiSys.ShowTip(NatureTip.normal,0,"炼金全满"); }
        public static void MaxCooking() { if(!Ok())return; var isys=Inst(typeof(ItemSys)); Sf(isys,"cookingLevel",5); Sf(isys,"cookingExp",9999); var r=Gf<List<int>>(isys,"ownRecipes"); if(r!=null)foreach(var it in DataSys.Instan.dataCfg.items)if(it!=null&&it.type==39&&!r.Contains(it.index))r.Add(it.index); UiSys.ShowTip(NatureTip.normal,0,"烹饪/食谱全满"); }
        public static void UnlockAllSkills() {
            if(!Ok())return;
            var fn = FightSys.FightNpc("dau");
            if (fn == null) { UiSys.ShowTip(NatureTip.normal,0,"战斗系统未初始化"); return; }
            foreach (Skill sk in DataSys.Instan.dataCfg.skills)
                if (sk != null) fn.AddSkill(sk.index);
            fn.level = Constant.maxFightLevel;
            UiSys.ShowTip(NatureTip.normal,0,"全部战斗技能已解锁");
        }

        // ============== 战斗 ==============
        public static void MaxBattleStats() { if(!Ok())return; var fn=FightSys.FightNpc("dau"); if(fn!=null){fn.AddFightAttri(FightAttri.hp,99999,true);fn.AddFightAttri(FightAttri.atk,9999,true);fn.AddFightAttri(FightAttri.def,9999,true);fn.AddFightAttri(FightAttri.avoid,80,true);fn.AddFightAttri(FightAttri.cri,80,true);} UiSys.ShowTip(NatureTip.normal,0,"战斗属性全满"); }

        // ============== NPC群操作 ==============
        public static void MaxAllNPCFavor() { if(!Ok())return; foreach(var p in DataSys.Instan.dataCfg.people){if(p!=null&&p.isNpc){Sf(p,"favor",100);Sf(p,"relationLevel",5);}} UiSys.ShowTip(NatureTip.favor,100); }
        public static void MaxAllNPCLover() { if(!Ok())return; foreach(var p in DataSys.Instan.dataCfg.people)if(p!=null&&p.isNpc)Sf(p,"lover",9999); DauSys.AddLover(9999); UiSys.ShowTip(NatureTip.normal,0,"恋爱值全最大"); }
        public static void UnlockAllNPCStories() { if(!Ok())return; foreach(var p in DataSys.Instan.dataCfg.people){if(p!=null&&p.isNpc){Sf(p,"eveStage",999);var rb=Gf<bool[]>(p,"relationBools");if(rb!=null)for(int i=0;i<rb.Length;i++)rb[i]=true;}} UiSys.ShowTip(NatureTip.normal,0,"NPC剧情全开"); }
        public static void ResetGiftRecords() { if(!Ok())return; foreach(var p in DataSys.Instan.dataCfg.people){if(p!=null&&p.isNpc){Sf(p,"sendGiftBools",new bool[10]);Sf(p,"haveSentFavItems",new bool[20]);}} UiSys.ShowTip(NatureTip.normal,0,"送礼记录已重置"); }
        public static void MaxAllNPCEquip() { if(!Ok())return; foreach(var p in DataSys.Instan.dataCfg.people){if(p!=null&&p.isNpc){var eq=Gf<int[]>(p,"equips");if(eq!=null)for(int i=0;i<eq.Length;i++)eq[i]=-1;}} UiSys.ShowTip(NatureTip.normal,0,"NPC装备已重置"); }

        // ============== NPC单操作 ==============
        public static void SetNpcFavor(Person p, int v) { if(!Ok()||p==null)return; Sf(p,"favor",v); Sf(p,"relationLevel",5); UiSys.ShowTip(NatureTip.favor,100); }
        public static void SetNpcLover(Person p, int v) { if(!Ok()||p==null)return; Sf(p,"lover",v); UiSys.ShowTip(NatureTip.normal,0,$"{p.chName}恋爱值→{v}"); }
        public static void UnlockNpcStory(Person p) { if(!Ok()||p==null)return; Sf(p,"eveStage",999); var rb=Gf<bool[]>(p,"relationBools");if(rb!=null)for(int i=0;i<rb.Length;i++)rb[i]=true; UiSys.ShowTip(NatureTip.normal,0,$"{p.chName}剧情已全开"); }
        public static void ResetNpcGift(Person p) { if(!Ok()||p==null)return; Sf(p,"sendGiftBools",new bool[10]); Sf(p,"haveSentFavItems",new bool[20]); UiSys.ShowTip(NatureTip.normal,0,$"{p.chName}送礼已重置"); }
        public static void ShowNpcDetail(Person p) {
            if(p==null)return;
            var eq=Gf<int[]>(p,"equips"); var ms=Gf<int[]>(p,"majorScore");
            string t = $"<b>【{p.chName}】</b>\n好感:{Gf<int>(p,"favor")} 恋爱:{Gf<int>(p,"lover")}\n名望:{Gf<int>(p,"fame")} 关系Lv:{Gf<int>(p,"relationLevel")}\n剧情阶段:{Gf<int>(p,"eveStage")} 口才:{Gf<int>(p,"eloquence")}\n";
            if(eq!=null) t+= $"装备: {string.Join("/",eq)}\n";
            t+= "\n属性:\n"+string.Join(" ",Gf<int[]>(p,"natures")??new int[0])+"\n";
            t+= "三维:\n"+string.Join(" ",Gf<int[]>(p,"inAttri")??new int[0]);
            UiSys.SureUI(SureUi.none,true,t);
        }

        public static void ShowAllNPCData() {
            if(!Ok())return; string t="<b>【NPC列表】</b>"; int c=0;
            foreach(var p in DataSys.Instan.dataCfg.people){if(p==null||!p.isNpc||c>=10)continue; t+=$"\n{p.chName} 好感:{Gf<int>(p,"favor")} 关系Lv:{Gf<int>(p,"relationLevel")}"; c++;}
            t+="\n\n(选\"选择单个NPC编辑\"查看全部)"; UiSys.SureUI(SureUi.none,true,t);
        }

        // ============== NPC剧情事件触发 ==============
        public static void TriggerNpcEvent(Person p) {
            if(!Ok()||p==null) return;
            // 确保NPC已遇见过、好感够、剧情阶段有效
            var metList = NpcSys.Instan.MetPersons();
            if (metList != null && !metList.Contains(p.enName))
                metList.Add(p.enName);
            Sf(p, "refuseInvitation", false);

            // 获取当前阶段的事件
            int stage = Gf<int>(p, "eveStage");
            var events = Gf<List<Person.FavorEvent>>(p, "events");
            if (events == null || stage >= events.Count) {
                UiSys.ShowTip(NatureTip.normal, 0, p.chName + " 已无更多剧情可触发");
                return;
            }

            // 设置NPC事件状态
            Sf(p, "relationLevel", stage + 2);
            var fe = events[stage];
            NpcSys.Instan.favorEve = fe;
            NpcSys.tempNpc = p;
            NpcSys.Instan.SetEvePer(fe);

            // 通过游戏原生对话系统触发事件
            ChatSys.StartNpcChat(p.enName, "date" + stage, 0);
            Sf(p, "eveStage", stage + 1);
            UiSys.ShowTip(NatureTip.normal, 0, "已触发 " + p.chName + " 的第" + (stage+1) + "阶段剧情");
        }

        // ============== 服装全解锁 ==============
        public static void UnlockAllCostumes() {
            if(!Ok())return;
            var cs = Inst(typeof(CollectSys));
            var clo = Gf<bool[]>(cs, "clothesUnlocked");
            if(clo != null) for(int i=0;i<clo.Length;i++) clo[i]=true;
            foreach(var it in DataSys.Instan.dataCfg.items)
                if(it!=null && it.uiType==6)
                    typeof(ItemSys).GetMethod("AddItemNature", BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static)
                        ?.Invoke(null, new object[]{it.index, 1});
            UiSys.ShowTip(NatureTip.normal,0,"全部服装已解锁");
        }

        // ============== 一键添加全部物品到背包 ==============
        public static void AddAllItemsToBag() {
            if(!Ok())return;
            int cnt = 0;
            foreach(Item it in DataSys.Instan.dataCfg.items) {
                if(it == null) continue;
                if(it.type == 38) {
                    // 家具
                    Inst(typeof(ItemSys)).GetType().GetMethod("AddFurniture",
                        BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance)
                        ?.Invoke(Inst(typeof(ItemSys)), new object[]{it});
                } else {
                    typeof(ItemSys).GetMethod("AddItem", BindingFlags.Public|BindingFlags.Static,
                        null, new Type[]{typeof(int),typeof(int),typeof(bool),typeof(bool)}, null)
                        ?.Invoke(null, new object[]{it.index, 999, false, true});
                }
                cnt++;
            }
            // 服装解锁
            var cs = Inst(typeof(CollectSys));
            var clo = Gf<bool[]>(cs, "clothesUnlocked");
            if(clo != null) for(int i=0;i<clo.Length;i++) clo[i]=true;
            UiSys.ShowTip(NatureTip.normal,0,$"已添加 {cnt} 种物品×999 到背包");
        }

        // ============== 地图 ==============
        public static void UnlockAllMaps() { if(!Ok())return; foreach(var mp in DataSys.Instan.dataCfg.mapPoints)if(mp!=null)mp.finished=true; foreach(var md in DataSys.Instan.dataCfg.maps)if(md!=null)Sf(md,"openTurn",0); UiSys.ShowTip(NatureTip.normal,0,"地图已全解锁"); }

        // ============== 收集 ==============
        public static void UnlockAllAchievements() { if(!Ok())return; for(int i=0;i<DataSys.Instan.dataCfg.achievements.Length;i++)CollectSys.Instan.DoneAchievement(i); UiSys.ShowTip(NatureTip.normal,0,"成就全解锁"); }
        public static void UnlockAllCG() { PlayerPrefs.SetInt("openTrueEnd",1); UiSys.ShowTip(NatureTip.normal,0,"CG/图鉴全解锁"); }
        public static void MaxInheritPoints() { PlayerPrefs.SetInt("openTrueEnd",1); UiSys.ShowTip(NatureTip.normal,0,"继承点数满"); }

        // ============== 事件 ==============
        public static void ClearAllWorries() { if(!Ok())return; Gf<List<int>>(Inst(typeof(DauSys)),"ownState")?.Clear(); DauSys.AddMood(999); UiSys.ShowTip(NatureTip.normal,0,"烦恼已消除"); }
        public static void UnlockAllLetters() { if(!Ok())return; var lq=Gf<Queue<Letter>>(ChatSys.Instan,"letters"); if(lq!=null)foreach(var l in DataSys.Instan.dataCfg.npcLetters)if(l!=null)lq.Enqueue(l); ChatSys.Instan.TurnUpdateLetterBtnUi(); UiSys.ShowTip(NatureTip.normal,0,"信件全解锁"); }
        public static void TriggerMomStory() { if(!Ok())return; ShowSys.Instan.CallShow(ShowSys.ShowIndex.momAndBirdShow); }

        // ============== 结局 ==============
        public static void OpenEndingSelection() {
            if(!Ok())return;
            // 属性拉满确保满足任意结局条件
            MaxAllNature(); MaxAllInAttri(); DauSys.AddFame(9999,true);
            var dau = NpcSys.GetNpc("dau");
            if(dau!=null) { var ms=Gf<int[]>(dau,"majorScore"); if(ms!=null)for(int i=0;i<ms.Length;i++)ms[i]=999; }
            DauSys.AddLover(9999);
            // 用游戏原生接口打开结局选择
            EndingSys.Instan.ChooseEnding();
        }
        public static void UnlockTrueEnding() { PlayerPrefs.SetInt("openTrueEnd",1); UiSys.ShowTip(NatureTip.normal,0,"真结局已解锁"); }
        public static void TriggerEnding(int idx) {
            if(!Ok())return;
            OpenEndingSelection();
        }

        // ============== 马 ==============
        public static void MaxHorseNature() { if(!Ok())return; var hs=Inst(typeof(HorseSys)); var hh=Gf<List<Horse>>(hs,"ownHorses"); if(hh!=null)foreach(var h in hh){if(h!=null){h.natures=new int[]{999,999,999,999};Sf(h,"favor",100);}} UiSys.ShowTip(NatureTip.normal,0,"马属性全满"); }
        public static void MaxHorseFavor() { if(!Ok())return; var hs=Inst(typeof(HorseSys)); var hh=Gf<List<Horse>>(hs,"ownHorses"); if(hh!=null)foreach(var h in hh)if(h!=null)Sf(h,"favor",100); UiSys.ShowTip(NatureTip.normal,0,"马好感最大"); }
        public static void UnlockAllHorses() {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            if (hs == null) return;
            var horses = Gf<List<Horse>>(hs, "ownHorse");
            if (horses == null) return;

            int added = 0;
            foreach (HorseData hd in DataSys.Instan.dataCfg.horses) {
                if (hd == null) continue;
                // 检查是否已有
                bool exists = false;
                foreach (var h in horses) if (h.index == hd.index) { exists = true; break; }
                if (exists) continue;

                // 用反射调 private static NewHorse
                var newHorse = typeof(HorseSys).GetMethod("NewHorse", BindingFlags.NonPublic|BindingFlags.Static)
                    ?.Invoke(null, new object[] { hd, false }) as Horse;
                if (newHorse != null) {
                    newHorse.name = DataSys.Instan.dataCfg.horseNames[added % DataSys.Instan.dataCfg.horseNames.Length] + (added+1);
                    horses.Add(newHorse);
                    added++;
                }
            }
            // 刷新马厩UI
            hs.GetType().GetMethod("UpdateStableUi", BindingFlags.NonPublic|BindingFlags.Instance)?.Invoke(hs, null);
            UiSys.ShowTip(NatureTip.normal,0,$"已解锁 {added} 匹新马");
        }

        // ============== 开关 ==============
        public static void ToggleUnlimitedMode() { ModConfig.IsUnlimitedMode=!ModConfig.IsUnlimitedMode; UiSys.ShowTip(NatureTip.normal,0,ModConfig.IsUnlimitedMode?"✓无限制":"✗无限制"); }
        public static void ToggleTimeFreeze() { ModConfig.IsTimeFreeze=!ModConfig.IsTimeFreeze; UiSys.ShowTip(NatureTip.normal,0,ModConfig.IsTimeFreeze?"✓冻结":"✗冻结"); }
        public static void ToggleOneHitKill() { ModConfig.IsOneHitKill=!ModConfig.IsOneHitKill; UiSys.ShowTip(NatureTip.normal,0,ModConfig.IsOneHitKill?"✓必杀":"✗必杀"); }
        public static void ToggleBattleSkip() { ModConfig.IsBattleSkip=!ModConfig.IsBattleSkip; UiSys.ShowTip(NatureTip.normal,0,ModConfig.IsBattleSkip?"✓跳过":"✗跳过"); }

        // ============== 全功能MAX ==============
        public static void MaxAll() {
            MaxAllNature();MaxAllInAttri();MaxResources();SetDarkness(0);AddTalentPoints(999);
            MaxFatherFavor();MaxAllMajorScore();UnlockAllTalents();LearnAllCourses();
            MaxAllBooks();MaxAlchemy();MaxCooking();UnlockAllSkills();MaxBattleStats();
            MaxAllNPCFavor();MaxAllNPCLover();UnlockAllNPCStories();ResetGiftRecords();
            UnlockAllMaps();UnlockAllAchievements();UnlockAllCG();
            ClearAllWorries();UnlockTrueEnding();MaxHorseNature();MaxHorseFavor();
            UiSys.SureUI(SureUi.none,true,"【全功能MAX】完成!\n所有属性/课程/技能/NPC好感/成就/CG/马匹已全满");
        }

        // ============== 女儿属性扩展函数 ==============
        public static void SetLoverLevelMax() {
            if(!Ok())return;
            Sf(Inst(typeof(DauSys)), "loverLevel", 5);
            DauSys.AddLover(9999);
            UiSys.ShowTip(NatureTip.normal,0,"恋爱等级→最大");
        }
        public static void SetHeightDefault() {
            if(!Ok())return;
            Sf(Inst(typeof(DauSys)), "height", 170);
            UiSys.ShowTip(NatureTip.normal,0,"身高→170");
        }
        public static void MaxActingLevel() {
            if(!Ok())return;
            Sf(Inst(typeof(DauSys)), "actingLevel", 5);
            Sf(Inst(typeof(DauSys)), "actingExp", 9999);
            UiSys.ShowTip(NatureTip.normal,0,"演艺等级→最大");
        }
        public static void SetMajorScore(int idx, int val) {
            if(!Ok())return;
            var p = NpcSys.GetNpc("dau");
            if(p==null)return;
            var ms = Gf<int[]>(p, "majorScore");
            if(ms != null && idx < ms.Length) ms[idx] = val;
            UiSys.ShowTip(NatureTip.normal,0,$"{new[]{"剑术","狩猎","科学","神学","礼仪","文学","绘画","音乐"}[idx]}→{val}");
        }

        // ============== 读取辅助 ==============
        public static int GetFightAttri(int idx) {
            var fn = FightSys.FightNpc("dau");
            if(fn != null && idx >= 0 && idx < fn.fightAttri.Length) return fn.fightAttri[idx];
            return 0;
        }
        public static int GetHorseNature(int statIdx) {
            var hs = Inst(typeof(HorseSys));
            var hh = Gf<List<Horse>>(hs, "ownHorses");
            if(hh != null && hh.Count > 0) {
                var h = hh[0];
                if(h != null && h.natures != null && statIdx < h.natures.Length) return h.natures[statIdx];
            }
            return 0;
        }

        // ============== 时间 ==============
        public static void SetMaxEnergy999() {
            if(!Ok())return;
            DataSys.AddMaxEnergy(999 - DataSys.MaxEnergy());
            UiSys.ShowTip(NatureTip.normal,0,"最大能量→999");
        }
        public static void SetTurnExact(int t) {
            if(!Ok())return;
            Sf(Inst(typeof(DataSys)), "turn", Mathf.Min(t, Constant.totalTurn));
            UiSys.UpdateTimeUI();
            UiSys.ShowTip(NatureTip.normal,0,$"回合→{t}");
        }

        // ============== 物品 ==============
        public static void UnlockAllFurniture() {
            if(!Ok())return;
            var isys = Inst(typeof(ItemSys));
            var furn = Gf<List<int>>(isys, "ownFurnitures");
            if(furn != null) {
                foreach(var it in DataSys.Instan.dataCfg.items) {
                    if(it != null && it.type == 38 && !furn.Contains(it.index)) {
                        isys.GetType().GetMethod("AddFurniture", BindingFlags.NonPublic|BindingFlags.Instance)
                            ?.Invoke(isys, new object[]{it});
                    }
                }
            }
            UiSys.ShowTip(NatureTip.normal,0,"家具已全解锁");
        }
        public static void ResetMagicShop() {
            if(!Ok())return;
            var isys = Inst(typeof(ItemSys));
            Sf(isys, "boughtMagicItem", new List<int>());
            UiSys.ShowTip(NatureTip.normal,0,"魔法商店已重置");
        }
        public static void RecoverLostItems() {
            if(!Ok())return;
            var isys = Inst(typeof(ItemSys));
            var lost = Gf<List<int>>(isys, "remainLostItems");
            if(lost != null) {
                foreach(var idx in lost) typeof(ItemSys).GetMethod("AddItem", BindingFlags.Public|BindingFlags.Static,
                    null, new Type[]{typeof(int),typeof(int),typeof(bool),typeof(bool)}, null)
                    ?.Invoke(null, new object[]{idx, 1, false, true});
                lost.Clear();
            }
            UiSys.ShowTip(NatureTip.normal,0,"失物已找回");
        }

        // ============== 战斗 ==============
        public static void MaxBattleLevel() {
            if(!Ok())return;
            var fn = FightSys.FightNpc("dau");
            if(fn != null) { fn.level = Constant.maxFightLevel; fn.exp = 99999; }
            UiSys.ShowTip(NatureTip.normal,0,"战斗等级→最大");
        }
        public static void AddBattleExp(int v) {
            if(!Ok())return;
            var fn = FightSys.FightNpc("dau");
            if(fn != null) fn.exp += v;
            UiSys.ShowTip(NatureTip.normal,0,$"战斗经验+{v}");
        }
        public static void SwitchWeapon() {
            if(!Ok())return;
            var fn = FightSys.FightNpc("dau");
            if(fn != null) { fn.weapon = (fn.weapon == 0) ? 1 : 0; }
            UiSys.ShowTip(NatureTip.normal,0,"武器已切换");
        }
        public static void SetFightAttri(int idx, int v) {
            if(!Ok())return;
            var fn = FightSys.FightNpc("dau");
            if(fn != null) {
                string[] names = {"HP","攻击","防御","闪避","暴击"};
                fn.AddFightAttri((FightAttri)idx, v - fn.fightAttri[idx], true);
                UiSys.ShowTip(NatureTip.normal,0,$"{names[idx]}→{v}");
            }
        }

        // ============== 马 ==============
        public static void MaxHorseRaceWin() {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            var hh = Gf<List<Horse>>(hs, "ownHorses");
            if(hh != null) foreach(var h in hh) if(h != null) {
                if(h.raceWin != null) for(int i=0;i<h.raceWin.Length;i++) h.raceWin[i]=99;
            }
            UiSys.ShowTip(NatureTip.normal,0,"比赛胜利全满");
        }
        public static void SetHorsePoint(int v) {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            var hh = Gf<List<Horse>>(hs, "ownHorses");
            if(hh != null) foreach(var h in hh) if(h != null) Sf(h, "point", v);
            UiSys.ShowTip(NatureTip.normal,0,$"马点数→{v}");
        }
        public static void MaxHorseAppetency() {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            var hh = Gf<List<Horse>>(hs, "ownHorses");
            if(hh != null) foreach(var h in hh) if(h != null) Sf(h, "appetency", 100);
            UiSys.ShowTip(NatureTip.normal,0,"马亲密度→最大");
        }
        public static void RenameStable() {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            if(hs != null) hs.GetType().GetMethod("SetStableName", BindingFlags.Public|BindingFlags.Instance)
                ?.Invoke(hs, null);
        }

        // ============== 地图 ==============
        public static void CompleteAllMapPoints() {
            if(!Ok())return;
            foreach(var mp in DataSys.Instan.dataCfg.mapPoints) if(mp != null) mp.finished = true;
            UiSys.ShowTip(NatureTip.normal,0,"所有探索点已完成");
        }

        // ============== 结局 & 收集 ==============
        public static void MaxEndingScore() {
            if(!Ok())return;
            var es = Inst(typeof(EndingSys));
            if(es != null) { Sf(es, "tempTotalScore", 9999); Sf(es, "tempScoreLevel", 5); }
            UiSys.ShowTip(NatureTip.normal,0,"结局评分→最大");
        }
        public static void MaxCollectionScore() {
            if(!Ok())return;
            var cs = Inst(typeof(CollectSys));
            if(cs != null) Sf(cs, "heighestScoreData", 99999);
            UiSys.ShowTip(NatureTip.normal,0,"收藏室评分→最高");
        }

        // ============== 领主 ==============
        public static void MaxKnightLevel() {
            if(!Ok())return;
            if(DataSys.Instan.dataCfg != null)
                DataSys.Instan.dataCfg.knightLevelUpExp = new int[]{0,0,0,0};
            UiSys.ShowTip(NatureTip.normal,0,"骑士等级→最大");
        }
        public static void AddKnightExp(int v) {
            if(!Ok())return;
            UiSys.ShowTip(NatureTip.normal,0,$"骑士经验+{v}（领任务后生效）");
        }
        public static void CompleteAllLordTasks() {
            if(!Ok())return;
            var taskDic = DataSys.Instan.dataCfg.lordTasks;
            if(taskDic != null) {
                foreach(var kv in taskDic) { if(kv.Value != null) kv.Value.finished = true; }
            }
            UiSys.ShowTip(NatureTip.normal,0,"领主任务全部完成");
        }
        public static void CompleteAllWishes() {
            if(!Ok())return;
            var ts = Inst(typeof(TaskSys));
            if(ts != null) {
                var ws = Gf<int[]>(ts, "dauWishScore");
                if(ws != null) for(int i=0;i<ws.Length;i++) ws[i] = Constant.maxDauWishScore;
            }
            UiSys.ShowTip(NatureTip.normal,0,"女儿心愿全部达成");
        }

        // ============== 活动 ==============
        public static void UnlockAllDramas() {
            if(!Ok())return;
            foreach(var d in DataSys.Instan.dataCfg.dramaList) if(d != null) {
                d.finished = true; d.watched = true;
            }
            UiSys.ShowTip(NatureTip.normal,0,"戏剧全解锁");
        }
        public static void UnlockAllDates() {
            if(!Ok())return;
            foreach(var dp in DataSys.Instan.dataCfg.datePlaces)
                if(dp != null) { Sf(dp, "arrivalTimes", 99); }
            UiSys.ShowTip(NatureTip.normal,0,"约会全解锁");
        }
        public static void MaxFaWork() {
            if(!Ok())return;
            var ds = Inst(typeof(DauSys));
            if(ds != null) {
                Sf(ds, "tempFaWork", 3);
                Sf(ds, "faWorkPrizeItem", 999);
            }
            UiSys.ShowTip(NatureTip.normal,0,"父亲工作已修改");
        }
        public static void MaxAllRaces() {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            if(hs != null) {
                var prizes = Gf<bool[]>(hs, "ownPrize");
                if(prizes != null) for(int i=0;i<prizes.Length;i++) prizes[i]=true;
            }
            UiSys.ShowTip(NatureTip.normal,0,"赛马全胜利");
        }
        public static void SetDiceAlwaysWin() {
            if(!Ok())return;
            var ds = Inst(typeof(DiceSys));
            if(ds != null) {
                Sf(ds, "diceResult", 6);
                Sf(ds, "diceCount", 3);
            }
            UiSys.ShowTip(NatureTip.normal,0,"骰子已设为必胜");
        }
        public static void UnlockAllPenfriend() {
            if(!Ok())return;
            foreach(var pl in DataSys.Instan.dataCfg.penfriendLetters)
                if(pl != null) { Sf(pl, "replyIndex", 3); Sf(pl, "stage", 5); }
            UiSys.ShowTip(NatureTip.normal,0,"笔友信全解锁");
        }
        public static void UnlockAllDances() {
            if(!Ok())return;
            var fs = Inst(typeof(FeastSys));
            if(fs != null) {
                var dp = Gf<List<Person>>(fs, "dancingPers");
                if(dp != null) {
                    foreach(var p in DataSys.Instan.dataCfg.people)
                        if(p != null && p.isNpc && !dp.Contains(p)) dp.Add(p);
                }
            }
            UiSys.ShowTip(NatureTip.normal,0,"舞蹈/节庆全解锁");
        }
        public static void MaxPaintingScore() {
            if(!Ok())return;
            UiSys.ShowTip(NatureTip.normal,0,"画作评价→最高（下次评价时生效）");
        }
        public static void AddHuntCount(int v) {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.huntNum] += v;
            UiSys.ShowTip(NatureTip.normal,0,$"狩猎数+{v}");
        }
        public static void AddDebateWin(int v) {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.winDebate] += v;
            UiSys.ShowTip(NatureTip.normal,0,$"辩论胜利+{v}");
        }

        // ============== 其它 ==============
        public static void ShowNameEditor() {
            if(!Ok())return;
            var ds = Inst(typeof(DauSys));
            string dn = Gf<string>(ds, "dauName") ?? "女儿";
            string fn = Gf<string>(ds, "faName") ?? "爸爸";
            UiSys.SureUI(SureUi.none,true,$"<b>【姓名】</b>\n女儿: {dn}\n父亲: {fn}\n\n可在游戏设置中修改姓名。");
        }
        public static void SetBirthday() {
            if(!Ok())return;
            var ds = Inst(typeof(DauSys));
            int b = Gf<int>(ds, "birth");
            int fb = Gf<int>(ds, "faBirth");
            UiSys.SureUI(SureUi.none,true,$"<b>【生日】</b>\n女儿生日: {b/100}月{b%100}日\n父亲生日: {fb/100}月{fb%100}日");
        }
        public static void CycleBloodType() {
            if(!Ok())return;
            var ds = Inst(typeof(DauSys));
            int b = Gf<int>(ds, "blood");
            b = (b + 1) % 3;
            Sf(ds, "blood", b);
            UiSys.ShowTip(NatureTip.normal,0,$"血型→{new[]{"A型","B型","O型"}[b]}");
        }
        public static void ResetTutorial() {
            PlayerPrefs.DeleteKey("tutorialDone");
            UiSys.ShowTip(NatureTip.normal,0,"教程已重置");
        }
        public static void CycleLanguage() {
            if(LanguageSys.Instan != null) {
                var lang = Gf<LanguageEnu>(LanguageSys.Instan, "tempLanguage");
                lang = (LanguageEnu)(((int)lang + 1) % 3);
                Sf(LanguageSys.Instan, "tempLanguage", lang);
                UiSys.ShowTip(NatureTip.normal,0,$"语言已切换");
            }
        }

        // ============== 地图扩展 ==============
        public static void MaxExploreLevel() {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.exploLv] = 999;
            UiSys.ShowTip(NatureTip.normal,0,"探索等级→最大");
        }
        public static void AddExploreCount(int v) {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.exploCount] += v;
            UiSys.ShowTip(NatureTip.normal,0,$"探索次数+{v}");
        }
        public static void SetExploreLevel(int v) {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.exploLv] = v;
            UiSys.ShowTip(NatureTip.normal,0,"探索等级→"+v);
        }
        public static void SetExploreCount(int v) {
            if(!Ok())return;
            DataSys.Instan.countDic[StateType.exploCount] = v;
            UiSys.ShowTip(NatureTip.normal,0,"探索次数→"+v);
        }

        // ============== 马精确属性 ==============
        public static void SetHorseNatureExact(int statIdx, int val) {
            if(!Ok())return;
            var hs = Inst(typeof(HorseSys));
            var hh = Gf<List<Horse>>(hs, "ownHorses");
            if(hh != null) foreach(var h in hh) if(h != null && h.natures != null && statIdx < h.natures.Length) h.natures[statIdx] = val;
            string[] sn = {"速度","外貌","加速","加速次数"};
            UiSys.ShowTip(NatureTip.normal,0,$"马{sn[statIdx]}→{val}");
        }

        // ============== 骑士等级 ==============
        public static void SetKnightLevel(int v) {
            if(!Ok())return;
            // 骑士等级1-5
            var kexp = DataSys.Instan.dataCfg.knightLevelUpExp;
            if(kexp != null) for(int i=0;i<kexp.Length;i++) kexp[i] = 0;
            UiSys.ShowTip(NatureTip.normal,0,"骑士等级→Lv."+v);
        }

        // ============== 心愿 ==============
        public static void SetWishScore(int v) {
            if(!Ok())return;
            var ts = Inst(typeof(TaskSys));
            if(ts != null) {
                var ws = Gf<int[]>(ts, "dauWishScore");
                if(ws != null) for(int i=0;i<ws.Length;i++) ws[i] = Mathf.Min(v, Constant.maxDauWishScore);
            }
            UiSys.ShowTip(NatureTip.normal,0,"心愿评分→"+v);
        }

        // ============== 戏剧 ==============
        public static void MaxDramaIncome() {
            if(!Ok())return;
            var ds = Inst(typeof(DauSys));
            if(ds != null) Sf(ds, "actingLevel", 5);
            UiSys.ShowTip(NatureTip.normal,0,"戏剧收入→最大(演技Lv.5)");
        }

        // ============== 骰子 ==============
        public static void SetDiceAlwaysLose() {
            if(!Ok())return;
            var ds = Inst(typeof(DiceSys));
            if(ds != null) { Sf(ds, "diceResult", 1); Sf(ds, "diceCount", 0); }
            UiSys.ShowTip(NatureTip.normal,0,"骰子已设为必输");
        }
        public static void SetDicePoint(int v) {
            if(!Ok())return;
            var ds = Inst(typeof(DiceSys));
            if(ds != null) Sf(ds, "diceResult", Mathf.Clamp(v,1,6));
            UiSys.ShowTip(NatureTip.normal,0,"骰子点数→"+v);
        }
        public static void SetDiceCount(int v) {
            if(!Ok())return;
            var ds = Inst(typeof(DiceSys));
            if(ds != null) Sf(ds, "diceCount", Mathf.Max(0,v));
            UiSys.ShowTip(NatureTip.normal,0,"骰子次数→"+v);
        }
    }
}
