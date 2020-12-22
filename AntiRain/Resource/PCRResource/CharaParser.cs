using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AntiRain.DatabaseUtils;
using AntiRain.DatabaseUtils.Helpers.PCRDataDB;
using PyLibSharp.Requests;
using Sora.Tool;

namespace AntiRain.Resource.PCRResource
{
    /// <summary>
    /// 角色别名处理
    /// </summary>
    internal class CharaParser
    {
        #region 属性
        private CharaDBHelper CharaDBHelper { get; }
        #endregion

        #region 构造函数
        internal CharaParser()
        {
            this.CharaDBHelper = new CharaDBHelper();
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// <para>从云端获取数据更新</para>
        /// <para>会覆盖原有数据</para>
        /// </summary>
        internal bool UpdateCharaNameByCloud()
        {
            ReqResponse res;
            //从服务器获取数据
            ConsoleLog.Info("角色数据更新","尝试从云端获取更新");
            try
            {
                res = Requests.Get("https://api.redive.lolikon.icu/gacha/unitdata.py",
                                   new ReqParams {Timeout = 5000});
            }
            catch (Exception e)
            {
                ConsoleLog.Error("角色数据更新", $"发生了网络错误\n{ConsoleLog.ErrorLogBuilder(e)}");
                return false;
            }
            
            //python字典数据匹配正则
            Regex PyDict = new Regex(@"\d+:\[\'.+\'],", RegexOptions.IgnoreCase);
            
            if (res.StatusCode == HttpStatusCode.OK)
            {
                //匹配所有名称数据
                MatchCollection DictMatchRes = PyDict.Matches(res.Text);
                ConsoleLog.Info("角色数据更新",$"角色数据获取成功({DictMatchRes.Count})");
                //名称数据列表
                List<PCRChara> chareNameList = new List<PCRChara>();
                //处理匹配结果
                foreach (Match chara in DictMatchRes)
                {
                    string[] charaData = chara.Value.Split(':');
                    //角色ID
                    int.TryParse(charaData[0], out int charaId);
                    //角色名
                    string charaNames = charaData[1].Substring(1, charaData[1].Length - 3)
                                                    .Replace("\'", string.Empty)
                                                    .Replace(" ", string.Empty);
                    //添加至列表
                    chareNameList.Add(new PCRChara
                    {
                        CharaId = charaId,
                        Name    = charaNames
                    });
                }
                return CharaDBHelper.UpdateCharaData(chareNameList);
            }
            ConsoleLog.Error("角色数据更新",$"获取角色数据失败 code{(int)res.StatusCode} {res.StatusCode}");
            return false;
        }

        /// <summary>
        /// 查找角色ID
        /// </summary>
        /// <param name="name"></param>
        internal int FindCharaIdByName(string name)
            => CharaDBHelper.FindCharaId(name);

        //TODO 根据别名查找角色名
        //TODO 根据ID查找角色名
        #endregion
    }
}
