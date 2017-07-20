using System;

namespace CSharpLib.Common.NoSql.Memcached
{
    /// <summary>
    /// CRC tool
    /// </summary>
    public class CRCTool
    {
        private int order = 16;
        private ulong polynom = 0x1021;
        private int direct = 1;
        private ulong crcinit = 0xFFFF;
        private ulong crcxor = 0x0;
        private int refin = 0;
        private int refout = 0;

        private ulong crcmask;
        private ulong crchighbit;
        private ulong crcinit_direct;
        private ulong crcinit_nondirect;
        private ulong[] crctab = new ulong[256];

        /// <summary>
        /// 
        /// </summary>
        public enum CRCCode
        {
            CRC_CCITT,
            CRC16,
            CRC32
        }

        /// <summary>
        /// 
        /// </summary>
        public CRCTool()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodingType"></param>
        public void Init(CRCCode CodingType)
        {
            switch (CodingType)
            {
                case CRCCode.CRC_CCITT:
                    order = 16; direct = 1; polynom = 0x1021; crcinit = 0xFFFF; crcxor = 0; refin = 0; refout = 0;
                    break;
                case CRCCode.CRC16:
                    order = 16; direct = 1; polynom = 0x8005; crcinit = 0x0; crcxor = 0x0; refin = 1; refout = 1;
                    break;
                case CRCCode.CRC32:
                    order = 32; direct = 1; polynom = 0x4c11db7; crcinit = 0xFFFFFFFF; crcxor = 0xFFFFFFFF; refin = 1; refout = 1;
                    break;
            }

            crcmask = ((((ulong)1 << (order - 1)) - 1) << 1) | 1;
            crchighbit = (ulong)1 << (order - 1);

            generate_crc_table();

            ulong bit, crc;
            int i;
            if (direct == 0)
            {
                crcinit_nondirect = crcinit;
                crc = crcinit;
                for (i = 0; i < order; i++)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if (bit != 0)
                    {
                        crc ^= polynom;
                    }
                }
                crc &= crcmask;
                crcinit_direct = crc;
            }
            else
            {
                crcinit_direct = crcinit;
                crc = crcinit;
                for (i = 0; i < order; i++)
                {
                    bit = crc & 1;
                    if (bit != 0)
                    {
                        crc ^= polynom;
                    }
                    crc >>= 1;
                    if (bit != 0)
                    {
                        crc |= crchighbit;
                    }
                }
                crcinit_nondirect = crc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong crctablefast(byte[] p)
        {
            ulong crc = crcinit_direct;
            if (refin != 0)
            {
                crc = reflect(crc, order);
            }
            if (refin == 0)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = (crc << 8) ^ crctab[((crc >> (order - 8)) & 0xff) ^ p[i]];
                }
            }
            else
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = (crc >> 8) ^ crctab[(crc & 0xff) ^ p[i]];
                }
            }
            if ((refout ^ refin) != 0)
            {
                crc = reflect(crc, order);
            }
            crc ^= crcxor;
            crc &= crcmask;
            return (crc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong crctable(byte[] p)
        {
            ulong crc = crcinit_nondirect;
            if (refin != 0)
            {
                crc = reflect(crc, order);
            }
            if (refin == 0)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = ((crc << 8) | p[i]) ^ crctab[(crc >> (order - 8)) & 0xff];
                }
            }
            else
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = (ulong)(((int)(crc >> 8) | (p[i] << (order - 8))) ^ (int)crctab[crc & 0xff]);
                }
            }
            if (refin == 0)
            {
                for (int i = 0; i < order / 8; i++)
                {
                    crc = (crc << 8) ^ crctab[(crc >> (order - 8)) & 0xff];
                }
            }
            else
            {
                for (int i = 0; i < order / 8; i++)
                {
                    crc = (crc >> 8) ^ crctab[crc & 0xff];
                }
            }

            if ((refout ^ refin) != 0)
            {
                crc = reflect(crc, order);
            }
            crc ^= crcxor;
            crc &= crcmask;

            return (crc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong crcbitbybit(byte[] p)
        {
            int i;
            ulong j, c, bit;
            ulong crc = crcinit_nondirect;

            for (i = 0; i < p.Length; i++)
            {
                c = (ulong)p[i];
                if (refin != 0)
                {
                    c = reflect(c, 8);
                }

                for (j = 0x80; j != 0; j >>= 1)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if ((c & j) != 0)
                    {
                        crc |= 1;
                    }
                    if (bit != 0)
                    {
                        crc ^= polynom;
                    }
                }
            }

            for (i = 0; (int)i < order; i++)
            {

                bit = crc & crchighbit;
                crc <<= 1;
                if (bit != 0) crc ^= polynom;
            }

            if (refout != 0)
            {
                crc = reflect(crc, order);
            }
            crc ^= crcxor;
            crc &= crcmask;

            return (crc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong crcbitbybitfast(byte[] p)
        {
            int i;
            ulong j, c, bit;
            ulong crc = crcinit_direct;

            for (i = 0; i < p.Length; i++)
            {
                c = (ulong)p[i];
                if (refin != 0)
                {
                    c = reflect(c, 8);
                }

                for (j = 0x80; j > 0; j >>= 1)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if ((c & j) > 0) bit ^= crchighbit;
                    if (bit > 0) crc ^= polynom;
                }
            }

            if (refout > 0)
            {
                crc = reflect(crc, order);
            }
            crc ^= crcxor;
            crc &= crcmask;

            return (crc);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ushort CalcCRCITT(byte[] p)
        {
            uint uiCRCITTSum = 0xFFFF;
            uint uiByteValue;

            for (int iBufferIndex = 0; iBufferIndex < p.Length; iBufferIndex++)
            {
                uiByteValue = ((uint)p[iBufferIndex] << 8);
                for (int iBitIndex = 0; iBitIndex < 8; iBitIndex++)
                {
                    if (((uiCRCITTSum ^ uiByteValue) & 0x8000) != 0)
                    {
                        uiCRCITTSum = (uiCRCITTSum << 1) ^ 0x1021;
                    }
                    else
                    {
                        uiCRCITTSum <<= 1;
                    }
                    uiByteValue <<= 1;
                }
            }
            return (ushort)uiCRCITTSum;
        }


        #region private

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="bitnum"></param>
        /// <returns></returns>
        private ulong reflect(ulong crc, int bitnum)
        {
            ulong i, j = 1, crcout = 0;

            for (i = (ulong)1 << (bitnum - 1); i != 0; i >>= 1)
            {
                if ((crc & i) != 0)
                {
                    crcout |= j;
                }
                j <<= 1;
            }
            return (crcout);
        }

        /// <summary>
        /// 
        /// </summary>
        private void generate_crc_table()
        {
            int i, j;
            ulong bit, crc;

            for (i = 0; i < 256; i++)
            {
                crc = (ulong)i;
                if (refin != 0)
                {
                    crc = reflect(crc, 8);
                }
                crc <<= order - 8;

                for (j = 0; j < 8; j++)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if (bit != 0) crc ^= polynom;
                }

                if (refin != 0)
                {
                    crc = reflect(crc, order);
                }
                crc &= crcmask;
                crctab[i] = crc;
            }
        }

        #endregion
    }
}