using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace space_engineers.Autobuild.GatlingTest
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      int shootCount = 0;

      Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Update100;
      }

      public void Main ( string argument, UpdateType updateSource )
      {
         IMyTextSurface surface = GridTerminalSystem.GetBlockWithName ( "LCD" ) as IMyTextSurface;

         if ( surface != null )
         {
            Echo ( "Surface panel found" );

            surface.ContentType = ContentType.SCRIPT;

            using ( var frame = surface.DrawFrame ())
            {
               MySprite test = MySprite.CreateText ( "Test", "Debug", new Color ( 1f ), 2f, TextAlignment.CENTER );

               frame.Add ( test );
            }
         }
         else
         {
            Echo("No surface panel found");
         }
      }
   }
}
