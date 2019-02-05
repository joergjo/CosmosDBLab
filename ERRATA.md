# Addendum and Errata
This is a collection of corrections and useful additions to the [lab instructions](https://cosmosdb.github.io/labs/).

## General Remarks

### Fixed vs. Unlimited Collections
Some of the instructions call out the difference between _Fixed_ and _Unlimited_ collections. This distinction is not applicable anymore going forward:
- Fixed collections have been removed from the Azurre Portal UI. They can be created using the SDK, but that capbility will be deprecated as well.
- Unlimited collections can now be created with the same minimum throughout of 400 RU/s as Fixed collections. Hence, there is also no cost advantage anymore that justified Fixed collections.  
- This also means that partitioning is mandatory, as Fixed collections _can not_ scale.

### Using Visual Studio Code as C# IDE
The lab instructions use Visual Studio Code only as basic text editor. All C# build tasks are run from a command prompt. 

If you want to use Visual Studio Code as an IDE
- Install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
- Open the project's root folder and wait for the extension to activate. Once it asks you to add supporting files to the project, do confirm.
- Configure your default build task.
- For more details, see https://code.visualstudio.com/Docs/languages/csharp#.

Important keyboard short-cuts:
- `CTRL-SHIFT-P`: Show Visual Studio Code command bar
- `ALT-SHIFT-F`: Format code
- `CTRL-SHIFT-B`: Build project
- `F5`: Debug project

## Errata

### Lab 1
#### Exercise 3, Task VI
The default index layout has changed an no longer uses Hash indeces by default. See https://docs.microsoft.com/en-us/azure/cosmos-db/index-types.

#### Exercise 5, Task I
After opening the project in Visual Studio Code, open `.vscode\tasks.json` and replace
```
"args": [
	"build",
	"${workspaceFolder}/cosmos-demo.csproj"
],
```
with
```
"args": [
	"build",
	"${workspaceFolder}/benchmark.csproj"
],
```
to build the project in Visual Studio Code.

### Lab 2
#### Exercise 1, Task I
Note that the _Add Collection_ dialog has been updated since the screenshot in the lab insructions has been created. 

#### Exercise 1, Task III
Note that the _New Linked Service (Azure Blob Storage)_ dialog has been updated since the screenshot in the lab insructions has been created. 

You only need to specify the `SAS URI` (from the lab instructions) here. Leave the `SAS Token` field blank.

Note that the _Choose the input file or folder_ dialog has been updated since the screenshot in the lab insructions has been created. 

Note that the _New Linked Service (Azure Cosmos DB (SQL API))_ dialog has been updated since the screenshot in the lab insructions has been created. Use the `From Azure subscription` option to simply pick the target database in the UI.

### Lab 5
#### Exercise 1, Task III
The target collection for this task is the `TransactionCollection`.

#### Exercise 2, Task III
The target collection for this task is the `PeopleCollection`.

#### Exercise 5, Task I
The old code in step 11 is wrong. It should read
```
await Console.Out.WriteLineAsync($"ETag: {response.Resource.ETag}");
```
