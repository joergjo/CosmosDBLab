## About
This is a completed version of the official Cosmos DB labs available at https://cosmosdb.github.io/labs/. Instead of deleting the same `Main()` method over and over again, I have created invidiual namespaces and static classes that hold every lab's code, broken down into separate methods. To run any of the labs, simply call `Lab{1-4}.Excercices.{Method()}` from `Program.Main`.

The Lab3 folder also includes a JavaScript [source file](Lab3/sprocs.js) with all Stored Procedures used in Lab 3, but written using modern ES2015 language features such as arrow functions, string interpolation, and `const` declarations. Yes, your Stored Procedures in Cosmos DB _don't_ have to look like some JavaScript that your parents might have written...

When running the labs, please refer to the addendum and [errata](ERRATA.md) for any corrections or additional instructions you might find useful. 
