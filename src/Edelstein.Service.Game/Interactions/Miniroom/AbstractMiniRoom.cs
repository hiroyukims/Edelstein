using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Edelstein.Core.Extensions;
using Edelstein.Core.Services;
using Edelstein.Network.Packet;
using Edelstein.Service.Game.Fields.User;
using Edelstein.Service.Game.Interactions.Miniroom.Trade;
using MoreLinq;

namespace Edelstein.Service.Game.Interactions.Miniroom
{
    public abstract class AbstractMiniRoom : IMiniRoom
    {
        protected abstract MiniRoomType Type { get; }
        protected abstract byte MaxUsers { get; }

        protected readonly IDictionary<byte, FieldUser> Users;

        protected AbstractMiniRoom()
        {
            Users = new SortedDictionary<byte, FieldUser>();
        }

        public virtual async Task<bool> Enter(FieldUser user)
        {
            var result = MiniRoomEnterResult.Success;
            var id = byte.MaxValue;

            foreach (var i in Enumerable.Range(1, MaxUsers))
            {
                if (Users.ContainsKey((byte) i)) continue;
                id = (byte) i;
                break;
            }

            if (id > MaxUsers) result = MiniRoomEnterResult.Full;
            if (Users.ContainsKey(id)) result = MiniRoomEnterResult.Etc;

            if (result == MiniRoomEnterResult.Success)
            {
                Users[id] = user;

                using (var p = new Packet(SendPacketOperations.MiniRoom))
                {
                    p.Encode<byte>((byte) MiniRoomAction.EnterResult);
                    p.Encode<byte>((byte) Type);
                    p.Encode<byte>(MaxUsers);

                    if (this is TradingRoom)
                    {
                        var others = Users.Values.Where(u => u != user);
                        var character = user.Character;
                        var other = others.FirstOrDefault()?.Character;

                        p.Encode<byte>(0x1);

                        if (other != null)
                        {
                            p.Encode<byte>(0x0);
                            other.EncodeLook(p);
                            p.Encode<string>(other.Name);
                            p.Encode<short>(other.Job);
                        }

                        p.Encode<byte>(0x1);
                        character.EncodeLook(p);
                        p.Encode<string>(character.Name);
                        p.Encode<short>(character.Job);
                    }
                    else
                    {
                        p.Encode<byte>(id);

                        Users.ForEach(kv =>
                        {
                            var character = kv.Value.Character;
                            p.Encode<byte>((byte) (kv.Key - 1));
                            character.EncodeLook(p);
                            p.Encode<string>(character.Name);
                            p.Encode<short>(character.Job);
                        });
                    }

                    p.Encode<byte>(0xFF);
                    await user.SendPacket(p);
                }


                using (var p = new Packet(SendPacketOperations.MiniRoom))
                {
                    var character = user.Character;

                    p.Encode<byte>((byte) MiniRoomAction.Enter);
                    p.Encode<byte>((byte) (this is TradingRoom ? 0x0 : id));
                    character.EncodeLook(p);
                    p.Encode<string>(character.Name);
                    p.Encode<short>(character.Job);
                    await BroadcastPacket(user, p);
                }

                return true;
            }

            using (var p = new Packet(SendPacketOperations.MiniRoom))
            {
                p.Encode<byte>((byte) MiniRoomAction.EnterResult);
                p.Encode<byte>(0x0);
                p.Encode<byte>((byte) result);
                await user.SendPacket(p);
            }

            return false;
        }

        public virtual async Task Leave(FieldUser user, MiniRoomLeaveType type = MiniRoomLeaveType.Kicked)
        {
            var pair = Users.FirstOrDefault(kv => kv.Value == user);

            Users.Remove(pair);
            using (var p = new Packet(SendPacketOperations.MiniRoom))
            {
                p.Encode<byte>((byte) MiniRoomAction.Leave);
                p.Encode<byte>(pair.Key);
                p.Encode<byte>((byte) type);
                await BroadcastPacket(p);
            }

            await user.Interact(this, true);
        }

        public virtual Task Close(MiniRoomLeaveType type = MiniRoomLeaveType.DestoryByAdmin)
            => Task.WhenAll(Users.Values.ToList().Select(u => Leave(u, type)));

        public Task BroadcastPacket(IPacket packet)
            => BroadcastPacket(null, packet);

        public Task BroadcastPacket(FieldUser source, IPacket packet)
            => Task.WhenAll(
                Users.Values
                    .Where(u => u != source)
                    .Select(u => u.SendPacket(packet))
            );

        public virtual Task OnPacket(MiniRoomAction action, FieldUser user, IPacket packet)
        {
            switch (action)
            {
                case MiniRoomAction.Leave:
                    return Leave(user);
                default:
                    return Task.CompletedTask;
            }
        }

        public Task OnPacket(RecvPacketOperations operation, FieldUser user, IPacket packet)
        {
            switch (operation)
            {
                case RecvPacketOperations.MiniRoom:
                    return OnPacket((MiniRoomAction) packet.Decode<byte>(), user, packet);
                default:
                    return Task.CompletedTask;
            }
        }
    }
}