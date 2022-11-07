using System;
using System.Runtime.InteropServices;

namespace PlDotNET_Handler
{
    public static class array_handler
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int get_maxdim();
        public static int maxdim = get_maxdim();

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getArrayAttributes(IntPtr datum, ref int type_id, ref int ndims, int[] dims, byte** nullmap);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe int pldotnet_getArrayDatum(IntPtr array_datum, IntPtr[] results, int nelems, int type_id);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumArray(int element_id, int dimNumber, int[] dimLengths, IntPtr[] datums, byte[] nullmap = null);

        /// <summary>
        /// This is a recursive function that aims to create a multidimensional
        /// array from an one-dimensional one. The dimensions of the
        /// multidimensional array must be specified when you create the "Array"
        /// object that you will pass for "multi_array". For that purpose, you
        /// can use the "Array.CreateInstance()" method. Also, to use this
        /// function, you should pass only the first two arguments!
        /// </summary>
        /// <param name="original_array">The original one-dimensional array.</param>
        /// <param name="multi_array"> It is an empty array with the new
        /// dimensions, which must be passed by reference, so the variable you
        /// pass here will be modified to store the elements of the "original_array".
        /// </param>
        /// <param name="auxiliar"> This one is only an integer array that helps
        /// to set the values to the new multidimensional array. You can pass
        /// an empty array with with length equal to the number of dimensions.
        /// However, you don't need! </param>
        /// <param name="contEl"> This one is an element counter, so you can pass
        /// 0, but you don't need! </param>
        /// <param name="loc"> This one is an auxiliary integer that helps to
        /// loop over the dimensions of the new array, so you can pass 1, but
        /// you don't need! </param>
        /// <returns> Returns the number of elements of the original_array. </returns>
        public static int reshapeArray(Array original_array, ref Array multi_array, int[] auxiliar = null, int contEl = 0, int loc = 1)
        {
            if (contEl >= original_array.Length)
                return contEl;
            else if (contEl == 0 || auxiliar == null)
                auxiliar = new int[multi_array.Rank];
            int ndim = multi_array.Rank;
            int[] dim = new int[ndim];
            for (int i = 0; i < ndim; i++)
                dim[i] = multi_array.GetLength(i);

            if (loc == 1)
                for (int i = 0; i < dim[ndim - loc]; i++)
                {
                    multi_array.SetValue(original_array.GetValue(contEl++), auxiliar);
                    auxiliar[ndim - loc] += 1;
                }
            for (int i = 1; i < loc; i++)
            {
                auxiliar[ndim - loc] += 1;
                if (auxiliar[ndim - loc] < dim[ndim - loc])
                    contEl = reshapeArray(original_array, ref multi_array, auxiliar, contEl, i);
            }
            auxiliar[ndim - loc] = 0;
            contEl = reshapeArray(original_array, ref multi_array, auxiliar, contEl, ++loc);
            return contEl;
        }

        /// <summary>
        /// This is a recursive function that aims to create an one-dimensional
        /// array from a multidimensional one. The "flat_array" must be created
        /// with the same length as the "original_array", and you can use the
        /// "Array.CreateInstance()" method for that.
        /// Also, to use this function, you should pass only the first two arguments!
        /// </summary>
        /// <param name="original_array">The original multidimensional dimensional array.</param>
        /// <param name="flat_array"> It is an empty one-dimensional array with
        /// length equals to the "original_array". This argument must be
        /// passed by reference, so the variable you pass here will be modified
        /// to store the elements of the "original_array".</param>
        /// <param name="auxiliar"> This one is only an integer array that helps
        /// to set the values to the new flat array. You can pass an empty array
        /// with with length equal to the number of dimensions of the original_array.
        /// However, you don't need! </param>
        /// <param name="contEl"> This one is an element counter, so you can pass
        /// 0, but you don't need! </param>
        /// <param name="loc"> This one is an auxiliary integer that helps to
        /// loop over the dimensions of the new array, so you can pass 1, but
        /// you don't need! </param>
        /// <returns> Returns the number of elements of the original_array </returns>
        public static int flatArray(Array original_array, ref Array flat_array, int[] auxiliar = null, int contEl = 0, int loc = 1)
        {
            if (contEl >= original_array.Length)
                return contEl;
            else if (contEl == 0 || auxiliar == null)
                auxiliar = new int[original_array.Rank];

            int ndim = original_array.Rank;
            int[] dim = new int[ndim];
            for (int i = 0; i < ndim; i++)
                dim[i] = original_array.GetLength(i);

            if (loc == 1)
                for (int i = 0; i < dim[ndim - loc]; i++)
                {
                    flat_array.SetValue(original_array.GetValue(auxiliar), contEl++);
                    auxiliar[ndim - loc] += 1;
                }
            for (int i = 1; i < loc; i++)
            {
                auxiliar[ndim - loc] += 1;
                if (auxiliar[ndim - loc] < dim[ndim - loc])
                    contEl = flatArray(original_array, ref flat_array, auxiliar, contEl, i);
            }
            auxiliar[ndim - loc] = 0;
            contEl = flatArray(original_array, ref flat_array, auxiliar, contEl, ++loc);
            return contEl;
        }
    }
}