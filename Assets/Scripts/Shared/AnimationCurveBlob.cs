using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Shared
{
    public struct AnimationCurveBlob
    {
        private BlobArray<float> _sampledValues;
        private float2 _timeRange;

        public float2 TimeRange => _timeRange;

        public static BlobAssetReference<AnimationCurveBlob> Build(AnimationCurve curve, int intervalCount,
            Allocator allocator)
        {
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var root = ref blobBuilder.ConstructRoot<AnimationCurveBlob>();

            var sampledValues = blobBuilder.Allocate(ref root._sampledValues, intervalCount + 1);
            var timeFrom = curve.keys[0].time;
            var timeTo = curve.keys[curve.keys.Length - 1].time;
            var timeStep = (timeTo - timeFrom) / intervalCount;

            for (var i = 0; i < intervalCount + 1; i++)
            {
                sampledValues[i] = curve.Evaluate(timeFrom + i * timeStep);
            }

            root._timeRange = new float2(timeFrom, timeTo);

            return blobBuilder.CreateBlobAssetReference<AnimationCurveBlob>(allocator);
        }

        public float Evaluate(float time)
        {
            var intervalCount = _sampledValues.Length - 1;
            var clamp01 = math.unlerp(_timeRange.x, _timeRange.y, math.clamp(time, _timeRange.x, _timeRange.y));
            var timeInterval = clamp01 * intervalCount;
            var segmentIndex = (int) math.floor(timeInterval);
            if (segmentIndex >= intervalCount)
            {
                return _sampledValues[intervalCount];
            }

            var bottom = _sampledValues[segmentIndex];
            var top = _sampledValues[segmentIndex + 1];

            return math.lerp(bottom, top, timeInterval - segmentIndex);
        }
    }
}