# TreeStore.LiteDb

Implements the persistent for TreeStore.Model using a LiteDb document database.

## Traversal Algorithms

Litedb has no native understanding of recursive data structures like trees.
Therefore recursive operations have to be implemented for recursive copy or delete operations.

There are two of it:

- CategoryCopyTraverser: copy a category to a subcategory. May include entities and subcategories
- CategoryRemovalTraverser: remove a subcategory if empty or may also delete entites and subcategories



