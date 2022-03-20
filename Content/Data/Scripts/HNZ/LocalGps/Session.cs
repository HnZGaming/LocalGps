using System;
using System.Collections.Generic;
using System.IO;
using HNZ.LocalGps.Interface;
using HNZ.Utils;
using HNZ.Utils.Communications;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components;
using VRageMath;

namespace HNZ.LocalGps
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Session : MySessionComponentBase, ICommandListener
    {
        static readonly Logger Log = LoggerManager.Create(nameof(Session));

        ContentFile<Config> _configFile;
        Dictionary<string, Action<Command>> _commands;
        ProtobufModule _protobufModule;
        CommandModule _commandModule;
        LocalGpsModule _localGpsModule;

        public override void LoadData()
        {
            LoggerManager.SetPrefix(nameof(LocalGps));

            _commands = new Dictionary<string, Action<Command>>
            {
                { "reload", Command_Reload },
            };

            _protobufModule = new ProtobufModule((ushort)nameof(LocalGps).GetHashCode());
            _protobufModule.Initialize();

            _commandModule = new CommandModule(_protobufModule, 1, "lg", this);
            _commandModule.Initialize();

            _localGpsModule = new LocalGpsModule(_protobufModule, 2);
            _localGpsModule.Initialize();

            MyAPIGateway.Utilities.RegisterMessageHandler(LocalGpsApi.ModVersion, OnModMessageReceived);

            ReloadConfig();
        }

        void ReloadConfig()
        {
            _configFile = new ContentFile<Config>("Config.cfg", Config.CreateDefault());
            _configFile.ReadOrCreateFile();
            Config.Instance = _configFile.Content;
            LoggerManager.SetLogConfig(Config.Instance.LogConfigs);
        }

        protected override void UnloadData()
        {
            _protobufModule?.Close();
            _commandModule?.Close();
            _localGpsModule?.Close();
            MyAPIGateway.Utilities.UnregisterMessageHandler(LocalGpsApi.ModVersion, OnModMessageReceived);
        }

        public override void UpdateBeforeSimulation()
        {
            _protobufModule.Update();
            _commandModule.Update();
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

        void ICommandListener.ProcessCommandOnServer(Command command)
        {
            _commands.GetValueOrDefault(command.Header, null)?.Invoke(command);
        }

        void Command_Reload(Command command)
        {
            ReloadConfig();
            command.Respond("Local GPS", Color.White, "config reloaded");
        }
    }
}