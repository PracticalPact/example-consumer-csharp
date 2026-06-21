# Example Consumer C#
This is an example of a micro-service that can act as a Consumer. It's an extremely minimal frontend for a forum where you can log in and create posts. 

It relies on two other microservices: a backend and an SSO. Neither of these need to actually run for you to perform CDCT. In fact, SSO has not been implemented at all.

The backend, which is the Provider, has been implemented at https://github.com/PracticalPact/example-provider-csharp.

## Run
Because SSO does not exist, you can not just run this program normally. You can however run the Consumer tests and create a contract.

Start by exploring PostClient.cs a bit to get an understanding of how this application works. 

On the ```main``` branch you can find the full Consumer test implementation in the ConsumerTests project. You can read through that and run the tests with ```dotnet test```. 

On the ```without-cdct``` branch all CDCT has been stripped. You can try to implement it yourself from that starting point.