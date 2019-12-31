using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;

namespace Geomethod.Etl
{
	public class UnexpectedQuotationStatusException : GeomethodEtlException { public UnexpectedQuotationStatusException(string msg) : base(msg) { } }

	public class CsvConverter
	{
		enum QuotationStatus{None,First,Second};

        #region Fields
        readonly char sepToken;
		const char quoteToken='"';
		static readonly string quoteTokenStr=quoteToken.ToString();
		static readonly string quoteTokenDblStr=quoteTokenStr+quoteTokenStr;
		#endregion

		#region Properties
		public char Separator { get { return sepToken; } }
		#endregion

		#region Construction
		public CsvConverter():this(';')
		{
		}
		public CsvConverter(char separator)
		{
			this.sepToken=separator;
		}
		#endregion

		#region Converting
		public bool Parse(StreamReader sr, List<string> sc)
		{
			sc.Clear();
			if(sr.EndOfStream) return false;
			QuotationStatus quotationStatus = QuotationStatus.None;
			string s=sr.ReadLine();
			int startPos = 0;
			for (int i = 0; ; i++)
			{
				if (i >= s.Length)
				{
					if (sr.EndOfStream) break;
					if (quotationStatus == QuotationStatus.First)
					{
						s += sr.ReadLine();// "\r\n"+
						i--;
						continue;
					}
					break;
				}
				char c = s[i];
				switch (quotationStatus)
				{
					case QuotationStatus.None:
						if (i == startPos && c == quoteToken)
						{
							quotationStatus = QuotationStatus.First;
						}
						else
						{
							if (c == sepToken)
							{
								sc.Add(s.Substring(startPos, i - startPos));
								startPos = i + 1;
							}
						}
						break;
					case QuotationStatus.First:
						if (c == quoteToken) quotationStatus = QuotationStatus.Second;
						break;
					case QuotationStatus.Second:
						if (c == sepToken)
						{
							sc.Add(ReplaceQuotes(s.Substring(startPos + 1, i - startPos - 2)));
							quotationStatus = QuotationStatus.None;
							startPos = i + 1;
						}
						else quotationStatus = QuotationStatus.First;
						break;
					default:
						UnexpectedQuotationStatus(quotationStatus, s);
						break;
				}
			}
			switch (quotationStatus)
			{
				case QuotationStatus.Second:
					sc.Add(ReplaceQuotes(s.Substring(startPos + 1, s.Length - startPos - 2)));
					break;
				case QuotationStatus.First:
					sc.Add(ReplaceQuotes(s.Substring(s.Length - startPos - 1)));
					break;
				case QuotationStatus.None:
					sc.Add(s.Substring(startPos));
					break;
				default:
					UnexpectedQuotationStatus(quotationStatus, s);
					break;
			}
			return true;
		}
		public void Parse(string s, List<string> sc)
		{
			sc.Clear();
			if(s.Length==0) return;
			QuotationStatus quotationStatus=QuotationStatus.None;
			int startPos=0;
			for(int i=0;i<s.Length;i++)
			{
				char c=s[i];
				switch(quotationStatus)
				{
					case QuotationStatus.None:
						if(i==startPos && c==quoteToken)
						{
							quotationStatus=QuotationStatus.First;
						}
						else
						{
							if(c==sepToken)
							{
								sc.Add(s.Substring(startPos,i-startPos));
								startPos=i+1;
							}
						}
						break;
					case QuotationStatus.First:
						if(c==quoteToken) quotationStatus=QuotationStatus.Second;
						break;
					case QuotationStatus.Second:
						if(c==sepToken)
						{
							sc.Add(ReplaceQuotes(s.Substring(startPos+1,i-startPos-2)));
							quotationStatus=QuotationStatus.None;
							startPos=i+1;
						}
						else quotationStatus=QuotationStatus.First;
						break;
					default: 
						UnexpectedQuotationStatus(quotationStatus,s);
						break;
				}
			}
			switch(quotationStatus)
			{
				case QuotationStatus.Second:
					sc.Add(ReplaceQuotes(s.Substring(startPos+1,s.Length-startPos-2)));
					break;
				case QuotationStatus.First:
					sc.Add(ReplaceQuotes(s.Substring(s.Length-startPos-1)));
					break;
				case QuotationStatus.None:
					sc.Add(s.Substring(startPos));
					break;
				default: 
					UnexpectedQuotationStatus(quotationStatus, s); 
					break;
			}
		}
		public string ToCsvString(string s)
		{
			if(s==null) return "";
			bool quotation=false;
			if(s.IndexOf('\n')>=0) quotation=true;
			if(s.IndexOf('\r')>=0) quotation=true;
			if(s.IndexOf(sepToken)>=0) quotation=true;			
			if(s.IndexOf(quoteToken)>=0)
			{
				s=s.Replace(quoteTokenStr,quoteTokenDblStr);
			}
			if(quotation) s=quoteToken+s+quoteToken;
			return s;
		}
		public string ToCsvLine(IEnumerable<string> sc)
		{
			StringBuilder sb = new StringBuilder(64);
			AppendCsvLine(sb, sc);
			return sb.ToString();
		}
		public void AppendCsvLine(StringBuilder sb, IEnumerable<string> sc)
		{
			bool first = true;
			foreach (string s in sc)
			{
				if (first) first = false;
				else sb.Append(sepToken);
				sb.Append(ToCsvString(s));
			}
		}
		#endregion

		#region Aux
		void UnexpectedQuotationStatus(QuotationStatus qs, string s)
		{
			throw new UnexpectedQuotationStatusException(string.Format("Unexpected QuotationStatus {0} in the string: {1}", qs, s));
		}
		string ReplaceQuotes(string s)
		{
			return s.Replace(quoteTokenDblStr, quoteTokenStr);
		}
		#endregion

	}
}
