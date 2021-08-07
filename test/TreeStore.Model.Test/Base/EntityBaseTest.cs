using TreeStore.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class EntityBaseTest
    {
        public static IEnumerable<object[]> GetEntityBaseInstancesForInitzialization()
        {
            yield return new EntityModel().Yield().ToArray();
            yield return new CategoryModel().Yield().ToArray();
            yield return new TagModel().Yield().ToArray();
            yield return new FacetModel().Yield().ToArray();
            yield return new FacetPropertyModel().Yield().ToArray();
        }

        public static IEnumerable<object[]> GetEntityBaseInstancesForEquality()
        {
            var refId = Guid.NewGuid();
            var differentId = Guid.NewGuid();

            yield return new object[] { new EntityModel { Id = refId }, new EntityModel { Id = refId }, new EntityModel { Id = differentId }, new FacetPropertyModel { Id = refId } };
            yield return new object[] { new CategoryModel { Id = refId }, new CategoryModel { Id = refId }, new CategoryModel { Id = differentId }, new TagModel { Id = refId } };
            yield return new object[] { new TagModel { Id = refId }, new TagModel { Id = refId }, new TagModel { Id = differentId }, new FacetModel { Id = refId } };
            yield return new object[] { new FacetModel { Id = refId }, new FacetModel { Id = refId }, new FacetModel { Id = differentId }, new FacetPropertyModel { Id = refId } };
            yield return new object[] { new FacetPropertyModel { Id = refId }, new FacetPropertyModel { Id = refId }, new FacetPropertyModel { Id = differentId }, new EntityModel { Id = refId } };
        }

        [Theory]
        [MemberData(nameof(GetEntityBaseInstancesForEquality))]
        public void EntityBases_are_equal_if_Id_are_equal_and_Type(NamedBase refEntity, NamedBase sameId, NamedBase differentId, NamedBase differentType)
        {
            // ACT & ASSERT

            Assert.Equal(refEntity, refEntity);
            Assert.Equal(refEntity.GetHashCode(), refEntity.GetHashCode());
            Assert.Equal(refEntity, sameId);
            Assert.Equal(refEntity.GetHashCode(), sameId.GetHashCode());
            Assert.NotEqual(refEntity, differentId);
            Assert.NotEqual(refEntity.GetHashCode(), differentId.GetHashCode());
            Assert.NotEqual(refEntity, differentType);
            Assert.NotEqual(refEntity.GetHashCode(), differentType.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(GetEntityBaseInstancesForInitzialization))]
        public void EntityBases_have_empty_name(NamedBase entityBase)
        {
            // ACT

            var result = entityBase.Name;

            // ASSERT

            Assert.Equal(string.Empty, result);
        }
    }
}