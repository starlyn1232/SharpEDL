namespace SharpEDL.Utils
{
    internal static class Constants
    {
        // Binaries
        public static string EMMCDL_EXE = "emmcdl.exe";
        public static string FH_EXE = "fh_loader.exe";
        public static string QSAHARA_EXE = "QSaharaServer.exe";

        // Magics

        // EMMCDL
        public const string EMMC_INIT = "Version 2.15";
        public const string EMMC_OKAY = "Status: 0 The operation completed successfully.";
        public const string EMMC_LIST = " (COM";
        public const string EMMC_HWID = "MSM_HW_ID: ";
        public const string EMMC_HASH = "OEM_PK_HASH: ";
        public const string EMMC_SBL = "SBL SW Version: ";

        // QSahara
        public const string QSAHARA_ERROR = ": ERROR: ";

        // FH Loader
        public const string FH_INIT = "Base Version: 19.06.10.18.44";
    }
}
