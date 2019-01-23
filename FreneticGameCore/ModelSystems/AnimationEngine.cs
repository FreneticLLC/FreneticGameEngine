//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.Files;
using FreneticUtilities.FreneticExtensions;
using BEPUutilities;
using FreneticGameCore.UtilitySystems;
using FreneticGameCore.MathHelpers;
using FreneticUtilities.FreneticToolkit;
using FreneticGameCore.CoreSystems;

namespace FreneticGameCore.ModelSystems
{
    /// <summary>
    /// System for animations.
    /// </summary>
    public class AnimationEngine
    {
        /// <summary>
        /// Constructs the animation helper.
        /// </summary>
        public AnimationEngine()
        {
            Animations = new Dictionary<string, SingleAnimation>();
            string[] HBones = new string[] { "neck02", "neck03", "head", "jaw", "levator02.l", "levator02.r", "special01", "special03", "special06.l", "special06.r",
                                             "temporalis01.l", "temporalis01.r", "temporalis02.l", "temporalis02.r", "special04", "oris02", "oris01", "oris06.l",
                                             "oris07.l", "oris06.r", "oris07.r", "tongue00", "tongue01", "tongue02", "tongue03", "tongue04", "tongue07.l", "tongue07.r",
                                             "tongue06.l", "tongue06.r", "tongue05.l", "tongue05.r", "levator03.l", "levator04.l", "levator05.l", "levator03.r",
                                             "levator04.r", "levator05.r", "oris04.l", "oris03.l", "oris04.r", "oris03.r", "oris06", "oris05", "levator06.l", "levator06.r",
                                             "special05.l", "eye.l", "orbicularis03.l", "orbicularis04.l", "special05.r", "eye.r", "orbicularis03.r", "orbicularis04.r",
                                             "oculi02.l", "oculi01.l", "oculi02.r", "oculi01.r", "risorius02.l", "risorius03.l", "risorius02.r", "risorius03.r" };
            string[] LBones = new string[] { "pelvis.l", "upperleg01.l", "upperleg02.l", "lowerleg01.l", "lowerleg02.l", "foot.l", "toe1-1.l", "toe1-2.l",
                                             "toe2-1.l", "toe2-2.l", "toe2-3.l", "toe3-1.l", "toe3-2.l", "toe3-3.l", "toe4-1.l", "toe4-2.l", "toe4-3.l",
                                             "toe5-1.l", "toe5-2.l", "toe5-3.l", "pelvis.r", "upperleg01.r", "upperleg02.r", "lowerleg01.r", "lowerleg02.r",
                                             "foot.r", "toe1-1.r", "toe1-2.r", "toe2-1.r", "toe2-2.r", "toe2-3.r", "toe3-1.r", "toe3-2.r", "toe3-3.r",
                                             "toe4-1.r", "toe4-2.r", "toe4-3.r", "toe5-1.r", "toe5-2.r", "toe5-3.r" };
            foreach (string str in HBones)
            {
                HeadBones.Add(str);
            }
            foreach (string str in LBones)
            {
                LegBones.Add(str);
            }
        }

        /// <summary>
        /// The usual head bones.
        /// </summary>
        public HashSet<string> HeadBones = new HashSet<string>();
        //public HashSet<string> TorsoBones = new HashSet<string>();
        /// <summary>
        /// The usual leg bones.
        /// </summary>
        public HashSet<string> LegBones = new HashSet<string>();

        /// <summary>
        /// All known animations.
        /// </summary>
        public Dictionary<string, SingleAnimation> Animations;

        /// <summary>
        /// Gets an animation by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="Files">The file system.</param>
        /// <returns>The animation.</returns>
        public SingleAnimation GetAnimation(string name, FileHandler Files)
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
                SysConsole.Output(OutputType.ERROR, "Loading an animation: " + ex.ToString());
                sa = new SingleAnimation() { Name = namelow, Length = 1, Engine = this };
                Animations.Add(sa.Name, sa);
                return sa;
            }
        }

        /// <summary>
        /// Loads an animation by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="Files">The file system.</param>
        /// <returns>The animation.</returns>
        SingleAnimation LoadAnimation(string name, FileHandler Files)
        {
            if (Files.Exists("animations/" + name + ".anim"))
            {
                SingleAnimation created = new SingleAnimation() { Name = name };
                string[] data = Files.ReadText("animations/" + name + ".anim").SplitFast('\n');
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
                    List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
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
                            SysConsole.Output(OutputType.WARNING, "Invalid key dat: " + dat[0]);
                        }
                        else
                        {
                            string key = dat[0].Trim();
                            string value = dat[1].Substring(0, dat[1].Length - 1).Trim();
                            entries.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                    bool isgeneral = type == "general" && entr == 0;
                    SingleAnimationNode node = null;
                    if (!isgeneral)
                    {
                        node = new SingleAnimationNode() { Name = type.ToLowerFast() };
                    }
                    foreach (KeyValuePair<string, string> entry in entries)
                    {
                        if (isgeneral)
                        {
                            if (entry.Key == "length")
                            {
                                created.Length = StringConversionHelper.StringToDouble(entry.Value);
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown GENERAL key: " + entry.Key);
                            }
                        }
                        else
                        {
                            if (entry.Key == "positions")
                            {
                                string[] poses = entry.Value.SplitFast(' ');
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
                            else if (entry.Key == "rotations")
                            {
                                string[] rots = entry.Value.SplitFast(' ');
                                for (int x = 0; x < rots.Length; x++)
                                {
                                    if (rots[x].Length > 0)
                                    {
                                        string[] posdata = rots[x].SplitFast('=');
                                        node.RotTimes.Add(StringConversionHelper.StringToDouble(posdata[0]));
                                        node.Rotations.Add(new BEPUutilities.Quaternion(StringConversionHelper.StringToFloat(posdata[1]), StringConversionHelper.StringToFloat(posdata[2]),
                                            StringConversionHelper.StringToFloat(posdata[3]), StringConversionHelper.StringToFloat(posdata[4])));
                                    }
                                }
                            }
                            else if (entry.Key == "parent")
                            {
                                node.ParentName = entry.Value.ToLowerFast();
                            }
                            else if (entry.Key == "offset")
                            {
                                string[] posdata = entry.Value.SplitFast('=');
                                node.Offset = new Location(StringConversionHelper.StringToFloat(posdata[0]),
                                    StringConversionHelper.StringToFloat(posdata[1]), StringConversionHelper.StringToFloat(posdata[2]));
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown NODE key: " + entry.Key);
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

    /// <summary>
    /// Represents one single animation.
    /// </summary>
    public class SingleAnimation
    {
        /// <summary>
        /// The name of the animation.
        /// </summary>
        public string Name;

        /// <summary>
        /// The length in seconds.
        /// </summary>
        public double Length;

        /// <summary>
        /// The backing engine.
        /// </summary>
        public AnimationEngine Engine;

        /// <summary>
        /// All nodes in the animation.
        /// </summary>
        public List<SingleAnimationNode> Nodes = new List<SingleAnimationNode>();

        /// <summary>
        /// A mapping of nodes.
        /// </summary>
        public Dictionary<string, SingleAnimationNode> node_map = new Dictionary<string, SingleAnimationNode>();

        /// <summary>
        /// Gets a node by name.
        /// </summary>
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

    /// <summary>
    /// A single node in an animation.
    /// </summary>
    public class SingleAnimationNode
    {
        /// <summary>
        /// Tne name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// The parent node.
        /// </summary>
        public SingleAnimationNode Parent = null;

        /// <summary>
        /// The name of the parent.
        /// </summary>
        public string ParentName;

        /// <summary>
        /// The offset.
        /// </summary>
        public Location Offset;

        /// <summary>
        /// The position time stamps.
        /// </summary>
        public List<double> PosTimes = new List<double>();

        /// <summary>
        /// The positions.
        /// </summary>
        public List<Location> Positions = new List<Location>();

        /// <summary>
        /// The rotation time stamps.
        /// </summary>
        public List<double> RotTimes = new List<double>();

        /// <summary>
        /// The rotations.
        /// </summary>
        public List<BEPUutilities.Quaternion> Rotations = new List<BEPUutilities.Quaternion>();

        /// <summary>
        /// Finds a position by time.
        /// </summary>
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

        /// <summary>
        /// Lerps a position by time.
        /// </summary>
        /// <param name="aTime">The time.</param>
        /// <returns>The position.</returns>
        public Vector3 LerpPos(double aTime)
        {
            if (Positions.Count == 0)
            {
                return new Vector3(0, 0, 0);
            }
            if (Positions.Count == 1)
            {
                Location pos = Positions[0];
                return new Vector3((double)pos.X, (double)pos.Y, (double)pos.Z);
            }
            int index = FindPos(aTime);
            int nextIndex = index + 1;
            if (nextIndex >= Positions.Count)
            {
                Location pos = Positions[0];
                return new Vector3((double)pos.X, (double)pos.Y, (double)pos.Z);
            }
            double deltaT = PosTimes[nextIndex] - PosTimes[index];
            double factor = (aTime - PosTimes[index]) / deltaT;
            if (factor < 0 || factor > 1)
            {
                Location pos = Positions[0];
                return new Vector3((double)pos.X, (double)pos.Y, (double)pos.Z);
            }
            Location start = Positions[index];
            Location end = Positions[nextIndex];
            Location deltaV = end - start;
            Location npos = start + (double)factor * deltaV;
            return new Vector3((double)npos.X, (double)npos.Y, (double)npos.Z);
        }

        /// <summary>
        /// Finds a rotation by time.
        /// </summary>
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

        /// <summary>
        /// Lerps a rotation by time.
        /// </summary>
        /// <param name="aTime">The time.</param>
        /// <returns>The rotation.</returns>
        public BEPUutilities.Quaternion LerpRotate(double aTime)
        {
            if (Rotations.Count == 0)
            {
                return BEPUutilities.Quaternion.Identity;
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
            BEPUutilities.Quaternion start = Rotations[index];
            BEPUutilities.Quaternion end = Rotations[nextIndex];
            BEPUutilities.Quaternion res = BEPUutilities.Quaternion.Slerp(start, end, (double)factor);
            res.Normalize();
            return res;
        }

        /// <summary>
        /// Gets the final matrix for a bone.
        /// </summary>
        /// <param name="aTime">The time.</param>
        /// <param name="adjs">The adjustments if any.</param>
        /// <returns>The resultant matrix.</returns>
        public Matrix GetBoneTotalMatrix(double aTime, Dictionary<string, Matrix> adjs = null)
        {
            Matrix pos = Matrix.CreateTranslation(LerpPos(aTime));
            Matrix rot = Matrix.CreateFromQuaternion(LerpRotate(aTime));
            pos.Transpose();
            rot.Transpose();
            Matrix combined;
            if (adjs != null && adjs.TryGetValue(Name, out Matrix t))
            {
                combined = pos * rot * t;
            }
            else
            {
                combined = pos * rot;
            }
            if (Parent != null)
            {
                combined = Parent.GetBoneTotalMatrix(aTime, adjs) * combined;
                //combined *= Parent.GetBoneTotalMatrix(aTime);
            }
            return combined;
        }
    }
}
