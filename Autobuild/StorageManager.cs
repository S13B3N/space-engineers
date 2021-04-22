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
         new Display        (  5 ),
         new Computer       ( 15 ),
         new Construction   ( 25 ),
         new Interiorplate  (  6 ),
         new Girder         (  7 ),
         new LargeSteelTube (  8 ),
         new Motor          (  9 ),
         new MetalGrid      ( 10 ),
         new PowerCell      ( 11 ),
         new SmallSteelTube ( 12 ),
         new Steelplate     ( 13 ),
         new SolarCell      ( 14 ),
         new UraniumIngot   ( 18 ),
      };

      //------------------------------------------------------------------------

      List<IMyTerminalBlock> ToCargoContainerList { get; set; }

      List<IMyCargoContainer> FromCargoContainerList { get; set; }

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update10;

         ToCargoContainerList = new List<IMyTerminalBlock> ();

         FromCargoContainerList = new List<IMyCargoContainer> ();
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         Echo ( "Transfersystem 2000 started" );

         Scan ();

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

         Echo ( "Transfersystem 2000 finished" );
      }

      private void Scan ()
      {
         List<IMyCargoContainer> cargoContainerList = new List<IMyCargoContainer>();

         GridTerminalSystem.GetBlocksOfType ( cargoContainerList, searchItem => searchItem.CustomName.Contains ( "Test" ));

         List<MyInventoryItem> inventoryItemList = new List<MyInventoryItem> ();

         cargoContainerList[0].GetInventory ().GetItems ( inventoryItemList );

         for ( int nIndex = 0; nIndex < cargoContainerList.Count; nIndex++ )
         {
            MyInventoryItem inventoryItem = inventoryItemList[nIndex];

            Echo ( inventoryItem.Type.TypeId + " - " + inventoryItem.Type.SubtypeId );
         }
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

      class Display        : Item { public Display        ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "Display"         ); }}
      class Computer       : Item { public Computer       ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "Computer"        ); }}
      class Construction   : Item { public Construction   ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "Construction"    ); }}
      class Interiorplate  : Item { public Interiorplate  ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "InteriorPlate"   ); }}
      class Girder         : Item { public Girder         ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "Girder"          ); }}
      class LargeSteelTube : Item { public LargeSteelTube ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "LargeTube"       ); }}
      class Motor          : Item { public Motor          ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "Motor"           ); }}
      class MetalGrid      : Item { public MetalGrid      ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "MetalGrid"       ); }}
      class PowerCell      : Item { public PowerCell      ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "PowerCell"       ); }}
      class SmallSteelTube : Item { public SmallSteelTube ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "SmallTube"       ); }}
      class Steelplate     : Item { public Steelplate     ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "SteelPlate"      ); }}
      class SolarCell      : Item { public SolarCell      ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Component", "SolarCell"       ); }}
      class UraniumIngot   : Item { public UraniumIngot   ( int amount ) : base ( amount ) { Type = new MyItemType ( "MyObjectBuilder_Ingot"    , "Uranium"         ); }}
   }
}
