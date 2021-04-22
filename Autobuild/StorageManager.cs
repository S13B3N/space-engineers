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
      // The name for the destination container must contain this tag
      string tagTo = "#To";

      // The name for the source container must contain this tag
      string tagFrom = "#From";

      // The amount of the items for the destination container
      Item[] itemList = {
         new BulletproofGlass ( 22 ),
         new Canvas           ( 22 ),
         new Computer         ( 15 ),
         new Construction     ( 25 ),
         new Display          (  5 ),
         new Interiorplate    (  6 ),
         new Explosives       ( 11 ),
         new Girder           (  7 ),
         new LargeSteelTube   (  8 ),
         new Motor            (  9 ),
         new MetalGrid        ( 10 ),
         new Missile200       (  8 ),
         new Nato5p56_45      (  8 ),
         new Nato25_184       (  8 ),
         new PowerCell        ( 11 ),
         new SmallSteelTube   ( 12 ),
         new Steelplate       ( 13 ),
         new SolarCell        ( 14 ),
         new SuperConductor   ( 12 ),
         new Thruster         ( 12 ),
         new UraniumIngot     ( 18 ),
      };

      //------------------------------------------------------------------------

      List<IMyTerminalBlock> ToCargoContainerList { get; set; }

      List<IMyCargoContainer> FromCargoContainerList { get; set; }

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update100;

         ToCargoContainerList = new List<IMyTerminalBlock> ();

         FromCargoContainerList = new List<IMyCargoContainer> ();
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         Scan ();

         Transfer ();
      }

      public void Transfer ()
      {
         Echo ( "###--- Transfersystem 2000 started" );

         GridTerminalSystem.GetBlocksOfType ( ToCargoContainerList, searchItem => searchItem.CustomName.Contains ( tagTo ));

         GridTerminalSystem.GetBlocksOfType ( FromCargoContainerList, searchItem => searchItem.CustomName.Contains ( tagFrom ));

         for ( int nIndex = 0; nIndex < itemList.Length; nIndex++ )
         {
            for ( int nToContainerIndex = 0; nToContainerIndex < ToCargoContainerList.Count; nToContainerIndex++ )
            {
               for ( int nFromContainerIndex = 0; nFromContainerIndex < FromCargoContainerList.Count; nFromContainerIndex++ )
               {
                  Transfer ( FromCargoContainerList[nFromContainerIndex].GetInventory (), ToCargoContainerList[nToContainerIndex].GetInventory (), itemList[nIndex] );
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

      private void Transfer ( IMyInventory fromInventory, IMyInventory toInventory, Item item )
      {
         MyInventoryItem? steelPlateFoundTo = toInventory.FindItem ( item.Type );

         VRage.MyFixedPoint diffAmount = item.Amount;

         if (( steelPlateFoundTo.HasValue ))
         {
            diffAmount = diffAmount - steelPlateFoundTo.Value.Amount;
         }

         if ( 0 < diffAmount )
         {
            MyInventoryItem? steelPlateFound = fromInventory.FindItem ( item.Type );

            if ( steelPlateFound.HasValue )
            {
               fromInventory.TransferItemTo ( toInventory, steelPlateFound.Value, diffAmount );
            }
         }
      }

      //------------------------------------------------------------------------

      class Item { public int Amount { get; set; } public MyItemType Type { get; set; } public Item ( int amount ) { Amount = amount; }}

      class BulletproofGlass : Item { public BulletproofGlass ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "BulletproofGlass" ); }}
      class Canvas           : Item { public Canvas           ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Canvas"           ); }}
      class Computer         : Item { public Computer         ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Computer"         ); }}
      class Construction     : Item { public Construction     ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Construction"     ); }}
      class Display          : Item { public Display          ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Display"          ); }}
      class Explosives       : Item { public Explosives       ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Explosives"       ); }}
      class Interiorplate    : Item { public Interiorplate    ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "InteriorPlate"    ); }}
      class Girder           : Item { public Girder           ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Girder"           ); }}
      class LargeSteelTube   : Item { public LargeSteelTube   ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "LargeTube"        ); }}
      class Motor            : Item { public Motor            ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Motor"            ); }}
      class MetalGrid        : Item { public MetalGrid        ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "MetalGrid"        ); }}
      class Missile200       : Item { public Missile200       ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_AmmoMagazine", "Missile200mm"     ); }}
      class Nato5p56_45      : Item { public Nato5p56_45      ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mm"   ); }}
      class Nato25_184       : Item { public Nato25_184       ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_AmmoMagazine", "NATO_25x184mm"    ); }}
      class PowerCell        : Item { public PowerCell        ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "PowerCell"        ); }}
      class SmallSteelTube   : Item { public SmallSteelTube   ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "SmallTube"        ); }}
      class Steelplate       : Item { public Steelplate       ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "SteelPlate"       ); }}
      class SolarCell        : Item { public SolarCell        ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "SolarCell"        ); }}
      class SuperConductor   : Item { public SuperConductor   ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Superconductor"   ); }}
      class Thruster         : Item { public Thruster         ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component"   , "Thrust"           ); }}
      class UraniumIngot     : Item { public UraniumIngot     ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Ingot"       , "Uranium"          ); }}
   }
}
