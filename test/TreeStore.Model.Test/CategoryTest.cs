using System;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class CategoryTest
    {
        #region Category hierarchy structure

        [Fact]
        public void Category_has_no_parent()
        {
            // ACT
            var result = new CategoryModel();

            // ASSERT
            Assert.Null(result.Parent);
        }

        [Fact]
        public void Category_has_facet_with_same_name()
        {
            // ACT
            var result = new CategoryModel("name");

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.Equal("name", result.Facet.Name);
        }

        [Fact]
        public void Category_name_changes_facet_name()
        {
            // ARRANGE
            var result = new CategoryModel("name");

            // ACT
            result.Name = "changed-name";

            // ASSERT
            Assert.Equal("changed-name", result.Name);
            Assert.Equal("changed-name", result.Facet.Name);
        }

        [Fact]
        public void Categeory_rejects_null_facet()
        {
            // ARRANGE
            var category = new CategoryModel("name");

            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => category.Facet = null);

            // ASSERT
            Assert.Equal("value", result.ParamName);
        }

        [Fact]
        public void Category_corrects_Parent_for_ctor_subcategories()
        {
            // ACT
            var category = new CategoryModel();
            var result = new CategoryModel("cat", new FacetModel(), category);

            // ASSERT
            Assert.Same(result, category.Parent);
        }

        #endregion Category hierarchy structure

        [Fact]
        public void Category_adds_subcategory()
        {
            // ARRANGE

            var category = new CategoryModel();
            var subcategory = new CategoryModel();

            // ACT

            category.AddSubCategory(subcategory);

            // ASSERT

            Assert.Equal(category, subcategory.Parent);
        }

        [Fact]
        public void Category_yields_own_Facet()
        {
            // ARRANGE

            var facet1 = new FacetModel();
            var category = new CategoryModel("cat", facet1);

            // ACT

            var result = category.Facets().ToArray();

            // ASSERT

            Assert.Equal(new[] { facet1 }, result);
        }

        [Fact]
        public void Category_clones_shallow_with_new_id()
        {
            // ARRANGE

            var category = new CategoryModel();
            var subcategory = new CategoryModel();
            category.AddSubCategory(subcategory);

            // ACT

            var result = (CategoryModel)category.Clone();

            // ASSERT

            Assert.Equal(category.Name, result.Name);
            Assert.NotEqual(category.Id, result.Id);
        }
    }
}