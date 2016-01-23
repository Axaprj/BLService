# BLService: Business Logic Server as Windows Service (architectural prototype)
## Session authorization manager example
http://www.codeproject.com/Tips/1072548/Business-Logic-Server-as-Windows-Service-Architect

### Version
1.1.1

## AuthListTest 
AuthListTest is a performance comparison test utility of SessionData (authorization list manager) library.
In the SessionData implemented following modes:

  - Raw: Direct access to sessions database
  - Cached: Access to sessions data via specialized in-process cache manager
  - Service: Access to sessions data via specialized service process (AuthService launching required)
  
### Command Line options

```cmd
AuthListTest
	{
			[-DBINIT <# of users>]
		|	{
				{ [-RAW] | [-SERVICE] } 
				[-SILENT] 
				[-ITERATIONS <#>] 
				[-THREADS <#>]
			}
	}
```
* [-DBINIT] - Database initialization with number registered users parameter
* [-RAW] - Raw mode test (Cached mode test by default)
* [-SERVICE] - Service mode test (AuthService launching required)  
* [-SILENT] - suppress messages, output measurement result only 
* [-ITERATIONS <#>] - number of authentication operations in each thread
* [-THREADS <#>] - number of test threads (users number)

## AuthService
Authorization list manager memory resident service. 
Can be used as command line utility (for debugging) and registered as Windows Service.

### Dependencies
* NFX (Server UNISTACK framework) 2.0.0.18 NuGet Package
* System.Data.SQLite (x86/x64) 1.0.99.0 NuGet Package
* SQLite netFx46 setup bundle 2015 1.0.99.0 (for VS design mode)

## License

*MIT License* 

Copyright (c) 2016 Igor Alexeev, axaprj2000@yahoo.com
