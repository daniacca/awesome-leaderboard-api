# Awesome LeaderBoard Test Project

Welcome to the Awesome LeaderBoard project repository!
This test is written with dotnet 6 and uses Docker and Docker Compose for create a running system.

After cloning the To run the overall system:

```
$ docker-compose up -d --build
```

## System Infrastructure - Overview

The infrastructure is composing by the following node:

- _AwesomeLeaderboard_ - REST web api with all the endpoints for clients integration;
- _DataWriterService_ - Background service, execute writing operations (insert/update) to database;
- _Mongo_ - NoSql Database used for saving users data for generating the leaderboard;
  - I choose Mongo for the following reasons:
    - There is no Schema at all => data could be stored on objects that are serialized in a JSON-like format (mongo uses BSON). This examples comes with a simply data model that have no relations at all;
    - NoSql databases are tipically a better choiche for distribuited systems that could hold massive amount of data and have typically low latency response;
    - Data on NoSql databases are usually de-normalized, so performing sharding and database scaling is easier;
- _Redis_ - Key-Value Database used as Distributed Cache;
  - I integrated Redis as cache for the following reason:
    - Could be used for caching data in the future, if the platform grow enough;
    - At the moment, I used Redis to keep track of the "event" state and the relative results. Every time is requested to "register" a user or to update one score, the corresponding event is created and the event state is tracked down in Redis since is faster than any Sql/NoSql databases and this could be a volatile information that could be lost after some time;
- _RabbitMq_ - AMQP Message broker used for asyncronous communication between microservices node;
  - I integrated Rabbit Mq for the following reason:
    - It's lightweight, open-source, portable, scalable, simple to integrate and really reliable message broker;
    - The message ordering in this applications is not an "hard" constraint;
- _Redis Commander_ - Docker Images used only for testing purpose, it starts a GUI for connecting on Redis cache and see the data content in it;

Here the overall schema (simplified) of the infrastructure:

![Infrastucture](/doc/images/infrastructure.png)

## Web API enpoints and integration

I have configured Swagger on the API, you can view it on: [Awesome Leaderboard - Swagger page](https://localhost:60001/swagger/index.html).

The API are divided on a total of 2 controllers and 4 endpoints, all the endpoints actions are under the ```/api``` path.
You have:

- GET ```/api/leaderboard``` => obtain the leaderboard and the ordered list of users from the top score to the worst. You can specify two ```integer``` parameters in query string:
  - ```top``` => cut the leader board at the top X element specified;
  - ```minScore``` => filter the leader board and answer with all the users that have a min score that is more or equal than the value specified;
- POST ```/api/user``` => request a user registration, take the following JSON body as input:
  ```
  {
    username:     string
    initialScore: integer($int64), optional
  }
  ```
  If no errors occurs the API answer with HTTP Status Code 202 (Accepted) and the registration request is forwarded to the DataWriter Service. The response body will have the following JSON:
  ```
  {
    eventId: string
  }
  ```
  The client could use the returned eventId to check the operation result with the event-result endpoint, as described below.
- PUT ```/api/user/{userName}``` => request an update of a user score, take the following JSON body as input:
  ```
  {
    newScore: integer($int64)
  }
  ```
  If no errors occurs the API answer with HTTP Status Code 202 (Accepted) and the registration request is forwarded to the DataWriter Service. The response body will be exact the same as for the previous endpoint, so the client will have back the eventId to check the result of the requested operation.
- GET ```/api/user/event-result/{type}/{eventId}``` => the client application could use this endpoint to with a given eventId to check the operation outcome. Give the following parameters in the URL as inputs to the endpoint:
  - ```type``` => should be ```register``` or ```update```, based on which was operation requested by the eventId you want to check;
  - ```eventId``` => should be the id string that the client obtain back from the web api when calling one of the previous method;
The endpoint answer back with one of the following response:
  - ```HTTP Status Code BAD REQUEST (400)``` if the input data were not correct;
  - ```HTTP Status Code GONE (410)``` if the server was unable to find the given ```eventId```;
  - ```HTTP Status Code OK (200)``` if the writing operation is finished with success;
  - ```HTTP Status Code PROCESSING (102)``` if the writing operation is not finished yet;
  - ```HTTP Status Code CONFLICT (409)``` if the writing operation is finished with errors, which could means:
    - In case of new user registration, the specified userName is already taken by another user;
    - In case of update operation, the specified userName could not be finded in the database

## Solution Limitations and possible bottlenecks

- For this type of challenge, every container is a "single node" and no orchestrator system were implemented (like Docker Swarm of K8s), of course this is not a reliable solution and at least one orchestrator system should be used;
- The API node should be redundant with more containers running;
- The Data Writer node should be redundant too. Here we have a possible bottleneck, since we have two queues that invoke async operation on the same services. We can specialize more by divinding this into two services, one for the registration and another one for the data update;
- Another possible bottleneck in the long run could be that the "writer" service and the "reader" service (the API in our case) connects on the same DB. If the complexity and the users should grow more, we could adopt a full CQRS pattern (with more databases with data replication) eventually in combination with the Event Sourcing;
- We could use cache (like redis) to store some user data, as for example, the first 5 or 10 users (it depends on system use), and read from cache first;
- RabbitMq, Redis and Mongo services should become "cluster" service in order to have high reliability;
- To guarantee high throughput we could take in consideration the idea of substitute RabbitMq with Kafka, which guarantee higher throughput with fewer hardware capabilities;

## Additional Informations

In this section I will make some general discussion about the code architecture (and what I was thinking when creating the code).

### Repository structure and projects organization

If you open the Visual Studio Solution file you will find four main "directory":

1. *Common*: contains all the projects that's are "shared" between other projects of the code base. Here you can find the definition of the DB Model, the System events, and the implementation of the "Data Access" layer, a common layer that I define to read/write data from Database;
2. *Integrations*: contains all the "wrapper" implementations for the used third-party services, in this case Redis, RabbitMq and MongoDB.
    - This projects should be more abstract possible as they should contain only general integration logic specific to the integrated system and should not depend from business logic of specific application;
    - I wrapped the integrations logic in this projects to keep the "integration" layer separated, to have a common interfaces to be used in the "application layer";
    - I choose to implement a "Repository pattern" for reading and writing on MongoDb. I avoid the "Unit of work" pattern applying the KISS (keep it simple stupid) principle to this coding challenge (and moreover, with distributed system we may use different patterns as well). This repository implementation could be optimized and developed more;
3. *Microservices*: contains all the projects that form all the running applications of our system;
4. *Tests*: contains all the unit test implemented;
    - For simplicity I do not implement all the possible unit - integration - e2e test, I have written two XUnit test projects that contains some unit test for the Mongo Db Repository and the Redis wrapper;