using JetEazy.QMath;

namespace JetEazy.GdxCore3.Model
{
    class GdxMotorVector : QVector
    {
        public GdxMotorVector(params double[] values) : base(values)
        {
        }
        public GdxMotorVector() : base(6)
        {
        }
        public double u
        {
            get
            {
                return this[3];
            }
            set
            {
                this[3] = value;
            }
        }
        public double theta_y
        {
            get
            {
                return this[4];
            }
            set
            {
                this[4] = value;
            }
        }
        public double theta_z
        {
            get
            {
                return this[5];
            }
            set
            {
                this[5] = value;
            }
        }
    }
}
