using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace PlDotNET_Handler
{
    public static class NullMap
    {
        /// <summary>
        /// This function checks the Postgres nullmap to identify if an
        /// element of the array is null.
        /// </summary>
        /// <param name="nullmap">The Postgres nullmap.</param>
        /// <param name="offset">The element offset.</param>
        public static unsafe bool check_null(byte[] nullmap, int offset)
        {
            // Checks whether the array value at [offset]
            // should be null, based on the given nullmap

            int bytelen = nullmap.Length;

            // this is the postgres convention: an empty nullmap
            // means that no elements are null
            if (bytelen == 0) { return false; }

            int bitlen = bytelen * 8;
            if (offset > bitlen)
            {
                throw new ArgumentOutOfRangeException("Illegal offset");
            }

            int byte_offset = offset / 8;
            int bit_offset = offset % 8;
            byte relevant_byte = nullmap[byte_offset];
            int this_is_null = (relevant_byte >> bit_offset) & 0x1;

            return (this_is_null != 1);
        }

        // TODO(rosicley) - check if we need this function
        // Set the postgres nullmap
        public static void set_null(byte[] nullmap, int offset)
        {
            // Sets the given bit in the provided nullmap
            // Does this work, or do we need nullmap to be an explicit ref type?

            int bytelen = nullmap.Length;

            // this is the postgres convention: an empty nullmap
            // means that no elements are null
            if (bytelen == 0) { return; }

            int bitlen = bytelen * 8;
            if (offset > bitlen)
            {
                throw new ArgumentOutOfRangeException("Illegal offset");
            }

            int byte_offset = offset / 8;
            int bit_offset = offset % 8;
            nullmap[byte_offset] |= (byte)(1 << bit_offset);
        }
    }

    // Warning: do not use `base_type_handler` directly.  Instead see
    // `struct_type_handler<T>` and `object_type_handler<T>`, below.
    public abstract class base_type_handler<T>
    {
        /// <summary>
        /// Converts a PostgreSQL datum to dotnet value.
        /// </summary>
        /// <param name="datum">The Postgres datum.</param>
        /// <returns> Returns the dotnet value. </returns>
        public abstract T input_value(IntPtr datum);

        /// <summary>
        /// Converts a dotnet value to PostgreSQL datum.
        /// </summary>
        /// <param name="value">The dotnet value.</param>
        /// <returns> Returns the PostgreSQL datum. </returns>
        public abstract IntPtr output_value(T value);

        public Array? input_nullable_array(IntPtr datum, bool isnull)
        {
            return (isnull ? null : input_array(datum));
        }

        public IntPtr output_nullable_array(Array? value, OID element_oid)
        {
            return (value == null ? int_handler.pldotnet_createDatumInt32(0) : output_array((Array)value, element_oid));
        }

        /// <summary>
        /// Converts a PostgreSQL array (multidimensional or not) in an Array object (dotnet style).
        /// If the PostgreSQL array has null values, this function calls input_nullable_array
        /// to correctly handle with this array.
        /// </summary>
        /// <param name="datum">The Postgres array.</param>
        /// <returns> Returns an Array object (multidimensional or not).</returns>
        public unsafe Array input_array(IntPtr datum)
        {
            int ndims = 0;
            int[] raw_dims = new int[array_handler.maxdim];
            byte* nullmap;
            int type_id = 0;
            array_handler.pldotnet_getArrayAttributes(datum, ref type_id, ref ndims, raw_dims, &nullmap);

            int[] dims = raw_dims[..ndims];
            int nelems = 1;
            for (int i = 0; i < ndims; i++)
            {
                nelems *= dims[i];
            }

            IntPtr[] datums = new IntPtr[nelems];
            int array_ret = array_handler.pldotnet_getArrayDatum(datum, datums, nelems, type_id);

            if (array_ret != 0)
            {
                throw new System.Exception($"Got error from pldotnet_getArrayDatum(): {array_ret}");
            }

            var datum_list = new List<IntPtr>();
            datum_list.AddRange(datums);

            if (nullmap != null)
            {
                int nullmap_len = (nelems / 8) + 1;
                var nullmap_p = new IntPtr(nullmap).ToInt64();
                ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(nullmap, nullmap_len);
                byte[] nullmap2 = nativeSpan.ToArray();
                return input_nullable_array(datum_list, dims, nullmap2);
            }

            elog.pldotnet_Info("The input array has no null values!");

            var ret = datum_list.Select((datum, index) => input_value(datum)).ToArray();
            Array ret2 = Array.CreateInstance(typeof(object), dims);
            array_handler.reshapeArray(ret, ref ret2);

            return ret2;
        }

        /// <summary>
        /// Creates a PostgreSQL array from an Array object and the OID of its element.
        /// If the dotnet array has null values, the output_nullable_array will be
        /// called.
        /// </summary>
        /// <param name="value">The dotnet array.</param>
        /// <param name="element_oid">The OID of the elements.</param>
        /// <returns> Returns a PostgreSQL array.</returns>
        public unsafe IntPtr output_array(Array value, OID element_oid)
        {
            int nelems = value.Length;
            Array flat_array = Array.CreateInstance(typeof(object), nelems);
            array_handler.flatArray(value, ref flat_array);
            // flat_array variable is an one-dimensional array with the .NET types now

            int dimNumber = value.Rank;
            int[] dimLengths = new int[dimNumber];
            for (int i = 0; i < dimNumber; i++)
                dimLengths[i] = value.GetLength(i);

            // TODO - We need to find a way to do this in a prettier way.
            for (int i = 0; i < nelems; i++)
                if (flat_array.GetValue(i) == null)
                    return output_nullable_array(flat_array, dimLengths, element_oid);

            elog.pldotnet_Info("The output array has no null value!");

            IntPtr[] datums = new IntPtr[nelems]; // datums will be passed to C!
            for (int i = 0; i < nelems; i++)
                datums[i] = output_value((T)flat_array.GetValue(i));

            return array_handler.pldotnet_createDatumArray((int)element_oid, dimNumber, dimLengths, datums);
        }

        /// <summary>
        /// Converts a PostgreSQL array with null values in an Array object (dotnet style).
        /// This function is called from input_array.
        /// </summary>
        /// <param name="datums">The flat list of datums.</param>
        /// <param name="dims">The size of each dimension.</param>
        /// <param name="nullmap">The Postgres nullmap.</param>
        /// <returns> Returns an Array object (multidimensional or not).</returns>
        Array input_nullable_array(List<IntPtr> datums, int[] dims, byte[] nullmap)
        {
            elog.pldotnet_Info("The input array has null values!");

            int nelms = datums.Count;
            object[] ret = new object[nelms];

            for (int i = 0, cont = 0; i < nelms; i++)
            {
                bool is_null = NullMap.check_null(nullmap, i);
                ret[i] = is_null ? null : input_value(datums[cont++]);
            }

            Array ret2 = Array.CreateInstance(typeof(object), dims);
            array_handler.reshapeArray(ret, ref ret2);
            return ret2;
        }

        /// <summary>
        /// Creates a PostgreSQL array from an Array object with null values.
        /// This function is called from output_array.
        /// </summary>
        /// <param name="flat_array">The flat array with the dotnet values.</param>
        /// <param name="dims">The size of each dimension of the original dotnet array.</param>
        /// <param name="element_oid">The OID of the elements.</param>
        /// <returns> Returns a PostgreSQL array.</returns>
        unsafe IntPtr output_nullable_array(Array flat_array, int[] dims, OID element_oid)
        {
            elog.pldotnet_Info("The output array has null values!");
            int nelems = flat_array.Length;
            byte[] nulls = new byte[nelems];
            IntPtr[] datums = new IntPtr[nelems];

            for (int i = 0; i < nelems; i++)
            {
                if (flat_array.GetValue(i) == null)
                {
                    nulls[i] = 1;
                    datums[i] = int_handler.pldotnet_createDatumInt32(0);
                }
                else
                {
                    datums[i] = output_value((T)flat_array.GetValue(i));
                }
            }
            return array_handler.pldotnet_createDatumArray((int)element_oid, dims.Length, dims, datums, nulls);
        }
    }

    // This is kind of funny.  The syntactic sugar of `T?` varies depending
    // on whether it's a struct type (int, struct, etc) or an object type,
    // so even though the code is apparently the same, these need to be
    // differentiated. See:
    //
    // https://stackoverflow.com/questions/19831157/c-sharp-generic-type-constraint-for-everything-nullable
    // https://stackoverflow.com/questions/69353518/why-t-is-not-a-nullable-type
    // https://stackoverflow.com/questions/9236468/the-type-string-must-be-a-non-nullable-type-in-order-to-use-it-as-parameter-t

    // use this for struct types: int, struct, etc
    public abstract class struct_type_handler<T> : base_type_handler<T> where T : struct
    {
        // converts psql to nullable dotnet type; wraps `input_value()`
        // Given that T is a struct, this `T?` syntactic sugar actually creates a Nullable<T>
        // (I think.)
        public T? input_nullable_value(IntPtr datum, bool isnull)
        {
            return (isnull ? null : input_value(datum));
        }

        public IntPtr output_nullable_value(T? value)
        {
            return (value == null ? int_handler.pldotnet_createDatumInt32(0) : output_value((T)value));
        }
    }

    // use this for object types: string, NpgsqlPoint, etc
    public abstract class object_type_handler<T> : base_type_handler<T> where T : class
    {
        // converts psql to nullable dotnet type; wraps `input_value()`
        // Given that T is a class, this `T?` syntactic sugar merely allows null
        // (I think.)
        public T? input_nullable_value(IntPtr datum, bool isnull)
        {
            return (isnull ? null : input_value(datum));
        }

        public IntPtr output_nullable_value(T? value)
        {
            return (value == null ? int_handler.pldotnet_createDatumInt32(0) : output_value((T)value));
        }
    }

    public class OIDHandler : System.Attribute
    {
        // Use array_type=ANYNONARRAYOID to indicate that there is
        // no array support for this base_type.
        public OID base_type;
        public OID array_type;
        public OIDHandler(OID base_type, OID array_type)
        {
            this.base_type = base_type;
            this.array_type = array_type;
        }
    }

    public class elog
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_Elog(int level, string nessage);

        public static void pldotnet_Info(string message)
        {
            pldotnet_Elog(17, message);
        }
    }
}
