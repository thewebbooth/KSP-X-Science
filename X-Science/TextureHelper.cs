using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ScienceChecklist {
	/// <summary>
	/// Contains static methods to assist in creating textures.
	/// </summary>
	internal static class TextureHelper {
		/// <summary>
		/// Creates a new Texture2D from an embedded resource.
		/// </summary>
		/// <param name="resource">The location of the resource in the assembly.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <returns></returns>
		public static Texture2D FromResource (string resource, int width, int height) {
			var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
			var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource).ReadToEnd();
			tex.LoadImage(iconStream);
			tex.Apply();
			return tex;
		}
	}
}
