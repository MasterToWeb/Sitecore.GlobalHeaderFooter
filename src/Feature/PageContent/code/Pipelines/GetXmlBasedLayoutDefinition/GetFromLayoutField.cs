using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
using Sitecore.Mvc.Presentation;
using Sitecore.Sites;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using System.Web.Caching;
using Sitecore.Mvc.Extensions;

namespace MasterToWeb.Feature.PageContent.Pipelines.GetXmlBasedLayoutDefinition
{
    /// <summary>
    /// The processor replaces the out of the box processor, but retrieves the layout the same way.  
    /// However, it looks to see if the page utilizes a global header/footer, and injects the presentation
    /// from a globally defined setting or an individual page setting into the presentation XML.
    /// </summary>
    public class GetFromLayoutField : GetXmlBasedLayoutDefinitionProcessor
    {
        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            if (args.Result != null || PageContext.Current.Item == null) return;

            SiteContext siteInfo = SiteContext.Current;
            if (siteInfo == null) return;

            string key = string.Format("LayoutXml.{0}.{1}",
                Context.Language.Name,
                PageContext.Current.Item.ID);

            //TODO: Below commented code would enable caching of the results for performance.  
            //TODO: Currently disabled, but should be validated once the actual header components are added.

            //XElement pageLayoutXml = (XElement)HttpContext.Current.Cache[key];
            //if (pageLayoutXml != null && Context.PageMode.IsNormal)
            //{
            //    args.Result = pageLayoutXml;
            //    return;
            //}

            XElement content = GetFromField(PageContext.Current.Item);
            if (content != null && (Context.PageMode.IsPreview || Context.PageMode.IsNormal))
            {
                Item item = PageContext.Current.Item;

                if (item != null &&
                    (item.DescendsFrom(Constants.Templates.HasGlobalStructure.ID)) &&
                    !item.TemplateID.Equals(Constants.Templates.HeaderPage.ID) &&
                    !item.TemplateID.Equals(Constants.Templates.FooterPage.ID))
                {
                    XElement currentPageDxElement = content.Element("d");
                    if (currentPageDxElement == null)
                    {
                        args.Result = content;
                        return;
                    }

                    //if item uses global page structure, pull settings from root item
                    Item rootItem = Context.Database.GetItem(siteInfo.RootPath);

                    //header
                    Item headerItem = MainUtil.GetBool(item[Constants.Templates.HasGlobalStructure.Fields.UseGlobalHeader], false)
                        ? rootItem
                        : item;

                    if (headerItem != null && headerItem.DescendsFrom(Constants.Templates.GlobalPageStructure.ID))
                    {
                        if (!string.IsNullOrEmpty(headerItem[Constants.Templates.GlobalPageStructure.Fields.HeaderItem]))
                        {
                            currentPageDxElement.Add(GetContent(headerItem[Constants.Templates.GlobalPageStructure.Fields.HeaderItem]));
                        }
                    }


                    //footer
                    Item footerItem = MainUtil.GetBool(item[Constants.Templates.HasGlobalStructure.Fields.UseGlobalFooter], false)
                       ? rootItem
                       : item;
                  
                    if (footerItem != null && footerItem.DescendsFrom(Constants.Templates.GlobalPageStructure.ID))
                    {
                        if (!string.IsNullOrEmpty(footerItem[Constants.Templates.GlobalPageStructure.Fields.FooterItem]))
                        {
                            currentPageDxElement.Add(GetContent(footerItem[Constants.Templates.GlobalPageStructure.Fields.FooterItem]));
                        }
                    }
                }
            }

            try
            {
                if (Context.PageMode.IsPreview || Context.PageMode.IsNormal)
                {
                    HttpContext.Current.Cache.Add(
                    key,
                    content,
                    null,
                    DateTime.Now.AddMinutes(5),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.AboveNormal,
                    null);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in GetFromLayoutField processor", ex, this);
            }

            args.Result = content;
        }
        /// <summary>
        /// From given Item's Layout extract the elements with given placeholder.
        /// </summary>
        /// <param name="fromItemId"></param>
        /// <param name="fromItemPlaceholder"></param>
        /// <returns></returns>
        private List<XElement> GetContent(string fromItemId)
        {
            if (string.IsNullOrEmpty(fromItemId))
            {
                return new List<XElement>();
            }

            XElement inputLayout = GetItemLayout(fromItemId);
            if (inputLayout != null)
            {
                XElement dXElementInInputLayout = inputLayout.Element("d");
                if (dXElementInInputLayout != null)
                {
                    return dXElementInInputLayout.Elements().ToList();
                }
            }

            return new List<XElement>();
        }

        /// <summary>
        /// Returns Layout Field Value for given ItemID
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private XElement GetItemLayout(string itemId)
        {
            Item currentItem = Context.Database.GetItem(itemId);
            if (currentItem != null)
            {
                return GetFromField(currentItem);
            }
            return null;
        }


        /// <summary>
        /// Gets the xml from the current items layout field
        /// </summary>
        /// <returns></returns>
        private XElement GetFromField(Item item)
        {
            if (item == null)
            {
                return null;
            }

            LayoutField layoutField = new LayoutField(item);
            if (layoutField == null)
            {
                return null;
            }

            string fieldValue = layoutField.Data.InnerXml;
            if (fieldValue.IsWhiteSpaceOrNull())
            {
                return null;
            }

            return XDocument.Parse(fieldValue).Root;
        }

        
    }
}