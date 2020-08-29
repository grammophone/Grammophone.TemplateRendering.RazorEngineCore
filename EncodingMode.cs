using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.TemplateRendering.RazorEngine
{
	/// <summary>
	/// Specifies how to encode strings generated from templates.
	/// </summary>
	public enum EncodingMode
	{
		/// <summary>
		/// The generated strings are to be escaped according to HTML rules.
		/// </summary>
		Html,

		/// <summary>
		/// The generated strings are kept unchanged, with no escapings.
		/// </summary>
		RawText
	}
}
