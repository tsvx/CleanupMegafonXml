using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

static class Proga
{
	static XNamespace xmlns = "urn:schemas-microsoft-com:office:spreadsheet";
	static XNamespace xss = "urn:schemas-microsoft-com:office:spreadsheet";

	static void Main(string[] args)
	{
		var xpatterns = XDocument.Load("CleanupMegafonXml.config").Element("Patterns");
		CleanUpXml(args[0], xpatterns);
	}

	private static void CleanUpXml(string fname, XElement xpatterns)
	{
		var xdoc = XDocument.Load(fname);

		var xtable = xdoc.Element(xmlns + "Workbook").Element(xmlns + "Worksheet").Element(xmlns + "Table");
		var xaErc = xtable.Attribute(xss + "ExpandedRowCount");
		Console.WriteLine("ExpandedRowCount = " + (int)xaErc);
		var xrows = xtable.Elements(xmlns + "Row");
		int nrows = xrows.Count();
		Console.WriteLine("RowCount = {0}", nrows);

		var xpRowsToRemove = xpatterns.Element("RowsToRemove");
		var xeToDel = new List<XElement>();
		foreach (var xrow in xrows)
		{
			var xtd = GetListToDel(xrow, xpRowsToRemove);
			if (xtd != null)
			{
				xeToDel.AddRange(xtd);
			}
		}
		Console.WriteLine();
		Console.WriteLine("Found {0} rows to remove.", xeToDel.Count);
		xaErc.SetValue((int)xaErc - xeToDel.Count);
		xeToDel.Remove();
		nrows -= xeToDel.Count;
		Console.WriteLine("{0} rows left", nrows);		
		
		Console.WriteLine("Non-data rows:");
		var xpDataRow = xpatterns.Element("DataRow");
		int i = 0, ndr = 0;
		foreach (var xrow in xrows)
		{
			i++;
			if (!xrow.MatchesRowPattern(xpDataRow))
			{
				Console.Write(" {0}", i);
				ndr++;
			}
		}
		Console.WriteLine();
		Console.WriteLine("{0} non-data rows found", ndr);

		string outname = Path.ChangeExtension(fname, "cleaned.xml");
		//var xsettings = new XmlWriterSettings() { Indent = true, IndentChars = "" };
		//using (var xw = XmlWriter.Create(outname, xsettings))
		//	xdoc.Save(xw);
		xdoc.Save(outname, SaveOptions.OmitDuplicateNamespaces);
		Console.WriteLine("Saved to " + outname);
	}

	private static IEnumerable<XElement> GetListToDel(XElement xrow, XElement xpattern)
	{
		var toDel = new List<XElement>();
		int rid = 1;
		foreach (var xrp in xpattern.Elements("Row"))
		{
			int rpid = (int)xrp.Attribute("id");
			if (rid > rpid)
				throw new InvalidDataException("rpid < rid");
			while (rid < rpid)
			{
				xrow = xrow.NextNode as XElement;
				if (xrow == null || xrow.Name.Namespace != xmlns || xrow.Name.LocalName != "Row")
					return null;
				rid++;
			}
			if (!xrow.MatchesRowPattern(xrp))
				return null;
			toDel.Add(xrow);
		}
		return toDel;
	}

	static bool MatchesRowPattern(this XElement xrow, XElement xRowPattern)
	{
		XElement xcell = xrow.Element(xmlns + "Cell");
		foreach (var xcp in xRowPattern.Elements("Cell"))
		{
			var xaCellCount = xcp.Attribute("count");
			int count = xaCellCount == null ? 1 : (int)xaCellCount;
			while (count-- > 0)
			{
				if (xcell == null || xcell.Name.Namespace != xmlns || xcell.Name.LocalName != "Cell" || !xcell.MatchesCellPattern(xcp))
					return false;
				xcell = xcell.NextNode as XElement;
			}
		}
		return xcell == null;
	}

	static bool MatchesCellPattern(this XElement xcell, XElement xcp)
	{
		var cellIsEmpty = (bool?)xcp.Attribute("empty");
		if (cellIsEmpty.HasValue)
		{
			if (cellIsEmpty.Value ^ !xcell.HasElements)
				return false;
		}
		var xcpData = xcp.Element("Data");
		if (xcpData != null)
		{
			var xdata = xcell.Element(xmlns + "Data");
			if (xdata == null)
				return false;
			var dataIsEmpty = (bool?)xcpData.Attribute("empty");
			if (dataIsEmpty.HasValue)
			{
				if (dataIsEmpty.Value ^ String.IsNullOrEmpty(xdata.Value))
					return false;
			}
			var dataEquals = (string)xcpData.Attribute("equals");
			if (dataEquals != null && xdata.Value != dataEquals)
				return false;
			var dataNotEquals = (string)xcpData.Attribute("notEquals");
			if (dataNotEquals != null && xdata.Value == dataNotEquals)
				return false;
			var dataStartsWith = (string)xcpData.Attribute("startsWith");
			if (dataStartsWith != null && !xdata.Value.StartsWith(dataStartsWith))
				return false;
			var dataType = (string)xcpData.Attribute("type");
			if (dataType != null && dataType != (string)xdata.Attribute(xss + "Type"))
				return false;
			var dataRegex = (string)xcpData.Attribute("regex");
			if (dataRegex != null && !Regex.IsMatch(xdata.Value, dataRegex, RegexOptions.Compiled))
				return false;
			var dataPrint = (bool?)xcpData.Attribute("print");
			if (dataPrint == true)
			{
				string val = xdata.Value;
				if (dataStartsWith != null)
					val = val.Substring(dataStartsWith.Length);
				Console.Write(val);
			}
		}
		return true;
	}
}