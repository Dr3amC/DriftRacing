﻿using System;
using UnityEngine.Rendering;

namespace Procedural
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VertexAttributeAttribute : Attribute
    {
        public readonly VertexAttribute Attribute;
        public readonly VertexAttributeFormat Format = VertexAttributeFormat.Float32;
        public readonly int Dimensions = 3;

        public VertexAttributeAttribute(VertexAttribute attribute)
        {
            Attribute = attribute;
        }

        public VertexAttributeAttribute(VertexAttribute attribute, int dimensions)
        {
            Attribute = attribute;
            Dimensions = dimensions;
        }

        public VertexAttributeDescriptor ToDescriptor()
        {
            return new VertexAttributeDescriptor(Attribute, Format, Dimensions);
        }
    }
}