using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

// Assuming the min value is 0 in all cases.

namespace disk_editor
{
    static class WidthScaling
    {

        #region BigInteger nth root from http://stackoverflow.com/questions/20977218/fast-approximation-of-a-square-root
        static BigInteger RoughRoot(BigInteger x, int root)
        {
            var bytes = x.ToByteArray();    // get binary representation
            var bits = (bytes.Length - 1) * 8;  // get # bits in all but msb
            // add # bits in msb
            for (var msb = bytes[bytes.Length - 1]; msb != 0; msb >>= 1)
                bits++;
            var rtBits = bits / root + 1;   // # bits in the root
            var rtBytes = rtBits / 8 + 1;   // # bytes in the root
            // avoid making a negative number by adding an extra 0-byte if the high bit is set
            var rtArray = new byte[rtBytes + (rtBits % 8 == 7 ? 1 : 0)];
            // set the msb
            rtArray[rtBytes - 1] = (byte)(1 << (rtBits % 8));
            // make new BigInteger from array of bytes
            return new BigInteger(rtArray);
        }
        #endregion

        public static BigInteger IntegerRoot(BigInteger n, int root)
        {
            var oldValue = new BigInteger(0);
            var newValue = RoughRoot(n, root);
            int i = 0;
            // I limited iterations to 100, but you may want way less
            while (BigInteger.Abs(newValue - oldValue) >= 1 && i < 100)
            {
                oldValue = newValue;
                newValue = ((root - 1) * oldValue
                            + (n / BigInteger.Pow(oldValue, root - 1))) / root;
                i++;
            }
            return newValue;
        }

        static public Double get_algebraic_mean(ulong[] source_values)
        {
            ulong array_sum = 0;
            for (int i = 0; i < source_values.Count(); i++)
            {
                ulong source_value = source_values[i];

                array_sum += source_value;
            }

            Double algebraic_mean = (Double)array_sum / (Double)source_values.Count();

            return algebraic_mean;
        }

        static public Double get_geometric_mean(ulong[] source_values)
        {
            BigInteger array_product = new BigInteger((ulong)1);

            for (int i = 0; i < source_values.Count(); i++)
            {
                BigInteger source_value = new BigInteger(source_values[i]);

                if (source_value > 0)
                {
                    array_product *= source_value;
                }
            }

            BigInteger geometric_mean = WidthScaling.IntegerRoot(array_product, source_values.Count());

            return (Double)geometric_mean;
        }

        static public Double linear_scale_single_ratio(ulong source_value, ulong max_value)
        {
            Double result_value = (Double)source_value / (Double)max_value;

            return result_value;
        }

        static public Double[] linear_scale_multi_ratio(ulong[] source_values)
        {
            ulong max_value = source_values.Max();

            Double[] result_values = new Double[source_values.Count()];
            for (int i = 0; i < source_values.Count(); i++)
            {
                result_values[i] = linear_scale_single_ratio(source_values[i], max_value);
            }

            return result_values;
        }

        static public Double log_scale_single_ratio(ulong source_value, ulong max_value, Double log_base = 10.0)
        {
            Double log_max = Math.Log(max_value, log_base);

            Double result_value = Math.Log(source_value, log_base) / log_max;

            return result_value;
        }

        static public Double[] log_scale_multi_ratio(ulong[] source_values)
        {
            ulong max_value = source_values.Max();
            Double geometric_mean = get_geometric_mean(source_values);

            Double[] result_values = new Double[source_values.Count()];
            for (int i = 0; i < source_values.Count(); i++)
            {
                result_values[i] = log_scale_single_ratio(source_values[i], max_value, geometric_mean);
            }

            return result_values;
        }
    }
}
