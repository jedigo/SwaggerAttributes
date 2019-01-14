Swagger Attributes minimal unit test
------------------------------------

This contains a minimal test for custom swagger attributes.

This shows that the order of attribute processing when run with 

     dotnet test

is different from when it is run under 
 
    dotnet test /p:altCover=true


This includes all the necessary test fixtures.