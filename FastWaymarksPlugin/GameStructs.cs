using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace FastWaymarksPlugin
{
    //	A helper struct to make working with the active flags in a waymark preset structure easier.
    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 1)]
    public struct BitField8
    {
        public byte Data;

        public bool this[int i]
        {
            get
            {
                return i switch
                {
                    0 => (Data & 1) > 0,
                    1 => (Data & 2) > 0,
                    2 => (Data & 4) > 0,
                    3 => (Data & 8) > 0,
                    4 => (Data & 16) > 0,
                    5 => (Data & 32) > 0,
                    6 => (Data & 64) > 0,
                    7 => (Data & 128) > 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(i), "Array index out of bounds.")
                };
            }
            set
            {
                Data = i switch
                {
                    0 => (byte)((value ? 1 : 0) | Data),
                    1 => (byte)((value ? 2 : 0) | Data),
                    2 => (byte)((value ? 4 : 0) | Data),
                    3 => (byte)((value ? 8 : 0) | Data),
                    4 => (byte)((value ? 16 : 0) | Data),
                    5 => (byte)((value ? 32 : 0) | Data),
                    6 => (byte)((value ? 64 : 0) | Data),
                    7 => (byte)((value ? 128 : 0) | Data),
                    _ => throw new ArgumentOutOfRangeException(nameof(i), "Array index out of bounds.")
                };
            }
        }

        public override string ToString()
        {
            return $"0x{Data:X}";
        }
    }

    public static class FieldMarkerPresetExt {
        public static string AsString(this FieldMarkerPreset preset)
        {
            return $"A: {preset.Markers[0].X} {preset.Markers[0].Y} {preset.Markers[0].Z}\r\n" +
                   $"B: {preset.Markers[1].X} {preset.Markers[1].Y} {preset.Markers[1].Z}\r\n" +
                   $"C: {preset.Markers[2].X} {preset.Markers[2].Y} {preset.Markers[2].Z}\r\n" +
                   $"D: {preset.Markers[3].X} {preset.Markers[3].Y} {preset.Markers[3].Z}\r\n" +
                   $"1: {preset.Markers[4].X} {preset.Markers[4].Y} {preset.Markers[4].Z}\r\n" +
                   $"2: {preset.Markers[5].X} {preset.Markers[5].Y} {preset.Markers[5].Z}\r\n" +
                   $"3: {preset.Markers[6].X} {preset.Markers[6].Y} {preset.Markers[6].Z}\r\n" +
                   $"4: {preset.Markers[7].X} {preset.Markers[7].Y} {preset.Markers[7].Z}\r\n" +
                   $"Active Flags: {preset.ActiveMarkers}\r\n" +
                   $"ContentFinderCondition: {preset.ContentFinderConditionId}\r\n" +
                   $"Timestamp: {preset.Timestamp}";
        }
    }

    public static class MarkerPresetPlacementExt {
        public static string AsString(this MarkerPresetPlacement preset)
        {
            return $"A: {preset.Active[0]} {preset.X[0]} {preset.Y[0]} {preset.Z[0]}\r\n" +
                   $"B: {preset.Active[1]} {preset.X[1]} {preset.Y[1]} {preset.Z[1]}\r\n" +
                   $"C: {preset.Active[2]} {preset.X[2]} {preset.Y[2]} {preset.Z[2]}\r\n" +
                   $"D: {preset.Active[3]} {preset.X[3]} {preset.Y[3]} {preset.Z[3]}\r\n" +
                   $"1: {preset.Active[4]} {preset.X[4]} {preset.Y[4]} {preset.Z[4]}\r\n" +
                   $"2: {preset.Active[5]} {preset.X[5]} {preset.Y[5]} {preset.Z[5]}\r\n" +
                   $"3: {preset.Active[6]} {preset.X[6]} {preset.Y[6]} {preset.Z[6]}\r\n" +
                   $"4: {preset.Active[7]} {preset.X[7]} {preset.Y[7]} {preset.Z[7]}\r\n";
        }
    }
}