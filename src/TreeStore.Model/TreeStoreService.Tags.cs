using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public partial class TreeStoreService
    {
        ///<inheritdoc/>
        public Task<TagResult?> GetTagByIdAsync(Guid id, CancellationToken none)
        {
            return Task.FromResult(this.model.Tags.FindById(id)?.ToTagResult());
        }

        ///<inheritdoc/>
        public Task<TagResult> UpdateTagAsync(Guid id, UpdateTagRequest updateTagRequest, CancellationToken none)
        {
            var tag = this.model.Tags.FindById(id);

            if (tag is null)
            {
                this.logger.LogError("Tag(id='{tagId}') wasn't updated: Tag(id='{tagId}') doesn't exist", id);

                throw new InvalidOperationException($"Tag(id='{id}') wasn't updated: Tag(id='{id}') doesn't exist");
            }

            this.Apply(updateTagRequest, tag);

            this.model.Tags.Upsert(tag);

            return Task.FromResult(tag.ToTagResult());
        }

        ///<inheritdoc/>
        public Task<bool> DeleteTagAsync(Guid id, CancellationToken none)
        {
            var tag = this.model.Tags.FindById(id);
            if (tag is null)
            {
                this.logger.LogError("Tag(id='{tagId}') wasn't deleted: Tag(id='{tagId}') doesn't exist", id);

                return Task.FromResult(false);
            }

            return Task.FromResult(this.model.Tags.Delete(tag));
        }

        /// <inheritdoc/>
        public Task<TagResult> CreateTagAsync(CreateTagRequest createTagRequest, CancellationToken cancellationToken)
        {
            if (createTagRequest is null)
                throw new ArgumentNullException(nameof(createTagRequest));

            return Task.FromResult(this.model.Tags.Upsert(this.Apply(createTagRequest)).ToTagResult());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TagResult>> GetTagsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.model.Tags.FindAll().Select(t => t.ToTagResult()));
        }

        #region Apply

        private TagModel Apply(CreateTagRequest createTagRequest)
        {
            var tag = new TagModel
            {
                Name = createTagRequest.Name
            };

            if (createTagRequest.Facet is not null)
                this.Apply(createTagRequest.Facet, tag.Facet);

            return tag;
        }

        private TagModel Apply(UpdateTagRequest updateTagRequest, TagModel tag)
        {
            tag.Name = updateTagRequest.Name ?? tag.Name;

            if (updateTagRequest.Facet is not null)
                this.Apply(updateTagRequest.Facet, tag.Facet);

            return tag;
        }

        private void Apply(FacetRequest facetRequest, FacetModel facet)
        {
            facetRequest.Deletes?.ForEach(deletion =>
            {
                facet.RemoveProperty(deletion.Id);
            });

            facetRequest.Updates?.ForEach(update =>
            {
                var facetProperty = facet.GetProperty(update.Id);
                if (facetProperty is not null)
                    this.Apply(update, facetProperty);
            });

            facetRequest.Creates?.ForEach(creation =>
            {
                facet.AddProperty(this.Apply(creation));
            });
        }

        private FacetPropertyModel Apply(UpdateFacetPropertyRequest updateFacetPropertyRequest, FacetPropertyModel facetProperty)
        {
            facetProperty.Name = updateFacetPropertyRequest.Name ?? facetProperty.Name;

            return facetProperty;
        }

        private FacetPropertyModel Apply(CreateFacetPropertyRequest createFacetPropertyRequest)
        {
            return new FacetPropertyModel
            {
                Name = createFacetPropertyRequest.Name,
                Type = createFacetPropertyRequest.Type,
            };
        }

        #endregion Apply
    }
}