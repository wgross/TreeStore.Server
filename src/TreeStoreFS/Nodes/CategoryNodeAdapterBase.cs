using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Common;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public abstract class CategoryNodeAdapterBase : NodeAdapterBase,

        // Path Traversal
        IGetChildItems,
        // ItemCmdletProvider
        IGetItem,
        // ContainerCmdletProvider
        INewChildItem, IRemoveChildItem, ICopyChildItemRecursive, IMoveChildItem, IRenameChildItem,
        // DynamicItemPropertyCommandProvider
        INewItemProperty, IRenameItemProperty
    {
        protected CategoryNodeAdapterBase(ITreeStoreService treeStoreService)
            : base(treeStoreService)
        { }

        protected abstract CategoryResult Category { get; }

        /// <summary>
        /// Any tree store node has an Id.
        /// </summary>
        public Guid Id => this.Category.Id;

        #region IGetChildItems

        /// <inheritdoc/>
        public bool HasChildItems() => this.Category.Entities.Length > 0 || this.Category.Categories.Length > 0;

        /// <inheritdoc/>
        public IEnumerable<ProviderNode> GetChildItems()
        {
            if (!this.HasChildItems())
            {
                // the node was fetched before with the ids of entites and categories belonging to it.
                // if none were there the call back to fetch the children again is avoided.
                // Since the nodes only lives as long as a single path traversal lasts there in no risk to represent the
                // backend state wrongly here.
                return Array.Empty<ProviderNode>();
            }

            var result = Await(this.TreeStoreService.GetCategoriesByIdAsync(this.Category.Id, CancellationToken.None));

            if (result is null)
                return Array.Empty<ProviderNode>();

            IEnumerable<ProviderNode> childCategories = this.Category.Categories.Select(c => this.CreateCategoryNode(c));
            IEnumerable<ProviderNode> childEntities = this.Category.Entities.Select(e => this.CreateEntityNode(e));

            return childCategories.Union(childEntities).ToArray();
        }

        #endregion IGetChildItems

        #region IGetItem

        /// <inheritdoc/>
        PSObject IGetItem.GetItem() => this.AddAllFacetProperties(PSObject.AsPSObject(this.Category));

        private PSObject AddAllFacetProperties(PSObject pso)
        {
            foreach (var property in this.Category.Facet!.Properties)
                pso.Properties.Add(new PSNoteProperty(property.Name, property.Type));

            return pso;
        }

        #endregion IGetItem

        #region INewChildItem

        /// <inheritdoc/>
        ProviderNode? INewChildItem.NewChildItem(string childName, string? itemTypeName, object? newItemValue)
        {
            return Guard.Against.Null(itemTypeName, nameof(itemTypeName)).ToLowerInvariant() switch
            {
                "category" => this.CreateCategoryNode(Await(this.NewChildContainer(this.Category.Id, Guard.Against.Null(childName, nameof(childName))))),
                "entity" => this.CreateEntityNode(Await(this.NewChildEntity(this.Category.Id, Guard.Against.Null(childName, nameof(childName))))),

                _ => throw new NotImplementedException()
            };
        }

        private async Task<EntityResult> NewChildEntity(Guid parentId, string childName)
        {
            return await this.TreeStoreService.CreateEntityAsync(
               new CreateEntityRequest(
                   Name: childName,
                   CategoryId: parentId),
                   CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<CategoryResult> NewChildContainer(Guid parentId, string childName)
        {
            return await this.TreeStoreService.CreateCategoryAsync(
                new CreateCategoryRequest(
                    Name: childName,
                    ParentId: parentId,
                    Facet: null),
                    CancellationToken.None).ConfigureAwait(false);
        }

        #endregion INewChildItem

        // TODO: Evaluate: entityResult might be a complete entity node instead of an EntityReference because it can't have children like the catagory
        private LeafNode CreateEntityNode(EntityReferenceResult entityResult) => new(entityResult.Name, new EntityNodeAdapter(this.TreeStoreService, entityResult.Id));

        protected ContainerNode CreateCategoryNode(CategoryReferenceResult category) => new(category.Name, new CategoryNodeAdapter(this.TreeStoreService, category.Id));

        #region IRemoveChildItem

        /// <inheritdoc/>
        void IRemoveChildItem.RemoveChildItem(string childName, bool recurse)
        {
            Guard.Against.NullOrEmpty(childName, nameof(childName));

            var category = Array.Find(this.Category.Categories, c => childName.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
            if (category is not null)
            {
                Await(this.TreeStoreService.DeleteCategoryAsync(parentId: this.Category.Id, childName, recurse, CancellationToken.None));
            }

            var entity = Array.Find(this.Category.Entities, e => childName.Equals(e.Name, StringComparison.OrdinalIgnoreCase));
            if (entity is not null)
            {
                Await(this.TreeStoreService.DeleteEntityAsync(entity.Id, CancellationToken.None));
            }
        }

        #endregion IRemoveChildItem

        #region ICopyChildItem, ICopyChildItemRecursive

        /// <inheritdoc/>
        ProviderNode? ICopyChildItem.CopyChildItem(ProviderNode nodeToCopy, string[] destination)
        {
            if (Guard.Against.Null(nodeToCopy, nameof(nodeToCopy)).Underlying is CategoryNodeAdapterBase categoryNode)
            {
                var result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, false, CancellationToken.None));

                return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
            }
            else if (Guard.Against.Null(nodeToCopy, nameof(nodeToCopy)).Underlying is EntityNodeAdapter entityNode)
            {
                var result = Await(this.TreeStoreService.CopyEntityToAsync(entityNode.Id, this.Category.Id, CancellationToken.None));

                return new LeafNode(result!.Name, new EntityNodeAdapter(this.TreeStoreService, result!.Id));
            }
            else
            {
                throw new NotImplementedException("missing case");
            }
        }

        /// <inheritdoc/>
        ProviderNode? ICopyChildItemRecursive.CopyChildItemRecursive(ProviderNode nodeToCopy, string[] destination)
        {
            CategoryResult? result = null;
            if (Guard.Against.Null(nodeToCopy, nameof(nodeToCopy)).Underlying is CategoryNodeAdapterBase categoryNode)
                result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, true, CancellationToken.None));

            return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
        }

        #endregion ICopyChildItem, ICopyChildItemRecursive

        #region IRenameChildItem

        /// <inheritdoc/>
        void IRenameChildItem.RenameChildItem(string childName, string newName)
        {
            var childToRename = this.GetChildByName(Guard.Against.NullOrEmpty(childName, nameof(childName)));
            if (childToRename is null)
            {
                throw new InvalidOperationException($"Child item (name='{childName}') wasn't renamed: It doesn't exist");
            }

            var existingChild = this.GetChildByName(Guard.Against.NullOrEmpty(newName, nameof(newName)));
            if (existingChild is not null)
            {
                throw new InvalidOperationException($"Child item (name='{childName}') wasn't renamed: There is already a child with name='{newName}'");
            }

            Await(this.RenameChildItemAsync(childToRename, newName));
        }

        private object? GetChildByName(string childName)
            => (object?)Array.Find(this.Category.Categories, c => c.Name.Equals(childName, StringComparison.OrdinalIgnoreCase))
            ?? (object?)Array.Find(this.Category.Entities, e => e.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));

        private async Task RenameChildItemAsync(object childToRename, string newName)
        {
            switch (childToRename)
            {
                case EntityReferenceResult er:
                    await this.TreeStoreService.UpdateEntityAsync(er.Id, new UpdateEntityRequest(Name: newName), CancellationToken.None).ConfigureAwait(false);
                    break;

                case CategoryReferenceResult cr:
                    await this.TreeStoreService.UpdateCategoryAsync(cr.Id, new UpdateCategoryRequest(Name: newName), CancellationToken.None).ConfigureAwait(false);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown reference type: '{childToRename.GetType()}'");
            }
        }

        #endregion IRenameChildItem

        #region IMoveChildItem

        ProviderNode? IMoveChildItem.MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
        {
            if (Guard.Against.Null(nodeToMove, nameof(nodeToMove)).Underlying is CategoryNodeAdapterBase categoryNode)
            {
                var result = Await(this.TreeStoreService.MoveCategoryToAsync(categoryNode.Id, this.Category.Id, CancellationToken.None));

                return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
            }
            else if (Guard.Against.Null(nodeToMove, nameof(nodeToMove)).Underlying is EntityNodeAdapter entityNode)
            {
                var result = Await(this.TreeStoreService.MoveEntityToAsync(entityNode.Id, this.Category.Id, CancellationToken.None));

                return new LeafNode(result!.Name, new EntityNodeAdapter(this.TreeStoreService, result!.Id));
            }
            else throw new NotImplementedException("missing case");
        }

        #endregion IMoveChildItem

        #region INewItemProperty

        void INewItemProperty.NewItemProperty(string propertyName, string? propertyTypeName, object? value)
        {
            if (!Enum.TryParse<FacetPropertyTypeValues>(propertyTypeName, ignoreCase: true, out var propertyType))
                throw new InvalidOperationException($"FacetProperty(name='{propertyName}') wasn't created: type '{propertyTypeName}' is unknown");

            var existingDestinationProperty = this.Category.Facet!.Properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (existingDestinationProperty is not null)
                throw new InvalidOperationException($"Creating property(name='{propertyName}') failed: property name is duplicate");
            
            // send the update but don't keep the result. The category is fetch again during the next command

            Await(this.TreeStoreService.UpdateCategoryAsync(this.Category.Id, new UpdateCategoryRequest(
                Facet: new FacetRequest(
                    Properties: new CreateFacetPropertyRequest(Name: propertyName, propertyType))),
                    CancellationToken.None));
        }

        #endregion INewItemProperty

        #region IRenameItemProperty

        void IRenameItemProperty.RenameItemProperty(string sourceProperty, string destinationProperty)
        {
            var propertyToChange = this.Category.Facet!.Properties.FirstOrDefault(p => p.Name.Equals(sourceProperty, StringComparison.OrdinalIgnoreCase));
            if (propertyToChange is null)
                throw new InvalidOperationException($"Renaming property(name='{sourceProperty}') failed: property doesn't exist");

            var existingDestinationProperty = this.Category.Facet!.Properties.FirstOrDefault(p => p.Name.Equals(destinationProperty, StringComparison.OrdinalIgnoreCase));
            if (existingDestinationProperty is not null)
                throw new InvalidOperationException($"Renaming property(name='{sourceProperty}') failed: property name is duplicate");

            // send the update but don't keep the result. The category is fetch again during the next command

            Await(this.TreeStoreService.UpdateCategoryAsync(this.Category.Id, new UpdateCategoryRequest(
                Facet: new FacetRequest(
                    Properties: new UpdateFacetPropertyRequest(Id: propertyToChange.Id, Name: destinationProperty))),
                    CancellationToken.None));
        }

        #endregion IRenameItemProperty
    }
}