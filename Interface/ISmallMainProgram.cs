using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace space_engineers.Interface
{
   public interface ISmallMainProgram
   {
      void Main ( string argument, UpdateType updateSource );
   }
}
