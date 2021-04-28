using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace space_engineers.Scripts.WelderShip
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      enum EInternalState
      {
         Idle                        ,
         SendWelderActiviationRequest,
         WaitForWelderActiviation    ,
         Assembling                  ,
         Error
      }

      //------------------------------------------------------------------------
      // Assembler2000 systems
      //------------------------------------------------------------------------

      private const String m_shipName     = "ship_id_assembler2000";
      private const String m_dockyardName = "ship_id_dockyard2000" ;

      private const String m_nameTextPanel     = "ship_textpanel_01"   ;
      private const String m_nameProjector     = "ship_projector_01"   ;
      private const String m_nameThrusterGroup = "ship_trustergroup_01";

      private IMyTextPanel    m_textPanel          ;
      private IMyTextPanel    m_logTextPanel       ;
      private IMyProjector    m_projector          ;
      private IMyRadioAntenna m_radioAntenna       ;
      private List<IMyThrust> m_listThrusterForward;

      private const float m_backThrust            = 133.0f; // Thrust of thruster in KN
      private const float m_backThrustPercent     =  0.05f; // Thrust of thruster in percent
      private const float m_maxDistancePerSecond  =   1.0f; // Shipspeed per second
      private const float m_distanceCheckInterval =   1.0f; // Speedcheck interval

      //------------------------------------------------------------------------
      // Internal assembling states
      //------------------------------------------------------------------------

      private String         m_argument     ;
      private UpdateType     m_updateSource ;
      private DateTime       m_dateTimeNow  ;
      private EInternalState m_internalState;

      private DateTime m_lastAssemblingChange;
      private int      m_lastRemainingBlocks ;

      private Vector3D m_currentPosition ;
      private Vector3D m_lastPosition    ;
      private DateTime m_lastPositionTime;

      private List<String> m_listError;

      //------------------------------------------------------------------------
      // Messaging and cryptography
      //------------------------------------------------------------------------

      private String m_alphabet  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_| "                           ;
      private String m_secretKey = "578421324877894231878613219875123123187532187683218646512184511657";

      private Dictionary<char, int> m_dictAlphabet = new Dictionary<char, int> ();

      //------------------------------------------------------------------------

      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update10;

         m_listThrusterForward = new List<IMyThrust> ();

         m_listError = new List<String> ();

         m_lastPosition = Me.CubeGrid.GridIntegerToWorld ( Me.Position );

         m_internalState = EInternalState.Idle;

         for ( int nIndex = 0; nIndex < m_alphabet.Length; nIndex++ )
         {
            m_dictAlphabet.Add ( m_alphabet  [nIndex], nIndex );
         }
      }

      //------------------------------------------------------------------------

      public void Main ( string argument, UpdateType updateSource )
      {
         if ( GetShipSystems ())
         {
            //------------------------------------------------------------------
            // Environment
            //------------------------------------------------------------------

            m_textPanel.WritePublicText ( "" );

            m_listError.Clear ();

            m_argument     = argument    ;
            m_updateSource = updateSource;
            m_dateTimeNow  = DateTime.Now;

            m_currentPosition = Me.CubeGrid.GridIntegerToWorld ( Me.Position );

            //------------------------------------------------------------------

            DrawText ( "//--------------------------------------------" );
            DrawText ( "// ASSEMBLER2000 BY VX TEK AUTOMATING SYSTEMS " );
            DrawText ( "//--------------------------------------------" );
            DrawText ( ""                                               );

            DrawText ( "Date " +m_dateTimeNow.ToShortDateString () +" "+m_dateTimeNow.ToLongTimeString ());

            DrawText ( "Current position  : " + m_currentPosition.X + ":" + m_currentPosition.Y +":" + m_currentPosition.Z );
            DrawText ( "Last position     : " + m_lastPosition   .X + ":" + m_lastPosition   .Y +":" + m_lastPosition   .Z );

            DrawText ( "Current ship speed: " + Vector3D.Distance ( m_lastPosition, m_currentPosition ));

            //------------------------------------------------------------------
            // State machine
            //------------------------------------------------------------------

            switch ( m_internalState )
            {
               case EInternalState.Idle       : { StateIdle       (); } break;
               case EInternalState.Assembling : { StateAssembling (); } break;
               case EInternalState.Error      : { StateAssembling (); } break;
            }
         }
         else
         {
            m_internalState = EInternalState.Error;
         }
      }

      //------------------------------------------------------------------------

      private bool GetShipSystems ()
      {
         bool bInitShipSystemOk = false;

         if ( GetBlock<IMyTextPanel> ( m_nameTextPanel, out m_textPanel ))
         {
            if ( GetBlock<IMyProjector> ( m_nameProjector, out m_projector ))
            {
               IMyBlockGroup blockGroup = GridTerminalSystem.GetBlockGroupWithName ( m_nameThrusterGroup );

               List<IMyTerminalBlock> listTerminalBlocks = new List<IMyTerminalBlock> ();

               blockGroup.GetBlocks ( listTerminalBlocks );

               for ( int nIndex = 0; nIndex < listTerminalBlocks.Count; nIndex++ )
               {
                  IMyThrust thrust = listTerminalBlocks[nIndex] as IMyThrust;

                  if ( thrust !=null )
                  {
                     m_listThrusterForward.Add ( thrust );
                  }
               }

               if ( m_listThrusterForward.Count == 0 )
               {
                  m_listError.Add ( "Cant find blockgroup with name <" + m_nameThrusterGroup + ">" );

                  Echo ( "Cant find blockgroup with name <" + m_nameThrusterGroup + ">" );

                  if ( m_textPanel != null )
                  {
                     DrawText ( "Cant find blockgroup with name <" + m_nameThrusterGroup + ">" );
                  }
               }
               else
               {
                  bInitShipSystemOk = true;
               }
            }
         }

         return bInitShipSystemOk;
      }

      //------------------------------------------------------------------------

      private void StateIdle ()
      {
         DrawText ( "State: Idle" );

         if (( m_updateSource == UpdateType.Trigger ) && ( m_argument == "STARTASSEMBLING" ))
         {
            m_lastAssemblingChange = m_dateTimeNow              ;
            m_lastRemainingBlocks  = m_projector.RemainingBlocks;

            m_lastPosition = Me.CubeGrid.GridIntegerToWorld ( Me.Position );

            m_lastPositionTime = m_dateTimeNow;

            m_internalState = EInternalState.Assembling;
         }
         else
         {
            SetThrusterOverride ( 0.0f );
         }
      }

      private void StateAssembling ()
      {
         DrawText ( "State: Assembling" );

         DrawText ( "RemainingBlocks: " + m_projector.RemainingBlocks );

         if ( m_projector.RemainingBlocks == 0 )
         {
            SetThrusterOverride ( 0.0f );

            m_internalState = EInternalState.Idle;
         }
         else
         {
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
               SetThrusterOverride ( 0.0f );

               m_lastAssemblingChange = m_dateTimeNow;
            }
            else
            {
               DrawText ( "XXXXXX" );

               if ( m_projector.RemainingBlocks != m_lastRemainingBlocks )
               {
                  m_lastRemainingBlocks = m_projector.RemainingBlocks;

                  m_lastAssemblingChange = m_dateTimeNow;
               }

               //---------------------------------------------------------------
               // Check speed
               //---------------------------------------------------------------

               double distance = Vector3D.Distance ( m_currentPosition, m_lastPosition );

               DrawText ( "XXXXXX" + distance );

               TimeSpan timeElapsed = m_dateTimeNow - m_lastPositionTime;

               if ( timeElapsed.TotalSeconds >= m_distanceCheckInterval )
               {
                  if ( distance > m_maxDistancePerSecond )
                  {
                     SetThrusterOverride ( 0.0f );
                  }
                  else
                  {
                     SetThrusterOverride ( m_backThrustPercent );
                  }

                  m_lastPosition     = m_currentPosition;
                  m_lastPositionTime = m_dateTimeNow    ;
               }
            }

            //------------------------------------------------------------------
            // Checking progress
            //------------------------------------------------------------------

            TimeSpan timeElapsedLastChange = m_dateTimeNow - m_lastAssemblingChange;

            if ( timeElapsedLastChange.Seconds > 60 )
            {
               m_listError.Add ( "No progress, sthudown engines and go in error state" );

               SetThrusterOverride ( 0.0f );

               m_internalState = EInternalState.Error;
            }
         }
      }

      private void StateErorr ()
      {
         DrawText ( "State: Error" );

         for ( int nIndex = 0; nIndex < m_listError.Count; nIndex++ )
         {
            DrawText ( m_listError[nIndex] );
         }
      }

      private void StateSendWelderActiviationRequest ()
      {
         // sender|receiver|signature|signaturevalue|messagetype

         String message = m_shipName+"|"+m_dockyardName+"|0|0|ACTIVATEWELDER".ToUpper ();

         TransmitMessage ( message );

         m_internalState = EInternalState.WaitForWelderActiviation;
      }

      private void StateWaitForWelderActiviation ()
      {
         DrawText ( "State: WaitForWelderActiviation" );

         if ( m_updateSource == UpdateType.Terminal )
         {
            String message = ReceiveMessage ( m_argument );

            List<String> listSplittedMessage = message.Split ( '|' ).ToList ();

            // sender|receiver|signature|signaturevalue|messagetype

            if ( listSplittedMessage.Count == 5 )
            {
               String senderId       = listSplittedMessage[0];
               String receiverId     = listSplittedMessage[1];
               String signature      = listSplittedMessage[2];
               String signatureValue = listSplittedMessage[3];
               String messageType    = listSplittedMessage[4];

               if (( receiverId == m_shipName ) && ( senderId == m_dockyardName ))
               {
                  if ( messageType == "WELDERON" )
                  {
                     m_internalState = EInternalState.Assembling;
                  }
               }
            }
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
            m_listError.Add ( "Cant find block with name <" + key + ">" );

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

      private void SetThrusterOverride ( float thrustOverride )
      {
         for ( int nIndex = 0; nIndex < m_listThrusterForward.Count; nIndex++ )
         {
            m_listThrusterForward[nIndex].ThrustOverridePercentage = thrustOverride;
         }
      }

      private void TransmitMessage ( String plainMessage )
      {
         int[] shiftBuffer = GetShiftBuffer ( plainMessage, m_secretKey );

         //m_radioAntenna.TransmitMessage ( CaesarEncrypt ( plainMessage, shiftBuffer ));
      }

      private String ReceiveMessage ( String encryptedMessage )
      {
         int[] shiftBuffer = GetShiftBuffer ( m_argument, m_secretKey );

         return CaesarDecrypt ( m_argument, shiftBuffer );
      }

      //------------------------------------------------------------------------
      // Cryptography
      //------------------------------------------------------------------------

      private String CaesarEncrypt ( String plainText, int[] shiftBuffer )
      {
         StringBuilder encryptedText = new StringBuilder ( plainText.Length );

         for ( int nIndex = 0; nIndex < plainText.Length; nIndex++ )
         {
            char c = plainText[nIndex];

            int nCharIndex = m_dictAlphabet[c];

            int nEncryptedIndex = ( nCharIndex + shiftBuffer[nIndex] ) % m_alphabet.Length;

            encryptedText.Append ( m_alphabet[nEncryptedIndex] );
         }

         return encryptedText.ToString ();
      }

      private String CaesarDecrypt ( String encryptedText, int[] shiftBuffer )
      {
         StringBuilder plainText = new StringBuilder ( encryptedText.Length );

         for ( int nIndex = 0; nIndex < encryptedText.Length; nIndex++ )
         {
            char c = encryptedText[nIndex];

            int nCharIndex = m_dictAlphabet[c];

            int nEncryptedIndex = ( nCharIndex - shiftBuffer[nIndex] ) % m_alphabet.Length;

            if ( nEncryptedIndex < 0 )
            {
               nEncryptedIndex = m_alphabet.Length + nEncryptedIndex;
            }

            plainText.Append ( m_alphabet[nEncryptedIndex] );
         }

         return plainText.ToString ();
      }

      private int[] GetShiftBuffer ( String plainText, String secretKey )
      {
         int[] shiftBuffer = new int[plainText.Length];

         for ( int nIndex = 0; nIndex < plainText.Length; nIndex++ )
         {
            if ( nIndex < secretKey.Length )
            {
               int shift;

               int.TryParse ( secretKey[nIndex] + "", out shift );

               shiftBuffer[nIndex] = shift;
            }
            else
            {
               char c = plainText[nIndex];

               if ( m_dictAlphabet.ContainsKey ( c ))
               {
                  shiftBuffer[nIndex] = m_dictAlphabet[c];
               }
               else
               {
                  shiftBuffer[nIndex] = 0;
               }
            }
         }

         return shiftBuffer;
      }
   }
}
