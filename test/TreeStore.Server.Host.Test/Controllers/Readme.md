# Controller Tests

The controller tests do not directly call the controller classes. 
Instead the use the controller through an AspNetCore TestServer. The makes the controller test an integration test of

- Controller 
- Client implemention of the ```ITreeStoreService``` (TreeStoreServer.Client)
- Mappers the TreeStore-Model to the DTOs in TreeStore.Abstractions and vice versa

Separate tests for these components will be implemented if border cases, or issues are surfacing and require changes of the code there.


