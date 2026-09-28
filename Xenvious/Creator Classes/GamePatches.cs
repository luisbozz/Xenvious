using System;
using System.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Switchable byte patches in the game executable for the creator (not in script bytecode,
    /// that is ScrPatchesRunner). Each patch finds its site with an [AOB] pattern when Xenvious
    /// attaches, keeps the original bytes and writes either them or the patch.
    ///
    /// Once patched the pattern no longer matches, so a patch left on by an earlier Xenvious run
    /// cannot be found again: it stays on until GTA restarts, and the switch shows as unavailable.
    /// </summary>
    public sealed class GamePatch
    {
        public string Name { get; }
        private readonly Func<string> _pattern;
        private readonly int _offset;
        private readonly byte[] _patch;
        private long _address;
        private byte[] _original;

        public GamePatch(string name, Func<string> pattern, int offset, byte[] patch)
        {
            Name = name;
            _pattern = pattern;
            _offset = offset;
            _patch = patch;
        }

        public bool Available => _address != 0 && _original != null;

        public void Resolve(byte[] buffer)
        {
            _address = 0;
            _original = null;
            string pattern = _pattern();
            if (string.IsNullOrWhiteSpace(pattern))
            {
                Log.Info($"No {Name} pattern for GTA {GameVariant.DisplayName(GameVariant.Current)}; the switch stays unavailable.", source: "GamePatches");
                return;
            }
            ulong hit = GTA.ScanModule(pattern, buffer);
            if (hit == 0)
            {
                Log.Warn($"{Name} did not match; either it is still patched from an earlier run (restart GTA) or the pattern needs updating.", source: "GamePatches");
                return;
            }
            long address = (long)hit + _offset;
            byte[] original = Read(address);
            // Found through the per-process cache after it was patched: the originals are gone.
            if (original == null || original.SequenceEqual(_patch))
                return;
            _address = address;
            _original = original;
        }

        public bool IsOn => Available && (Read(_address)?.SequenceEqual(_patch) ?? false);

        public void Set(bool on)
        {
            if (!Available || !MainWindow.m.IsProcOpen)
                return;
            MainWindow.m.memory(_address.ToString("X")).SetBytes(on ? _patch : _original);
        }

        private byte[] Read(long address)
            => MainWindow.m.IsProcOpen ? MainWindow.m.memory(address.ToString("X")).GetBytes(_patch.Length) : null;
    }

    public static class GamePatches
    {
        // Creator camera: 11 bytes from 5 before the match are the collision handling; NOPs let
        // the camera pass through walls and the ground.
        public static readonly GamePatch CameraNoCollision = new GamePatch("AOB_creator_cam_nocollision",
            () => GTA.Offsets.Editor.AOB_creator_cam_nocollision, -5,
            Enumerable.Repeat((byte)0x90, 11).ToArray());

        // The function that returns how much of the creator budget is used: xorps xmm0, xmm0; ret
        // makes it return 0.0, so the budget bar never fills.
        public static readonly GamePatch NoBudget = new GamePatch("AOB_creator_budget",
            () => GTA.Offsets.Editor.AOB_creator_budget, 0,
            new byte[] { 0x0F, 0x57, 0xC0, 0xC3 });

        public static void Resolve(byte[] buffer)
        {
            CameraNoCollision.Resolve(buffer);
            NoBudget.Resolve(buffer);
        }
    }
}
