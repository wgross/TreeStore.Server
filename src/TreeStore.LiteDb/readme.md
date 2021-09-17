# TreeStore.LiteDb

Implements the persistent for TreeStore.Model using a LiteDb document database.

## Traversal Algorithms

Litedb has no native understanding of recursive data structures like trees.
Therefore recursive operations have to be implemented for recursive copy or delete operations.

There are two of it:

- CategoryCopyTraverser: copy a category to a subcategory. May include entities and subcategories
- CategoryRemovalTraverser: remove a subcategory if empty or may also delete entites and subcategories

### Traversal of Category ancestors

Quering a catageory or an entity (having a catageory) result in a traversal from the loweset category to the root of the category tree.
This is implemented by reading each of the categories by id. 
Obviously this will caise a performance problem for deep category trees by increasing asymp. 
runtime of rdaing a category or entity to O(log(n)) where n is the nomber of catagories in the model.

It is planned to tackle the problem by a single cache of the category tree. 




