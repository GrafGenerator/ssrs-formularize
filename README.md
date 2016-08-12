# SSRS-Formularize

## Overview
**Formularizer RE** - is a custom rendering extension for SSRS, that has an ability to output report to Excel file using specified template, with ability to export native Excel formulas to result file (i.e. support dynamic reports).


## SSRS pipeline injection

Copy compiled binary files to ***bin*** folder of Reporting Services, update config files as following and restart Reporting Services. 

### rsreportserver.config
```xml
<Configuration>
	<Extensions>
		<Render>
			<Extension Name="Formularizer" Type="Formularizer.FormularizerRe, FormularizerRe"/>
		</Render>
	</Extensions>
</Configuration>
```


### rssrvpolicy.config
```xml
<configuration>
  <mscorlib>
    <security>
      <policy>
        <PolicyLevel version="1">
          <CodeGroup
                  class="FirstMatchCodeGroup"
                  version="1"
                  PermissionSetName="Nothing">
				  
			<CodeGroup
				class="FirstMatchCodeGroup"
				version="1"
				PermissionSetName="FullTrust"
				Name="rsExtensionAssembly"
				Description="A special code group for Formularizer extension">
				<IMembershipCondition
					class="UrlMembershipCondition"
					version="1"
					Url="{Disk}:\Program Files\Microsoft SQL Server\{Instance}\Reporting Services\ReportServer\bin\FormularizerRe.dll"/>
			</CodeGroup>
			
          </CodeGroup>
        </PolicyLevel>
      </policy>
    </security>
  </mscorlib>
</configuration>
```

## Automating
For automatic copying on build use this in post-build event
```bash
xcopy ".\FormularizerRe.dll" "{Disk}:\Program Files\Microsoft SQL Server\{Instance}\Reporting Services\ReportServer\bin" /f /c /b /v /y
xcopy ".\Reporting.FunctionalExtensions.dll" "{Disk}:\Program Files\Microsoft SQL Server\{Instance}\Reporting Services\ReportServer\bin" /f /c /b /v /y
xcopy ".\DocumentFormat.OpenXml.dll" "{Disk}:\Program Files\Microsoft SQL Server\{Instance}\Reporting Services\ReportServer\bin" /f /c /b /v /y
```