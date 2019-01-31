/* EXCERCISE 2 */

// Task II
function greetCaller(name) {
    const context = getContext();
    const response = context.getResponse();
    response.setBody(`Hello ${name}`);
}

// Task III
function createDocument(doc) {
    const context = getContext();
    const collection = context.getCollection();
    const accepted = collection.createDocument(
        collection.getSelfLink(),
        doc,
        (err, newDoc) => {
            if (err) throw new Error(`Error ${err.message}`);
            context.getResponse().setBody(newDoc);
        }
    );
    if (!accepted) return;
}

// Task IV
function createDocumentWithLogging(doc) {
    console.log("procedural-start ");
    const context = getContext();
    const collection = context.getCollection();
    console.log("metadata-retrieved ");
    const accepted = collection.createDocument(
        collection.getSelfLink(),
        doc,
        (err, newDoc) => {
            console.log("callback-started ");
            if (err) throw new Error(`Error ${err.message}`);
            context.getResponse().setBody(newDoc.id);
        }
    );
    console.log("async-doc-creation-started ");
    if (!accepted) return;
    console.log("procedural-end");
}

// Task V
function createDocumentWithFunction(document) {
    var context = getContext();
    var collection = context.getCollection();
    if (!collection.createDocument(collection.getSelfLink(), document, documentCreated))
        return;
    function documentCreated(error, newDocument) {
        if (error) throw new Error(`Error ${error.message}`);
        context.getResponse().setBody(newDocument);
    }
}

// Task VI
function createTwoDocuments(companyName, industry, taxRate) {
    const context = getContext();
    const collection = context.getCollection();
    const firstDocument = {
        company: companyName,
        industry: industry
    };
    const secondDocument = {
        company: companyName,
        tax: {
            exempt: false,
            rate: taxRate
        }
    };
    const firstAccepted = collection.createDocument(collection.getSelfLink(), firstDocument,
        (firstError, newFirstDocument) => {
            if (firstError) throw new Error(`Error ${firstError.message}`);
            const secondAccepted = collection.createDocument(collection.getSelfLink(), secondDocument,
                (secondError, newSecondDocument) => {
                    if (secondError) throw new Error(`Error ${secondError.message}`);
                    context.getResponse().setBody({
                        companyRecord: newFirstDocument,
                        taxRecord: newSecondDocument
                    });
                }
            );
            if (!secondAccepted) return;
        }
    );
    if (!firstAccepted) return;
}

function createTwoDocumentsAndRollback(companyName, industry, taxRate) {
    const context = getContext();
    const collection = context.getCollection();
    const firstDocument = {
        company: companyName,
        industry: industry
    };
    const secondDocument = {
        company: `${companyName}_taxprofile`,
        tax: {
            exempt: false,
            rate: taxRate
        }
    };
    const firstAccepted = collection.createDocument(collection.getSelfLink(), firstDocument,
        (firstError, newFirstDocument) => {
            if (firstError) throw new Error(`Error ${firstError.message}`);
            console.log(`Created: ${newFirstDocument.id}`);
            var secondAccepted = collection.createDocument(collection.getSelfLink(), secondDocument,
                (secondError, newSecondDocument) => {
                    if (secondError) throw new Error(`Error ${secondError.message}`);
                    console.log(`Created: ${newSecondDocument.id}`);
                    context.getResponse().setBody({
                        companyRecord: newFirstDocument,
                        taxRecord: newSecondDocument
                    });
                }
            );
            if (!secondAccepted) return;
        }
    );
    if (!firstAccepted) return;
}


/* EXCERCISE 3 */

// Task 1
function bulkUpload(docs) {
    const collection = getContext().getCollection();
    const collectionLink = collection.getSelfLink();
    const count = 0;
    if (!docs) throw new Error("The array is undefined or null.");
    const docsLength = docs.length;
    if (docsLength == 0) {
        getContext().getResponse().setBody(0);
        return;
    }
    tryCreate(docs[count], callback);
    function tryCreate(doc, callback) {
        const isAccepted = collection.createDocument(collectionLink, doc, callback);
        if (!isAccepted) getContext().getResponse().setBody(count);
    }
    function callback(err, doc, options) {
        if (err) throw err;
        count++;
        if (count >= docsLength) {
            getContext().getResponse().setBody(count);
        } else {
            tryCreate(docs[count], callback);
        }
    }
}

function bulkDelete(query) {
    const collection = getContext().getCollection();
    const collectionLink = collection.getSelfLink();
    const response = getContext().getResponse();
    const responseBody = {
        deleted: 0,
        continuation: true
    };
    if (!query) throw new Error("The query is undefined or null.");
    tryQueryAndDelete();
    function tryQueryAndDelete(continuation) {
        const requestOptions = { continuation: continuation };
        const isAccepted = collection.queryDocuments(collectionLink, query, requestOptions, (err, retrievedDocs, responseOptions) => {
            if (err) throw err;
            if (retrievedDocs.length > 0) {
                tryDelete(retrievedDocs);
            } else if (responseOptions.continuation) {
                tryQueryAndDelete(responseOptions.continuation);
            } else {
                responseBody.continuation = false;
                response.setBody(responseBody);
            }
        });
        if (!isAccepted) {
            response.setBody(responseBody);
        }
    }
    function tryDelete(documents) {
        if (documents.length > 0) {
            const isAccepted = collection.deleteDocument(documents[0]._self, {}, (err, responseOptions) => {
                if (err) throw err;
                responseBody.deleted++;
                documents.shift();
                tryDelete(documents);
            });
            if (!isAccepted) {
                response.setBody(responseBody);
            }
        } else {
            tryQueryAndDelete();
        }
    }
}