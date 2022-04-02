using System;
using VRage.Game.ModAPI;

namespace HNZ.FlashGps
{
    public sealed class ClientGps
    {
        public IMyGps Gps { get; set; }
        public ClientGpsFollow Follow { get; set; }
        public DateTime? DecayTime { get; set; }
    }
}