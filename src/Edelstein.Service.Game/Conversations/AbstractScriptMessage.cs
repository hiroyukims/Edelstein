using Edelstein.Network.Packet;

namespace Edelstein.Service.Game.Conversations
{
    public abstract class AbstractScriptMessage : IScriptMessage
    {
        public abstract ScriptMessageType Type { get; }
        public ISpeaker Speaker { get; }

        public AbstractScriptMessage(ISpeaker speaker)
            => Speaker = speaker;

        public void Encode(IPacket packet)
        {
            packet.Encode<byte>(Speaker.TypeID);
            packet.Encode<int>(Speaker.TemplateID);
            packet.Encode<byte>((byte) Type);
            packet.Encode<byte>((byte) Speaker.Param);

            EncodeData(packet);
        }

        public virtual bool Validate(object response) => true;

        protected abstract void EncodeData(IPacket packet);
    }
}