using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Grammophone.TemplateRendering.RazorEngine
{
	/// <summary>
	/// Implements the <see cref="IRenderProvider"/> using the RazorEngine library.
	/// It is thread-save as required by the interface contract.
	/// Thus it should be configured for a singleton lifetime.
	/// </summary>
	public class RazorEngineRenderProvider : IRenderProvider
	{
		#region Private fields

		/// <summary>
		/// The RazorEngine service.
		/// </summary>
		private IRazorEngineService razorEngineService;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="templateFolderRoots">
		/// A collection of folder roots to search for Razor templates.
		/// These can be relative paths, which will be transformed to absolute
		/// based on the application's root.
		/// </param>
		public RazorEngineRenderProvider(IEnumerable<string> templateFolderRoots)
		{
			if (templateFolderRoots == null) throw new ArgumentNullException(nameof(templateFolderRoots));

			templateFolderRoots = templateFolderRoots.Select(f => NormalizePath(f));

			Initialize(templateFolderRoots);
		}

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="templateFolderRoot">
		/// A folder root to search for Razor templates.
		/// It can be relative a path, which will be transformed to absolute
		/// based on the application's root.
		/// </param>
		public RazorEngineRenderProvider(string templateFolderRoot)
		{
			if (templateFolderRoot == null) throw new ArgumentNullException(nameof(templateFolderRoot));

			templateFolderRoot = NormalizePath(templateFolderRoot);

			Initialize(Enumerable.Repeat(templateFolderRoot, 1));
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The list of folders to search for Razor templates.
		/// </summary>
		/// <remarks>
		/// These folder names are always absolute. If relative folders were passed in
		/// the contstructor, these are converted to absolute, starting from the
		/// application's root.
		/// </remarks>
		public IEnumerable<string> TemplateFolderRoots { get; private set; }

		#endregion

		#region public methods

		/// <summary>
		/// Cleanup the engine and attempt to remove temporary files.
		/// </summary>
		public void Dispose()
		{
			this.razorEngineService.Dispose();
		}

		/// <summary>
		/// Render a template.
		/// </summary>
		/// <param name="templateKey">
		/// The relative to <see cref="TemplateFolderRoots"/> filename 
		/// of the Razor template, preferably without the .cshtml or .vbhtml 
		/// suffix.
		/// </param>
		/// <param name="textWriter">
		/// The writer used for output.
		/// </param>
		/// <param name="dynamicProperties">
		/// The items in the dictionary become properties of the ViewBag.
		/// </param>
		public void Render(string templateKey, TextWriter textWriter, IDictionary<string, object> dynamicProperties)
		{
			var engineKey = this.razorEngineService.GetKey(templateKey);

			this.razorEngineService.RunCompile(engineKey, viewBag: new DynamicViewBag(dynamicProperties));
		}

		/// <summary>
		/// Render a template using a strong-type <paramref name="model"/>.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="templateKey">
		/// The relative to <see cref="TemplateFolderRoots"/> filename 
		/// of the Razor template, preferably without the .cshtml or .vbhtml 
		/// suffix.
		/// </param>
		/// <param name="textWriter">
		/// The writer used for output.
		/// </param>
		/// <param name="model">The object to be set as Model.</param>
		/// <param name="dynamicProperties">
		/// If present, the items in the dictionary become properties of the ViewBag.
		/// </param>
		public void Render<M>(string templateKey, TextWriter textWriter, M model, IDictionary<string, object> dynamicProperties = null)
		{
			var engineKey = this.razorEngineService.GetKey(templateKey);

			this.razorEngineService.RunCompile(
				engineKey,
				modelType: typeof(M),
				model: model,
				viewBag: dynamicProperties != null ? new DynamicViewBag(dynamicProperties) : null);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// If <paramref name="pathName"/> is a relative one, it converts it
		/// to absolute by combining it with the application's root.
		/// </summary>
		/// <param name="pathName">The path name.</param>
		/// <returns>Returns the normalized path name.</returns>
		private static string NormalizePath(string pathName)
		{
			// Is this an absolute or a relative path?
			if (!Path.IsPathRooted(pathName))
			{
				// If not, translate it according to the AppDomain's base. 
				pathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathName);
			}

			return pathName;
		}

		/// <summary>
		/// Finishes constructor work.
		/// </summary>
		private void Initialize(IEnumerable<string> templateFolderRoots)
		{
			if (templateFolderRoots == null) throw new ArgumentNullException(nameof(templateFolderRoots));

			this.TemplateFolderRoots = templateFolderRoots;

			var configuration = new TemplateServiceConfiguration()
			{
				TemplateManager = new ResolvePathTemplateManager(templateFolderRoots),
			};

			this.razorEngineService = RazorEngineService.Create(configuration);
		}

		#endregion
	}
}
