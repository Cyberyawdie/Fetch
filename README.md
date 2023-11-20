# Reward Points Demo
This web service includes:

  1. The ability for a payer to add points.

2. The ability for a user to spend points and check balances.

This project was created in C# using a Minimal API to simulate a simple rewards points API operations.
To keep this project simple, the code used for all the processes is located in the program.cs file.

<h3>Prerequisites</h3>

* Visual Studio Community 2022 with ASP.NET and web development, and .NET 6.0
* Swagger

### API Endpoints ###

##### ADD POINTS

 POST  `/rewards/add-points`
 
 Sample rewards you can use to test this project. 
 
* `{ "payer": "DANNON", "points": 1000}`

* `{ "payer": "UNILEVER", "points": 200}` 
 
* `{ "payer": "MILLER COORS", "points": 10000}`
 
* `{ "payer": "DANNON", "points": 300}`



##### SPEND POINTS

POST   `/rewards/spend-points`

Sample points you can spend to test this project. Points get redeemed from the oldest payer first. If you try to redeem more then your total points in your account, your transaction will not be processed.

* `{ "points": 5000 }`



##### BALANCE

GET   `/rewards/points-balance`  

Send a request to see your remainding balance from each payer and total points.
