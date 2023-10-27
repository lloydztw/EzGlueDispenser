using System.Drawing;


namespace Eazy_Project_Interface
{
    public interface IRecipe
    {
        int CaliCamExpo { get; set; }
        string CaliCamCaliData { get; set; }
        PointF CaliPicCenter { get; set; }
        int JudgeCamExpo { get; set; }
        int JudgeCamCaliData { get; set; }
        string JudgeThetaOffset { get; set; }
        int DispensingTime { get; set; }
        int UVTime { get; set; }
        string OtherRecipe { get; set; }
    }

}
