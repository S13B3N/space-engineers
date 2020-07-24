using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
   partial class Program : MyGridProgram
   {
      public Program ()
      {
         Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update10;
      }

      public void Save ()
      {
      }
      public void Main ( string argument, UpdateType updateSource )
      {
         Panic ();
      }

      public void Panic ()
      {
         IMyTimerBlock timer = ( IMyTimerBlock ) GridTerminalSystem.GetBlockWithName ( "TimerT" );

         if ( timer != null )
         {
            if ( timer.IsCountingDown )
            {
               Engage ();
            }
            else
            {
               Idle ();
            }
         }
      }

      private void Engage ()
      {
         List<IMyThrust> listOfThrust = new List<IMyThrust> ();

         GridTerminalSystem.GetBlocksOfType<IMyThrust> ( listOfThrust );

         foreach ( IMyThrust thrust in listOfThrust )
         {
            if ( thrust.CustomName.ToUpper ().Contains ( "UP" ))
            {
               thrust.ThrustOverride = thrust.MaxThrust;
            }
         }

         //---------------------------------------------------------------------

         List<IMyCockpit> listOfCockpit = new List<IMyCockpit> ();

         GridTerminalSystem.GetBlocksOfType<IMyCockpit> ( listOfCockpit );

         foreach ( IMyCockpit cockpit in listOfCockpit )
         {
            cockpit.DampenersOverride = false;
         }

         //---------------------------------------------------------------------

         List<IMyLandingGear> listOfLandingGear = new List<IMyLandingGear> ();

         GridTerminalSystem.GetBlocksOfType<IMyLandingGear> ( listOfLandingGear );

         foreach ( IMyLandingGear landingGear in listOfLandingGear )
         {
            landingGear.AutoLock = false;

            landingGear.Unlock ();
         }
      }

      private void Idle ()
      {
         List<IMyThrust> listOfThrust = new List<IMyThrust> ();

         GridTerminalSystem.GetBlocksOfType<IMyThrust> ( listOfThrust );

         foreach ( IMyThrust thrust in listOfThrust )
         {
            if ( thrust.CustomName.ToUpper ().Contains ( "UP" ))
            {
               thrust.ThrustOverride = 0.0f;
            }
         }

         //---------------------------------------------------------------------

         List<IMyCockpit> listOfCockpit = new List<IMyCockpit> ();

         GridTerminalSystem.GetBlocksOfType<IMyCockpit> ( listOfCockpit );

         foreach ( IMyCockpit cockpit in listOfCockpit )
         {
            cockpit.DampenersOverride = true;
         }

         //---------------------------------------------------------------------

         List<IMyLandingGear> listOfLandingGear = new List<IMyLandingGear> ();

         GridTerminalSystem.GetBlocksOfType<IMyLandingGear> ( listOfLandingGear );

         foreach ( IMyLandingGear landingGear in listOfLandingGear )
         {
            landingGear.AutoLock = true;
         }
      }
   }
}
