using System.Collections.Generic;
using System.IO;
using HNZ.LocalGps.Interface;
using HNZ.Utils.Communications;
using HNZ.Utils.Logging;
using VRage;

namespace HNZ.LocalGps
{
    public sealed class LocalGpsModule : IProtobufListener
    {
        static readonly Logger Log = LoggerManager.Create(nameof(LocalGpsModule));

        readonly ProtobufModule _protobufModule;
        readonly byte _loadId;

        readonly Dictionary<long, LocalGpsCollection> _gps;
        //todo buffer sending multiple data

        public LocalGpsModule(ProtobufModule protobufModule, byte loadId)
        {
            _protobufModule = protobufModule;
            _loadId = loadId;
            _gps = new Dictionary<long, LocalGpsCollection>();
        }

        public void Initialize()
        {
            _protobufModule.AddListener(this);
        }

        public void Close()
        {
            _protobufModule.RemoveListener(this);
            _gps.Clear();
        }

        public void Update()
        {
            foreach (var c in _gps)
            {
                c.Value.Update();
            }
        }

        public void SendAddOrUpdateGps(long moduleId, LocalGpsSource src, bool reliable = true, ulong? playerId = null)
        {
            Log.Debug($"Sending add or update: {moduleId}, {src.Id}, \"{src.Name}\"");

            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteAddOrUpdateLocalGps(moduleId, src);
                _protobufModule.SendDataToClients(_loadId, stream.Data, reliable, playerId);
            }
        }

        public void SendRemoveGps(long moduleId, long gpsId, bool reliable = true, ulong? playerId = null)
        {
            Log.Debug($"Sending remove: {moduleId}, {gpsId}");

            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteRemoveLocalGps(moduleId, gpsId);
                _protobufModule.SendDataToClients(_loadId, stream.Data, reliable, playerId);
            }
        }

        bool IProtobufListener.TryProcessProtobuf(byte loadId, BinaryReader reader)
        {
            if (loadId != _loadId) return false;

            bool isAddOrUpdate;
            long moduleId;
            LocalGpsSource src;
            long gpsId;
            reader.ReadLocalGps(out isAddOrUpdate, out moduleId, out src, out gpsId);
            if (isAddOrUpdate)
            {
                AddOrUpdateGps(moduleId, src);
            }
            else
            {
                RemoveGps(moduleId, gpsId);
            }

            return true;
        }

        void AddOrUpdateGps(long moduleId, LocalGpsSource src)
        {
            Log.Debug($"Received add or update: {moduleId}, {src.Id}, \"{src.Name}\"");

            GetLocalGpsCollection(moduleId).AddOrUpdateGps(src);
        }

        void RemoveGps(long moduleId, long gpsId)
        {
            Log.Debug($"Received remove: {moduleId}, {gpsId}");

            GetLocalGpsCollection(moduleId).RemoveGps(gpsId);
        }

        LocalGpsCollection GetLocalGpsCollection(long moduleId)
        {
            LocalGpsCollection c;
            if (!_gps.TryGetValue(moduleId, out c))
            {
                c = _gps[moduleId] = new LocalGpsCollection();
            }

            return c;
        }
    }
}