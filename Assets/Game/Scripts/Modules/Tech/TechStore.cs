using UnityEngine;
using JulyArch;

namespace IsleWorks.Tech
{
    public class TechData
    {
        public int CurrentEra;
    }

    /// <summary>
    /// 科技存储，管理当前时代进度。
    /// </summary>
    public class TechStore : StoreBase<TechData>, ITechQueries
    {
        public int CurrentEra => Data.CurrentEra;

        public void AdvanceEra()
        {
            Data.CurrentEra++;
            Debug.Log($"Era advanced to {Data.CurrentEra}");
        }
    }
}
