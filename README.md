# OffChainStorage API

The API provides an interface for managing files in server-side storage (OffChain). 
The "OffChain" designation underscores that while data storage operations are conducted on the server, there's a seamless integration with an Ethereum wallet.
This Ethereum wallet is not merely for user authentication, but it also offers an added layer of security for user data stored on the server, ensuring its integrity and authenticity without taxing the blockchain.
![изображение](https://github.com/X3nX3n/Ethereum-OffChain-Storage/assets/47632045/dcbdae15-ba8e-4505-a46f-bef492c86c87)


## Dependencies:
- **Microsoft.AspNetCore.Authorization**: For authorization.
- **Microsoft.AspNetCore.Mvc**: The main framework for the web-API.
- **MimeKit**: Used to determine the MIME-type of a file.
- **System.Security.Claims**: Used for handling user claims.
- **Nethereum.Signer**: For working with Ethereum signatures.
- **System.Security.Cryptography**: Cryptographic services.
- **Swashbuckle.AspNetCore.Annotations**: Annotations for Swagger.
- **Microsoft.Extensions.Hosting**: Interfaces and extensions for hosting.
- **OffChainStorage.Services**: Custom services (e.g., IVerifier interface).
- **Microsoft.Extensions.Logging**: Logging.

## Controller: `StorageController`

### Methods:

- **GetAllFilesList**: Returns a list of all files in the user's storage.
- **UploadFiles**: Uploads a list of files to the user's storage.
- **DownloadFile**: Downloads a file from the user's storage. A file signature check is possible using an Ethereum wallet key.
- **RenameFile**: Renames a file in the user's storage.
- **MoveFile**: Moves a file from the old path to a new path. If the directory does not exist, it creates it.
- **CopyFile**: Copies a file from the old path to a new path. If the directory does not exist, it creates it.
- **DeleteFiles**: Deletes a list of files from the user's storage.

## Usage examples:

- **To get a list of all files**: 
  ```
  GET /api/Storage/GetAllFilesList
  ```

- **To upload files**: 
  ```
  POST /api/Storage/UploadFiles
  ```

- **To download a file**: 
  ```
  GET /api/Storage/DownloadFile?filePath=my_folder/my_file.txt&IsSignatureCheck=true
  ```

> And so on for other methods.

## Features:
- When downloading a file, you can verify the signature if the file was signed using an Ethereum wallet key.
- Logging is used to track errors when working with files.


## Authentication Using an Ethereum Wallet

Rather than the traditional method of logging in with a username and password, we utilize a digital signature generated by an Ethereum wallet.

### How It Works:
1. **Request to Server:** Users, when intending to log in, transmit their Ethereum public address to our server, either via the web UI or an API.
  
2. **Getting a Unique Message:** Our server then crafts a unique message (which is hardcoded by default) and sends this back to the user.
   
3. **Message Signing:** The user signs this unique message with their Ethereum wallet.
   
4. **Signature Dispatch:** This signed message is then sent back to our server.
    
5. **Verifying the Signature:** Within our controller's code, we've embedded the IVerifier service (essentially, your verifier) which is deployed to cross-check the signature against the known Ethereum public address. If this check reveals any discrepancy, an authorization error is flagged: 
    ```csharp
    if (!verifier.VerifyEthSignature(path, signature, signerAddress))
        return Unauthorized("Signature verification failed");
    ```

6. **Token Generation:** On successful verification, our server hands out an authentication token to the user. This token serves as an identity confirmation tool for various operations within the system.

### Authorization:

Our code is fortified with the [Authorize] attribute. This denotes that certain controller methods demand authorization. Essentially, before granting access, the system ensures that any incoming request is authenticated.

#### Process:
- A user dispatches a request to our server carrying an 'Authorization' header embedding the token (typically, this is a JWT or a Bearer token).
- Our server rigorously checks the token's legitimacy, such as verifying its signature and checking its expiry date.
- If everything checks out and the token is validated, access to the intended method is granted. Otherwise, the user encounters an authorization error.

### Getting Started:

1. **Repository:** Start by cloning the repository.
2. **Dependencies:** Install all the necessary dependencies.
3. **Database Connection:** Set up a connection to your database or any other storage solution, if required.
4. **Kickstart:** Boot up the project and maneuver through the API as per your requirements.
