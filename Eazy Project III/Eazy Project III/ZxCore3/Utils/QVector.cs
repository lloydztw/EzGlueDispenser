/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2008/12/01 The class is created by LeTian Chang
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/


using System;

namespace JetEazy.QMath
{
    public class QVector
    {
        public static int Percision = 6;

        #region PRIVATE_DATA
        protected double[] m_VD;
        #endregion

        public QVector(params double[] values)
        {
            int N = values.Length;
            m_VD = new double[N];
            for (int i = 0; i < N; i++)
                m_VD[i] = _FIX(values[i]);
        }
        public QVector(int dimensions)
        {
            m_VD = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
                m_VD[i] = 0.0;
        }
        public QVector(QVector src) 
            : this(src.m_VD)
        {
            //int iSize = src.Size;
            //m_VD = new double[iSize];
            //for (int i = 0; i < iSize; i++)
            //    m_VD[i] = src.m_VD[i];
        }
        public QVector Slice(int start, int len = 0)
        {
            int end = len <= 0 ? Dimensions : Math.Min(start + len, Dimensions);
            int n = end - start;
            var v = new QVector(n);
            for (int k = 0, i = start; i < end; i++, k++)
                v[k] = this[i];
            return v;
        }

        /// <summary>
        /// 維度
        /// </summary>
        public int Dimensions
        {
            get { return m_VD.Length; }
        }
        /// <summary>
        /// 維度
        /// </summary>
        public int Length
        {
            get { return m_VD.Length; }
        }
        /// <summary>
        /// 維度
        /// </summary>
        public int Size
        {
            get { return m_VD.Length; }
        }

        public double this[int idx]
        {
            get
            {
                return idx<m_VD.Length ? m_VD[idx] : 0.0;
            }
            set
            {
                if (idx < m_VD.Length)
                    m_VD[idx] = _FIX(value);
            }
        }
        public double[] V
        {
            get { return m_VD; }
        }

        public double X
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }
        public double Y
        {
            get
            {
                return this[1];
            }
            set
            {
                this[1] = value;
            }
        }
        public double Z
        {
            get
            {
                return this[2];
            }
            set
            {
                this[2] = value;
            }
        }

        public double x
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }
        public double y
        {
            get
            {
                return this[1];
            }
            set
            {
                this[1] = value;
            }
        }
        public double z
        {
            get
            {
                return this[2];
            }
            set
            {
                this[2] = value;
            }
        }

        /// <summary>
        /// 向量長度
        /// </summary>
        public double NormLength
        {
            get
            {
                double dLenSQ = this * this;
                return _FIX(Math.Sqrt(dLenSQ));
            }
        }
        /// <summary>
        /// 向量長度 平方
        /// </summary>
        public double NormLengthSQ
        {
            get
            {
                double dLenSQ = this * this;
                return dLenSQ;
            }
        }

        public static bool AreEqual(QVector v1, QVector v2, int percision = -1)
        {
            if (v1 == v2)
                return true;

            if (v1.Dimensions != v2.Dimensions)
                return false;

            if (percision < 0)
            {
                for (int i = 0, N = v1.Dimensions; i < N; i++)
                    if (v1[i] != v2[i])
                        return false;
            }
            else
            {
                for (int i = 0, N = v1.Dimensions; i < N; i++)
                    if (Math.Round(v1[i], percision) != Math.Round(v2[i], percision))
                        return false;
            }


            return true;
        }
        public bool IsEqualTo(QVector v)
        {
            //註: 不要 override Equals, 它會影響 Directory<QVector,> 鍵值比對 !!!
            return AreEqual(this, v);
        }

        public QVector Offset(params double[] values)
        {
            int N = Math.Min(this.Dimensions, values.Length);
            for (int i = 0; i < N; i++)
                this[i] += values[i];   //<<< this[i] 已經內含 _FIX
            return this;
        }
        public static double operator *(QVector v1, QVector v2)
        {
            if (v1 == null || v2 == null)
                return 0.0;

            if (!_check_dimensions(v1, v2))
                return 0.0;

            double ret = 0.0;
            for (int i = 0, N = v1.Dimensions; i < N; i++)
                ret += _FIX(v1[i] * v2[i]);

            return _FIX(ret);
        }
        public static QVector operator +(QVector v1, QVector v2)
        {
            if (!_check_dimensions(v1, v2))
                return null;

            QVector v = new QVector(v1.Dimensions);
            for (int i = 0, N = v1.Dimensions; i < N; i++)
                v[i] = v1[i] + v2[i];   //<<< v[i] 已經內含 _FIX

            return v;
        }
        public static QVector operator -(QVector v1, QVector v2)
        {
            if (!_check_dimensions(v1, v2))
                return null;

            int N = v1.Dimensions;
            QVector v = new QVector(N);
            for (int i = 0; i < N; i++)
                v[i] = v1[i] - v2[i];   //<<< v[i] 已經內含 _FIX

            return v;
        }
        public static QVector operator *(QVector v1, double s)
        {
            s = _FIX(s);
            int N = v1.Dimensions;
            QVector v = new QVector(N);
            for (int i = 0; i < N; i++)
                v[i] = v1[i] * s;       //<<< v[i] 已經內含 _FIX
            return v;
        }
        public static QVector operator /(QVector v1, double s)
        {
            s = _FIX(s);
            int N = v1.Dimensions;
            QVector v = new QVector(N);
            for (int i = 0; i < N; i++)
                v[i] = v1[i] / s;       //<<< v[i] 已經內含 _FIX
            return v;
        }

        public override string ToString()
        {
            return ToString("0.0000");
        }
        public string ToString(bool compact)
        {
            return ToString("0.0000", compact);
        }
        public string ToString(string format, bool compact = false)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int iSize = this.Size;

            if (!compact)
                sb.Append("{");

            for (int i = 0; i < iSize; i++)
            {
                if (!compact)
                {
                    if (m_VD[i] >= 0.0)
                        sb.Append(" ");
                    sb.Append(" ");
                }
                sb.Append(m_VD[i].ToString(format));
                if (i != iSize - 1)
                    sb.Append(",");
            }

            if(!compact)
                sb.Append(" }");

            return sb.ToString();
        }
        public void FromString(string str)
        {
            //string str1 = str.Replace("{", "");
            //string str0 = str1.Replace("}", "");
            //string[] strTokens = str0.Split(',');
            //int N = System.Math.Min(strTokens.Length, this.Dimensions);
            //if (N > 0)
            //{
            //    //QVector v = new QVector(iSize);
            //    //double d = 0.0;
            //    for (int i = 0; i < N; i++)
            //    {
            //        if (double.TryParse(strTokens[i], out double d))
            //            this[i] = d;
            //    }
            //    //return v;
            //}
            ////return null;
            
            QVector v = Parse(str);
            if (v != null)
            {
                int N = Math.Min(this.Dimensions, v.Dimensions);
                for (int i = 0; i < N; i++)
                    this[i] = v[i];
            }
        }
        public static QVector Parse(string str)
        {
            str = str.Replace("{", "").Replace("}", "");
            string[] strs = str.Split(',');
            int N = strs.Length;
            QVector v = new QVector(N);
            bool ok = false;
            for (int i = 0; i < N; i++)
            {
                if (double.TryParse(strs[i], out double d))
                {
                    v[i] = d;
                    ok = true;
                }
            }
            return ok ? v : null;
        }


        #region PRIVATE_FUNCTIONS
        static bool _check_dimensions(QVector v1, QVector v2)
        {
            if (v1.Size != v2.Size)
            {
                throw new Exception("QVectors must be in the same Dimensions for Vector Operation !");
                return false;
            }
            return true;
        }
        static double _FIX(double v)
        {
            return (double)Math.Round(v, Percision);
        }
        #endregion
    }
}
