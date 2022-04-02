﻿using System.IO;
using Sandbox.ModAPI;
using VRage;

namespace HNZ.FlashGps.Interface
{
    public class FlashGpsApi
    {
        public static readonly long ModVersion = "FlashGpsApi 1.0.*".GetHashCode();

        readonly long _moduleId;

        public FlashGpsApi(long moduleId)
        {
            _moduleId = moduleId;
        }

        public void AddOrUpdateFlashGps(FlashGpsSource src)
        {
            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteAddOrUpdateFlashGps(_moduleId, src);
                MyAPIGateway.Utilities.SendModMessage(ModVersion, stream.Data);
            }
        }

        public void RemoveFlashGps(long gpsId)
        {
            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteRemoveFlashGps(_moduleId, gpsId);
                MyAPIGateway.Utilities.SendModMessage(ModVersion, stream.Data);
            }
        }
    }
}