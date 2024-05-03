//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;

namespace FGECore.PropertySystem;

/// <summary>Helper class to manage property saving/loading.</summary>
public class PropertySaveSystem
{
    /// <summary>All type saver methods.</summary>
    public static Dictionary<Type, PropertySaverLoader> TypeSavers = new(1024);

    /// <summary>All type loader methods.</summary>
    public static Dictionary<string, PropertySaverLoader> TypeLoaders = new(1024);

    /// <summary>Ensures initialization.</summary>
    static PropertySaveSystem()
    {
        Internal.EnsureInit();
    }

    /// <summary>Internal data used by the <see cref="PropertySaveSystem"/>.</summary>
    public static class Internal
    {
        /// <summary>Whether the system is already inited.</summary>
        public static bool Initted = false;

        /// <summary>Configures the default set of savers and readers for the FGE core.</summary>
        public static void EnsureInit()
        {
            if (Initted)
            {
                return;
            }
            Initted = true;
            // Core Helpers
            TypeSavers.Add(typeof(bool), new PropertySaverLoader()
            {
                Saver = (o) => [(byte)(((bool)o) ? 1 : 0)],
                Loader = (b) => b[0] != 0,
                SaveString = "C/bool"
            });
            TypeSavers.Add(typeof(byte), new PropertySaverLoader()
            {
                Saver = (o) => [(byte)o],
                Loader = (b) => b[0],
                SaveString = "C/byte"
            });
            TypeSavers.Add(typeof(sbyte), new PropertySaverLoader()
            {
                Saver = (o) => [unchecked((byte)((sbyte)o))],
                Loader = (b) => unchecked((sbyte)(b[0])),
                SaveString = "C/sbyte"
            });
            TypeSavers.Add(typeof(ushort), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((ushort)o),
                Loader = (b) => BitConverter.ToUInt16(b, 0),
                SaveString = "C/ushort"
            });
            TypeSavers.Add(typeof(short), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((short)o),
                Loader = (b) => BitConverter.ToInt16(b, 0),
                SaveString = "C/short"
            });
            TypeSavers.Add(typeof(uint), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((uint)o),
                Loader = (b) => BitConverter.ToUInt32(b, 0),
                SaveString = "C/uint"
            });
            TypeSavers.Add(typeof(int), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((int)o),
                Loader = (b) => BitConverter.ToInt32(b, 0),
                SaveString = "C/int"
            });
            TypeSavers.Add(typeof(ulong), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((ulong)o),
                Loader = (b) => BitConverter.ToUInt64(b, 0),
                SaveString = "C/ulong"
            });
            TypeSavers.Add(typeof(long), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((long)o),
                Loader = (b) => BitConverter.ToInt64(b, 0),
                SaveString = "C/long"
            });
            TypeSavers.Add(typeof(float), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((float)o),
                Loader = (b) => BitConverter.ToSingle(b, 0),
                SaveString = "C/float"
            });
            TypeSavers.Add(typeof(double), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((double)o),
                Loader = (b) => BitConverter.ToDouble(b, 0),
                SaveString = "C/double"
            });
            TypeSavers.Add(typeof(string), new PropertySaverLoader()
            {
                Saver = (o) => StringConversionHelper.UTF8Encoding.GetBytes(o as string),
                Loader = (b) => StringConversionHelper.UTF8Encoding.GetString(b),
                SaveString = "C/string"
            });
            // FGE/Core Helpers
            TypeSavers.Add(typeof(Location), new PropertySaverLoader()
            {
                Saver = (o) => ((Location)o).ToDoubleBytes(),
                Loader = (b) => Location.FromDoubleBytes(b, 0),
                SaveString = "C/location"
            });
            TypeSavers.Add(typeof(Color3F), new PropertySaverLoader()
            {
                Saver = (o) => ((Color3F)o).ToBytes(),
                Loader = (b) => Color3F.FromBytes(b),
                SaveString = "C/color3f"
            });
            TypeSavers.Add(typeof(Color4F), new PropertySaverLoader()
            {
                Saver = (o) => ((Color4F)o).ToBytes(),
                Loader = (b) => Color4F.FromBytes(b),
                SaveString = "C/color4f"
            });
            TypeSavers.Add(typeof(Quaternion), new PropertySaverLoader()
            {
                Saver = (o) => ((Quaternion)o).ToDoubleBytes(),
                Loader = (b) => Quaternion.FromDoubleBytes(b, 0),
                SaveString = "C/quaternion"
            });
            // End default helpers
            foreach (PropertySaverLoader psl in TypeSavers.Values)
            {
                TypeLoaders.Add(psl.SaveString, psl);
            }
        }
    }
}
