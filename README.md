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


## Idea behind this extension and usage
### The goal and the difficulties
This extension aims to restore ability to export native Excel formulas when exporting report to Excel file. This functionality was dropped in SSRS after 2008 version. 
Since SSRS doesn't provide extension point to control how Excel file is generated, there is no way to restore this functionality gracefully. Thus, only hacky ways are in play.
The general idea of this extension is to mark some report items with identifiers, "anchors", then reference them while writing formulas. 

Unfortunately, there's no way to operate with report items expressions (or other info to get report items relations) while working inside rendering extension (except digging into internal SSRS layout engine), so required info there can be only passed in some other way. It seems that the only applicable way is to use action hyperlinks to pass info to rendering extensions. 

Uh, not the comfortable one, but at least it works.

Workflow is next: FormularizerRE internally renders report to XLSX format, and then patches generated document to insert Excel formulas according to information provided in report.

### Formulas (report items) relations
Basically there are two type of information that can be assigned to report item to build the formula:
* identifier or anchor,
* formula in special DSL, that can reference anchors and can be compiled to Excel formula.

In order to set this information, one needs to:
* add custom function to the report, EncodeFormula, see [encode-function.vb](encode-function.vb)
* set action for report item, "Go to URL" option and set expression in following format:
```vb
=code.EncodeFormula("current item identitier (anchor)", "current item formula")
```

Both anchor and formula can be empty string. Also, both can be specified at the same time, so cell can contain formula calculated and in its turn can be referenced by other cells.

**Important:** To make FormularizerRE process exported document, one also need to mark document to processing by extension (this is done to not affect performance and replace origin ExcelRE by this extension). To mark document for processing, add custom property to report with name `formularize` and any value.

Report item anchor can be variable string, it can be programmatically generated since you free to provide any string you want to the function (expression is code, all is up to user).

Report formula is basically a text that is Excel formula, with one difference - Excel-style cell 
references are replaced by references in FormularizerRE DSL. When redering document, references are resolved to Excel cells addresses.

There basically three types of references resolving:
* `cell` - resolves first cell with identifier
* `cells` - resolve a list of cells with identifier
* `column` - resolve Excel column range

### DSL specification

`cell-spec` = empty | `anchor` | `formula` | `anchor` + `formula`

`anchor` = string

`formula` = free-style combination of `static-text` and `reference-spec`

`static-text` = any text except curly braces

`reference-spec` = {`reference-type` '`anchor`' [`reference-scope` ['`sheet-name`'] ]}

`reference-type` = `cell` | `cells` | `column`

`reference-scope` = `column` | `row` | `all`

`sheet-name` = string


### Formulas behavior explanation
**Note:** current cell is cell for which formula is compiled at current time.

Different cases for formulas:
* `cell 'id'` - compiles to address of first cell found with this identifier
  * `cell 'id' column` - search first cell with identifier in the same column as the current cell
  * `cell 'id' row` - search first cell with identifier in the same row as the current cell
  * `cell 'id' all` - search first cell with identifier in the same sheet as the current cell
  * `cell 'id' column|row|all 'sheet1'` - search first cell with identifier in the same column|row as current cell  or in whole sheet (`all`), named `sheet1` (sheet address generated too)

* `cells 'id'` - compiles to comma-separated list of cell addresses found with this identifier
  * `cells 'id' column` - search all cells with identifier in the same column as the current cell
  * `cells 'id' row` - search all cells with identifier in the same row as the current cell
  * `cells 'id' all` - search all cells with identifier in the same sheet as the current cell
  * `cells 'id' column|row|all 'sheet1'` - search all cells with identifier in the same column|row as current cell  or in whole sheet (`all`), named `sheet1` (sheet address generated too)

* `column 'id'` - compiles to address of column range, containing first cell found with this identifier (f.e., if cell is D4, column range is D:D)
  * `column 'id'` - search first cell with identifier and return column range for it
  * `column 'id' 'sheet1'` - search first cell with identifier on sheet named `sheet1` and return column range for it (sheet address generated too)

### Usage examples
* Generate IF formula according to value of cell in the same row
  * Report items are textboxes txtA, txtB and txtC in table row.
  * txtA has anchor `comp_value`.
  * txtB has anchor `output_value`.
  * txtC has formula `IF({cell 'comp_value' row} > 0, {cell 'output_value' row}, "N/A")`.
  * Result is formula `IF(A1 > 0, B1, "N/A")` for first row, row numbers 2 for second row, 3 for third and so on.
  * The same is for `column` and `all` scopes.

* Generate SUM formula for few cells in the same row
  * Report items are textboxes txtA, txtB, txtC, txtD in table row.
  * txtA, txtB, txtC have anchors `sum_value`.
  * txtD has formula `SUM({cells 'sum_value' row})`.
  * Result is formula `SUM(A1, B1, C1)` for first row, row numbers 2 for second row, 3 for third and so on.
  * The same is for `column` and `all` scopes.

* Generate SUM formula for cells column range
  * Report items are textboxes txtA in table row and separate textbox txtR.
  * txtA has anchor `column_value`.
  * txtR has formula `SUM({column 'column_value'})`.
  * Result is formula `SUM(A:A)`.

If add sheet part of formula, then cells on another sheet will be searched and references to them returned.

For more understanding how it works see unit-tests of extension, there are good examples covering all features.

## Troubleshooting
In case any formula compilation will fail, output Excel document will contain error message in failed cell for easy debugging. If after exporting report Excel cannot open document, saying that it is corrupted, then there are problems with formula, some symbols in it corrupting internal Excel XML markup. 
To get what's wrong with the document the next can be done
* Make copy of exported document, open it with Excel, agree for document restore and check what cell don't have formulas generated.
* Change extension of original document to zip (it's actually not XLSX, but ZIP archive with XML files) and check internal data for cell to see, what corrupting xml data, and make a fixes.

Most of failure cases should be handled by Formularizer engine, so if any such issue happens, feel free to post issue here, on Github.