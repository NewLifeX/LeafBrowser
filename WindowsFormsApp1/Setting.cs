using System;
using System.ComponentModel;
using NewLife.Xml;

namespace LeafBrower
{
    /// <summary>浏览器配置</summary>
    [Description("浏览器")]
    [XmlConfigFile("Config\\Browser.config", 10_000)]
    class Setting : XmlConfig<Setting>
    {
        /// <summary>访问地址</summary>
        [Description("访问地址")]
        public String Url { get; set; } = "https://nnhy.cnblogs.com";

        /// <summary>数据目录。默认..\\Data</summary>
        [Description("数据目录。默认..\\Data")]
        public String DataPath { get; set; } = "..\\Data";
}
}