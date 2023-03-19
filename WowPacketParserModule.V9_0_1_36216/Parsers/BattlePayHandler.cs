using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V9_0_1_36216.Parsers
{
    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            var bit4 = packet.ReadBit("HasCreatureDisplayInfoID", idx);
            var bit12 = packet.ReadBit("HasFileDataID", idx);

            var bits16 = packet.ReadBits(10);
            var bits529 = packet.ReadBits(10);
            var bits1042 = packet.ReadBits(13);
            var bits5139 = packet.ReadBits(13);
            var bits9236 = packet.ReadBits(13);

            var bit9240 = packet.ReadBit("flags", idx);
            var bit9248 = packet.ReadBit("unkt2", idx);
            var bit9264 = packet.ReadBit("unkt3", idx);
            var bit9256 = packet.ReadBit("unkt4", idx);

            var bits13396 = packet.ReadBits(13);
            var bits17493 = packet.ReadBits(13);

            var bit9272 = packet.ReadInt32("VisualsSize", idx);

            var bit13392 = packet.ReadInt32("unktint1", idx);
            var bit21496 = packet.ReadInt32("unktint2", idx);
            var bit21500 = packet.ReadInt32("unktint3", idx);

            if (bit4)
                packet.ReadInt32("CreatureDisplayInfoID", idx);
            if (bit12)
                packet.ReadInt32("FileDataID", idx);

            packet.ReadWoWString("Name1", bits16, idx);
            packet.ReadWoWString("Name2", bits529, idx);
            packet.ReadWoWString("Name3", bits1042, idx);
            packet.ReadWoWString("Name4", bits5139, idx);
            packet.ReadWoWString("Name5", bits9236, idx);

            if (bit9240)
                packet.ReadInt32("flags", idx);
            if (bit9248)
                packet.ReadInt32("Unkt2Id", idx);
            if (bit9256)
                packet.ReadInt32("Unkt4Id", idx);
            if (bit9264)
                packet.ReadInt32("Unkt3Id", idx);

            packet.ReadWoWString("Name6", bits13396, idx);
            packet.ReadWoWString("Name7", bits17493, idx);

            for (int j = 0; j < bit9272; j++)
            {
                packet.ResetBitReader();
                var ProductName = packet.ReadBits(10);
                packet.ReadInt32("Visual", idx, j);
                packet.ReadInt32("Visual", idx, j);
                packet.ReadInt32("unktvisual", idx, j);
                packet.ReadWoWString("ProductName", ProductName, idx, j);
            }
        }

        private static void ReadBattlePayProduct915(Packet packet, params object[] idx)
        {
            packet.ReadInt32("ProductID", idx);
            packet.ReadInt64("NormalPriceFixedPoint", idx);
            packet.ReadInt64("CurrentPriceFixedPoint", idx);

            var ProductIDsSize = packet.ReadInt32("ProductIDsSize", idx);
            packet.ReadInt32("UnkInt2", idx);
            packet.ReadInt32("UnkInt3", idx);
            var UnkIntsSize = packet.ReadInt32("UnkIntsSize", idx);

            packet.ReadInt32("UnkInt4", idx);

            for (int j = 0; j < ProductIDsSize; j++)
            {
                packet.ReadInt32("ProductIDs", idx, j);
            }
            for (int j = 0; j < UnkIntsSize; j++)
            {
                packet.ReadInt32("UnkInts", idx, j);
            }

            packet.ResetBitReader();

            packet.ReadBits("ChoiceType", 7, idx);
            var HasBattlepayDisplayInfo = packet.ReadBit("HasBattlepayDisplayInfo", idx);

            if (HasBattlepayDisplayInfo)
                ReadBattlepayDisplayInfo(packet, idx);

        }



        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE)]
        public static void HandletBattlePayGetProductListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt32("CurrencyID");

            var ProductInfoSize = packet.ReadUInt32("ProductInfoSize");
            var int36 = packet.ReadUInt32("ProductSize");
            var int20 = packet.ReadUInt32("ProductGroupSize");
            var int26 = packet.ReadUInt32("ShopSize");

            for (uint index = 0; index < ProductInfoSize; index++)
            {
                ReadBattlePayProduct915(packet, index);         
            }
            //////////////////////////////////////////////////////////////

            for (uint j = 0; j < int36; j++)
            {
                packet.ReadUInt32("ProductId", j);
                packet.ReadByte("Type", j);
                packet.ReadUInt32("Flags", j);
                packet.ReadUInt32("UnkInt1", j);
                packet.ReadUInt32("DisplayId", j);
                packet.ReadUInt32("ItemId", j);
                packet.ReadUInt32("UnkInt4", j);
                packet.ReadUInt32("UnkInt5", j);

                packet.ReadUInt32("UnkInt6", j);
                packet.ReadUInt32("UnkInt7", j);
                packet.ReadUInt32("UnkInt8", j);
                packet.ReadUInt32("UnkInt9", j);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, j);
                var UnkBit = packet.ReadBit("UnkBit", j);
                var UnkBits = packet.ReadBit("UnkBits", j);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j);

                if (UnkBits)
                    packet.ReadBits("PetResultVariable", 4, j);

                for (uint g = 0; g < ItemsSize; g++)
                {
                    packet.ReadUInt32("Id", g);
                    packet.ReadByte("UnkByte", g);
                    packet.ReadUInt32("ItemID", g);
                    packet.ReadUInt32("Quantity", g);
                    packet.ReadUInt32("UnkInt1", g);
                    packet.ReadUInt32("UnkInt2", g);

                    packet.ResetBitReader();

                    var HasPet = packet.ReadBit("HasPet", g);
                    var PetResult = packet.ReadBit("PetResult", g);
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g);

                    if (PetResult)
                        packet.ReadBits("PetResultVariable", 4, g);

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, g);

                }
                packet.ReadWoWString("Name", UnkString, j);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, j);


            }

            for (int i = 0; i < int20; i++)
            {
                packet.ReadInt32("GroupID", i);
                packet.ReadInt32("IconFileDataID", i);
                packet.ReadByte("DisplayType", i);
                packet.ReadInt32("Ordering", i);
                packet.ReadInt32("unkt", i);

                packet.ResetBitReader();

                var bits4 = packet.ReadBits("nameLength", 8, i);
                var bits7 = packet.ReadBits("IsAvailableDescription", 24, i);
                packet.ReadWoWString("Name", bits4, i);
                if (bits7 > 1)
                    packet.ReadWoWString("Description", bits7, i);

            }

            for (uint i = 0; i < int26; i++)
            {
                packet.ReadUInt32("EntryID", i);
                packet.ReadUInt32("GroupID", i);
                packet.ReadUInt32("ProductID", i);
                packet.ReadInt32("Ordering", i);
                packet.ReadInt32("VasServiceType", i);
                packet.ReadByte("StoreDeliveryType", i);

                packet.ResetBitReader();

                var bit5172 = packet.ReadBit("HasBattlepayDisplayInfo", i);
                if (bit5172)
                    ReadBattlepayDisplayInfo(packet, i);
            }
        }

        private static void ReadBattlePayDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadInt64("DistributionID", index);

            packet.ReadInt32("Status", index);
            packet.ReadInt32("ProductID", index);

            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadInt32("TargetVirtualRealm", index);
            packet.ReadInt32("TargetNativeRealm", index);

            packet.ReadInt64("PurchaseID", index);

            packet.ResetBitReader();

            var bit5248 = packet.ReadBit("HasBattlePayProduct", index);

            packet.ReadBit("Revoked", index);

            if (bit5248)
            {
                packet.ReadUInt32("ProductId", index);
                packet.ReadByte("Type", index);
                packet.ReadUInt32("Flags", index);
                packet.ReadUInt32("UnkInt1", index);
                packet.ReadUInt32("DisplayId", index);
                packet.ReadUInt32("ItemId", index);
                packet.ReadUInt32("UnkInt4", index);
                packet.ReadUInt32("UnkInt5", index);

                packet.ReadUInt32("UnkInt6", index);
                packet.ReadUInt32("UnkInt7", index);
                packet.ReadUInt32("UnkInt8", index);
                packet.ReadUInt32("UnkInt9", index);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, index);
                var UnkBit = packet.ReadBit("UnkBit", index);
                var UnkBits = packet.ReadBit("UnkBits", index);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, index);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", index);

                if (UnkBits)
                    packet.ReadBits("PetResultVariable", 4, index);

                for (uint g = 0; g < ItemsSize; g++)
                {
                    packet.ReadUInt32("Id", g);
                    packet.ReadByte("UnkByte", g);
                    packet.ReadUInt32("ItemID", g);
                    packet.ReadUInt32("Quantity", g);
                    packet.ReadUInt32("UnkInt1", g);
                    packet.ReadUInt32("UnkInt2", g);

                    packet.ResetBitReader();

                    var HasPet = packet.ReadBit("HasPet", g);
                    var PetResult = packet.ReadBit("PetResult", g);
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g);

                    if (PetResult)
                        packet.ReadBits("PetResultVariable", 4, g);

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, g);

                }
                packet.ReadWoWString("Name", UnkString, index);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, index);
            }
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleBattlePayGetDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            packet.ResetBitReader();
            var int6 = packet.ReadBits("BattlePayDistributionObjectCount", 11);
            for (uint index = 0; index < int6; index++)
                ReadBattlePayDistributionObject(packet, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE)]
        public static void HandleBattlePayDistributionUpdate(Packet packet)
        {
            ReadBattlePayDistributionObject(packet);
        }


    }
}