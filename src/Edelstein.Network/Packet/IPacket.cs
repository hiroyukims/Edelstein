using System;

namespace Edelstein.Network.Packet
{
    public interface IPacket : IDisposable
    {
        byte[] Buffer { get; }
        int Length { get; }

        IPacket Encode<T>(T value);
        IPacket EncodeFixedString(string value, int length);
        T Decode<T>();
    }
}