OffChainStorage API

The API provides an interface for managing files in server-side storage (OffChain).
Dependencies:

Microsoft.AspNetCore.Authorization: For authorization.
Microsoft.AspNetCore.Mvc: The main framework for the web-API.
MimeKit: Used to determine the MIME-type of a file.
System.Security.Claims: Used for handling user claims.
Nethereum.Signer: For working with Ethereum signatures.
System.Security.Cryptography: Cryptographic services.
Swashbuckle.AspNetCore.Annotations: Annotations for Swagger.
Microsoft.Extensions.Hosting: Interfaces and extensions for hosting.
OffChainStorage.Services: Custom services (e.g., IVerifier interface).
Microsoft.Extensions.Logging: Logging.

Controller: StorageController
Methods:

GetAllFilesList: Returns a list of all files in the user's storage.
UploadFiles: Uploads a list of files to the user's storage.
DownloadFile: Downloads a file from the user's storage. A file signature check is possible using an Ethereum wallet key.
RenameFile: Renames a file in the user's storage.
MoveFile: Moves a file from the old path to a new path. If the directory does not exist, it creates it.
CopyFile: Copies a file from the old path to a new path. If the directory does not exist, it creates it.
DeleteFiles: Deletes a list of files from the user's storage.

Usage examples:

To get a list of all files:

	GET /api/Storage/GetAllFilesList

To upload files:

	POST /api/Storage/UploadFiles

To download a file:

	GET /api/Storage/DownloadFile?filePath=my_folder/my_file.txt&IsSignatureCheck=true

And so on for other methods.

Features:

When downloading a file, you can verify the signature if the file was signed using an Ethereum wallet key.
Logging is used to track errors when working with files.
	
Authentication Using an Ethereum Wallet

Instead of classic authentication with a username and password, a digital signature created using an Ethereum wallet is used.
Authentication Process:

Request Submission to the Server: When a user wants to log in, they send their public Ethereum address to the server via a web interface or API.

Receiving a Unique Message: The server generates a unique message (hardcoded by default) and sends it back to the user.

Signing the Message: The user uses their Ethereum wallet to sign the unique message.

Sending the Signature to the Server: The signed message is sent back to the server.

Signature Verification: In the controller code, we have an IVerifier service (your verifier) that is used to verify the signature against the public Ethereum address.

	if (!verifier.VerifyEthSignature(path, signature, signerAddress))
	return Unauthorized("Signature verification failed");

Token Issuance: If the check is successful, the server issues an authentication token to the user. This token is then used to confirm the user's identity when performing various operations in the system.

Authorization:

The code uses the [Authorize] attribute, indicating the need for authorization to access the controller methods. This attribute requires the request to be authenticated before accessing the method.
How it Works:

The client sends a request to the server with the Authorization header, which contains the token (usually this is JWT or Bearer token).
The server checks the token for validity (for example, by verifying the signature and the token's expiration date).
If the token is valid, the request is allowed, and the user gets access to the method. Otherwise, an authorization error is returned.

Installation and Setup:

Clone the repository.
Install the required dependencies.
Set up the connection to the database or other storage (if necessary).
Start the project and use the API according to your needs.