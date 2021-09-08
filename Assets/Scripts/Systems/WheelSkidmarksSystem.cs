using Components;
using Components.Effects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    public class WheelSkidmarksSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, in WheelSkidmarks skidmarks, in Surface wheelOnSurface, in WheelContact contact,
                in WheelOutput output, in Wheel wheel, in Rotation rotation, in WheelContactVelocity velocity) =>
            {
                
            });
        }
        
        private struct Sequences
        {
            private DynamicBuffer<SkidmarksPoint> _points;
            private DynamicBuffer<SkidmarksSequence> _sequences;
            private int _to;
            private int _length;

            public Sequences(DynamicBuffer<SkidmarksPoint> points, DynamicBuffer<SkidmarksSequence> sequences)
            {
                _points = points;
                _sequences = sequences;
                var from = sequences.Length > 0 ? sequences[sequences.Length - 1].End + 1 : 0;
                _to = points.Length - 1;
                _length = _to - from + 1;
            }

            public void Complete()
            {
                if (_length == 1)
                {
                    _points.Length -= _length;
                }
                else if (_length > 1)
                {
                    _sequences.Add(new SkidmarksSequence
                    {
                        End = _to
                    });
                }
            }

            public void Continue(SkidmarksPoint point)
            {
                const float maxDistanceBetweenPointSq = 0.1f * 0.1f;

                if (_length > 1)
                {
                    _points[_to] = point;
                    if (math.distancesq(_points[_to - 1].Position, point.Position) > maxDistanceBetweenPointSq)
                    {
                        _points.Add(point);
                    }
                }
                else if (_length == 1)
                {
                    if (math.distancesq(_points[_to].Position, point.Position) > maxDistanceBetweenPointSq)
                    {
                        _points.Add(point);
                    }
                }
                else
                {
                    _points.Add(point);
                }
            }
        }
    }
}