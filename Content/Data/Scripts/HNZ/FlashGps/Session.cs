using System;
using System.Collections.Generic;
using System.IO;
using HNZ.FlashGps.Interface;
using HNZ.Utils;
using HNZ.Utils.Communications;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components;
using VRageMath;

namespace HNZ.FlashGps
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Session : MySessionComponentBase, ICommandListener
    {
        static readonly Logger Log = LoggerManager.Create(nameof(Session));

        ContentFile<Config> _configFile;
        Dictionary<string, Action<Command>> _commands;
        ProtobufModule _protobufModule;
        CommandModule _commandModule;
        Network _network;

        public override void LoadData()
        {
            LoggerManager.SetPrefix(nameof(FlashGps));

            _commands = new Dictionary<string, Action<Command>>
            {
                { "reload", Command_Reload },
            };

            _protobufModule = new ProtobufModule((ushort)nameof(FlashGps).GetHashCode());
            _protobufModule.Initialize();

            _commandModule = new CommandModule(_protobufModule, 1, "fg", this);
            _commandModule.Initialize();

            _network = new Network(_protobufModule, 2);
            _network.Initialize();

            MyAPIGateway.Utilities.RegisterMessageHandler(FlashGpsApi.ModVersion, OnModMessageReceived);

            ReloadConfig();
        }

        void ReloadConfig()
        {
            _configFile = new ContentFile<Config>("FlashGps.cfg", Config.CreateDefault());
            _configFile.ReadOrCreateFile();
            Config.Instance = _configFile.Content;
            LoggerManager.SetConfigs(Config.Instance.LogConfigs);
        }

        protected override void UnloadData()
        {
            _protobufModule?.Close();
            _commandModule?.Close();
            _network?.Close();
            MyAPIGateway.Utilities.UnregisterMessageHandler(FlashGpsApi.ModVersion, OnModMessageReceived);
        }

        public override void UpdateBeforeSimulation()
        {
            _protobufModule.Update();
            _commandModule.Update();
            _network.Update();
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
                FlashGpsSource src;
                long gpsId;
                binaryReader.ReadFlashGps(out isAddOrUpdate, out moduleId, out src, out gpsId);
                if (isAddOrUpdate)
                {
                    _network.SendAddOrUpdateGps(moduleId, src);
                }
                else
                {
                    _network.SendRemoveGps(moduleId, gpsId);
                }
            }
        }

        bool ICommandListener.ProcessCommandOnClient(Command command)
        {
            //todo test admin
            //todo filter
            _commands.GetValueOrDefault(command.Header, null)?.Invoke(command);
            return false;
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