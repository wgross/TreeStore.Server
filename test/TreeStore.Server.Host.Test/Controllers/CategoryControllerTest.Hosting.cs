using TreeStore.Model;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public partial class CategoryControllerTest : TreeStoreServerHostTestBase
    {
        private readonly Category rootCategory;

        public CategoryControllerTest()
        {
            // model
            this.rootCategory = DefaultRootCategory();
        }
    }
}