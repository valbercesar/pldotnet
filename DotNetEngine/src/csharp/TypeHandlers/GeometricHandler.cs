using System;
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace PlDotNET_Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL point data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.POINTOID, OID.POINTARRAYOID)]
    public class PointHandler : StructTypeHandler<NpgsqlPoint>
    {
        public PointHandler()
        {
            this.ElementOID = OID.POINTOID;
            this.ArrayOID = OID.POINTARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPointAttributes(IntPtr datum, ref double x, ref double y);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPoint(double x, double y);

        /// <inheritdoc />
        public override NpgsqlPoint InputValue(IntPtr datum)
        {
            double x = 0.0, y = 0.0;
            pldotnet_getDatumPointAttributes(datum, ref x, ref y);
            return new NpgsqlPoint(x, y);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlPoint value)
        {
            return pldotnet_createDatumPoint(value.X, value.Y);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL line data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.LINEOID, OID.LINEARRAYOID)]
    public class LineHandler : StructTypeHandler<NpgsqlLine>
    {
        public LineHandler()
        {
            this.ElementOID = OID.LINEOID;
            this.ArrayOID = OID.LINEARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumLineAttributes(IntPtr datum, ref double a, ref double b, ref double c);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumLine(double a, double b, double c);

        /// <inheritdoc />
        public override NpgsqlLine InputValue(IntPtr datum)
        {
            double a = 0.0, b = 0.0, c = 0.0;
            pldotnet_getDatumLineAttributes(datum, ref a, ref b, ref c);
            return new NpgsqlLine(a, b, c);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlLine value)
        {
            return pldotnet_createDatumLine(value.A, value.B, value.C);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL lseg data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.LSEGOID, OID.LSEGARRAYOID)]
    public class LineSegmentHandler : StructTypeHandler<NpgsqlLSeg>
    {
        public LineSegmentHandler()
        {
            this.ElementOID = OID.LSEGOID;
            this.ArrayOID = OID.LSEGARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumLineSegmentAttributes(IntPtr datum, ref double x1, ref double y1, ref double x2, ref double y2);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumLineSegment(double x1, double y1, double x2, double y2);

        /// <inheritdoc />
        public override NpgsqlLSeg InputValue(IntPtr datum)
        {
            double x1 = 0.0, y1 = 0.0, x2 = 0.0, y2 = 0.0;
            pldotnet_getDatumLineSegmentAttributes(datum, ref x1, ref y1, ref x2, ref y2);
            return new NpgsqlLSeg(x1, y1, x2, y2);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlLSeg value)
        {
            return pldotnet_createDatumLineSegment(value.Start.X, value.Start.Y, value.End.X, value.End.Y);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL box data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.BOXOID, OID.BOXARRAYOID)]
    public class BoxHandler : StructTypeHandler<NpgsqlBox>
    {
        public BoxHandler()
        {
            this.ElementOID = OID.BOXOID;
            this.ArrayOID = OID.BOXARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumBoxAttributes(IntPtr datum, ref double x1, ref double y1, ref double x2, ref double y2);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBox(double x1, double y1, double x2, double y2);

        /// <inheritdoc />
        public override NpgsqlBox InputValue(IntPtr datum)
        {
            double x1 = 0.0, y1 = 0.0, x2 = 0.0, y2 = 0.0;
            pldotnet_getDatumBoxAttributes(datum, ref x1, ref y1, ref x2, ref y2);
            NpgsqlPoint upperRight = new NpgsqlPoint(x1, y1);
            NpgsqlPoint lowerLeft = new NpgsqlPoint(x2, y2);
            return new NpgsqlBox(upperRight, lowerLeft);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlBox value)
        {
            var x1 = Math.Max(value.UpperRight.X, value.LowerLeft.X);
            var y1 = Math.Max(value.UpperRight.Y, value.LowerLeft.Y);
            var x2 = Math.Min(value.UpperRight.X, value.LowerLeft.X);
            var y2 = Math.Min(value.UpperRight.Y, value.LowerLeft.Y);
            return pldotnet_createDatumBox(x1, y1, x2, y2);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL path data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.PATHOID, OID.PATHARRAYOID)]
    public class PathHandler : StructTypeHandler<NpgsqlPath>
    {
        public PathHandler()
        {
            this.ElementOID = OID.PATHOID;
            this.ArrayOID = OID.PATHARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPathAttributes(IntPtr datum, ref int pointNumber, ref int closed);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPathCoordinates(IntPtr datum, double[] xCoordinates, double[] yCoordinates);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPath(int npts, int closed, double[] xCoordinates, double[] yCoordinates);

        /// <inheritdoc />
        public override NpgsqlPath InputValue(IntPtr datum)
        {
            int npts = 0, closed = 0;
            pldotnet_getDatumPathAttributes(datum, ref npts, ref closed);
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            bool open = closed == 0 ? true : false;
            NpgsqlPath origPath = new NpgsqlPath(npts, open);
            pldotnet_getDatumPathCoordinates(datum, xCoordinates, yCoordinates);
            for (int i = 0; i < npts; i++)
            {
                origPath.Add(new NpgsqlPoint(xCoordinates[i], yCoordinates[i]));
            }
            return origPath;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlPath value)
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

    /// <summary>
    /// A type handler for the PostgreSQL polygon data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.POLYGONOID, OID.POLYGONARRAYOID)]
    public class PolygonHandler : StructTypeHandler<NpgsqlPolygon>
    {
        public PolygonHandler()
        {
            this.ElementOID = OID.POLYGONOID;
            this.ArrayOID = OID.POLYGONARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPolygonAttributes(IntPtr datum, ref int pointNumber);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumPolygonCoordinates(IntPtr datum, double[] xCoordinates, double[] yCoordinates);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumPolygon(int npts, double[] xCoordinates, double[] yCoordinates);

        /// <inheritdoc />
        public override NpgsqlPolygon InputValue(IntPtr datum)
        {
            int npts = 0;
            pldotnet_getDatumPolygonAttributes(datum, ref npts);
            double[] xCoordinates = new double[npts];
            double[] yCoordinates = new double[npts];
            NpgsqlPolygon origPolygon = new NpgsqlPolygon(npts);
            pldotnet_getDatumPolygonCoordinates(datum, xCoordinates, yCoordinates);
            for (int i = 0; i < npts; i++)
            {
                origPolygon.Add(new NpgsqlPoint(xCoordinates[i], yCoordinates[i]));
            }
            return origPolygon;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlPolygon value)
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

    /// <summary>
    /// A type handler for the PostgreSQL circle data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    /// </remarks>
    [OIDHandler(OID.CIRCLEOID, OID.CIRCLEARRAYOID)]
    public class CircleHandler : StructTypeHandler<NpgsqlCircle>
    {
        public CircleHandler()
        {
            this.ElementOID = OID.CIRCLEOID;
            this.ArrayOID = OID.CIRCLEARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumCircleAttributes(IntPtr datum, ref double x, ref double y, ref double r);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumCircle(double x, double y, double r);

        /// <inheritdoc />
        public override NpgsqlCircle InputValue(IntPtr datum)
        {
            double x = 0.0, y = 0.0, r = 0.0;
            pldotnet_getDatumCircleAttributes(datum, ref x, ref y, ref r);
            return new NpgsqlCircle(x, y, r);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlCircle value)
        {
            return pldotnet_createDatumCircle(value.Center.X, value.Center.Y, value.Radius);
        }
    }
}