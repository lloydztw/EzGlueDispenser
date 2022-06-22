using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eazy_Project_III
{
    public enum ModuleName : int
    {
        [Description("拾取模组")]
        /// <summary>
        /// 前3轴模组 校正平面 拾取玻璃 等操作
        /// </summary>
        MODULE_PICK = 0,
        [Description("点胶模组")]
        /// <summary>
        /// 中间3轴模组 用于点胶
        /// </summary>
        MODULE_DISPENSING = 1,
        [Description("微调模组")]
        /// <summary>
        /// 后3轴模组 用于调整投影对位 角度
        /// </summary>
        MODULE_ADJUST = 2,
    }

    class Enums
    {
    }

    
}
