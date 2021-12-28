using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using TreeStore.Common;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public sealed class EntityNodeAdapter : NodeAdapterBase,
        // ItemProvider
        IGetItem,
        // ItemPropertyProvider
        IGetItemProperty, ISetItemProperty, IClearItemProperty

    {
        private readonly Lazy<EntityResult?> entity;

        public EntityNodeAdapter(ITreeStoreService treeStoreService, Guid entityId)
            : base(treeStoreService)
        {
            this.entity = new Lazy<EntityResult?>(() => Await(this.TreeStoreService.GetEntityByIdAsync(entityId, CancellationToken.None)));
        }

        /// <summary>
        /// Any tree store node has an Id.
        /// </summary>
        public Guid Id => this.Entity.Id;

        public EntityResult Entity => this.entity.Value ?? throw new InvalidOperationException("Entity wasn't loaded");

        #region IGetItem

        PSObject? IGetItem.GetItem() => this.AddAllFacetProperties(PSObject.AsPSObject(this.Entity));

        #endregion IGetItem

        #region IGetItemProperty

        // TODO: the built in properties like Name should be included as well.

        PSObject IGetItemProperty.GetItemProperty(IEnumerable<string>? propertyToGet)
        {
            var pso = new PSObject();
            if (propertyToGet is null || !propertyToGet.Any())
            {
                AddAllFacetProperties(pso);
            }
            else
            {
                foreach (var propertyName in propertyToGet)
                {
                    var property = this.GetFacetPropertyByName(propertyName);

                    if (property is not null)
                        pso.Properties.Add(new PSNoteProperty(property.Name, property.Value));
                }
            }

            return pso;
        }

        private FacetPropertyValueResult? GetFacetPropertyByName(string propertyName)
            => this.Entity.Values.FirstOrDefault(v => v.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        private PSObject AddAllFacetProperties(PSObject pso)
        {
            foreach (var property in this.Entity.Values)
                pso.Properties.Add(new PSNoteProperty(property.Name, property.Value));

            return pso;
        }

        #endregion IGetItemProperty

        #region ISetItemProperty

        void ISetItemProperty.SetItemProperty(PSObject properties)
        {
            var facetProperyUpdates = properties
                .Properties
                .Select(psp => (psp: psp, fp: this.GetFacetPropertyByName(psp.Name)))
                .Where(epsp => epsp.fp is not null)
                .Select(epsp => new UpdateFacetPropertyValueRequest(epsp.fp!.Id, epsp.fp.Type, epsp.psp.Value));

            // update is sent but result is not kept. The node is remade during the next command anyway
            Await(this.TreeStoreService.UpdateEntityAsync(this.Entity.Id, new UpdateEntityRequest(
                Values: new FacetPropertyValuesRequest(facetProperyUpdates.ToArray())),
                CancellationToken.None));
        }

        #endregion ISetItemProperty

        #region IClearItemProperty

        void IClearItemProperty.ClearItemProperty(IEnumerable<string> propertyToClear)
        {
            var facetProperytoClear = Guard.Against.Null(propertyToClear, nameof(propertyToClear))
                .Select(pn => (pn: pn, fp: this.GetFacetPropertyByName(pn)))
                .Where(epsp => epsp.fp is not null)
                .Select(epsp => new UpdateFacetPropertyValueRequest(epsp.fp!.Id, epsp.fp.Type, null));

            // update is sent but result is not kept. The node is remade during the next command anyway
            Await(this.TreeStoreService.UpdateEntityAsync(this.Entity.Id, new UpdateEntityRequest(
                Values: new FacetPropertyValuesRequest(facetProperytoClear.ToArray())),
                CancellationToken.None));
        }

        #endregion IClearItemProperty
    }
}