using System;
using System.Net;
using NpgsqlTypes;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace PlDotNET
{
    public static class DotNetBridge
    {   
        // TODO(rosicley) - check if we need this function in the future
        public static NpgsqlPoint AddOne(NpgsqlPoint point)
        {
            return new NpgsqlPoint(point.X + 1, point.Y + 1);
        }

        /// <summary>
        /// This function adds one to each coordinates inside a Point.
        /// Just for prototyping, we can have a way to load this from C, without having to
        /// deal with the function overloarding issue.
        /// </summary>
        // TODO(rosicley) - check if we need this function in the future
        public static NpgsqlPoint AddOneNpgsqlPoint(NpgsqlPoint point)
        {
            return AddOne(point);
        }

        /// <summary>
        /// Free memmory pointed by a IntPtr
        /// </summary>
        // TODO(rosicley) - check if we need this function in the future
        public static unsafe void FreeGenericGCHandle(IntPtr p)
        {
            GCHandle gch = GCHandle.FromIntPtr(p);
            gch.Free();
        }

        public delegate void DelFreeGenericGCHandle(IntPtr p);

        /// <summary>
        /// This function is responsible to build a NpgsqlPoint and return a pointer
        /// </summary>
        // TODO(rosicley) - check if we need this function in the future
        public static unsafe System.IntPtr BuildNpgsqlPoint(double x, double y)
        {
            var point = new NpgsqlPoint(x, y);
            GCHandle handle = GCHandle.Alloc(point, GCHandleType.Pinned);
            return GCHandle.ToIntPtr(handle);
        }

        public delegate System.IntPtr DelBuildNpgsqlPoint(double x, double y);

        /// <summary>
        /// This function is responsible to build a NpsqlBox and return a pointer
        /// </summary>
        // TODO(rosicley) - check if we need this function in the future
        public static unsafe System.IntPtr BuildNpgsqlBox(System.IntPtr _p1, System.IntPtr _p2)
        {
            GCHandle gch_p1 = GCHandle.FromIntPtr(_p1);
            GCHandle gch_p2 = GCHandle.FromIntPtr(_p2);

            var p1 = (NpgsqlPoint)gch_p1.Target;
            var p2 = (NpgsqlPoint)gch_p2.Target;

            Engine.pldotnet_Info("=================================================================");
            Engine.pldotnet_Info("Now, let's see the value of those points after going back from C");
            PrintNpgsqlPoint(p1);
            PrintNpgsqlPoint(p2);
            Engine.pldotnet_Info("=================================================================");

            var box = new NpgsqlBox(p1, p2);

            GCHandle handle = GCHandle.Alloc(box, GCHandleType.Pinned);
            return GCHandle.ToIntPtr(handle);
        }

        // TODO(rosicley) - check if we need this function in the future
        public delegate System.IntPtr DelBuildNpgsqlBox(System.IntPtr _p1, System.IntPtr _p2);

        /// <summary>
        /// This function adds one to each coordinates inside a Box.
        /// Just for prototyping, we can have a way to load this from C, without having to
        /// deal with the function overloarding issue.
        /// </summary>
        // TODO(rosicley) - check if we need this function in the future
        public static unsafe System.IntPtr AddOneNpgsqlBox(System.IntPtr _box)
        {
            GCHandle gch_box = GCHandle.FromIntPtr(_box);

            var box = (NpgsqlBox)gch_box.Target;

            var result = new NpgsqlBox(
                AddOneNpgsqlPoint(box.UpperRight),
                AddOneNpgsqlPoint(box.LowerLeft)
            );

            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            return GCHandle.ToIntPtr(handle);
        }

        public delegate System.IntPtr DelAddOneNpgsqlBox(System.IntPtr _box);

        // TODO(rosicley) - check if we need this function in the future
        public static unsafe System.IntPtr UnpackBox(System.IntPtr _box)
        {
            // TODO
            // we need to write it to the buffer
            // it will be 4 floating point numbers in sequence
            // this should be the same format expected by Postgresq
            // So we can extend the Npgsql Handlers and BufferWriters to
            // handle this case
            // NOTE: just returning the input box to compile ir for now
            return _box;
        }

        // TODO(rosicley) - check if we need this function in the future
        public delegate System.IntPtr DelUnpackBox(System.IntPtr _box);

        // TODO(rosicley) - check if we need this function in the future
        public static void PrintNpgsqlPoint(NpgsqlPoint p)
        {
            Engine.pldotnet_Info($"This is the point {p.X}, {p.Y}");
        }

        // TODO(rosicley) - check if we need this function in the future
        public static unsafe void PrintNpgsqlBox(System.IntPtr _box)
        {
            GCHandle gch_box = GCHandle.FromIntPtr(_box);
            var box = (NpgsqlBox)gch_box.Target;

            Engine.pldotnet_Info("Printing the box");
            PrintNpgsqlPoint(box.LowerLeft);
            PrintNpgsqlPoint(box.UpperRight);
        }

        public delegate void DelPrintNpgsqlBox(System.IntPtr _box);

        // TODO(rosicley) - check if we need this function in the future
        public static unsafe void DeleteGenericList(System.IntPtr _list)
        {
            throw new NotImplementedException();
        }

        public delegate void DelDeleteGenericList(System.IntPtr _list);

        // DONUT
        // Create a new list of IntPtr
        // Intended for pldotnet to pass an array of Datum's to Engine.cs
        public static unsafe System.IntPtr BuildDatumList()
        {
            var l = new List<IntPtr>();
            GCHandle handle = GCHandle.Alloc(l, GCHandleType.Normal);
            return GCHandle.ToIntPtr(handle);
        }
        public delegate System.IntPtr DelBuildDatumList();

        // DONUT
        // Add an IntPtr(Datum) to a list of IntPtr
        // Intended for pldotnet to pass an array of Datum's to Engine.cs
        public static unsafe void AddDatumToList(System.IntPtr _list, System.IntPtr _datum)
        {
            GCHandle gch_list = GCHandle.FromIntPtr(_list);
            var list = (List<IntPtr>)gch_list.Target;
            list.Add(_datum);
        }
        public delegate void DelAddDatumToList(System.IntPtr _list, System.IntPtr _datum);
    }
}
