using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace space_engineers.Common
{
   class Snippets : MyGridProgram
   {
      IMyTextPanel m_textPanel;

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
