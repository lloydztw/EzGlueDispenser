﻿using JetEazy.Drivers.Laser;

namespace JetEazy.GdxCore3.Model
{
    internal class GdxGlobal
    {
        public readonly static GdxLog LOG = GdxLog.Singleton();

        public static void Init()
        {
            Facade = GdxFacade.Singleton;
            Facade.Init();
            INI = GdxFacadeIni.Singleton;
        }
        public static void Dispose()
        {
            Facade.Dispose();
        }

        internal static GdxFacade Facade
        {
            get;
            private set;
        }
        internal static GdxFacadeIO IO
        {
            get { return Facade.IO; }
        }
        internal static GdxFacadeIni INI
        {
            get;
            private set;
        }

        internal static IxLaser GetLaser(int id = 0)
        {
            return Facade.GetLaser(id);
        }
    }
}
