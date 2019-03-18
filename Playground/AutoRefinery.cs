using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace space_engineers.Playground.AutoRefinery
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      //------------------------------------------------------------------------
      // Machine configuration
      //------------------------------------------------------------------------

      float m_pistonRetractVelocity = 0.5f ;
      float m_pistonExtendVelocity  = 0.05f;

      //------------------------------------------------------------------------

      private EStateMachine m_stateMachine;

      private String     m_argument    ;
      private UpdateType m_updateSource;
      private DateTime   m_dateTimeNow ;

      private IMyTextPanel m_textPanel;

      private List<IMyShipDrill     > m_listDrill     = new List<IMyShipDrill     > ();
      private List<IMyPistonBase    > m_listPiston    = new List<IMyPistonBase    > ();
      private List<IMyAssembler     > m_listAssembler = new List<IMyAssembler     > ();
      private List<IMyCargoContainer> m_listCargo     = new List<IMyCargoContainer> ();

      //------------------------------------------------------------------------
      // Blockkeys
      //------------------------------------------------------------------------

      private String m_keyTextPanel = "ship_textpanel_01";

      //------------------------------------------------------------------------

      enum EStateMachine
      {
         Idle   ,
         Working,
         Error
      }

      //------------------------------------------------------------------------

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update1;

         m_stateMachine = EStateMachine.Idle;
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

            switch ( m_stateMachine )
            {
               case EStateMachine.Idle : { StateIdle (); } break;
            }
         }
         else
         {
            m_stateMachine = EStateMachine.Error;
         }
      }

      //------------------------------------------------------------------------

      private bool Init ()
      {
         bool bInitOk = true;

         if ( bInitOk ) { bInitOk = GetBlock ( m_keyTextPanel, out m_textPanel ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listDrill                     ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listPiston                    ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listAssembler                 ); }
         if ( bInitOk ) { bInitOk = GetBlock ( m_listCargo                     ); }

         return bInitOk;
      }

      //------------------------------------------------------------------------
      // States
      //------------------------------------------------------------------------

      private void StateIdle ()
      {
         DrawText ( "State: Idle" );

         if (( m_updateSource == UpdateType.Trigger ) && ( m_argument == "WORK" ))
         {
            m_stateMachine = EStateMachine.Working;
         }
         else
         {
            RetractPiston ();
            DrillOff      ();
         }
      }

      private void StateWorking ()
      {
         DrawText ( "State: Working" );

         DrillOn ();
      }

      private void StateError ()
      {
         DrawText ( "State: Error" );

         DrawText ( "Something went wrong shutdown all systems..." );
      }

      //------------------------------------------------------------------------
      // Blocks
      //------------------------------------------------------------------------

      private bool RetractPiston ()
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
            }
            else
            {
               piston.Velocity = 0.0f;
            }
         }

         return bAllPistonsRetracted;
      }

      private bool ExtendPiston ()
      {
         DrawText ( "Extend pistons..." );

         bool bAllPistonsExtended = true;

         for ( int nIndex = 0; nIndex < m_listPiston.Count; nIndex++ )
         {
            IMyPistonBase piston = m_listPiston[nIndex];

            float distance = Math.Abs ( piston.HighestPosition - piston.CurrentPosition );

            if ( distance > 0.003 )
            {
               bAllPistonsExtended = false;

               piston.Velocity = m_pistonExtendVelocity;

               break;
            }
            else
            {
               piston.Velocity = 0.0f;
            }
         }

         return bAllPistonsExtended;
      }

      private void DrillOn ()
      {
         for ( int nIndex = 0; nIndex < m_listDrill.Count; nIndex++ )
         {
            IMyShipDrill shipDrill = m_listDrill[nIndex];

            shipDrill.Enabled = true;
         }
      }

      private void DrillOff ()
      {
         for ( int nIndex = 0; nIndex < m_listDrill.Count; nIndex++ )
         {
            IMyShipDrill shipDrill = m_listDrill[nIndex];

            shipDrill.Enabled = false;
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
