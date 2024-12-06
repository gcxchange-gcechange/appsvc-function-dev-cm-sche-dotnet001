# Career Marketplace Scheduler

## Summary

Scheduler function app for Career Marketplace on GCXChange.

## Prerequisites

n/a

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
| delegateEmail              | 
| deleteFunctionUrl          |
| keyVaultUrl                |
| listId                     |
| secretNameClient           |
| secretNameDelegatePassword |
| secretNameDelegatePassword |
| tenantId                   |

## Version history

Version|Date|Comments
-------|----|--------
1.0|TBD|Initial release

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**
