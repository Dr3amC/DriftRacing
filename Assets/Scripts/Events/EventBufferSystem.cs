using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Events
{
    public abstract class EventBufferSystem<TEvent> : SystemBase where TEvent : struct
    {
        private List<EventBuffer> _buffers = new List<EventBuffer>();
        private JobHandle _producerJobs;

        public EventBuffer CreateBuffer()
        {
            var buffer = new EventBuffer(Allocator.TempJob);
            _buffers.Add(buffer);
            return buffer;
        }

        public void AddProducerJob(JobHandle jobHandle)
        {
            _producerJobs = JobHandle.CombineDependencies(_producerJobs, jobHandle);
        }

        protected abstract void Handle(TEvent @event);

        protected override void OnUpdate()
        {
            try
            {
                if (_buffers.Count == 0)
                {
                    return;
                }

                foreach (var buffer in _buffers)
                {
                    while (buffer.events.TryDequeue(out var @event))
                    {
                        Handle(@event);
                    }
                    buffer.Dispose();
                }
            }
            finally
            {
                _buffers.Clear();
                _producerJobs = default;
            }
        }

        public struct EventBuffer : IDisposable
        {
            internal NativeQueue<TEvent> events;

            public NativeQueue<TEvent>.ParallelWriter AsParallelWriter => events.AsParallelWriter();

            internal EventBuffer(Allocator allocator)
            {
                events = new NativeQueue<TEvent>(allocator);
            }

            public void Enqueue(TEvent @event)
            {
                events.Enqueue(@event);
            }

            public void Dispose()
            {
                events.Dispose();
            }
        }
    }
}