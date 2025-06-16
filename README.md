# Career Marketplace Scheduler

## Summary

Scheduler function app for Career Marketplace on GCXChange.

## Prerequisites

You need to add a queue to the storage account in your resource group called  `delete` 

## Version 

![dotnet 8](https://img.shields.io/badge/net8.0-blue.svg)

## API permission

MSGraph

| API / Permissions name    | Type        | Admin consent | Justification                       |
| ------------------------- | ----------- | ------------- | ----------------------------------- |
| User.Read                 | Delegated   | Yes           | Sign in and read user profile       | 
| Sites.Read.All            | Delegated   | Yes           |                                     | 

Sharepoint

n/a

## App setting

| Name                       | Description                                                                    |
| -------------------------- | ------------------------------------------------------------------------------ |
| AzureWebJobsStorage        | Connection string for the storage acoount                                      |
| containerName              | The name of the storage container that contains the backup files               |
| delegateEmail              | Account name used for delegated access                                         |
| deleteFunctionUrl          | API call for the delete functionality                                          |
| keyVaultUrl                | Key vault address                                                              |
| listId                     | ID of the job opportunity SharePoint list                                      |
| secretNameClient           | Secret name used for client authorization                                      |
| secretNameDelegatePassword | Secret name used for the delegated account password                            |
| tenantId                   | ID of the tenant that hosts the function app                                   |

## Version history

Version|Date|Comments
-------|----|--------
1.0|TBD|Initial release

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**
