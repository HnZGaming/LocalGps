using System;
using HNZ.Utils;
using ProtoBuf;
using VRageMath;

namespace HNZ.FlashGps.Interface
{
    [Serializable]
    [ProtoContract]
    public sealed class FlashGpsSource
    {
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public Color Color { get; set; }

        [ProtoMember(4)]
        public string Description { get; set; }

        [ProtoMember(5)]
        public Vector3D Position { get; set; }

        [ProtoMember(6)]
        public float DecaySeconds { get; set; } // 0 -> infinite

        /// <summary>
        /// Radius of this GPS to propagate to players based on server-side character positions.
        /// If set to 0, every player will receive this GPS.
        /// </summary>
        /// <remarks>
        /// Clients will stop receiving this GPS if the character has moved outside the radius.
        /// To ensure that the GPS will be removed from HUD, use `DecaySeconds`.
        /// </remarks>
        [ProtoMember(7, IsRequired = false)]
        public double Radius { get; set; } // 0 -> everyone

        /// <summary>
        /// Entity ID that the client HUD must attach this GPS to if replicated.
        /// If set to 0, the GPS will be shown at `Position`.
        /// </summary>
        /// <remarks>
        /// Until the entity is replicated, client will move the GPS in smooth interpolation of `Position`
        /// so that the player will see the GPS "moving" on HUD as following the entity.
        /// `Position` must be set on server so that this interpolation actually takes place.
        /// </remarks>
        [ProtoMember(8, IsRequired = false)]
        public long EntityId { get; set; } // 0 -> won't snap

        [ProtoMember(9, IsRequired = false)]
        public int PromoteLevel { get; set; } // 0 -> everyone

        /// <summary>
        /// List of player ID's who shouldn't see this GPS.
        /// If not set (null), every player will see this GPS.
        /// </summary>
        /// <remarks>
        /// Every client will "receive" this GPS, then filter the display of the GPS.
        /// To make the filter before sending the GPS, use `TargetPlayers` instead.
        /// </remarks>
        [ProtoMember(10, IsRequired = false)]
        public ulong[] ExcludedPlayers { get; set; } // null -> everyone

        /// <summary>
        /// List of player ID's who will receive this GPS.
        /// If not set (null), every client will receive this GPS.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [ProtoMember(11, IsRequired = false)]
        public ulong[] TargetPlayers { get; set; } // null -> everyone

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Color)}: {Color}, {nameof(Description)}: {Description}, {nameof(Position)}: {Position}, {nameof(Radius)}: {Radius}, {nameof(EntityId)}: {EntityId}, {nameof(PromoteLevel)}: {PromoteLevel}, {nameof(ExcludedPlayers)}: {ExcludedPlayers.SeqToString()}, {nameof(TargetPlayers)}: {TargetPlayers.SeqToString()}, {nameof(DecaySeconds)}: {DecaySeconds}";
        }
    }
}