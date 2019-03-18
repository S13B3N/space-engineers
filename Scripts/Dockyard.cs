using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace space_engineers.Scripts.Dockyard
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      //------------------------------------------------------------------------
      // Dockyard configuration
      //------------------------------------------------------------------------

      private float m_pistonExtendVelocity  =  0.10f;
      private float m_pistonRetractVelocity = -0.10f;

      //------------------------------------------------------------------------

      enum EInternalState
      {
         Idle      ,
         Prepare   ,
         Assembling,
         Error
      }

      //------------------------------------------------------------------------
      // Dockyard systems
      //------------------------------------------------------------------------

      private const String m_nameTextPanel    = "ship_textpanel_01" ;
      private const String m_nameLogTextPanel = "ship_textpanel_log";
      private const String m_nameProjector    = "ship_projector_01" ;

      private IMyTextPanel m_textPanel   ;
      private IMyTextPanel m_logTextPanel;
      private IMyProjector m_projector   ;

      private List<IMyExtendedPistonBase > m_listShipPiston  ;
      private List<IMyShipWelder         > m_listShipWelder  ;
      private List<IMyMotorRotor         > m_listRotor       ;
      private List<IMyLightingBlock      > m_listWarningLight;

      //------------------------------------------------------------------------
      // Internal assembling states
      //------------------------------------------------------------------------

      private String         m_argument     ;
      private UpdateType     m_updateSource ;
      private DateTime       m_dateTimeNow  ;
      private EInternalState m_internalState;

      private DateTime m_lastAssemblingChange;
      private int      m_lastRemainingBlocks ;

      private List<String> m_listError;

      //------------------------------------------------------------------------

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update10;

         m_listShipPiston   = new List<IMyExtendedPistonBase > ();
         m_listShipWelder   = new List<IMyShipWelder         > ();
         m_listRotor        = new List<IMyMotorRotor         > ();
         m_listWarningLight = new List<IMyLightingBlock      > ();

         m_listError = new List<String> ();

         m_internalState = EInternalState.Idle;
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         if ( InitSystems ())
         {
            //------------------------------------------------------------------
            // Environment
            //------------------------------------------------------------------

            m_listError.Clear (); // Clear errorlog

            m_textPanel.WritePublicText ( "" ); // Clear panel

            m_argument     = argument    ;
            m_updateSource = updateSource;
            m_dateTimeNow  = DateTime.Now;

            //------------------------------------------------------------------

            DrawText ( "//----------------------------------------------" );
            DrawText ( "// Freddi BY VX TEK AUTOMATING SYSTEMS"           );
            DrawText ( "//----------------------------------------------" );
            DrawText ( ""                                                 );

            DrawText ( "Date " +m_dateTimeNow.ToShortDateString () +" "+m_dateTimeNow.ToLongTimeString ());

            //------------------------------------------------------------------
            // State machine
            //------------------------------------------------------------------

            switch ( m_internalState )
            {
               case EInternalState.Idle       : { StateIdle       (); } break;
               case EInternalState.Prepare    : { StatePrepare    (); } break;
               case EInternalState.Assembling : { StateAssembling (); } break;
               case EInternalState.Error      : { StateError      (); } break;
            }
         }
         else
         {
            m_internalState = EInternalState.Error;
         }
      }

      //------------------------------------------------------------------------

      private bool InitSystems ()
      {
         bool bSystemOk = false;

         if ( GetBlock<IMyTextPanel> ( m_nameTextPanel, out m_textPanel ))
         {
            if ( GetBlock<IMyProjector> ( m_nameProjector, out m_projector ))
            {
               GridTerminalSystem.GetBlocksOfType ( m_listShipPiston );
               GridTerminalSystem.GetBlocksOfType ( m_listShipWelder );

               if ( m_listShipPiston.Count > 0 )
               {
                  if ( m_listShipWelder.Count > 0 )
                  {
                     bSystemOk = true;
                  }
                  else
                  {
                     Echo ( "Cant find shipwelder" );

                     if ( m_textPanel != null )
                     {
                        DrawText ( "Cant find shipwelder" );
                     }
                  }
               }
               else
               {
                  Echo ( "Cant find shipwelder" );

                  if ( m_textPanel != null )
                  {
                     DrawText ( "Cant find shipwelder" );
                  }
               }

               // Optional

               GridTerminalSystem.GetBlocksOfType ( m_listRotor        );
               GridTerminalSystem.GetBlocksOfType ( m_listWarningLight );

               GetBlock<IMyTextPanel> ( m_nameLogTextPanel, out m_logTextPanel );
            }
         }

         return bSystemOk;
      }

      //------------------------------------------------------------------------
      // State machine
      //------------------------------------------------------------------------

      private void StateIdle ()
      {
         DrawText ( "State: Idle" );

         if (( m_updateSource == UpdateType.Trigger ) && ( m_argument == "STARTASSEMBLING" ))
         {
            if (( m_projector.IsProjecting ) && ( m_projector.RemainingBlocks > 0 ))
            {
               m_lastAssemblingChange = m_dateTimeNow              ;
               m_lastRemainingBlocks  = m_projector.RemainingBlocks;

               m_internalState = EInternalState.Prepare;
            }
         }
         else
         {
            RetractPiston       ();
            ToggleOffShipWelder ();
            ShowIdleLights      ();
         }
      }

      private void StatePrepare ()
      {
         DrawText ( "State: Prepare" );

         if ( ExtendPiston ())
         {
            ToggleOnShipWelder ();

            m_internalState = EInternalState.Assembling;
         }
      }

      private void StateAssembling ()
      {
         DrawText ( "State: Assembling" );

         DrawText ( "RemainingBlocks: " + m_projector.RemainingBlocks );

         if ( m_projector.RemainingBlocks == 0 )
         {
            if ( RetractPiston ())
            {
               m_internalState = EInternalState.Idle;
            }
         }
         else
         {
            ShowWarningLights ();

            bool bWelding = false;

            List<IMyTerminalBlock> listTerminalBlock = new List<IMyTerminalBlock> ();

            GridTerminalSystem.GetBlocks ( listTerminalBlock );

            for ( int nIndex = 0; !bWelding && ( nIndex < listTerminalBlock.Count ); nIndex++ )
            {
               IMyTerminalBlock terminalBlock = listTerminalBlock [nIndex];

               IMySlimBlock slimBlock = terminalBlock.CubeGrid.GetCubeBlock ( terminalBlock.Position );

               if ( slimBlock.BuildLevelRatio < 1.0f )
               {
                  bWelding = true;
               }
            }

            if ( bWelding )
            {
               DrawText ( "Currently welding some blocks..." );

               StopPiston ();
            }
            else if ( m_projector.RemainingBlocks == m_lastRemainingBlocks )
            {
               DrawText ( "Okay lets retract piston to find remaining blocks..." );

               if ( RetractPiston ())
               {
                  m_internalState = EInternalState.Idle;
               }
            }

            //------------------------------------------------------------------
            // Checking timeout
            //------------------------------------------------------------------

            if ( m_projector.RemainingBlocks != m_lastRemainingBlocks )
            {
               DrawText ( "Progress in work, update block info..." );

               m_lastRemainingBlocks = m_projector.RemainingBlocks;

               m_lastAssemblingChange = m_dateTimeNow;
            }
            else
            {
               DrawText ( "No progress checking timeout..." );

               TimeSpan timeElapsedLastChange = m_dateTimeNow - m_lastAssemblingChange;

               if ( timeElapsedLastChange.Seconds > 60 )
               {
                  m_internalState = EInternalState.Idle;
               }
            }
         }
      }

      private void StateError ()
      {
         DrawText ( "State: Error" );

         ShowErrorLights ();

         for ( int nIndex = 0; nIndex < m_listError.Count; nIndex++ )
         {
            DrawText ( m_listError[nIndex] );
         }

         RetractPiston       ();
         ToggleOffShipWelder ();
      }

      //------------------------------------------------------------------------
      // Welder
      //------------------------------------------------------------------------

      private void ToggleOnShipWelder ()
      {
         for ( int nIndex = 0; nIndex < m_listShipWelder.Count; nIndex++ )
         {
            IMyShipWelder shipWelder = m_listShipWelder[nIndex];

            shipWelder.ApplyAction ( "OnOff_On" );
         }
      }

      private void ToggleOffShipWelder ()
      {
         for ( int nIndex = 0; nIndex < m_listShipWelder.Count; nIndex++ )
         {
            IMyShipWelder shipWelder = m_listShipWelder[nIndex];

            shipWelder.ApplyAction ( "OnOff_Off" );
         }
      }

      //------------------------------------------------------------------------
      // Piston
      //------------------------------------------------------------------------

      private bool ExtendPiston ()
      {
         DrawText ( "Extending pistons..." );

         bool bAllPistonsExtended = true;

         for ( int nIndex = 0; nIndex < m_listShipPiston.Count; nIndex++ )
         {
            IMyExtendedPistonBase piston = m_listShipPiston[nIndex];

            float distance = Math.Abs ( piston.HighestPosition - piston.CurrentPosition );

            if ( distance > 0.003 )
            {
               bAllPistonsExtended = false;

               piston.Velocity = m_pistonExtendVelocity;
            }
            else
            {
               piston.Velocity = 0.0f;
            }
         }

         return bAllPistonsExtended;
      }

      private bool RetractPiston ()
      {
         DrawText ( "Retracting pistons..." );

         bool bAllPistonsRetracted = true;

         for ( int nIndex = 0; nIndex < m_listShipPiston.Count; nIndex++ )
         {
            IMyExtendedPistonBase piston = m_listShipPiston[nIndex];

            float distance = Math.Abs ( piston.LowestPosition - piston.CurrentPosition );

            if ( distance > 0.003 )
            {
               bAllPistonsRetracted = false;

               piston.Velocity = m_pistonRetractVelocity;

               break;
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
         for ( int nIndex = 0; nIndex < m_listShipPiston.Count; nIndex++ )
         {
            IMyExtendedPistonBase piston = m_listShipPiston[nIndex];

            piston.Velocity = 0.0f;
         }
      }

      //------------------------------------------------------------------------
      // Lights
      //------------------------------------------------------------------------

      private void ShowIdleLights ()
      {
         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyMotorRotor rotor = m_listRotor[nIndex];

            rotor.Base.ApplyAction ( "OnOff_Off" );
         }

         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyLightingBlock warningLight = m_listWarningLight[nIndex];

            warningLight.Color = Color.White;

            warningLight.Enabled = true;
         }
      }

      private void ShowWarningLights ()
      {
         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyMotorRotor rotor = m_listRotor[nIndex];

            rotor.Base.ApplyAction ( "OnOff_On" );
         }

         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyLightingBlock warningLight = m_listWarningLight[nIndex];

            warningLight.Color = Color.Orange;

            warningLight.Enabled = true;
         }
      }

      private void ShowErrorLights ()
      {
         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyMotorRotor rotor = m_listRotor[nIndex];

            rotor.Base.ApplyAction ( "OnOff_On" );
         }

         for ( int nIndex = 0; nIndex < m_listRotor.Count; nIndex++ )
         {
            IMyLightingBlock warningLight = m_listWarningLight[nIndex];

            warningLight.Color = Color.Red;

            warningLight.Enabled = true;
         }
      }

      //------------------------------------------------------------------------
      // Helper
      //------------------------------------------------------------------------

      private bool GetBlock<T> ( String key, out T block ) where T : class
      {
         bool bFoundBlock = false;

         block = GridTerminalSystem.GetBlockWithName ( key ) as T;

         if ( block != null )
         {
            bFoundBlock = true;
         }
         else
         {
            //m_listError.Add ( "Cant find block with name <" + key + ">" );

            Echo ( "Cant find block with name <" + key + ">" );

            if ( m_textPanel != null )
            {
               DrawText ( "Cant find block with name <" + key + ">" );
            }
         }

         return bFoundBlock;
      }

      private void DrawText ( String text, bool bNewLine = true )
      {
         Echo ( text );

         if ( m_textPanel != null )
         {
            if ( bNewLine )
            {
               m_textPanel.WritePublicText ( text + Environment.NewLine, true );
            }
            else
            {
               m_textPanel.WritePublicText ( text, true );
            }
         }
      }
   }
}
