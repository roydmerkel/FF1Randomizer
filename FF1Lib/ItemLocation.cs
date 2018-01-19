﻿using System;
using System.Linq;
using RomUtilities;

namespace FF1Lib
{
    [Flags]
    public enum AccessRequirement
    {
        None = 0x00,
        Key = 0x01,
        Rod = 0x02,
        Oxyale = 0x04,
        Cube = 0x08,
        BlackOrb = 0x10,
        Lute = 0x20,
        All = 0xFF
    }
    public interface IRewardSource {
        int Address { get; }
        string Name { get; }
        MapLocation MapLocation { get; }
        Item Item { get; }
        AccessRequirement AccessRequirement { get; }
        bool IsUnused { get; }
        string SpoilerText { get; }

        void Put(FF1Rom rom);
    }
    public abstract class RewardSourceBase : IRewardSource 
    {
        public int Address { get; protected set; }
        public string Name { get; protected set; }
        public MapLocation MapLocation { get; protected set; }
        public Item Item { get; protected set; }
        public AccessRequirement AccessRequirement { get; protected set; }
        public bool IsUnused { get; protected set; }

        public virtual bool IsTreasure => false;
        public string SpoilerText =>
        $"{Name}{string.Join("", Enumerable.Repeat(" ", Math.Max(1, 30 - Name.Length)).ToList())}" +
        $"\t{Enum.GetName(typeof(Item), Item)}";

        protected RewardSourceBase(int address, string name, MapLocation mapLocation, Item item,
                            AccessRequirement accessRequirement = AccessRequirement.None,
                            bool isUnused = false)
        {
            Address = address;
            Name = name;
            Item = item;
            MapLocation = mapLocation;
            AccessRequirement = accessRequirement;
            IsUnused = isUnused;
        }
        protected RewardSourceBase(IRewardSource copyFromRewardSource, Item item)
        {
            Address = copyFromRewardSource.Address;
            Name = copyFromRewardSource.Name;
            Item = item;
            MapLocation = copyFromRewardSource.MapLocation;
            AccessRequirement = copyFromRewardSource.AccessRequirement;
            IsUnused = false;
        }
        public override int GetHashCode() => Address.GetHashCode();

        public virtual void Put(FF1Rom rom) => rom.Put(Address, new[] { (byte)Item });
    }

    public class TreasureChest : RewardSourceBase
    {
        private const int _treasureChestBaseAddress = 0x3100;
        public override bool IsTreasure => true;
        public TreasureChest(int address, string name, MapLocation mapLocation, Item item,
                             AccessRequirement accessRequirement = AccessRequirement.None,
                             bool isUnused = false)
            : base(address, name, mapLocation, item, accessRequirement, isUnused) { }

        public TreasureChest(IRewardSource copyFromRewardSource, Item item)
            : base(copyFromRewardSource, item) { }

    }
    public class MapObject : RewardSourceBase
    {
        private const int _mapObjectTalkDataAddress = 0x395D5;
        private const int _mapObjectTalkDataSize = 4;
        private const int _giftItemIndex = 3;

        private const int _mapObjTalkJumpTblAddress = 0x390D3;
        private const int _mapObjTalkJumpTblDataSize = 2;
        private readonly Blob _eventFlagRoutineAddress = Blob.FromHex("6B95");
        private readonly Blob _itemTradeRoutineAddress = Blob.FromHex("6C93");

        private readonly int _objectRoutineAddress;
        private readonly ObjectId _requiredGameEventFlag;
        private readonly Item _requiredItemTrade;
        private readonly bool _useVanillaRoutineAddress;

        public MapObject(ObjectId objectId, MapLocation mapLocation, Item item,
                         AccessRequirement accessRequirement = AccessRequirement.None,
                         ObjectId requiredGameEventFlag = ObjectId.None,
                         Item requiredItemTrade = Item.None,
                        bool useVanillaRoutineAddress = false)
            : base(_mapObjectTalkDataAddress + _giftItemIndex + 
                   _mapObjectTalkDataSize * (byte)objectId,
                   Enum.GetName(typeof(ObjectId), objectId),
                   mapLocation, 
                   item, 
                   accessRequirement) 
        {
            _objectRoutineAddress = (byte)objectId * _mapObjTalkJumpTblDataSize + _mapObjTalkJumpTblAddress;
            _requiredGameEventFlag = requiredGameEventFlag;
            _requiredItemTrade = requiredItemTrade;
            _useVanillaRoutineAddress = useVanillaRoutineAddress;
            if (_requiredGameEventFlag != ObjectId.None && _requiredItemTrade != Item.None)
                throw new InvalidOperationException(
                    $"Attempted to Put invalid npc item placement: \n{SpoilerText}");
        }

        public MapObject(IRewardSource copyFromRewardSource, Item item)
            : base(copyFromRewardSource, item) 
        {
            var copyFromMapObject = copyFromRewardSource as MapObject;
            if (copyFromMapObject == null)
                return;

            _objectRoutineAddress = copyFromMapObject._objectRoutineAddress;
            _requiredGameEventFlag = copyFromMapObject._requiredGameEventFlag;
            _requiredItemTrade = copyFromMapObject._requiredItemTrade;
            _useVanillaRoutineAddress = copyFromMapObject._useVanillaRoutineAddress;
            if (_requiredGameEventFlag != ObjectId.None && _requiredItemTrade != Item.None)
                throw new InvalidOperationException(
                    $"Attempted to Put invalid npc item placement: \n{SpoilerText}");
        }
        
        public override void Put(FF1Rom rom)
        {
            if (_useVanillaRoutineAddress) {
                base.Put(rom);
                return;
            }

            if (_requiredItemTrade != Item.None)
            { 
                rom.Put(_objectRoutineAddress, _itemTradeRoutineAddress); 
                rom.Put(Address - _giftItemIndex, new [] { (byte)_requiredItemTrade }); 
            }
            else 
            {
                rom.Put(_objectRoutineAddress, _eventFlagRoutineAddress);
                rom.Put(Address - _giftItemIndex, new [] { (byte)_requiredGameEventFlag }); 
            }

            base.Put(rom);
        }
    }

    public class ItemShopSlot : RewardSourceBase
    {
        public ItemShopSlot(int address, string name, MapLocation mapLocation, Item item) 
            : base(address, name, mapLocation, item) {}

        public ItemShopSlot(IRewardSource copyFromRewardSource, Item item)
            : base(copyFromRewardSource, item) { }
        
        public override void Put(FF1Rom rom)
        {
            if (Item > Item.Soft)
                throw new InvalidOperationException(
                    $"Attempted to Put invalid item shop placement: \n{SpoilerText}");
            base.Put(rom);
        }
    }
}