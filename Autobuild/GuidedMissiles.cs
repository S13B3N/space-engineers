using Sandbox.ModAPI.Ingame;
using space_engineers.Interface;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace space_engineers.Autobuild.GuidedMissiles
{
   public class Program : MyGridProgram, ISmallMainProgram
   {
      string[] missileTagList = {
         "#A#",
         "#B#",
         "#C#",
         "#D#",
         "#E#",
         "#F#",
         "#G#",
         "#H#",
      };

      int m_defaultShootCount = 1;

      IMyProjector m_projector = null;

      List<IMyShipWelder> m_welderList = new List<IMyShipWelder> ();

      List<Missile> m_missilesAvailable = new List<Missile>();

      public List<Missile> m_activeAvailable = new List<Missile>();

      int shootCountdown = 0;

      bool fireMissiles = false;

      public double ThisShipSize = 10;

      double LaunchDist = 15;

      public IMyCameraBlock m_TOWCamera;

      public IMyShipController m_RC;

      public double m_globalTimestep = 0.016;

      public double m_PNGain = 3;

      MissileControl m_missileControl;

      public Program ()
      {
         m_missileControl = new MissileControl ( this );

         List<IMyTerminalBlock> TempCollection3 = new List<IMyTerminalBlock>();

         //GridTerminalSystem.GetBlocksOfType<IMyShipController>(TempCollection3, a => a.DetailedInfo != "NoUse");
         GridTerminalSystem.GetBlocksOfType<IMyShipController> ( TempCollection3 );
         if ( TempCollection3.Count > 0 )
         {
            m_RC = TempCollection3[0] as IMyShipController;
         }

         ThisShipSize = ( Me.CubeGrid.WorldVolume.Radius );

         ThisShipSize = LaunchDist == 0 ? ThisShipSize : LaunchDist;

         Runtime.UpdateFrequency = UpdateFrequency.Update1;
      }

      public void Main ( String argument, UpdateType updateSource )
      {
         Echo ( "Next gen guided missiles" );

         InitStationSystem ();

         GetMissiles ();

         foreach ( Missile missile in m_missilesAvailable )
         {
            Echo ( "Missile:" + missile.IsFunctional ());

            missile.ThrusterInfo ();

            missile.GetForwardVector ();
         }

         Echo ( string.Format ( "{0} missile available and functional", m_missilesAvailable.Count ));
         Echo ( string.Format ( "{0} missile active and fighting"     , m_activeAvailable  .Count ));

         if ( argument == "Fire" )
         {
            shootCountdown = m_defaultShootCount;

            fireMissiles = true;

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
         }
         else if ( fireMissiles )
         {
            if ( 0 < shootCountdown )
            {
               DestroyConveyorWithGatling ();

               shootCountdown--;
            }
            else
            {
               Runtime.UpdateFrequency = UpdateFrequency.Update1;

               Echo ( "Fire" );

               foreach ( Missile missile in m_missilesAvailable )
               {
                  Echo ( "Missile:" + missile.m_missileTag );

                  missile.PrepareForLaunch ();

                  missile.Launch ();

                  DestroyConveyorWithGatling ();

                  m_activeAvailable.Add ( missile );
               }

               foreach ( Missile missile in m_activeAvailable )
               {
                  m_missilesAvailable.Remove ( missile );
               }

               fireMissiles = false;

               //Stop ();
            }
         }

         bool bMissileInDangerRange = false;

         foreach ( Missile missile in m_activeAvailable )
         {
            double distance = ( m_projector.GetPosition () - missile.GetPosition ()).Length ();

            if ( distance < 100 )
            {
               bMissileInDangerRange = true;
            }
         }

         if ( fireMissiles || bMissileInDangerRange )
         {
            ToggleShipSystem ( false );
         }
         else
         {
            ToggleShipSystem ( true );
         }

         m_missileControl.Control ();
      }

      private void InitStationSystem ()
      {
         //m_projector = ( IMyProjector ) GridTerminalSystem.GetBlockWithName ( "#MissileProjector#" );

         List<IMyProjector> projectorList = new List<IMyProjector> ();

         GridTerminalSystem.GetBlocksOfType ( projectorList );

         m_projector = projectorList[0];

         GridTerminalSystem.GetBlocksOfType ( m_welderList );
      }

      private void ToggleShipSystem ( bool enabled )
      {
         foreach ( IMyShipWelder welder in m_welderList )
         {
            welder.Enabled = enabled;
         }

         m_projector.Enabled = enabled;
      }

      private void DestroyConveyorWithGatling ()
      {
         List<IMySmallGatlingGun> gatlingGunList = new List<IMySmallGatlingGun> ();

         GridTerminalSystem.GetBlocksOfType ( gatlingGunList );

         foreach ( IMySmallGatlingGun gatlingGun in gatlingGunList )
         {
            gatlingGun.ApplyAction ( "ShootOnce" );
         }

         //List<IMyShipMergeBlock> mergeBlockList = new List<IMyShipMergeBlock> ();

         //GridTerminalSystem.GetBlocksOfType ( mergeBlockList );

         //foreach ( IMyShipMergeBlock mergeBlock in mergeBlockList )
         //{
         //   mergeBlock.Enabled = false;
         //}
      }

      private void GetMissiles ()
      {
         m_missilesAvailable.Clear ();

         foreach ( string missileTag in missileTagList )
         {
            Missile missile = new Missile ( missileTag, this );

            missile.Init ( GridTerminalSystem );

            if ( missile.IsFunctional ())
            {
               m_missilesAvailable.Add ( missile );
            }
         }
      }

      public void Stop ()
      {
         throw new Exception ( "Stop" );
      }
   }

   public class Missile
   {
      public IMyGyro            m_gyroBlock         = null;
      public IMyLargeTurretBase m_remoteTurretBlock = null;
      public IMyShipMergeBlock  m_mergeBlock        = null;

      public List<IMyPowerProducer> m_powerBlock        = new List<IMyPowerProducer> ();
      public List<IMyThrust>        m_thrusterBlockList = new List<IMyThrust> ();
      public List<IMyTerminalBlock> m_warheadBlockList  = new List<IMyTerminalBlock> ();

      public Program m_program;

      public double MissileAccel  = 10;
      public double MissileMass   = 0;
      public double MissileThrust = 0;
      public bool   IsLargeGrid   = false;
      public double FuseDistance  = 7;

      public bool HAS_DETACHED = false;
      public bool IS_CLEAR     = false;

      public Vector3D TARGET_PREV_POS = new Vector3D();
      public Vector3D MIS_PREV_POS    = new Vector3D();

      public double PREV_Yaw = 0;
      public double PREV_Pitch = 0;

      public string m_missileTag = "";

      public string status = "";

      public Missile ( string missileTag, Program program )
      {
         m_missileTag = missileTag;

         m_program = program;
      }

      public void Init ( IMyGridTerminalSystem gridTerminalSystem )
      {
         List<IMyTerminalBlock> blockList = new List<IMyTerminalBlock> ();

         gridTerminalSystem.GetBlocksOfType<IMyTerminalBlock> ( blockList, searchItem => searchItem.CustomName.Contains ( m_missileTag ));

         foreach ( IMyTerminalBlock block in blockList )
         {
            if (( block is IMyGyro ) && ( m_gyroBlock == null ))
            {
               m_gyroBlock = ( IMyGyro ) block;
            }
            else if (( block is IMyLargeTurretBase ) && ( m_remoteTurretBlock == null ))
            {
               //m_remoteTurretBlock = ( IMyLargeTurretBase ) block;
            }
            else if (( block is IMyShipMergeBlock ) && ( m_mergeBlock == null ))
            {
               m_mergeBlock = ( IMyShipMergeBlock ) block;
            }
            else if ( block is IMyPowerProducer )
            {
               m_powerBlock.Add (( IMyPowerProducer ) block );
            }
            else if ( block is IMyThrust )
            {
               m_thrusterBlockList.Add (( IMyThrust ) block );
            }
            else if ( block is IMyWarhead )
            {
               m_warheadBlockList.Add (( IMyWarhead ) block );
            }
         }
      }

      public void ThrusterInfo ()
      {
         foreach ( IMyThrust thruster in m_thrusterBlockList )
         {
            //m_program.Echo ( string.Format ( "{0}:{1}:{2}", thruster.WorldMatrix.Forward.X, thruster.WorldMatrix.Forward.Y, thruster.WorldMatrix.Forward.Z ));
         }
      }

      public void PrepareForLaunch ()
      {
         m_program.Echo ( "Power on" );

         foreach ( IMyPowerProducer powerBlock in m_powerBlock )
         {
            powerBlock.Enabled = true;

            powerBlock.ApplyAction ( "OnOff_On" );
         }

         m_program.Echo ( "Thruster on" );

         Vector3D forwardVector = GetForwardVector ();

         double maxThrust = 0;

         int count = 0;

         foreach ( IMyThrust thruster in m_thrusterBlockList )
         {
            thruster.Enabled = true;

            if ( forwardVector == thruster.WorldMatrix.Forward )
            {
               thruster.ThrustOverride = thruster.MaxThrust;

               maxThrust = thruster.MaxThrust;

               count++;
            }
         }

         m_program.Echo ( string.Format ( "{1} Thrusters on maxthrust {0}", maxThrust, count ));

         m_program.Echo ( "Warhead on" );

         foreach ( IMyWarhead warhead in m_warheadBlockList )
         {
            warhead.ApplyAction ( "StartCountdown" );
         }
      }

      public void Launch ()
      {
         m_mergeBlock.Enabled = false;
      }

      public bool IsFunctional ()
      {
         bool HasGyro   = ( m_gyroBlock         != null );
         bool HasTurret = ( m_remoteTurretBlock != null );
         bool HasMerge  = ( m_mergeBlock        != null );
         bool HasPower  = ( m_powerBlock        != null );

         bool HasThruster = m_thrusterBlockList.Count > 0;
         bool HasWarheads = m_warheadBlockList .Count > 0;

         //m_program.Echo ( ""+ HasGyro + HasThruster + HasMerge + HasWarheads );

         return ( HasGyro && HasThruster && HasMerge && HasWarheads );
         //return HasGyro && HasTurret && HasMerge && HasPower && HasThruster && HasWarheads;
      }

      public void EchoSystemDetails ()
      {
         if ( IsFunctional ())
         {
            m_program.Echo ( "Missile is funcitonal" );
         }
         else
         {
            m_program.Echo ( "Missile is not funcitonal" );
         }

         m_program.Echo ( "Thrusters:"+m_thrusterBlockList.Count );
         m_program.Echo ( "Warheads:"+m_warheadBlockList.Count );
      }

      public Vector3D GetPosition ()
      {
         return m_gyroBlock.GetPosition ();
      }

      public Vector3D GetForwardVector ()
      {
         Dictionary<Vector3D, ForwardVectorInfo> vectorDict = new Dictionary<Vector3D, ForwardVectorInfo> ();

         foreach ( IMyThrust thruster in m_thrusterBlockList )
         {
            Vector3D forwardVector = thruster.WorldMatrix.Forward;

            double maxThrust = thruster.MaxEffectiveThrust;

            if ( vectorDict.ContainsKey ( forwardVector ) == false )
            {
               vectorDict.Add ( forwardVector, new ForwardVectorInfo ( 1, maxThrust ));
            }
            else
            {
               vectorDict[forwardVector].Add ( 1, maxThrust );
            }
         }

         List<KeyValuePair<Vector3D, ForwardVectorInfo>> vectorList = vectorDict.ToList ();

         vectorList.Sort (( item1, item2 ) => item2.Value.m_thrust.CompareTo ( item1.Value.m_thrust ));

         foreach ( var itemX in vectorList )
         {
            m_program.Echo ( "X-"+itemX.Value.m_count );
         }

         return vectorList[0].Key;
      }
   }


   public class MissileControl
   {
      Program m_program;

      double TOW_Distance = 3000;

      bool OverrideToLongTargets = false;

      MyDetectedEntityInfo TemporaryTarget;

      public MissileControl ( Program program )
      {
         m_program = program;
      }

      public void Control ()
      {
         List<Missile> abandonedMissiles = new List<Missile> ();

         foreach ( Missile missile in m_program.m_activeAvailable )
         {
            if ( missile.IS_CLEAR == true )
            {
               Guidance ( missile );
            }
            else if ( missile.IS_CLEAR == false )
            {
               if (( missile.m_gyroBlock.GetPosition () - m_program.Me.GetPosition ()).Length () > m_program.ThisShipSize )
               {
                  missile.IS_CLEAR = true;
               }
            }

            //Disposes If Out Of Range Or Destroyed (misses a beat on one missile)

            bool Isgyroout = missile.m_gyroBlock.CubeGrid.GetCubeBlock ( missile.m_gyroBlock.Position ) == null;

            bool Isthrusterout = missile.m_thrusterBlockList[0].CubeGrid.GetCubeBlock ( missile.m_thrusterBlockList[0].Position ) == null;

            bool Isouttarange = ( missile.m_gyroBlock.GetPosition () - m_program.Me.GetPosition ()).LengthSquared () > 9000 * 9000;

            if ( Isgyroout || Isthrusterout || Isouttarange )
            {
               abandonedMissiles.Add ( missile );
            }
         }

         foreach ( Missile missile in abandonedMissiles )
         {
            m_program.m_activeAvailable.Remove ( missile );
         }
      }

      void Guidance ( Missile missile )
      {

         //Finds Current Target
         Vector3D ENEMY_POS = EnemyScan ( missile );

         //---------------------------------------------------------------------------------------------------------------------------------

         //Sorts CurrentVelocities
         Vector3D MissilePosition = missile.m_gyroBlock.CubeGrid.WorldVolume.Center;
         Vector3D MissilePositionPrev = missile.MIS_PREV_POS;
         Vector3D MissileVelocity = ( MissilePosition - MissilePositionPrev ) / m_program.m_globalTimestep;

         Vector3D TargetPosition = ENEMY_POS;
         Vector3D TargetPositionPrev = missile.TARGET_PREV_POS;
         Vector3D TargetVelocity = (TargetPosition - missile.TARGET_PREV_POS) / m_program.m_globalTimestep;

         //Uses RdavNav Navigation APN Guidance System
         //-----------------------------------------------

         //Setup LOS rates and PN system
         Vector3D LOS_Old = Vector3D.Normalize(TargetPositionPrev - MissilePositionPrev);
         Vector3D LOS_New = Vector3D.Normalize(TargetPosition - MissilePosition);
         Vector3D Rel_Vel = Vector3D.Normalize(TargetVelocity - MissileVelocity);

         //And Assigners
         Vector3D am = new Vector3D(1, 0, 0); double LOS_Rate; Vector3D LOS_Delta;
         Vector3D MissileForwards = missile.m_thrusterBlockList[0].WorldMatrix.Backward;

         //Vector/Rotation Rates
         if (LOS_Old.Length() == 0)
         { LOS_Delta = new Vector3D(0, 0, 0); LOS_Rate = 0.0; }
         else
         { LOS_Delta = LOS_New - LOS_Old; LOS_Rate = LOS_Delta.Length() / m_program.m_globalTimestep; }

         //-----------------------------------------------

         //Closing Velocity
         double Vclosing = (TargetVelocity - MissileVelocity).Length();

         //If Under Gravity Use Gravitational Accel
         Vector3D GravityComp = -m_program.m_RC.GetNaturalGravity();

         //Calculate the final lateral acceleration
         Vector3D LateralDirection = Vector3D.Normalize(Vector3D.Cross(Vector3D.Cross(Rel_Vel, LOS_New), Rel_Vel));
         Vector3D LateralAccelerationComponent = LateralDirection * m_program.m_PNGain * LOS_Rate * Vclosing + LOS_Delta * 9.8 * (0.5 * m_program.m_PNGain); //Eases Onto Target Collision LOS_Delta * 9.8 * (0.5 * Gain)

         //If Impossible Solution (ie maxes turn rate) Use Drift Cancelling For Minimum T
         double OversteerReqt = (LateralAccelerationComponent).Length() / missile.MissileAccel;
         if (OversteerReqt > 0.98)
         {
            LateralAccelerationComponent = missile.MissileAccel * Vector3D.Normalize(LateralAccelerationComponent + (OversteerReqt * Vector3D.Normalize(-MissileVelocity)) * 40);
         }

         //Calculates And Applies Thrust In Correct Direction (Performs own inequality check)
         double ThrustPower = RdavUtils.Vector_Projection_Scalar(MissileForwards, Vector3D.Normalize(LateralAccelerationComponent)); //TESTTESTTEST
         ThrustPower = missile.IsLargeGrid ? MathHelper.Clamp(ThrustPower, 0.9, 1) : ThrustPower;

         ThrustPower = MathHelper.Clamp(ThrustPower, 0.4, 1); //for improved thrust performance on the get-go
         foreach (IMyThrust thruster in missile.m_thrusterBlockList)
         {
            if (thruster.ThrustOverride != (thruster.MaxThrust * ThrustPower)) //12 increment inequality to help conserve on performance
            { thruster.ThrustOverride = (float)(thruster.MaxThrust * ThrustPower); }
         }

         //Calculates Remaining Force Component And Adds Along LOS
         double RejectedAccel = Math.Sqrt(missile.MissileAccel * missile.MissileAccel - LateralAccelerationComponent.LengthSquared()); //Accel has to be determined whichever way you slice it
         if (double.IsNaN(RejectedAccel)) { RejectedAccel = 0; }
         LateralAccelerationComponent = LateralAccelerationComponent + LOS_New * RejectedAccel;

         //-----------------------------------------------

         //Guides To Target Using Gyros
         am = Vector3D.Normalize(LateralAccelerationComponent + GravityComp);
         double Yaw; double Pitch;
         GyroTurn6(am, 18, 0.3, missile.m_thrusterBlockList[0], missile.m_gyroBlock as IMyGyro, missile.PREV_Yaw, missile.PREV_Pitch, out Pitch, out Yaw);

         //Updates For Next Tick Round
         missile.TARGET_PREV_POS = TargetPosition;
         missile.MIS_PREV_POS = MissilePosition;
         missile.PREV_Yaw = Yaw;
         missile.PREV_Pitch = Pitch;

         //Detonates warheads in close proximity
         if ((TargetPosition - MissilePosition).LengthSquared() < 20 * 20 && missile.m_warheadBlockList.Count > 0) //Arms
         { foreach (var item in missile.m_warheadBlockList) { (item as IMyWarhead).IsArmed = true; } }
         if ((TargetPosition - MissilePosition).LengthSquared() < missile.FuseDistance * missile.FuseDistance && missile.m_warheadBlockList.Count > 0) //A mighty earth shattering kaboom
         { (missile.m_warheadBlockList[0] as IMyWarhead).Detonate(); }
      }

      Vector3D EnemyScan ( Missile missile )
      {
         // Guard clause
         if ( true || missile.m_remoteTurretBlock == null )
         {
            return m_program.m_RC.GetPosition () + m_program.m_RC.WorldMatrix.Forward * (( missile.m_gyroBlock.GetPosition () - m_program.Me.GetPosition ()).Length () + 300 );
         }

         var This_Missile_Director = missile.m_remoteTurretBlock as IMyLargeTurretBase;

         var ENEMY_POS = new Vector3D ();

         if ( m_program.m_TOWCamera != null )
         {
            Vector3D scanpos = m_program.m_RC.WorldMatrix.Forward * TOW_Distance;

            if ( m_program.m_TOWCamera.CanScan ( scanpos ))
            {
               TemporaryTarget = new MyDetectedEntityInfo();

               var ThisEntity = m_program.m_TOWCamera.Raycast ( scanpos );

               if ( ThisEntity.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies )
               {
                  TemporaryTarget = ThisEntity;

                  ENEMY_POS = ( Vector3D ) TemporaryTarget.HitPosition;
               }
            }
         }
         else if ( This_Missile_Director.GetTargetedEntity ().IsEmpty () == false && ! ( OverrideToLongTargets && TemporaryTarget.IsEmpty ()))
         {
            ENEMY_POS = This_Missile_Director.GetTargetedEntity ().Position;
         }
         else
         {
            if ( !TemporaryTarget.IsEmpty())
            {
               double TimeSinceFired = ( TemporaryTarget.TimeStamp - DateTime.Now.Millisecond ) / 1000.00;

               Vector3D Velocity = ( Vector3D )TemporaryTarget.Velocity;

               ENEMY_POS = ( Vector3D ) TemporaryTarget.HitPosition + Velocity * TimeSinceFired;
            }
            else
            {
               ENEMY_POS = m_program.m_RC.GetPosition () + m_program.m_RC.WorldMatrix.Forward * (( missile.m_gyroBlock.GetPosition () - m_program.Me.GetPosition ()).Length () + 300 );
            }
         }

         return ENEMY_POS;
      }

      void GyroTurn6 ( Vector3D TARGETVECTOR, double GAIN, double DAMPINGGAIN,IMyTerminalBlock REF, IMyGyro GYRO, double YawPrev, double PitchPrev, out double NewPitch, out double NewYaw )
      {
         //Pre Setting Factors
         NewYaw = 0;
         NewPitch = 0;

         //Retrieving Forwards And Up
         Vector3D ShipUp = REF.WorldMatrix.Up;
         Vector3D ShipForward = REF.WorldMatrix.Backward; //Backward for thrusters

         //Create And Use Inverse Quatinion                   
         Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
         var InvQuat = Quaternion.Inverse(Quat_Two);

         Vector3D DirectionVector = TARGETVECTOR; //RealWorld Target Vector
         Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

         //Convert To Local Azimuth And Elevation
         double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
         Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out  ShipForwardAzimuth, out ShipForwardElevation);

         //Post Setting Factors
         NewYaw = ShipForwardAzimuth;
         NewPitch = ShipForwardElevation;

         //Applies Some PID Damping
         ShipForwardAzimuth = ShipForwardAzimuth + DAMPINGGAIN * ((ShipForwardAzimuth - YawPrev) / m_program.m_globalTimestep );
         ShipForwardElevation = ShipForwardElevation + DAMPINGGAIN * ((ShipForwardElevation - PitchPrev) / m_program.m_globalTimestep );

         //Does Some Rotations To Provide For any Gyro-Orientation
         var REF_Matrix = MatrixD.CreateWorld(REF.GetPosition(), (Vector3)ShipForward, (Vector3)ShipUp).GetOrientation();
         var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, 0)), REF_Matrix); //Converts To World
         var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(GYRO.WorldMatrix.GetOrientation()));  //Converts To Gyro Local

         //Logic Checks for NaN's
         if (double.IsNaN(TRANS_VECT.X) || double.IsNaN(TRANS_VECT.Y) || double.IsNaN(TRANS_VECT.Z))
         { return; }

         //Applies To Scenario
         GYRO.Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X) * GAIN, -1000, 1000);
         GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * GAIN, -1000, 1000);
         GYRO.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z)) * GAIN, -1000, 1000);
         GYRO.GyroOverride = true;
      }
   }

   public class ForwardVectorInfo
   {
      public int m_count;
      public double m_thrust;

      public ForwardVectorInfo ( int count, double thrust )
      {
         m_count = count;
         m_thrust = thrust;
      }

      public void Add ( int count, double thrust )
      {
         m_count += count;
         m_thrust += thrust;
      }
   }

   public class RdavUtils
   {

      //Use For Solutions To Quadratic Equation
      public static bool Quadractic_Solv(double a, double b, double c, out double X1, out double X2)
      {
         //Default Values
         X1 = 0;
         X2 = 0;

         //Discrim Check
         Double Discr = b * b - 4 * c;
         if (Discr < 0)
         { return false; }

         //Calcs Values
         else
         {
            X1 = (-b + Math.Sqrt(Discr)) / (2 * a);
            X2 = (-b - Math.Sqrt(Discr)) / (2 * a);
         }
         return true;
      }

      //Handles Calculation Of Area Of Diameter
      public static double CalculateArea(double OuterDiam, double InnerDiam)
      {
         //Handles Calculation Of Area Of Diameter
         //=========================================
         double PI = 3.14159;
         double Output = ((OuterDiam * OuterDiam * PI) / 4) - ((InnerDiam * InnerDiam * PI) / 4);
         return Output;
      }

      //Use For Magnitudes Of Vectors In Directions (0-IN.length)
      public static double Vector_Projection_Scalar(Vector3D IN, Vector3D Axis_norm)
      {
         double OUT = 0;
         OUT = Vector3D.Dot(IN, Axis_norm);
         if (OUT  == double.NaN)
         { OUT = 0; }
         return OUT;
      }

      //Use For Vector Components, Axis Normalized, In not (vector 0 - in.length)
      public static Vector3D Vector_Projection_Vector(Vector3D IN, Vector3D Axis_norm)
      {
         Vector3D OUT = new Vector3D();
         OUT = Vector3D.Dot(IN, Axis_norm) * Axis_norm;
         if (OUT + "" == "NaN")
         { OUT = new Vector3D(); ; }
         return OUT;
      }

      //Use For Intersections Of A Sphere And Ray
      public static bool SphereIntersect_Solv(BoundingSphereD Sphere, Vector3D LineStart, Vector3D LineDirection, out Vector3D Point1, out Vector3D Point2)
      {
         //starting Values
         Point1 = new Vector3D();
         Point2 = new Vector3D();

         //Spherical intersection
         Vector3D O = LineStart;
         Vector3D D = LineDirection;
         Double R = Sphere.Radius;
         Vector3D C = Sphere.Center;

         //Calculates Parameters
         Double b = 2 * (Vector3D.Dot(O - C, D));
         Double c = Vector3D.Dot((O - C), (O - C)) - R * R;

         //Calculates Values
         Double t1, t2;
         if (!Quadractic_Solv(1, b, c, out t1, out t2))
         { return false; } //does not intersect
         else
         {
            Point1 = LineStart + LineDirection * t1;
            Point2 = LineStart + LineDirection * t2;
            return true;
         }
      }

      //Basic Gets Predicted Position Of Enemy (Derived From Keen Code)
      public static Vector3D GetPredictedTargetPosition2(IMyTerminalBlock shooter, Vector3 ShipVel, MyDetectedEntityInfo target, float shotSpeed)
      {
         Vector3D predictedPosition = target.Position;
         Vector3D dirToTarget = Vector3D.Normalize(predictedPosition - shooter.GetPosition());

         //Run Setup Calculations
         Vector3 targetVelocity = target.Velocity;
         targetVelocity -= ShipVel;
         Vector3 targetVelOrth = Vector3.Dot(targetVelocity, dirToTarget) * dirToTarget;
         Vector3 targetVelTang = targetVelocity - targetVelOrth;
         Vector3 shotVelTang = targetVelTang;
         float shotVelSpeed = shotVelTang.Length();

         if (shotVelSpeed > shotSpeed)
         {
            // Shot is too slow 
            return Vector3.Normalize(target.Velocity) * shotSpeed;
         }
         else
         {
            // Run Calculations
            float shotSpeedOrth = (float)Math.Sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed);
            Vector3 shotVelOrth = dirToTarget * shotSpeedOrth;
            float timeDiff = shotVelOrth.Length() - targetVelOrth.Length();
            var timeToCollision = timeDiff != 0 ? ((shooter.GetPosition() - target.Position).Length()) / timeDiff : 0;
            Vector3 shotVel = shotVelOrth + shotVelTang;
            predictedPosition = timeToCollision > 0.01f ? shooter.GetPosition() + (Vector3D)shotVel * timeToCollision : predictedPosition;
            return predictedPosition;
         }
      }

      //Generic Diagnostics Tools 
      public static class DiagTools
      {
         //Used For Customdata Plotting
         public static void Diag_Plot(IMyTerminalBlock Block, object Data1)
         {
            Block.CustomData = Block.CustomData + Data1 + "\n";
         }

         //Used For Fast Finding/Dynamically Renaming A Block Based On Type
         public static void Renam_Block_Typ(IMyGridTerminalSystem GTS, string RenameTo)
         {
            List<IMyTerminalBlock> TempCollection = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyRadioAntenna>(TempCollection);
            if (TempCollection.Count < 1)
            { return; }
            TempCollection[0].CustomName = RenameTo;
         }

         //Used For Fast Finding/Dynamically Renaming A Block Based On CustomData
         public static void Renam_Block_Cust(IMyGridTerminalSystem GTS, string customnam, string RenameTo)
         {
            List<IMyTerminalBlock> TempCollection = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyTerminalBlock>(TempCollection, a => a.CustomData == customnam);
            if (TempCollection.Count < 1)
            { return; }
            else
            { TempCollection[0].CustomName = RenameTo; }
         }

      }
   }
}
