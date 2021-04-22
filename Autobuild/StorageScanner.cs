using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace space_engineers.Autobuild
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      List<IMyCargoContainer> CargoContainerList { get; set; }

      List<IMyAssembler> AssemblerList { get; set; }

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update10;

         AssemblerList = new List<IMyAssembler> ();

         CargoContainerList = new List<IMyCargoContainer> ();
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         Echo  ( "Main :)" );

         GridTerminalSystem.GetBlocksOfType ( AssemblerList );

         GridTerminalSystem.GetBlocksOfType ( CargoContainerList );

         if ( AssemblerList.Count > 0 )
         {
            AssemblerList[0].ClearQueue ();

            AssemblerList[0].AddQueueItem ( MyDefinitionId.Parse ( CreateBlueprintName ( "Girder" )), 1000.0 );
         }

         for ( int index = 0; index < CargoContainerList.Count; index++ )
         {
            IMyCargoContainer cargoContainer = CargoContainerList[index];

            Echo  ( "Found cargo " + cargoContainer.CustomName );

            TraceInventory ( cargoContainer.GetInventory ());
         }
      }

      private void TraceInventory ( IMyInventory inventory )
      {
         List<MyInventoryItem> inventoryItemList = new List<MyInventoryItem> ();

         inventory.GetItems ( inventoryItemList );

         if ( inventoryItemList.Count == 0 )
         {
            Echo  ( "Inventory is empty" );
         }

         for ( int index = 0; index < inventoryItemList.Count; index++ )
         {
            MyInventoryItem inventoryItem = inventoryItemList[index];

            Echo ( string.Format ( "ItemId: {0}, Amount: {1}", inventoryItem.ItemId, inventoryItem.Amount ));
         }
      }

      public string CreateBlueprintName ( string name )
      {
         string[] compNeed = { "RadioCommunication"
                              , "Computer"
                              , "Reactor"
                              , "Detector"
                              , "Construction"
                              , "Thrust"
                              , "Motor"
                              , "Explosives"
                              , "Girder"
                              , "GravityGenerator"
                              , "Medical"
                              , "NATO_25x184mm"
                              , "NATO_5p56x45mm" };

         string comp = "";

         if ( compNeed.Contains ( name ))
         {
            comp = "Component";
         }

         return string.Format ( "MyObjectBuilder_BlueprintDefinition/{0}{1}", name, comp );
      }
   }
}
