using System;
using System.Collections.Generic;
using System.Numerics;
using Xenvious;

namespace Xenvious.AdvancedPlacement
{
    public sealed class PropPlacementService
    {
        /// <summary>
        /// Props a job may hold: the creator scripts compare the placed-prop count with 200
        /// on Legacy and with 300 on Enhanced.
        /// </summary>
        public static int PropLimit => GameVariant.IsEnhanced ? 300 : 200;

        // Fields that tie a prop to a dynamic prop (f_85) or another group (f_90), -1 = none.
        // A copied block must not inherit them, or the new props join that group.
        private const int FieldLinkedDynamicProp = 85;
        private const int FieldLinkedGroup = 90;

        private static long ElementBase(int index) => GTA.Offsets.Editor.Props.loc + (long)index * GTA.Offsets.Editor.Props.NEXT;

        public int GetCurrentPropCount()
        {
            return new Global(GTA.Offsets.Editor.Props.number).Get<int>();
        }

        public bool CanPlace(int amount, out int availableSlots)
        {
            var current = GetCurrentPropCount();
            availableSlots = Math.Max(0, PropLimit - current);
            return amount <= availableSlots;
        }

        public int GetModel(int index)
        {
            return new Global(GTA.Offsets.Editor.Props.model + (long)index * GTA.Offsets.Editor.Props.NEXT).Get<int>();
        }

        /// <summary>Position and rotation of placed prop <paramref name="index"/>.</summary>
        public PropPose GetPose(int index)
        {
            long off = (long)index * GTA.Offsets.Editor.Props.NEXT;
            var loc = new V3(
                new Global(GTA.Offsets.Editor.Props.loc + 0 + off).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.loc + 1 + off).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.loc + 2 + off).Get<float>());
            var rot = new V3(
                new Global(GTA.Offsets.Editor.Props.vrot + 0 + off).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.vrot + 1 + off).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.vrot + 2 + off).Get<float>());
            return PropPose.FromGta(loc, rot);
        }

        /// <summary>
        /// Appends props. Each new slot first gets a copy of <paramref name="templateIndex"/>
        /// (colour, lod, flags ...) so it does not inherit whatever a deleted prop left in
        /// that slot; then model, position, rotation and heading are written.
        /// </summary>
        public void PlaceProps(IReadOnlyList<(int Model, PropPose Pose)> props, int templateIndex = -1)
        {
            int current = GetCurrentPropCount();
            if (current + props.Count > PropLimit)
            {
                throw new InvalidOperationException("Prop limit reached.");
            }
            if (templateIndex < 0 && current > 0)
            {
                templateIndex = current - 1;
            }

            byte[] block = templateIndex >= 0 ? ReadBlock(templateIndex) : null;

            for (int i = 0; i < props.Count; i++)
            {
                int index = current + i;
                if (block != null)
                {
                    WriteBlock(index, block);
                    new Global(ElementBase(index) + FieldLinkedDynamicProp).SetInt(-1);
                    new Global(ElementBase(index) + FieldLinkedGroup).SetInt(-1);
                }
                WritePose(index, props[i].Model, props[i].Pose);
            }
            new Global(GTA.Offsets.Editor.Props.number).SetInt(current + props.Count);
        }

        private static void WritePose(int index, int model, in PropPose pose)
        {
            long off = (long)index * GTA.Offsets.Editor.Props.NEXT;
            V3 e = pose.EulerDeg;
            new Global(GTA.Offsets.Editor.Props.model + off).SetInt(model);
            new Global(GTA.Offsets.Editor.Props.loc + 0 + off).SetFloat((float)pose.Position.X);
            new Global(GTA.Offsets.Editor.Props.loc + 1 + off).SetFloat((float)pose.Position.Y);
            new Global(GTA.Offsets.Editor.Props.loc + 2 + off).SetFloat((float)pose.Position.Z);
            new Global(GTA.Offsets.Editor.Props.vrot + 0 + off).SetFloat((float)e.X);
            new Global(GTA.Offsets.Editor.Props.vrot + 1 + off).SetFloat((float)e.Y);
            new Global(GTA.Offsets.Editor.Props.vrot + 2 + off).SetFloat((float)e.Z);
            new Global(GTA.Offsets.Editor.Props.head + off).SetFloat((float)pose.Heading);
        }

        private static byte[] ReadBlock(int index)
        {
            int slots = (int)GTA.Offsets.Editor.Props.NEXT;
            long first = ElementBase(index);
            // Globals are paged in blocks of 2^18 slots; within one page a single read will do.
            if ((first >> 18) == ((first + slots - 1) >> 18))
            {
                return new Global(first).GetBytes(slots * 8);
            }
            var bytes = new byte[slots * 8];
            for (int k = 0; k < slots; k++)
            {
                BitConverter.GetBytes(new Global(first + k).Get<long>()).CopyTo(bytes, k * 8);
            }
            return bytes;
        }

        private static void WriteBlock(int index, byte[] block)
        {
            int slots = block.Length / 8;
            long first = ElementBase(index);
            if ((first >> 18) == ((first + slots - 1) >> 18))
            {
                new Global(first).SetBytes(block);
                return;
            }
            for (int k = 0; k < slots; k++)
            {
                new Global(first + k).SetLong(BitConverter.ToInt64(block, k * 8));
            }
        }

        public void UndoPlacement(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var current = GetCurrentPropCount();
            var target = Math.Max(0, current - amount);
            new Global(GTA.Offsets.Editor.Props.number).SetInt(target);
        }

        public IReadOnlyList<PlacedPropInfo> GetPlacedProps()
        {
            var count = GetCurrentPropCount();
            var list = new List<PlacedPropInfo>(count);
            for (int i = 0; i < count; i++)
            {
                var baseOffset = (long)i * GTA.Offsets.Editor.Props.NEXT;
                var model = new Global(GTA.Offsets.Editor.Props.model + baseOffset).Get<int>();

                var loc = new Vector3(
                    new Global(GTA.Offsets.Editor.Props.loc + 0 + baseOffset).Get<float>(),
                    new Global(GTA.Offsets.Editor.Props.loc + 1 + baseOffset).Get<float>(),
                    new Global(GTA.Offsets.Editor.Props.loc + 2 + baseOffset).Get<float>());

                var rot = new Vector3(
                    new Global(GTA.Offsets.Editor.Props.vrot + 0 + baseOffset).Get<float>(),
                    new Global(GTA.Offsets.Editor.Props.vrot + 1 + baseOffset).Get<float>(),
                    new Global(GTA.Offsets.Editor.Props.vrot + 2 + baseOffset).Get<float>());

                var heading = new Global(GTA.Offsets.Editor.Props.head + baseOffset).Get<float>();

                list.Add(new PlacedPropInfo(model, loc, rot, heading));
            }

            return list;
        }
    }

    public readonly struct PlacedPropInfo
    {
        public PlacedPropInfo(int modelId, Vector3 location, Vector3 rotation, float heading)
        {
            ModelId = modelId;
            Location = location;
            Rotation = rotation;
            Heading = heading;
        }

        public int ModelId { get; }
        public Vector3 Location { get; }
        public Vector3 Rotation { get; }
        public float Heading { get; }
    }
}
