## Programmatically  create Azure AD application and service principle object to authenticate with Azure Resource Manager

This **very** simple c# console application issues REST calls to create an Azure AD application and Service Principle Object to be used later for Azure Resource Manager APIs.

### Use this sample

Clone this repository and build the code. The console app will request for 3 input parameters:

1. Your Azure AD tenant ID
2. You Azure AD user name
3. Full path to the [Application.json](Application.json) file located in this repository

 [Application.json](Application.json) - this file describes the application to be created (name, url, and allowed roles). 

