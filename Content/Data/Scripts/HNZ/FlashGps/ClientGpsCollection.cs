using System.Collections.Generic;
using System.Linq;
using HNZ.FlashGps.Interface;
using HNZ.Utils;
using HNZ.Utils.Pools;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using DateTime = System.DateTime;

namespace HNZ.FlashGps
{
    public sealed class ClientGpsCollection
    {
        readonly Dictionary<long, ClientGps> _gpsEntries;

        public ClientGpsCollection()
        {
            _gpsEntries = new Dictionary<long, ClientGps>();
        }

        public void AddOrUpdateGps(FlashGpsSource src)
        {
            var localPlayer = MyAPIGateway.Session.LocalHumanPlayer;
            var character = localPlayer?.Character;
            if (character == null) return;

            var distance = Vector3D.Distance(character.GetPosition(), src.Position);
            if (distance > src.Radius && src.Radius > 0)
            {
                RemoveGps(src.Id);
                return;
            }

            if (localPlayer.PromoteLevel < (MyPromoteLevel)src.PromoteLevel)
            {
                RemoveGps(src.Id);
                return;
            }

            if (src.ExcludedPlayers?.Contains(localPlayer.SteamUserId) ?? false)
            {
                RemoveGps(src.Id);
                return;
            }

            ClientGps gpsEntry;
            if (!_gpsEntries.TryGetValue(src.Id, out gpsEntry)) // add
            {
                var gps = MyAPIGateway.Session.GPS.Create(src.Name, src.Description, src.Position, true, false);
                MyAPIGateway.Session.GPS.AddLocalGps(gps);
                PlaySound("HudGPSNotification3");

                _gpsEntries[src.Id] = gpsEntry = new ClientGps
                {
                    Gps = gps,
                    Follow = new ClientGpsFollow(gps),
                };
            }

            gpsEntry.Gps.Name = src.Name;
            gpsEntry.Gps.Description = src.Description;
            gpsEntry.Gps.GPSColor = src.Color;

            gpsEntry.Follow.SetTargetPosition(src.Position);
            gpsEntry.Follow.SetTargetEntity(src.EntityId);

            if (src.DecaySeconds > 0)
            {
                gpsEntry.DecayTime = DateTime.UtcNow + src.DecaySeconds.Seconds();
            }
        }

        public void RemoveGps(long gpsId)
        {
            ClientGps gpsEntry;
            if (_gpsEntries.TryGetValue(gpsId, out gpsEntry))
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gpsEntry.Gps);
                _gpsEntries.Remove(gpsId);
            }
        }

        public void Update()
        {
            var gpsEntries = ListPool<KeyValuePair<long, ClientGps>>.Create();
            gpsEntries.AddRange(_gpsEntries);

            foreach (var gpsEntry in gpsEntries)
            {
                gpsEntry.Value.Follow.Update();
                if (gpsEntry.Value.DecayTime > DateTime.UtcNow)
                {
                    RemoveGps(gpsEntry.Key);
                }
            }

            ListPool<KeyValuePair<long, ClientGps>>.Release(gpsEntries);
        }

        static void PlaySound(string cueName)
        {
            var character = MyAPIGateway.Session?.LocalHumanPlayer?.Character;
            if (character == null) return;

            var emitter = new MyEntity3DSoundEmitter(character as MyEntity);
            var sound = new MySoundPair(cueName);
            emitter.PlaySound(sound);
        }
    }
}