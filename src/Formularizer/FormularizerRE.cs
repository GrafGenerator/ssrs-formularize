using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.ReportingServices.Interfaces;
using Microsoft.ReportingServices.OnDemandReportRendering;
using Microsoft.ReportingServices.Rendering.ExcelOpenXmlRenderer;
using Report = Microsoft.ReportingServices.OnDemandReportRendering.Report;

namespace Formularizer
{
	public class FormularizerRe : IRenderingExtension
	{
		public void SetConfiguration(string configuration)
		{
		}

		public string LocalizedName
		{
			get { return "Excel"; }
		}

		public bool Render(Report report, NameValueCollection reportServerParameters, NameValueCollection deviceInfo,
			NameValueCollection clientCapabilities, ref Hashtable renderProperties,
			CreateAndRegisterStream createAndRegisterStream)
		{
			var excelRe = new ExcelOpenXmlRenderer();
			var tempStream = new ManuallyClosableMemoryStream();

			const string finalExtension = "xlsx";
			const string finalMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			const string formularizerMark = "formularize";

			excelRe.Render(report, reportServerParameters, deviceInfo, clientCapabilities, ref renderProperties,
				(name, extension, encoding, type, seek, operation) =>
					finalExtension.Equals(extension) &&
					finalMimeType.Equals(type)
						? tempStream
						: createAndRegisterStream(name, extension, encoding, type, seek, operation));

			var doPatch = report.CustomProperties.Any(
				cp => formularizerMark.Equals(cp.Name.Value, StringComparison.InvariantCultureIgnoreCase));

			if (doPatch)
			{
				tempStream.Position = 0;
				var excelDoc = SpreadsheetDocument.Open(tempStream, true);

				var patchResult = new Core.Formularizer(excelDoc).Patch();
				if (patchResult.Failure)
				{
					throw new InvalidOperationException(string.Format("Patching of XLSX document failed: {0}", patchResult.Error));
				}

				excelDoc = patchResult.Value;
				excelDoc.Close();
			}

			var finalStream = createAndRegisterStream(report.Name, finalExtension, null, finalMimeType, false,
				StreamOper.CreateAndRegister);

			tempStream.Position = 0;
			tempStream.WriteTo(finalStream);
			tempStream.CanClose = true;
			tempStream.Close();

			return false;
		}

		public void GetRenderingResource(CreateAndRegisterStream createAndRegisterStreamCallback,
			NameValueCollection deviceInfo)
		{
		}

		public bool RenderStream(string streamName, Report report, NameValueCollection reportServerParameters,
			NameValueCollection deviceInfo, NameValueCollection clientCapabilities, ref Hashtable renderProperties,
			CreateAndRegisterStream createAndRegisterStream)
		{
			return false;
		}
	}
}