using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Web;
using System;

namespace Sitecore.Support.Resources.Media
{
  public class MediaProvider : Sitecore.Resources.Media.MediaProvider
  {
    protected readonly Data.ID VersionedNodeID = Data.ID.Parse("{CC68CD9B-28E4-4143-BA18-88863A6B917B}");

    /// <summary>
    /// Gets a media URL.
    /// </summary>
    /// <param name="item">The media item.</param>
    /// <param name="options">The query string.</param>
    /// <returns>The media URL.</returns>
    [NotNull]
    public override string GetMediaUrl([NotNull] MediaItem item, [NotNull] MediaUrlOptions options)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(options, "options");

      bool hasMediaContent = options.Thumbnail || this.HasMediaContent(item);

      if (!hasMediaContent && item.InnerItem["path"].Length > 0)
      {
        return options.LowercaseUrls ? item.InnerItem["path"].ToLowerInvariant() : item.InnerItem["path"];
      }

      if (options.UseDefaultIcon && !hasMediaContent)
      {
        return options.LowercaseUrls ? Themes.MapTheme(Settings.DefaultIcon).ToLowerInvariant() : Themes.MapTheme(Settings.DefaultIcon);
      }

      Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");
      string prefix = this.MediaLinkPrefix;

      if (options.AbsolutePath)
      {
        prefix = options.VirtualFolder + prefix;
      }
      else if (prefix.StartsWith("/", StringComparison.InvariantCulture))
      {
        prefix = StringUtil.Mid(prefix, 1);
      }

      prefix = MainUtil.EncodePath(prefix, '/');

      if (options.AlwaysIncludeServerUrl)
      {
        prefix = FileUtil.MakePath(string.IsNullOrEmpty(options.MediaLinkServerUrl) ? WebUtil.GetServerUrl() : options.MediaLinkServerUrl, prefix, '/');
      }

      string extension = StringUtil.GetString(options.RequestExtension, item.Extension, Constants.AshxExtension);

      extension = StringUtil.EnsurePrefix('.', extension);

      #region patch changes

      var innerItem = item.InnerItem;
      if (innerItem != null && innerItem.Template.InnerItem.ParentID == VersionedNodeID)
      {
        string parameters = options.ToString();

        if (parameters.Length > 0)
        {
          extension += "?" + parameters;
        }
      }

      #endregion

      string mediaRoot = Constants.MediaLibraryPath + "/";
      string itemPath = item.InnerItem.Paths.Path;

      string path;

      if (options.UseItemPath
        && itemPath.StartsWith(mediaRoot, StringComparison.OrdinalIgnoreCase))
      {
        path = StringUtil.Mid(itemPath, mediaRoot.Length);
      }
      else
      {
        path = item.ID.ToShortID().ToString();
      }

      path = MainUtil.EncodePath(path, '/');
      path = prefix + path + (options.IncludeExtension ? extension : string.Empty);
      return options.LowercaseUrls ? path.ToLowerInvariant() : path;
    }
  }
}