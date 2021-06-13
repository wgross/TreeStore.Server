# Controller Tests

The controller tests do not directly call the controller classes. 
Instead the use the controller through an AspNetCore TestServer. The makes the controller test an intergartion test of

- Controller 
- Client implemention of the ```ITreeStoreService``` (TreeStoreServer.Client)
- Mediator pipeline 
- Mappers from the TreeStore-Model to the DTOs in TreeStore.Abstractions

Separate tests for these components will be implemented if border cases, or issues are surfacing and require changes of the code there.


