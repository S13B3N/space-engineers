using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace space_engineers.Playground.Refinery
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      //------------------------------------------------------------------------
      // Machine configuration
      //------------------------------------------------------------------------

      private float m_pistonExtendVelocity   =  0.1f;
      private float m_pistonRetractVelocity  =  1.0f;
      private float m_minFreeInventoryVolume = 10.0f;

      //------------------------------------------------------------------------

      private EMachineState m_machineState;

      private String     m_argument    ;
      private UpdateType m_updateSource;
      private DateTime   m_dateTimeNow ;

      private IMyTextPanel m_textPanel;

      private List<IMyPistonBase> m_listPiston = new List<IMyPistonBase> ();
      private List<IMyShipDrill > m_listDrill  = new List<IMyShipDrill > ();

      //------------------------------------------------------------------------
      // Constants
      //------------------------------------------------------------------------

      private String m_toggleOn  = "OnOff_On" ;
      private String m_toggleOff = "OnOff_Off";

      //------------------------------------------------------------------------
      // Blockkeys
      //------------------------------------------------------------------------

      private String m_keyTextPanel = "ship_textpanel_01";

      //------------------------------------------------------------------------

      enum EMachineState
      {
         Idle ,
         Work ,
         Error
      }

      //------------------------------------------------------------------------

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update10;

         m_machineState = EMachineState.Idle;
      }

      //------------------------------------------------------------------------

      public void Main ( String argument, UpdateType updateSource )
      {
         if ( Init ())
         {
            //------------------------------------------------------------------
            // Environment
            //------------------------------------------------------------------

            m_textPanel.WritePublicText ( "" ); // Clear panel

            m_argument     = argument    ;
            m_updateSource = updateSource;
            m_dateTimeNow  = DateTime.Now;

            //------------------------------------------------------------------

            DrawText ( "//----------------------------------------------" );
            DrawText ( "// Reffi BY VX TEK AUTOMATING SYSTEMS"            );
            DrawText ( "//----------------------------------------------" );
            DrawText ( ""                                                 );

            DrawText ( "Date " +m_dateTimeNow.ToShortDateString () +" "+m_dateTimeNow.ToLongTimeString ());

            //------------------------------------------------------------------
            // State machine
            //------------------------------------------------------------------

            switch ( m_machineState )
            {
               case EMachineState.Idle  : { StateIdle  (); } break;
               case EMachineState.Work  : { StateWork  (); } break;
               case EMachineState.Error : { StateError (); } break;
            }
         }
         else
         {
            m_machineState = EMachineState.Error;
         }
      }

      //------------------------------------------------------------------------

      private bool Init ()
      {
         bool bInitOk = true;

         if ( bInitOk ) { bInitOk = GetBlock ( m_keyTextPanel, out m_textPanel ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listDrill                     ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listPiston                    ); }
         //if ( bInitOk ) { bInitOk = GetBlock ( m_listAssembler                 ); }
         //if ( bInitOk ) { bInitOk = GetBlock ( m_listCargo                     ); }

         return bInitOk;
      }

      //------------------------------------------------------------------------
      // States
      //------------------------------------------------------------------------

      private void StateIdle ()
      {
         DrawText ( "Machine is in idle state..." );

         if (( m_updateSource == UpdateType.Trigger ) && ( m_argument == "STARTWORK" ))
         {
            DrawText ( "Machine going to work state..." );

            m_machineState = EMachineState.Work;
         }
         else
         {
            RetractPiston ();
            DrillOff      ();
         }
      }

      private void StateWork ()
      {
         DrawText ( "Machine is in work state..." );

         DrawText ( "Checking inventory..." );

         bool bInventoryFull = false;

         for ( int nIndex = 0; nIndex < m_listDrill.Count; nIndex++ )
         {
            IMyShipDrill drill = m_listDrill[nIndex];

            var inventory = drill.GetInventory ();

            float freeInventoryVolume = ( float ) ( inventory.MaxVolume - inventory.CurrentVolume );

            if ( freeInventoryVolume < m_minFreeInventoryVolume )
            {
               bInventoryFull = true;

               break;
            }
         }

         //---------------------------------------------------------------------

         if ( bInventoryFull )
         {
            DrawText ( "Inventory is full, waiting..." );

            StopPiston ();
            DrillOff   ();
         }
         else
         {
            DrawText ( "Mining some material..." );

            DrillOn ();

            if ( ExtendPiston ())
            {
               DrawText ( "All pistons extended, going to idle state..." );

               m_machineState = EMachineState.Idle;
            }
         }
      }

      private void StateError ()
      {
         DrawText ( "Machine is in error state..." );

         RetractPiston ();
         DrillOff      ();
      }

      //------------------------------------------------------------------------
      // Drill
      //------------------------------------------------------------------------

      private void DrillOn ()
      {
         DrawText ( "Drill on..." );

         for ( int nIndex = 0; nIndex < m_listDrill.Count; nIndex++ )
         {
            IMyShipDrill drill = m_listDrill[nIndex];

            drill.Enabled = true;

            drill.ApplyAction ( m_toggleOn );
         }
      }

      private void DrillOff ()
      {
         DrawText ( "Drill off..." );

         for ( int nIndex = 0; nIndex < m_listDrill.Count; nIndex++ )
         {
            IMyShipDrill drill = m_listDrill[nIndex];

            drill.Enabled = false;

            drill.ApplyAction ( m_toggleOff );
         }
      }

      //------------------------------------------------------------------------
      // Piston
      //------------------------------------------------------------------------

      private bool ExtendPiston ( bool bAll = false )
      {
         DrawText ( "Extending pistons..." );

         bool bAllPistonsExtended = true;

         for ( int nIndex = 0; nIndex < m_listPiston.Count; nIndex++ )
         {
            IMyPistonBase piston = m_listPiston[nIndex];

            float distance = Math.Abs ( piston.HighestPosition - piston.CurrentPosition );

            if ( distance > 0.003 )
            {
               bAllPistonsExtended = false;

               piston.Velocity = m_pistonExtendVelocity;

               if ( !bAll )
               {
                  break;
               }
            }
            else
            {
               piston.Velocity = 0.0f;
            }
         }

         return bAllPistonsExtended;
      }

      private bool RetractPiston ( bool bAll = false )
      {
         DrawText ( "Retracting pistons..." );

         bool bAllPistonsRetracted = true;

         for ( int nIndex = 0; nIndex < m_listPiston.Count; nIndex++ )
         {
            IMyPistonBase piston = m_listPiston[nIndex];

            float distance = Math.Abs ( piston.LowestPosition - piston.CurrentPosition );

            if ( distance > 0.003 )
            {
               bAllPistonsRetracted = false;

               piston.Velocity = m_pistonRetractVelocity;

               if ( !bAll )
               {
                  break;
               }
            }
            else
            {
               piston.Velocity = 0.0f;
            }
         }

         return bAllPistonsRetracted;
      }

      private void StopPiston ()
      {
         for ( int nIndex = 0; nIndex < m_listPiston.Count; nIndex++ )
         {
            IMyPistonBase piston = m_listPiston[nIndex];

            piston.Velocity = 0.0f;
         }
      }

      //------------------------------------------------------------------------
      // DrawText
      //------------------------------------------------------------------------

      private void DrawText ( IMyTextPanel textPanel, String text, bool bNewLine = true )
      {
         Echo ( text );

         if ( textPanel != null )
         {
            if ( bNewLine )
            {
               textPanel.WritePublicText ( text + Environment.NewLine, true );
            }
            else
            {
               textPanel.WritePublicText ( text, true );
            }
         }
      }

      private void DrawText ( String text, bool bNewLine = true )
      {
         DrawText ( m_textPanel, text, bNewLine );
      }

      //------------------------------------------------------------------------
      // GetBlock
      //------------------------------------------------------------------------

      private bool GetBlock<T> ( String key, out T block, IMyTextPanel textPanel = null ) where T : class
      {
         bool bFoundBlock = false;

         block = GridTerminalSystem.GetBlockWithName ( key ) as T;

         if ( block != null )
         {
            bFoundBlock = true;
         }
         else
         {
            String message = "Cant find block with name <" + key + ">";

            Echo ( message );

            if ( textPanel == null )
            {
               DrawText ( message );
            }
            else
            {
               DrawText ( textPanel, message );
            }
         }

         return bFoundBlock;
      }

      private bool GetBlock<T> ( List<T> listBlock, IMyTextPanel textPanel = null ) where T : class
      {
         bool bFoundBlock = false;

         GridTerminalSystem.GetBlocksOfType<T> ( listBlock );

         if ( listBlock.Count > 0 )
         {
            bFoundBlock = true;
         }
         else
         {
            String message = "Cant find blocks of type <" + typeof ( T ) + ">";

            Echo ( message );

            if ( textPanel == null )
            {
               DrawText ( message );
            }
            else
            {
               DrawText ( textPanel, message );
            }
         }

         return bFoundBlock;
      }
   }
}
