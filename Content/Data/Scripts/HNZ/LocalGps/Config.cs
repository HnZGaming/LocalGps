using System;
using System.Xml.Serialization;
using HNZ.Utils.Logging;
using VRage.Utils;

namespace HNZ.LocalGps
{
    [Serializable]
    public sealed class Config
    {
        public static Config Instance { get; set; }

        [XmlElement]
        public LogConfig[] LogConfigs;

        public static Config CreateDefault() => new Config
        {
            LogConfigs = new[]
            {
                new LogConfig
                {
                    Severity = MyLogSeverity.Info,
                    Prefix = "",
                }
            }
        };
    }
}