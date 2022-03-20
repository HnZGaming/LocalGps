using System.Collections.Generic;
using HNZ.LocalGps.Interface;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace HNZ.LocalGps
{
    public sealed class LocalGpsCollection
    {
        readonly Dictionary<long, IMyGps> _gps;
        readonly Dictionary<long, LocalGpsFollow> _gpsFollows;

        public LocalGpsCollection()
        {
            _gps = new Dictionary<long, IMyGps>();
            _gpsFollows = new Dictionary<long, LocalGpsFollow>();
        }

        public void AddOrUpdateGps(LocalGpsSource src)
        {
            var character = MyAPIGateway.Session.LocalHumanPlayer.Character;
            if (src.Radius >= 0 && Vector3D.Distance(character.GetPosition(), src.Position) > src.Radius)
            {
                RemoveGps(src.Id);
                return;
            }

            IMyGps gps;
            if (!_gps.TryGetValue(src.Id, out gps))
            {
                _gps[src.Id] = gps = MyAPIGateway.Session.GPS.Create(src.Name, src.Description, src.Position, true, false);
                _gpsFollows[src.Id] = new LocalGpsFollow(gps);
                MyAPIGateway.Session.GPS.AddLocalGps(gps);
            }

            gps.Name = src.Name;
            gps.Description = src.Description;
            gps.GPSColor = src.Color;

            var gpsFollow = _gpsFollows[src.Id];
            gpsFollow.SetTargetPosition(src.Position);
            gpsFollow.SetTargetEntity(src.EntityId);
        }

        public void RemoveGps(long gpsId)
        {
            IMyGps gps;
            if (_gps.TryGetValue(gpsId, out gps))
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gps);
                _gps.Remove(gpsId);
                _gpsFollows.Remove(gpsId);
            }
        }

        public void Update()
        {
            foreach (var p in _gpsFollows)
            {
                p.Value.Update();
            }
        }
    }
}