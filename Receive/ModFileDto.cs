using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Receive
{
    class ModFileDto
    {
        /// <summary>
        /// mod文件名
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// 标志位，0删除，1新增，2不变
        /// </summary>
        public string flag { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }
    }
}
