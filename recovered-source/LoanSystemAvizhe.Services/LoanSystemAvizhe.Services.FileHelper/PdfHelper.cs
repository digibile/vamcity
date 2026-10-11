using System;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace LoanSystemAvizhe.Services.FileHelper;

public class PdfHelper : IPdfHelper
{
	private readonly IConverter _converter;

	public PdfHelper(IConverter converter)
	{
		_converter = converter;
	}

	public void ConvertHtmlToPdfAsync(string html, string outputPath)
	{
		try
		{
			HtmlToPdfDocument document = new HtmlToPdfDocument
			{
				GlobalSettings = 
				{
					PaperSize = PaperKind.A4,
					Orientation = Orientation.Portrait
				},
				Objects = 
				{
					new ObjectSettings
					{
						HtmlContent = html,
						WebSettings = 
						{
							DefaultEncoding = "utf-8"
						}
					}
				}
			};
			byte[] bytes = _converter.Convert(document);
			File.WriteAllBytes(outputPath, bytes);
		}
		catch (Exception)
		{
			throw;
		}
	}
}
