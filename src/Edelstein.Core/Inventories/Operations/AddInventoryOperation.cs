using Edelstein.Core.Extensions;
using Edelstein.Data.Entities.Inventory;
using Edelstein.Network.Packet;

namespace Edelstein.Core.Inventories.Operations
{
    public class AddInventoryOperation : AbstractInventoryOperation
    {
        protected override InventoryOperationType Type => InventoryOperationType.Add;
        private readonly ItemSlot _item;

        public AddInventoryOperation(
            ItemInventoryType inventory,
            short slot,
            ItemSlot item
        ) : base(inventory, slot)
            => _item = item;

        public override void EncodeData(IPacket packet)
        {
            _item.Encode(packet);
        }
    }
}