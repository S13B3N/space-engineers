using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace space_engineers.Autobuild.StorageManager
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      TransferItem[] m_transferItemList = {

         new TransferItem () { TagTo = "#ToRefinery", TagFrom = "#From", Type = EItemType.Steelplate  , Amount = 15 },
         new TransferItem () { TagTo = "#ToRefinery", TagFrom = "#From", Type = EItemType.SolarCell   , Amount =  5 },
         new TransferItem () { TagTo = "#ToRefinery", TagFrom = "#From", Type = EItemType.Computer    , Amount =  8 },
         new TransferItem () { TagTo = "#ToRefinery", TagFrom = "#From", Type = EItemType.Construction, Amount = 11 },
         new TransferItem () { TagTo = "#ToTurret"  , TagFrom = "#From", Type = EItemType.Nato25_184  , Amount = 11 },
         new TransferItem () { TagTo = "#ToTurret"  , TagFrom = "#From", Type = EItemType.Nato5p56_45 , Amount = 11 },
         new TransferItem () { TagTo = "#ToTurret"  , TagFrom = "#From", Type = EItemType.Missile200  , Amount = 11 },


         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.BulletproofGlass, Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Canvas          , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Computer        , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Construction    , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Display         , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Explosives      , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Interiorplate   , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Girder          , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.LargeSteelTube  , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Motor           , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.MetalGrid       , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Missile200      , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Nato5p56_45     , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Nato25_184      , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.PowerCell       , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.SmallSteelTube  , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Steelplate      , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.SolarCell       , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.SuperConductor  , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.Thruster        , Amount = 7 },
         //new TransferItem () { TagTo = "#ToXXX"  , TagFrom = "#FromYYY", Type = EItemType.UraniumIngot    , Amount = 7 },
      };

      //------------------------------------------------------------------------

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update100;
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         Scan ();

         Transfer ();
      }

      public void Transfer ()
      {
         Echo ( "###--- Transfersystem 2000 started" );

         List<IMyCargoContainer> toCargoContainerList   = new List<IMyCargoContainer> ();
         List<IMyTerminalBlock>  fromCargoContainerList = new List<IMyTerminalBlock>  ();

         for ( int nIndex = 0; nIndex < m_transferItemList.Length; nIndex++ )
         {
            toCargoContainerList.Clear ();

            TransferItem transferItem = m_transferItemList[nIndex];

            MyItemType itemType = GetItemType ( transferItem.Type );

            GridTerminalSystem.GetBlocksOfType ( toCargoContainerList, searchItem => searchItem.CustomName.Contains ( transferItem.TagTo ));

            for ( int nToContainerIndex = 0; nToContainerIndex < toCargoContainerList.Count; nToContainerIndex++ )
            {
               IMyInventory toInventory = toCargoContainerList[nToContainerIndex].GetInventory ();

               fromCargoContainerList.Clear ();

               GridTerminalSystem.GetBlocksOfType ( fromCargoContainerList, searchItem => searchItem.CustomName.Contains ( transferItem.TagFrom ));

               for ( int nFromContainerIndex = 0; nFromContainerIndex < fromCargoContainerList.Count; nFromContainerIndex++ )
               {
                  MyInventoryItem? steelPlateFoundTo = toInventory.FindItem ( itemType );

                  VRage.MyFixedPoint diffAmount = transferItem.Amount;

                  if (( steelPlateFoundTo.HasValue ))
                  {
                     diffAmount = diffAmount - steelPlateFoundTo.Value.Amount;
                  }

                  if ( 0 < diffAmount )
                  {
                     IMyInventory fromInventory = fromCargoContainerList[nFromContainerIndex].GetInventory ();

                     MyInventoryItem? steelPlateFound = fromInventory.FindItem ( itemType );

                     if ( steelPlateFound.HasValue )
                     {
                        fromInventory.TransferItemTo ( toInventory, steelPlateFound.Value, diffAmount );
                     }
                  }
               }
            }
         }

         Echo ( "###--- Transfersystem 2000 finished" );
      }

      private void Scan ()
      {
         Echo ( "###--- Inventory scan 3001 started" );

         List<IMyCargoContainer> cargoContainerList = new List<IMyCargoContainer>();

         GridTerminalSystem.GetBlocksOfType ( cargoContainerList, searchItem => searchItem.CustomName.Contains ( "Test" ));

         for ( int cargoIndex = 0; cargoIndex < cargoContainerList.Count; cargoIndex++ )
         {
            List<MyInventoryItem> inventoryItemList = new List<MyInventoryItem> ();

            cargoContainerList[cargoIndex].GetInventory ().GetItems ( inventoryItemList );

            for ( int inventoryItemIndex = 0; inventoryItemIndex < inventoryItemList.Count; inventoryItemIndex++ )
            {
               MyInventoryItem inventoryItem = inventoryItemList[inventoryItemIndex];

               Echo ( inventoryItem.Type.TypeId + " - " + inventoryItem.Type.SubtypeId );
            }
         }

         Echo ( "###--- Inventory scan 3001 finished" );
      }

      private MyItemType GetItemType ( EItemType itemType )
      {
         return m_itemDictionary[itemType].ItemType;
      }

      //------------------------------------------------------------------------

      enum EItemType
      {
         BulletproofGlass,
         Canvas          ,
         Computer        ,
         Construction    ,
         Display         ,
         Explosives      ,
         Interiorplate   ,
         Girder          ,
         LargeSteelTube  ,
         Motor           ,
         MetalGrid       ,
         Missile200      ,
         Nato5p56_45     ,
         Nato25_184      ,
         PowerCell       ,
         SmallSteelTube  ,
         Steelplate      ,
         SolarCell       ,
         SuperConductor  ,
         Thruster        ,
         UraniumIngot    ,
      }

      Dictionary<EItemType, Item> m_itemDictionary = new Dictionary<EItemType, Item>
      {
         { EItemType.BulletproofGlass,  new  BulletproofGlass ()},
         { EItemType.Canvas          ,  new  Canvas           ()},
         { EItemType.Computer        ,  new  Computer         ()},
         { EItemType.Construction    ,  new  Construction     ()},
         { EItemType.Display         ,  new  Display          ()},
         { EItemType.Explosives      ,  new  Explosives       ()},
         { EItemType.Interiorplate   ,  new  Interiorplate    ()},
         { EItemType.Girder          ,  new  Girder           ()},
         { EItemType.LargeSteelTube  ,  new  LargeSteelTube   ()},
         { EItemType.Motor           ,  new  Motor            ()},
         { EItemType.MetalGrid       ,  new  MetalGrid        ()},
         { EItemType.Missile200      ,  new  Missile200       ()},
         { EItemType.Nato5p56_45     ,  new  Nato5p56_45      ()},
         { EItemType.Nato25_184      ,  new  Nato25_184       ()},
         { EItemType.PowerCell       ,  new  PowerCell        ()},
         { EItemType.SmallSteelTube  ,  new  SmallSteelTube   ()},
         { EItemType.Steelplate      ,  new  Steelplate       ()},
         { EItemType.SolarCell       ,  new  SolarCell        ()},
         { EItemType.SuperConductor  ,  new  SuperConductor   ()},
         { EItemType.Thruster        ,  new  Thruster         ()},
         { EItemType.UraniumIngot    ,  new  UraniumIngot     ()},
      };

      class TransferItem { public string TagTo { get; set; } public string TagFrom { get; set; } public EItemType Type { get; set; } public int Amount { get; set; }}

      class Item { public MyItemType ItemType { get; set; } public Item ( MyItemType itemType ) { ItemType = itemType; }}

      class BulletproofGlass : Item { public BulletproofGlass () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "BulletproofGlass" )){}}
      class Canvas           : Item { public Canvas           () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Canvas"           )){}}
      class Computer         : Item { public Computer         () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Computer"         )){}}
      class Construction     : Item { public Construction     () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Construction"     )){}}
      class Display          : Item { public Display          () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Display"          )){}}
      class Explosives       : Item { public Explosives       () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Explosives"       )){}}
      class Interiorplate    : Item { public Interiorplate    () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "InteriorPlate"    )){}}
      class Girder           : Item { public Girder           () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Girder"           )){}}
      class LargeSteelTube   : Item { public LargeSteelTube   () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "LargeTube"        )){}}
      class Motor            : Item { public Motor            () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Motor"            )){}}
      class MetalGrid        : Item { public MetalGrid        () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "MetalGrid"        )){}}
      class Missile200       : Item { public Missile200       () : base ( new MyItemType ( "MyObjectBuilder_AmmoMagazine", "Missile200mm"     )){}}
      class Nato5p56_45      : Item { public Nato5p56_45      () : base ( new MyItemType ( "MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mm"   )){}}
      class Nato25_184       : Item { public Nato25_184       () : base ( new MyItemType ( "MyObjectBuilder_AmmoMagazine", "NATO_25x184mm"    )){}}
      class PowerCell        : Item { public PowerCell        () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "PowerCell"        )){}}
      class SmallSteelTube   : Item { public SmallSteelTube   () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "SmallTube"        )){}}
      class Steelplate       : Item { public Steelplate       () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "SteelPlate"       )){}}
      class SolarCell        : Item { public SolarCell        () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "SolarCell"        )){}}
      class SuperConductor   : Item { public SuperConductor   () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Superconductor"   )){}}
      class Thruster         : Item { public Thruster         () : base ( new MyItemType ( "MyObjectBuilder_Component"   , "Thrust"           )){}}
      class UraniumIngot     : Item { public UraniumIngot     () : base ( new MyItemType ( "MyObjectBuilder_Ingot"       , "Uranium"          )){}}
   }
}
