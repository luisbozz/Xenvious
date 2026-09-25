using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Xenvious.AdvancedPlacement
{
    /// <summary>
    /// Provides dimensional data for creator props by writing a model id and sampling min/max bounds.
    /// </summary>
    public sealed class DimensionsProvider
    {
        private readonly Func<int, bool> _writeModel;
        private readonly Func<Vector3> _readMin;
        private readonly Func<Vector3> _readMax;
        private readonly TimeSpan _readDelay;

        public DimensionsProvider(
            Func<int, bool> writeModel,
            Func<Vector3> readMin,
            Func<Vector3> readMax,
            TimeSpan? readDelay = null)
        {
            _writeModel = writeModel ?? throw new ArgumentNullException(nameof(writeModel));
            _readMin = readMin ?? throw new ArgumentNullException(nameof(readMin));
            _readMax = readMax ?? throw new ArgumentNullException(nameof(readMax));
            _readDelay = readDelay ?? TimeSpan.FromMilliseconds(50);
        }

        public async Task<DimensionsResult> RequestDimensionsAsync(int modelId, CancellationToken cancellationToken)
        {
            var baselineMin = SafeRead(_readMin);
            var baselineMax = SafeRead(_readMax);

            var writeSucceeded = false;
            try
            {
                writeSucceeded = _writeModel(modelId);
            }
            catch (Exception ex)
            {
                return DimensionsResult.Failed($"Write failed: {ex.Message}");
            }

            if (!writeSucceeded)
            {
                return DimensionsResult.Failed("Model could not be written.");
            }

            try
            {
                await Task.Delay(_readDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                return DimensionsResult.Canceled();
            }

            var min = SafeRead(_readMin);
            var max = SafeRead(_readMax);

            var updated = !ApproximatelyEqual(baselineMin, min) || !ApproximatelyEqual(baselineMax, max);
            return DimensionsResult.Success(min, max, updated);
        }

        private static Vector3 SafeRead(Func<Vector3> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return Vector3.Zero;
            }
        }

        private static bool ApproximatelyEqual(in Vector3 left, in Vector3 right, float tolerance = 0.0001f)
        {
            return Math.Abs(left.X - right.X) < tolerance
                && Math.Abs(left.Y - right.Y) < tolerance
                && Math.Abs(left.Z - right.Z) < tolerance;
        }
    }

    public readonly struct DimensionsResult
    {
        private DimensionsResult(bool success, bool canceled, Vector3 min, Vector3 max, bool updated, string? error)
        {
            IsSuccess = success;
            IsCanceled = canceled;
            Min = min;
            Max = max;
            HasUpdatedValues = updated;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsCanceled { get; }
        public Vector3 Min { get; }
        public Vector3 Max { get; }
        public bool HasUpdatedValues { get; }
        public string? Error { get; }

        public static DimensionsResult Success(Vector3 min, Vector3 max, bool updated)
            => new DimensionsResult(true, false, min, max, updated, null);

        public static DimensionsResult Failed(string error)
            => new DimensionsResult(false, false, Vector3.Zero, Vector3.Zero, false, error);

        public static DimensionsResult Canceled()
            => new DimensionsResult(false, true, Vector3.Zero, Vector3.Zero, false, null);
    }
}
