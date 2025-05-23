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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.UtilitySystems;

namespace FGECore.ModelSystems;

/// <summary>System for animations.</summary>
public class AnimationEngine
{
    /// <summary>Constructs the animation helper.</summary>
    public AnimationEngine()
    {
        Animations = [];
        string[] HBones = [ "neck02", "neck03", "head", "jaw", "levator02.l", "levator02.r", "special01", "special03", "special06.l", "special06.r",
                                         "temporalis01.l", "temporalis01.r", "temporalis02.l", "temporalis02.r", "special04", "oris02", "oris01", "oris06.l",
                                         "oris07.l", "oris06.r", "oris07.r", "tongue00", "tongue01", "tongue02", "tongue03", "tongue04", "tongue07.l", "tongue07.r",
                                         "tongue06.l", "tongue06.r", "tongue05.l", "tongue05.r", "levator03.l", "levator04.l", "levator05.l", "levator03.r",
                                         "levator04.r", "levator05.r", "oris04.l", "oris03.l", "oris04.r", "oris03.r", "oris06", "oris05", "levator06.l", "levator06.r",
                                         "special05.l", "eye.l", "orbicularis03.l", "orbicularis04.l", "special05.r", "eye.r", "orbicularis03.r", "orbicularis04.r",
                                         "oculi02.l", "oculi01.l", "oculi02.r", "oculi01.r", "risorius02.l", "risorius03.l", "risorius02.r", "risorius03.r" ];
        string[] LBones = [ "pelvis.l", "upperleg01.l", "upperleg02.l", "lowerleg01.l", "lowerleg02.l", "foot.l", "toe1-1.l", "toe1-2.l",
                                         "toe2-1.l", "toe2-2.l", "toe2-3.l", "toe3-1.l", "toe3-2.l", "toe3-3.l", "toe4-1.l", "toe4-2.l", "toe4-3.l",
                                         "toe5-1.l", "toe5-2.l", "toe5-3.l", "pelvis.r", "upperleg01.r", "upperleg02.r", "lowerleg01.r", "lowerleg02.r",
                                         "foot.r", "toe1-1.r", "toe1-2.r", "toe2-1.r", "toe2-2.r", "toe2-3.r", "toe3-1.r", "toe3-2.r", "toe3-3.r",
                                         "toe4-1.r", "toe4-2.r", "toe4-3.r", "toe5-1.r", "toe5-2.r", "toe5-3.r" ];
        foreach (string str in HBones)
        {
            HeadBones.Add(str);
        }
        foreach (string str in LBones)
        {
            LegBones.Add(str);
        }
    }

    /// <summary>The usual head bones.</summary>
    public HashSet<string> HeadBones = [];
    //public HashSet<string> TorsoBones = new HashSet<string>();
    /// <summary>The usual leg bones.</summary>
    public HashSet<string> LegBones = [];

    /// <summary>All known animations.</summary>
    public Dictionary<string, SingleAnimation> Animations;

    /// <summary>Gets an animation by name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="Files">The file system.</param>
    /// <returns>The animation.</returns>
    public SingleAnimation GetAnimation(string name, FileEngine Files)
    {
        string namelow = name.ToLowerFast();
        if (Animations.TryGetValue(namelow, out SingleAnimation sa))
        {
            return sa;
        }
        try
        {
            sa = LoadAnimation(namelow, Files);
            Animations.Add(sa.Name, sa);
            return sa;
        }
        catch (Exception ex)
        {
            Logs.Error($"Loading an animation: {ex}");
            sa = new() { Name = namelow, Length = 1, Engine = this };
            Animations.Add(sa.Name, sa);
            return sa;
        }
    }

    /// <summary>Loads an animation by name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="Files">The file system.</param>
    /// <returns>The animation.</returns>
    SingleAnimation LoadAnimation(string name, FileEngine Files)
    {
        if (Files.TryReadFileText("animations/" + name + ".anim", out string fileText))
        {
            SingleAnimation created = new() { Name = name };
            string[] data = fileText.SplitFast('\n');
            int entr = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("//"))
                {
                    continue;
                }
                string type = data[i];
                if (data.Length <= i + 1 || data[i + 1] != "{")
                {
                    break;
                }
                List<(string, string)> entries = [];
                for (i += 2; i < data.Length; i++)
                {
                    if (data[i].Trim().StartsWith("//"))
                    {
                        continue;
                    }
                    if (data[i] == "}")
                    {
                        break;
                    }
                    string[] dat = data[i].SplitFast(':');
                    if (dat.Length <= 1)
                    {
                        Logs.Warning($"Invalid key dat: {dat[0]}");
                    }
                    else
                    {
                        string key = dat[0].Trim();
                        string value = dat[1][0..^1].Trim();
                        entries.Add((key, value));
                    }
                }
                bool isgeneral = type == "general" && entr == 0;
                SingleAnimationNode node = null;
                if (!isgeneral)
                {
                    node = new SingleAnimationNode() { Name = type.ToLowerFast() };
                }
                foreach ((string key, string value) in entries)
                {
                    if (isgeneral)
                    {
                        if (key == "length")
                        {
                            created.Length = StringConversionHelper.StringToDouble(value);
                        }
                        else
                        {
                            Logs.Warning($"Unknown GENERAL key: {key}");
                        }
                    }
                    else
                    {
                        if (key == "positions")
                        {
                            string[] poses = value.SplitFast(' ');
                            for (int x = 0; x < poses.Length; x++)
                            {
                                if (poses[x].Length > 0)
                                {
                                    string[] posdata = poses[x].SplitFast('=');
                                    node.PosTimes.Add(StringConversionHelper.StringToDouble(posdata[0]));
                                    node.Positions.Add(new Location(StringConversionHelper.StringToFloat(posdata[1]),
                                        StringConversionHelper.StringToFloat(posdata[2]), StringConversionHelper.StringToFloat(posdata[3])));
                                }
                            }
                        }
                        else if (key == "rotations")
                        {
                            string[] rots = value.SplitFast(' ');
                            for (int x = 0; x < rots.Length; x++)
                            {
                                if (rots[x].Length > 0)
                                {
                                    string[] posdata = rots[x].SplitFast('=');
                                    node.RotTimes.Add(StringConversionHelper.StringToDouble(posdata[0]));
                                    node.Rotations.Add(new MathHelpers.Quaternion(StringConversionHelper.StringToFloat(posdata[1]), StringConversionHelper.StringToFloat(posdata[2]),
                                        StringConversionHelper.StringToFloat(posdata[3]), StringConversionHelper.StringToFloat(posdata[4])));
                                }
                            }
                        }
                        else if (key == "parent")
                        {
                            node.ParentName = value.ToLowerFast();
                        }
                        else if (key == "offset")
                        {
                            string[] posdata = value.SplitFast('=');
                            node.Offset = new Location(StringConversionHelper.StringToFloat(posdata[0]),
                                StringConversionHelper.StringToFloat(posdata[1]), StringConversionHelper.StringToFloat(posdata[2]));
                        }
                        else
                        {
                            Logs.Warning($"Unknown NODE key: {key}");
                        }
                    }
                }
                if (!isgeneral)
                {
                    created.Nodes.Add(node);
                    created.node_map.Add(node.Name, node);
                }
                entr++;
            }
            foreach (SingleAnimationNode node in created.Nodes)
            {
                for (int i = 0; i < created.Nodes.Count; i++)
                {
                    if (created.Nodes[i].Name == node.ParentName)
                    {
                        node.Parent = created.Nodes[i];
                        break;
                    }
                }
            }
            created.Engine = this;
            return created;
        }
        else
        {
            throw new Exception("Invalid animation file - file not found: animations/" + name + ".anim");
        }
    }
}

/// <summary>Represents one single animation.</summary>
public class SingleAnimation
{
    /// <summary>The name of the animation.</summary>
    public string Name;

    /// <summary>The length in seconds.</summary>
    public double Length;

    /// <summary>The backing engine.</summary>
    public AnimationEngine Engine;

    /// <summary>All nodes in the animation.</summary>
    public List<SingleAnimationNode> Nodes = [];

    /// <summary>A mapping of nodes.</summary>
    public Dictionary<string, SingleAnimationNode> node_map = [];

    /// <summary>Gets a node by name.</summary>
    /// <param name="name">The name of it.</param>
    /// <returns>The node.</returns>
    public SingleAnimationNode GetNode(string name)
    {
        if (node_map.TryGetValue(name, out SingleAnimationNode node))
        {
            return node;
        }
        return null;
    }
}

/// <summary>A single node in an animation.</summary>
public class SingleAnimationNode
{
    /// <summary>Tne name of the node.</summary>
    public string Name;

    /// <summary>The parent node.</summary>
    public SingleAnimationNode Parent = null;

    /// <summary>The name of the parent.</summary>
    public string ParentName;

    /// <summary>The offset.</summary>
    public Location Offset;

    /// <summary>The position time stamps.</summary>
    public List<double> PosTimes = [];

    /// <summary>The positions.</summary>
    public List<Location> Positions = [];

    /// <summary>The rotation time stamps.</summary>
    public List<double> RotTimes = [];

    /// <summary>The rotations.</summary>
    public List<MathHelpers.Quaternion> Rotations = [];

    /// <summary>Finds a position by time.</summary>
    /// <param name="time">The time.</param>
    /// <returns>The position.</returns>
    int FindPos(double time)
    {
        for (int i = 0; i < Positions.Count - 1; i++)
        {
            if (time >= PosTimes[i] && time < PosTimes[i + 1])
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>Lerps a position by time.</summary>
    /// <param name="aTime">The time.</param>
    /// <returns>The position.</returns>
    public Location LerpPos(double aTime)
    {
        if (Positions.Count == 0)
        {
            return Location.Zero;
        }
        if (Positions.Count == 1)
        {
            return Positions[0];
        }
        int index = FindPos(aTime);
        int nextIndex = index + 1;
        if (nextIndex >= Positions.Count)
        {
            return Positions[0];
        }
        double deltaT = PosTimes[nextIndex] - PosTimes[index];
        double factor = (aTime - PosTimes[index]) / deltaT;
        if (factor < 0 || factor > 1)
        {
            return Positions[0];
        }
        Location start = Positions[index];
        Location end = Positions[nextIndex];
        Location deltaV = end - start;
        Location npos = start + (double)factor * deltaV;
        return npos;
    }

    /// <summary>Finds a rotation by time.</summary>
    /// <param name="time">The time.</param>
    /// <returns>The rotation.</returns>
    int FindRotate(double time)
    {
        for (int i = 0; i < Rotations.Count - 1; i++)
        {
            if (time >= RotTimes[i] && time < RotTimes[i + 1])
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>Lerps a rotation by time.</summary>
    /// <param name="aTime">The time.</param>
    /// <returns>The rotation.</returns>
    public MathHelpers.Quaternion LerpRotate(double aTime)
    {
        if (Rotations.Count == 0)
        {
            return MathHelpers.Quaternion.Identity;
        }
        if (Rotations.Count == 1)
        {
            return Rotations[0];
        }
        int index = FindRotate(aTime);
        int nextIndex = index + 1;
        if (nextIndex >= Rotations.Count)
        {
            return Rotations[0];
        }
        double deltaT = RotTimes[nextIndex] - RotTimes[index];
        double factor = (aTime - RotTimes[index]) / deltaT;
        if (factor < 0 || factor > 1)
        {
            return Rotations[0];
        }
        MathHelpers.Quaternion start = Rotations[index];
        MathHelpers.Quaternion end = Rotations[nextIndex];
        MathHelpers.Quaternion res = start.Slerp(end, factor);
        return res.Normalized();
    }

    /// <summary>Gets the final matrix for a bone.</summary>
    /// <param name="aTime">The time.</param>
    /// <param name="adjs">The adjustments if any.</param>
    /// <returns>The resultant matrix.</returns>
    public Matrix4x4 GetBoneTotalMatrix(double aTime, Dictionary<string, Matrix4x4> adjs = null)
    {
        Matrix4x4 pos = Matrix4x4.CreateTranslation(LerpPos(aTime).ToNumerics());
        Matrix4x4 rot = Matrix4x4.CreateFromQuaternion(LerpRotate(aTime).ToNumerics());
        Matrix4x4 combined = rot * pos;
        if (adjs is not null && adjs.TryGetValue(Name, out Matrix4x4 t))
        {
            // TODO: Why is this transpose needed?
            combined = Matrix4x4.Transpose(t) * combined;
        }
        if (Parent is not null)
        {
            combined *= Parent.GetBoneTotalMatrix(aTime, adjs);
        }
        return combined;
    }
}
