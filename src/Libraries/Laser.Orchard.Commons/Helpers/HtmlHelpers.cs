using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using System.Web.UI.HtmlControls;

namespace Laser.Orchard.Commons.Helpers {
    public static class HtmlHelpers {

        public static IHtmlString MakeSortable(this HtmlHelper htmlHelper, string containerSelector, string positionFieldSelector) {
            var jsSource = "        //<![CDATA[" + "\r\n" +
            "(function ($) {" + "\r\n" +
            "    $(\"" + containerSelector + "\").sortable({" + "\r\n" +
            "        start: function (event, ui) {" + "\r\n" +
            "            ui.item.startPos = ui.item.index();" + "\r\n" +
            "        }," + "\r\n" +
            "        update: function (event, ui) {" + "\r\n" +
            "           var positionElements = $(ui.item.context.parentElement).find('"+positionFieldSelector+"');" + "\r\n" +
            "           var index = 0;" + "\r\n" +
            "           positionElements.each(function () {" + "\r\n" +
            "               if ($(this).parent().is(':visible')) {" + "\r\n" +
            "                   $(this).val(index);" + "\r\n" +
            "                   index++;" + "\r\n" +
            "               }" + "\r\n" +
            "           });" + "\r\n" +
            "        }" + "\r\n" +
            "    });" + "\r\n" +
            "})(jQuery);" + "\r\n" +
            "//]]>" + "\r\n";
            TagBuilder tb = new TagBuilder("script");
            tb.Attributes.Add("type", "text/javascript");
            tb.InnerHtml = jsSource;
            var tag = tb.ToString(TagRenderMode.Normal);
            return MvcHtmlString.Create(tag);
        }
        public static MvcHtmlString LinkToAddNestedForm<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string linkText, string containerElement, string counterElement, string cssClass = null, Dictionary<string, object> DefaultPropertyValues = null, string codetoexecuteafter = "") where TProperty : IEnumerable<object> {
            // a fake index to replace with a real index
            var guid = Guid.NewGuid().ToString();
            // pull the name and type from the passed in expression
            string collectionProperty = ExpressionHelper.GetExpressionText(expression);
            var nestedObject = Activator.CreateInstance(typeof(TProperty).GetGenericArguments()[0]);
            if (DefaultPropertyValues != null) {
                try {
                    foreach (var dic in DefaultPropertyValues) {
                        nestedObject.GetType().GetProperty(dic.Key).SetValue(nestedObject, dic.Value, null);
                    }
                } catch (Exception) { }
            }  // save the field prefix name so we can reset it when we're doing
            string oldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;
            // if the prefix isn't empty, then prepare to append to it by appending another delimiter
            if (!string.IsNullOrEmpty(oldPrefix)) {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix += ".";
            }
            // append the collection name and our fake index to the prefix name before rendering
            htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix += string.Format("{0}[{1}]", collectionProperty, guid.ToString());
            string partial = htmlHelper.EditorFor(x => nestedObject).ToHtmlString();


            // done rendering, reset prefix to old name
            htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;



            // strip out the fake name injected in (our name was all in the prefix)
            partial = Regex.Replace(partial, @"[\._]?nestedObject", "");



            // encode the output for javascript since we're dumping it in a JS string
            partial = HttpUtility.JavaScriptStringEncode(partial);



            // create the link to render
            if (codetoexecuteafter.ToString().Length > 0)
                if (!codetoexecuteafter.EndsWith(";"))
                    codetoexecuteafter += ";";

            var js = string.Format("javascript:addNestedForm('{0}','{1}','{2}','{3}');" + codetoexecuteafter + "return false;", containerElement, counterElement, guid.ToString(), partial);
            TagBuilder a = new TagBuilder("a");
            a.Attributes.Add("href", "javascript:void(0)");
            a.Attributes.Add("onclick", js);
            if (cssClass != null) {
                a.AddCssClass(cssClass);
            }
            a.InnerHtml = linkText;



            return MvcHtmlString.Create(a.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString UniqueClientId(this HtmlHelper htmlHelper, string FieldName) {
            return MvcHtmlString.Create(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(FieldName));
        }
        public static IHtmlString LinkToRemoveNestedForm(this HtmlHelper htmlHelper, string linkText, string container, string deleteElement) {
            return LinkToRemoveNestedForm(htmlHelper, linkText, container, deleteElement, "");
        }
        public static IHtmlString LinkToRemoveNestedForm(this HtmlHelper htmlHelper, string linkText, string container, string deleteElement, string codetoexecuteafter = "") {
            if (codetoexecuteafter.ToString().Length > 0)
                if (!codetoexecuteafter.EndsWith(";"))
                    codetoexecuteafter += ";";
            var js = string.Format("javascript:removeNestedForm(this,'{0}','{1}');" + codetoexecuteafter + "return false;", container, deleteElement);
            TagBuilder tb = new TagBuilder("a");
            tb.Attributes.Add("href", "#");
            tb.Attributes.Add("onclick", js);
            tb.InnerHtml = linkText;
            var tag = tb.ToString(TagRenderMode.Normal);
            return MvcHtmlString.Create(tag);
        }
        public static IHtmlString StripTag(this HtmlHelper htmlHelper, string text, string tag) {
            var result = Regex.Replace(text, String.Format("<{0}[^>]*>(.*)</{0}>",tag), "");
            return MvcHtmlString.Create(result);
        }
        public static IHtmlString StripAttributes(this HtmlHelper htmlHelper, string text, string[] attributes) {
            var result = Regex.Replace(text, String.Format("({0})\\s*=\\s*\"[^\"]*\"", String.Join("|", attributes)), "");
            return MvcHtmlString.Create(result);
        }
        private static string JsEncode(this string s) {
            if (string.IsNullOrEmpty(s)) return "";
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            string t;
            for (i = 0; i < len; i += 1) {
                char c = s[i];
                switch (c) {
                    case '>':
                    case '"':
                    case '\\':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        //sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        //sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ') {
                            //t = "000" + Integer.toHexString(c); 
                            string tmp = new string(c, 1);
                            t = "000" + int.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
