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
            Echo ( "Cant find block with name <" + key + ">" );

            if ( textPanel != null )
            {
               //DrawText ( textPanel, "Cant find block with name <" + key + ">" );
            }
         }

         return bFoundBlock;
      }

      private void DrawText ( String text, bool bNewLine = true )
      {
         if ( bNewLine )
         {
            Echo ( text + Environment.NewLine );
         }
         else
         {
            Echo ( text );
         }

         //if ( m_textPanel != null )
         //{
         //   m_textPanel.WritePublicText ( text + Environment.NewLine, bNewLine );
         //}
      }
   }}
