using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Net;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Escc.ServiceClosures.AdministrationTool")]
[assembly: AssemblyDescription("Maintains service closure information on www.eastsussex.gov.uk")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("East Sussex County Council")]
[assembly: AssemblyProduct("Escc.ServiceClosures.AdministrationTool")]
[assembly: AssemblyCopyright("Copyright © East Sussex County Council 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("af02359c-605a-4c21-a5b0-c4934a616a60")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Recommended by FxCop
[assembly: System.CLSCompliant(true)]

// Assembly gets only permissions it needs -- not sure why but it WILL NOT serialise the Service class
// without throwing a CAS error. Must be a reason for it, maybe the Generic dictionary???
//[assembly: UIPermission(SecurityAction.RequestMinimum, Unrestricted = true)] // allow to open console window
//[assembly: EnvironmentPermission(SecurityAction.RequestMinimum, Unrestricted = true)] // to get default credentials for web service
//[assembly: WebPermission(SecurityAction.RequestMinimum, ConnectPattern = "^https?://[a-z0-9.]+/EsccWebTeam.ServiceClosures.WebService/ClosuresService.asmx$")] // connect to closures web service
//[assembly: WebPermission(SecurityAction.RequestMinimum, ConnectPattern = "^https?://[a-z0-9.]+/Czone.WebService.SchoolsInformation[A-Za-z0-9.]*/SchoolsInformationWebService.asmx$")] // connect to SID web service
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)] // permission to execute
//[assembly: PermissionSet(SecurityAction.RequestOptional, Name = "Nothing")]
