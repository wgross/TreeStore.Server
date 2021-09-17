# TreeStore.Abstractions

The project contains the definition of the ITreeStoreService. 
The service and the accompanying data structures describe a high level abstraction of the TreeStore Server software in C#.
Published interfaces and internally used interfaces implement the interface contract and allow to compose different architectural approaches to build software using the TreeStore data model.

## Data Structures as Sealed C# Records

The response and requests data structures of the TreeStore contract are meant to be short lived and immutable
and are  meant to be used in the data transfer to and in the data transfer from and to the ITreeStoreService implementations.

