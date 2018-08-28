We're built on top of Dashing so performance is pretty good!

The following tables list some benchmarks we've ran against various ORMs. For further information please take a look at the 
source code - it's highly probable that we've misunderstood some ORM and mis-represented it. Each test gives you an idea of the 
conciseness of writing code with Dashing - feel free to add anything to it.

Times are in milliseconds for 500 executions and this is running synchronous apis

Running SelectSingle
------------------------
Select a single entity by Id

     41 Dapper
     44 Dashing (By Id without Transaction)
     45 ServiceStack (Without transaction)
     52 LightSpeed (FindById without transaction)
     70 ServiceStack
     71 Dashing (By Id)
     90 Dashing
    100 LightSpeed (Linq)
    156 EF
    167 EF (Using Find with AutoDetectChangesEnabled = false)
    834 Simple.Data

Running Fetch
------------------------
Select a single entity, and eager load a single parent reference

     46 Dapper
     79 Dashing (Without transaction)
    106 Dashing
    170 EF

Running Get And Change
------------------------
Select a single entity, change a property and save the entity

     96 Dapper
    114 ServiceStack (without transaction)
    115 Dashing (By Id without transaction)
    145 LightSpeed (without explicit transaction)
    147 Dashing (By Id)
    148 ServiceStack
    151 LightSpeed
    191 Dashing
    475 EF

Running Fetch Collection
------------------------
Select a single entity, eager loading a collection

    151 Dapper (Multiple Result Method)
    166 Dapper (Naive)
    201 Dashing (without transaction)
    209 Dashing
    394 EF

Running Fetch Multiple Collections
------------------------
Select a single entity, eager loading 2 collections on the root entity

    349 Dashing
    571 EF

Running Fetch Multiple Multiple Collections
-----------------------
Select multiple entities, eager loading 2 collections

    2167 EF
    2379 Dashing (without Transaction)
    2490 Dashing

Running Fetch Multiple Chained Collections
------------------------


    3,311 Dashing (without Transaction)
    3,355 Dashing
    3,496 EF
