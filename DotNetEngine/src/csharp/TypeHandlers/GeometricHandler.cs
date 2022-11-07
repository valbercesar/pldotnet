using System;
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.POINTOID, OID.POINTARRAYOID)]
    public class point_handler : struct_type_handler<NpgsqlPoint>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPointAttributes(IntPtr datum, ref double x, ref double y);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPoint(double x, double y);

        public override NpgsqlPoint input_value(IntPtr datum)
        {
            double x = 0.0, y = 0.0;
            pldotnet_getDatumPointAttributes(datum, ref x, ref y);
            return new NpgsqlPoint(x, y);
        }

        public override IntPtr output_value(NpgsqlPoint value)
        {
            return pldotnet_createDatumPoint(value.X, value.Y);
        }
    }

    [OIDHandler(OID.LINEOID, OID.LINEARRAYOID)]
    public class line_handler : struct_type_handler<NpgsqlLine>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumLineAttributes(IntPtr datum, ref double a, ref double b, ref double c);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumLine(double a, double b, double c);

        public override NpgsqlLine input_value(IntPtr datum)
        {
            double a = 0.0, b = 0.0, c = 0.0;
            pldotnet_getDatumLineAttributes(datum, ref a, ref b, ref c);
            return new NpgsqlLine(a, b, c);
        }

        public override IntPtr output_value(NpgsqlLine value)
        {
            return pldotnet_createDatumLine(value.A, value.B, value.C);
        }
    }

    [OIDHandler(OID.LSEGOID, OID.LSEGARRAYOID)]
    public class lseg_handler : struct_type_handler<NpgsqlLSeg>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumLineSegmentAttributes(IntPtr datum, ref double x1, ref double y1, ref double x2, ref double y2);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumLineSegment(double x1, double y1, double x2, double y2);

        public override NpgsqlLSeg input_value(IntPtr datum)
        {
            double x1 = 0.0, y1 = 0.0, x2 = 0.0, y2 = 0.0;
            pldotnet_getDatumLineSegmentAttributes(datum, ref x1, ref y1, ref x2, ref y2);
            return new NpgsqlLSeg(x1, y1, x2, y2);
        }

        public override IntPtr output_value(NpgsqlLSeg value)
        {
            return pldotnet_createDatumLineSegment(value.Start.X, value.Start.Y, value.End.X, value.End.Y);
        }
    }

    [OIDHandler(OID.BOXOID, OID.BOXARRAYOID)]
    public class box_handler : struct_type_handler<NpgsqlBox>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumBoxAttributes(IntPtr datum, ref double x1, ref double y1, ref double x2, ref double y2);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBox(double x1, double y1, double x2, double y2);

        public override NpgsqlBox input_value(IntPtr datum)
        {
            double x1 = 0.0, y1 = 0.0, x2 = 0.0, y2 = 0.0;
            pldotnet_getDatumBoxAttributes(datum, ref x1, ref y1, ref x2, ref y2);
            NpgsqlPoint upperRight = new NpgsqlPoint(x1, y1);
            NpgsqlPoint lowerLeft = new NpgsqlPoint(x2, y2);
            return new NpgsqlBox(upperRight, lowerLeft);
        }

        public override IntPtr output_value(NpgsqlBox value)
        {
            var x1 = Math.Max(value.UpperRight.X, value.LowerLeft.X);
            var y1 = Math.Max(value.UpperRight.Y, value.LowerLeft.Y);
            var x2 = Math.Min(value.UpperRight.X, value.LowerLeft.X);
            var y2 = Math.Min(value.UpperRight.Y, value.LowerLeft.Y);
            return pldotnet_createDatumBox(x1, y1, x2, y2);
        }
    }

    [OIDHandler(OID.PATHOID, OID.PATHARRAYOID)]
    public class path_handler : struct_type_handler<NpgsqlPath>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPathAttributes(IntPtr datum, ref int pointNumber, ref int closed);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPathCoordinates(IntPtr datum, double[] xCoordinates, double[] yCoordinates);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPath(int npts, int closed, double[] xCoordinates, double[] yCoordinates);

        public override NpgsqlPath input_value(IntPtr datum)
        {
            int npts = 0, closed = 0;
            pldotnet_getDatumPathAttributes(datum, ref npts, ref closed);
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            bool open = closed == 0 ? true : false;
            NpgsqlPath orig_path = new NpgsqlPath(npts, open);
            pldotnet_getDatumPathCoordinates(datum, xCoordinates, yCoordinates);
            for (int i = 0; i < npts; i++)
            {
                orig_path.Add(new NpgsqlPoint(xCoordinates[i], yCoordinates[i]));
            }
            return orig_path;
        }

        public override IntPtr output_value(NpgsqlPath value)
        {
            int npts = value.Count;
            bool open = value.Open;
            int closed = open ? 0 : 1;
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            for (int i = 0; i < npts; i++)
            {
                xCoordinates[i] = value[i].X;
                yCoordinates[i] = value[i].Y;
            }
            return pldotnet_createDatumPath(npts, closed, xCoordinates, yCoordinates);
        }
    }

    [OIDHandler(OID.POLYGONOID, OID.POLYGONARRAYOID)]
    public class polygon_handler : struct_type_handler<NpgsqlPolygon>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPolygonAttributes(IntPtr datum, ref int pointNumber);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPolygonCoordinates(IntPtr datum, double[] xCoordinates, double[] yCoordinates);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPolygon(int npts, double[] xCoordinates, double[] yCoordinates);

        public override NpgsqlPolygon input_value(IntPtr datum)
        {
            int npts = 0;
            pldotnet_getDatumPolygonAttributes(datum, ref npts);
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            NpgsqlPolygon orig_polygon = new NpgsqlPolygon(npts);
            pldotnet_getDatumPolygonCoordinates(datum, xCoordinates, yCoordinates);
            for (int i = 0; i < npts; i++)
            {
                orig_polygon.Add(new NpgsqlPoint(xCoordinates[i], yCoordinates[i]));
            }
            return orig_polygon;
        }

        public override IntPtr output_value(NpgsqlPolygon value)
        {
            int npts = value.Count;
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            for (int i = 0; i < npts; i++)
            {
                xCoordinates[i] = value[i].X;
                yCoordinates[i] = value[i].Y;
            }
            return pldotnet_createDatumPolygon(npts, xCoordinates, yCoordinates);
        }
    }

    [OIDHandler(OID.CIRCLEOID, OID.CIRCLEARRAYOID)]
    public class circle_handler : struct_type_handler<NpgsqlCircle>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumCircleAttributes(IntPtr datum, ref double x, ref double y, ref double r);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumCircle(double x, double y, double r);

        public override NpgsqlCircle input_value(IntPtr datum)
        {
            double x = 0.0, y = 0.0, r = 0.0;
            pldotnet_getDatumCircleAttributes(datum, ref x, ref y, ref r);
            return new NpgsqlCircle(x, y, r);
        }

        public override IntPtr output_value(NpgsqlCircle value)
        {
            return pldotnet_createDatumCircle(value.Center.X, value.Center.Y, value.Radius);
        }
    }
}