namespace SharpEDL.Classes
{
    public class PortDetail
    {
        // Attributes
        public string HWID { get; set; }
        public string HASH { get; set; }
        public string SBL { get; set; }

        // Methods
        public override string ToString()
        {
            return
                $"HWID: {HWID}\n" +
                $"HASH: {HASH}\n" +
                $"SBL:  {SBL}";
        }
    }
}
