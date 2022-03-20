using System.IO;
using HNZ.LocalGps.Interface;
using HNZ.Utils.Communications;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components;

namespace HNZ.LocalGps
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Session : MySessionComponentBase
    {
        static readonly Logger Log = LoggerManager.Create(nameof(Session));

        ProtobufModule _protobufModule;
        LocalGpsModule _localGpsModule;

        public override void LoadData()
        {
            LoggerManager.SetPrefix(nameof(LocalGps));

            _protobufModule = new ProtobufModule((ushort)nameof(LocalGps).GetHashCode());
            _protobufModule.Initialize();

            _localGpsModule = new LocalGpsModule(_protobufModule, 2);
            _localGpsModule.Initialize();

            MyAPIGateway.Utilities.RegisterMessageHandler(LocalGpsApi.ModVersion, OnModMessageReceived);
        }

        protected override void UnloadData()
        {
            _protobufModule?.Close();
            _localGpsModule?.Close();
            MyAPIGateway.Utilities.UnregisterMessageHandler(LocalGpsApi.ModVersion, OnModMessageReceived);
        }

        public override void UpdateBeforeSimulation()
        {
            _protobufModule.Update();
            _localGpsModule.Update();
        }

        void OnModMessageReceived(object message)
        {
            var load = message as byte[];
            if (load == null) return; // shouldn't happen

            using (var stream = new ByteStream(load, load.Length))
            using (var binaryReader = new BinaryReader(stream))
            {
                bool isAddOrUpdate;
                long moduleId;
                LocalGpsSource src;
                long gpsId;
                binaryReader.ReadLocalGps(out isAddOrUpdate, out moduleId, out src, out gpsId);
                if (isAddOrUpdate)
                {
                    _localGpsModule.SendAddOrUpdateGps(moduleId, src);
                }
                else
                {
                    _localGpsModule.SendRemoveGps(moduleId, gpsId);
                }
            }
        }
    }
}