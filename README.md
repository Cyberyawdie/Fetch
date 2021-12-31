# Fetch Rewards Code Challenge
This web service includes:

  1. The ability for a user to add points.

2. The ability for a user to spend points and check balances.

This project was created in C# using a Minimal API and in-memory database to simulate CRUD operations.
To keep this project simple and to maintain code clarity, the code used for all the processes is located in the program.cs file.

<h3>Prerequisites</h3>

* Visual Studio Community 2022 with ASP.NET and web development, .NET 6.0 and Entity Framework
* Postman

### API Endpoints ###

##### ADD POINTS

 POST  `/rewards/points`
 
 Sample rewards you can use in Postman to test this project. 
 
* `{ "payer": "DANNON", "points": 1000, "timestamp": "2020-11-02T14:00:00Z" }`

* `{ "payer": "UNILEVER", "points": 200, "timestamp": "2020-10-31T11:00:00Z" }` 
 
* `{ "payer": "MILLER COORS", "points": 10000, "timestamp": "2020-11-01T14:00:00Z" }`
 
* `{ "payer": "DANNON", "points": 300, "timestamp": "2020-10-31T10:00:00Z" }`

* `{ "payer": "DANNON", "points": -200, "timestamp": "2020-10-31T15:00:00Z" }`



##### SPEND POINTS

POST   `/rewards/spend`

Sample spend points you can use in Postman to test this project.

* `{ "points": 5000 }`



##### BALANCE

GET   `/rewards/spend`  

Send a GET request to this endpoint to see your remainding balance from each payer.
