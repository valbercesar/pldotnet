using System;
using System.Runtime.InteropServices;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.MONEYOID, OID.MONEYARRAYOID)]
    public class money_handler : struct_type_handler<decimal>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumMoneyAttributes(IntPtr datum, ref long value);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumMoney(long value);

        public static void checkLimits(decimal value)
        {
            if (value < -92233720368547758.08M || value > 92233720368547758.07M)
            {
                throw new OverflowException($"The supplied value ({value}) is outside the range for a PostgreSQL money value.");
            }
        }

        public override decimal input_value(IntPtr datum)
        {
            long value = 0;
            pldotnet_getDatumMoneyAttributes(datum, ref value);
            decimal datum_value = new decimal(value);
            return datum_value / 100.0M;
        }

        public override IntPtr output_value(decimal value)
        {
            checkLimits(value);
            return pldotnet_createDatumMoney(Decimal.ToInt64(Math.Round(100.0M * value)));
        }
    }
}