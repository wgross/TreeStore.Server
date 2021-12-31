using System;
using TreeStore.Model.Abstractions;
using Xunit;

namespace TreeStore.Server.Host.Test.Serialization
{
    public class UpdateFacetProppertySerializationTest : SerializationTestBase
    {
        [Theory]
        [InlineData((int)10)]
        [InlineData((uint)10)]
        [InlineData((short)10)]
        [InlineData((ushort)10)]
        [InlineData((byte)10)]
        [InlineData((long)10)]
        public void Update_long_property(object value)
        {
            // ARRANGE
            var updateRequest = new UpdateFacetPropertyValueRequest(Guid.NewGuid(), FacetPropertyTypeValues.Long, value);

            // ACT
            var result = this.SerializationRoundTrip(updateRequest);

            // ASSERT
            Assert.Equal((long)10, result.Value);
        }

        [Theory]
        [InlineData((int)10)]
        [InlineData((uint)10)]
        [InlineData((short)10)]
        [InlineData((ushort)10)]
        [InlineData((byte)10)]
        [InlineData((long)10)]
        [InlineData((float)10)]
        [InlineData((double)10)]
        public void Update_double_property(object value)
        {
            // ARRANGE
            var updateRequest = new UpdateFacetPropertyValueRequest(Guid.NewGuid(), FacetPropertyTypeValues.Double, value);

            // ACT
            var result = this.SerializationRoundTrip(updateRequest);

            // ASSERT
            Assert.Equal((double)10, result.Value);
        }

        [Theory]
        [InlineData((int)10)]
        [InlineData((uint)10)]
        [InlineData((short)10)]
        [InlineData((ushort)10)]
        [InlineData((byte)10)]
        [InlineData((long)10)]
        [InlineData((float)10)]
        [InlineData((double)10)]
        public void Update_decimal_property(object value)
        {
            // ARRANGE
            var updateRequest = new UpdateFacetPropertyValueRequest(Guid.NewGuid(), FacetPropertyTypeValues.Decimal, value);

            // ACT
            var result = this.SerializationRoundTrip(updateRequest);

            // ASSERT
            Assert.Equal((decimal)10, result.Value);
        }
    }
}