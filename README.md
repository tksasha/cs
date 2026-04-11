- Books is example GraphQl application
- Sandbox is example Console application
- Cs12DotNet8 contains interesting code snippets from "C#12 and .NET 8 Modern Cross-Platform Development Funddamentals" book by Mark J. Price

# .csproj
```xml
<Nullable>true</Nullable>
 <!--Treat not Threat -->
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<WarningsAsErrors>CS8600</WarningsAsErrors>

<!-- Required for IDE0005 (Remove unnecessary usings) to work during build -->
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
```
