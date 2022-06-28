using JetEazy.QMath;
using OpenCvSharp;
using System;

namespace JetEazy.GdxCore3.Model
{
    public class GdxMotorCoordsTransform : IDisposable
    {
        static int MODE = 2;

        #region PRIVATE_DATA
        QVector mv_zero = new QVector(6);
        double alpha = -45;
        Mat ROT_vy = null;
        Mat ROT_vz = null;
        Mat ROT_Y = null;
        Mat ROT_Y_Inv = null;
        static double U(QVector mv)
        {
            return mv[3];
        }
        static double theta_y(QVector mv)
        {
            return mv[4];
        }
        static double theta_z(QVector mv)
        {
            return mv[5];
        }
        #endregion

        public GdxMotorCoordsTransform()
        {
        }
        public void Dispose()
        {
            cleanAll();
        }

        #region PRIVATE_FUNCTIONS
        void cleanAll()
        {
            cleanup(ROT_vy);
            ROT_vy = null;
            cleanup(ROT_vz);
            ROT_vz = null;
            cleanup(ROT_Y_Inv);
            ROT_Y = null;
            cleanup(ROT_Y_Inv);
            ROT_Y = null;
        }
        void cleanup(Mat m)
        {
            if (m != null)
                m.Dispose();
        }
        void buildRotationMatrix(QVector mv)
        {
            // 馬達命名與數學右手定則不同
            QVector dmv = mv - mv_zero;
            double angle_vy = -theta_z(dmv);
            double angle_vz = theta_y(dmv);

            cleanAll();
            // Rotation vY
            ROT_vy = buildRotateY(angle_vy);
            // Rotation vZ
            ROT_vz = buildRotateZ(angle_vz);
            // Rotation Y
            ROT_Y = buildRotateY(alpha);
            // 
            ROT_Y_Inv = ROT_Y.Inv();
        }
        Mat buildRotateY(double angle)
        {
            angle = angle * Math.PI / 180;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            Mat R = new Mat(3, 3, MatType.CV_64FC1,
                new double[] {
                     cos, 0, sin,
                       0, 1, 0,
                    -sin, 0, cos
                });
            return R;
        }
        Mat buildRotateZ(double angle)
        {
            angle = angle * Math.PI / 180;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            Mat R = new Mat(3, 3, MatType.CV_64FC1,
                new double[] {
                     cos, sin, 0,
                    -sin, cos, 0,
                       0,   0, 1
                });
            return R;
        }
        #endregion

        public void Init(QVector mv0, double ud0)
        {
            //mv_zero = new QVector(mv0);
            mv_zero = mv0 + new QVector(0, 0, 0, ud0, 0, 0);
        }
        public QVector CalcSphereCenterCompensation(QVector mv, QVector delta)
        {
            QVector dst = mv + delta;
            buildRotationMatrix(dst);

            double ud = U(dst) - U(mv_zero);
            using (Mat Ud = new Mat(3, 1, MatType.CV_64FC1, new double[] { ud, 0, 0 }))
            using (Mat v1 = ROT_vz * Ud)
            using (Mat v = ROT_vy * v1)
            using (Mat w = ROT_Y_Inv * v)
            {
                //Mirror Local Coordinates
                //using(Mat mirror)
                System.Diagnostics.Trace.Assert(w.Rows == 3 && w.Cols == 1);
                
                double a = Math.Abs(this.alpha) * Math.PI / 180;
                double sin = Math.Sin(a);
                double cos = Math.Cos(a);

                double dGx = -w.Get<double>(0, 0);
                double dGy = -w.Get<double>(1, 0);
                double dGz = -w.Get<double>(2, 0);

                //--------------------------------------------
                // double dGX = X - Z * sin(a) + U * cos(a);
                // double dGY = Y;
                // double dGZ = Z * cos(a) + U * sin(a);
                //--------------------------------------------
                double X, Y, Z, U;
                if (MODE == 0 && Math.Abs(cos) > 0.1)
                {
                    U = 0;
                    Z = dGz / cos;
                    Y = dGy;
                    X = dGx + Z * sin - U * cos;
                }
                else if (MODE == 1 && Math.Abs(sin) > 0.1)
                {
                    U = dGz / sin;
                    Z = 0;
                    Y = dGy;
                    X = dGx + Z * sin - U * cos;
                }
                else
                {
                    //================================================
                    // dGX = -Z * sin(a) + U * cos(a)
                    // dGZ =  Z * cos(a) + U * sin(a)
                    //------------------------------------------------
                    //  c * dGX = -Z * cs + U * cc
                    //  s * dGZ =  Z * sc + U * ss
                    //------------------------------------------------
                    //  c * dGX + s * dGZ = U (cc + ss)
                    //------------------------------------------------
                    //  cos(a) * dGX + sin(a) * dGZ = U (c^2 + s^2)
                    //================================================
                    // dGX = -Z * sin(a) + U * cos(a)
                    // dGZ =  Z * cos(a) + U * sin(a)
                    //------------------------------------------------
                    // -s * dGX = Z * ss  +  U * (-sc)
                    //  c * dGZ = Z * cc  +  U * (cs)
                    //------------------------------------------------
                    // -s * dGX + c * dGZ = Z * (ss + cc)
                    //------------------------------------------------
                    U = ( cos * dGx + sin * dGz);
                    Z = (-sin * dGx + cos * dGz);
                    Y = dGy;
                    X = 0;
                }
                return new QVector(X, Y, Z, U);
            }
        }
        public QVector SimpleSphereCenterCompensation(double UbcOffset, QVector initPos, QVector finalPos)
        {
            // X, Y, Z, U, θy, θz
            // Ignore dX, dY
            var dV = finalPos - initPos;
            double theta_y = dV[4] * Math.PI / 180;
            double theta_z = dV[5] * Math.PI / 180;
            double dU = UbcOffset * (1 - Math.Cos(theta_z));
            double dZ = UbcOffset * Math.Sin(theta_z);
            return new QVector(0, 0, dZ, dU, 0, 0);
        }
        public QVector MotorToWorld(double X, double Y, double Z, double u)
        {
            X -= mv_zero.X;
            Y -= mv_zero.Y;
            Z -= mv_zero.Z;
            u -= U(mv_zero);
            double a = Math.Abs(this.alpha) * Math.PI / 180;
            double GX = X - Z * Math.Sin(a) + u * Math.Cos(a);
            double GY = Y;
            double GZ = Z * Math.Cos(a) + u * Math.Sin(a);
            return new QVector(GX, GY, GZ);
        }
    }
}
