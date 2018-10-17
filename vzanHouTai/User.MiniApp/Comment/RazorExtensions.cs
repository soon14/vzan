using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Utility;

namespace User.MiniApp.Comment
{
    public static class RazorExtensions
    {
        #region 辅助方法

        /// <summary>
        /// 返回属性的名称和值
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static KeyValuePair<string, string> GetExpressionValue<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var key = ExpressionHelper.GetExpressionText(expression);
            TProperty value = htmlHelper.ViewData.Model == null ? default(TProperty) : MemberAccessor.Process(expression, htmlHelper.ViewData.Model);
            return new KeyValuePair<string, string>(key, Convert.ToString(value));
        }

        private static ConcurrentDictionary<Type, List<KeyValuePair<string, string>>> dicEnum = new ConcurrentDictionary<Type, List<KeyValuePair<string, string>>>();

        private static List<KeyValuePair<string, string>> GetKeyValueList(Type enumType)
        {
            var kvList = new List<KeyValuePair<string, string>>();
            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            var underlyingType = Enum.GetUnderlyingType(enumType);

            foreach (var field in fields)
            {
                var v = field.GetValue(null);
                var value = Convert.ChangeType(v, underlyingType).ToString();
                var display = field.GetCustomAttributes(typeof(DisplayAttribute), true)
                        .Cast<DisplayAttribute>()
                        .FirstOrDefault();
                kvList.Add(new KeyValuePair<string, string>(display != null ? display.Name : field.Name, value));
            }
            return kvList;
        }

        #endregion

        #region SelecrFor

        public static MvcHtmlString SelectFor<TEnum, TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return SelectFor(htmlHelper, expression, typeof(TEnum));
        }

        public static MvcHtmlString SelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Type enumType, string nullValue = null, int isDisable = 0)
        {
            var keyValueList = dicEnum.GetOrAdd(enumType, GetKeyValueList);
            var modelProperty = GetExpressionValue(htmlHelper, expression);
            var idOrClass = modelProperty.Key.Replace(".", "_");
            var isDisableKey = string.Empty;
            if (isDisable > 0)
            {
                isDisableKey = "disabled";
            }

            var builder = new StringBuilder();
            builder.AppendFormat("<select name={0} id={1} class=\"form-control {1}\"  {2} >", modelProperty.Key, idOrClass, isDisableKey).AppendLine();
            if (!nullValue.IsNullOrEmpty())
                builder.AppendFormat("<option value=\"\">{0}</option>", nullValue).AppendLine();

            foreach (var pair in keyValueList)
            {
                builder.AppendFormat("<option {0} value={1}>{2}</option>", modelProperty.Value == pair.Value ? "selected=\"selected\"" : "", pair.Value, pair.Key);
            }
            builder.AppendLine("</select>");

            return new MvcHtmlString(builder.ToString());
        }

        public static MvcHtmlString SelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Type enumType, bool disabled, string nullValue = null, object htmlAttributes = null)
        {
            var keyValueList = dicEnum.GetOrAdd(enumType, GetKeyValueList);
            var modelProperty = GetExpressionValue(htmlHelper, expression);
            var idOrClass = modelProperty.Key.Replace(".", "_");

            var builder = new StringBuilder();
            builder.AppendFormat("<select name={0} id={1} class=\"form-control {1}\" ", modelProperty.Key, idOrClass).AppendLine();
            if (!htmlAttributes.IsNull())
            {
                var htmlAttrArray = htmlAttributes.GetType().GetProperties();
                foreach (var item in htmlAttrArray)
                {
                    var val = item.GetValue(htmlAttributes, null);
                    builder.AppendFormat(" {0}=\"{1}\" ", item.Name, val);
                }
            }
            builder.Append(">");
            if (!nullValue.IsNullOrEmpty())
                builder.AppendFormat("<option value=\"\">{0}</option>", nullValue).AppendLine();

            foreach (var pair in keyValueList)
            {
                if (disabled)
                {
                    builder.AppendFormat("<option {0} value={1} {3}>{2}</option>", modelProperty.Value == pair.Value ? "selected=\"selected\"" : "", pair.Value, pair.Key, "disabled=\"disabled\"");
                }
                else
                {
                    builder.AppendFormat("<option {0} value={1}>{2}</option>", modelProperty.Value == pair.Value ? "selected=\"selected\"" : "", pair.Value, pair.Key);
                }

            }
            builder.AppendLine("</select>");

            return new MvcHtmlString(builder.ToString());
        }
        /// <summary>
        /// 使用时候初始化一下， ComponentsDropdowns.init();
        /// </summary>
        public static MvcHtmlString SelectMultipleFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Type enumType, string nullValue = null, int isDisable = 0, object htmlAttributes = null)
        {
            var keyValueList = dicEnum.GetOrAdd(enumType, GetKeyValueList);
            var modelProperty = GetExpressionValue(htmlHelper, expression);
            var idOrClass = modelProperty.Key.Replace(".", "_");
            var isDisableKey = string.Empty;
            if (isDisable > 0)
            {
                isDisableKey = "disabled";
            }

            var builder = new StringBuilder();
            builder.AppendFormat("<select name={0} id={1} class=\"form-control multiple {1}\" multiple ", modelProperty.Key, idOrClass).AppendLine();

            if (!htmlAttributes.IsNull())
            {
                var htmlAttrArray = htmlAttributes.GetType().GetProperties();
                foreach (var item in htmlAttrArray)
                {
                    var val = item.GetValue(htmlAttributes, null);
                    builder.AppendFormat(" {0}=\"{1}\" ", item.Name, val);
                }
            }
            builder.Append(">");

            if (!nullValue.IsNullOrEmpty())
                builder.AppendFormat("<option value=\"\">{0}</option>", nullValue).AppendLine();

            foreach (var pair in keyValueList)
            {
                builder.AppendFormat("<option {0} value={1}>{2}</option>", modelProperty.Value == pair.Value ? "selected=\"selected\"" : "", pair.Value, pair.Key);
            }
            builder.AppendLine("</select>");

            return new MvcHtmlString(builder.ToString());
        }
        #endregion

        #region 数字输入框

        /// <summary>
        /// 数字输入框
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString NumberFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var modelProperty = GetExpressionValue(htmlHelper, expression);
            var builder = new StringBuilder();
            builder.AppendFormat("<input type=\"number\" id=\"{0}\" name=\"{0}\" value=\"{1}\" ", modelProperty.Key, modelProperty.Value);
            if (htmlAttributes != null)
            {
                var htmlAttrArray = htmlAttributes.GetType().GetProperties();
                foreach (var item in htmlAttrArray)
                {
                    var val = item.GetValue(htmlAttributes, null);
                    builder.AppendFormat(" {0}=\"{1}\" ", item.Name, val);
                }
            }
            builder.Append(" />");
            return new MvcHtmlString(builder.ToString());
        }
        #endregion

        #region 上传控件

        /// <summary>
        /// 上传控件
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="expression">获取图片地址元素的name值</param>
        /// <param name="maxFilesize">最大文件大小   默认1 单位M</param>
        /// <param name="maxFiles">最大文件数量   默认1个</param>
        /// <param name="btnText">按钮文字</param>
        /// <param name="controllerWidth">按钮宽度  单位 像素</param>
        /// <param name="initImageList">已有文件列表 格式 [{id:"xx",url:""xxx},{id:"xx",url:""xxx}]</param>
        /// <param name="removeCallback">移除文件回调事件</param>
        /// <returns></returns>
        public static MvcHtmlString FileUploadFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, int maxFilesize = 1, int maxFiles = 1, string btnText = "点击上传,可拖拽", int controllerWidth = 113, List<object> initImageList = null, string removeCallback = "",int awm=0,int cityid = 0, string vueName = "")
        {
            var kv = GetExpressionValue(helper, expression);
            var initImageListString = "[]";
            if (initImageList != null && initImageList.Count > 0)
            {
                initImageListString = JsonConvert.SerializeObject(initImageList);
            }
            return FileUploadHtmlString(kv.Key, maxFilesize, maxFiles, btnText, controllerWidth, initImageListString, removeCallback, awm,cityid, vueName);
        }

        /// <summary>
        /// 上传控件
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="imgsName">获取图片地址元素的name值</param>
        /// <param name="maxFilesize">最大文件大小   默认1 单位M</param>
        /// <param name="maxFiles">最大文件数量   默认1个</param>
        /// <param name="btnText">按钮文字</param>
        /// <param name="controllerWidth">按钮宽度  单位 像素</param>
        /// <param name="initImageList">已有文件列表 格式 [{id:"xx",url:""xxx},{id:"xx",url:""xxx}]</param>
        /// <param name="removeCallback">移除文件回调事件</param>
        /// <returns></returns>
        public static MvcHtmlString FileUploadFor(this HtmlHelper helper, string imgsName, int maxFilesize = 1, int maxFiles = 1, string btnText = "点击上传,可拖拽", int controllerWidth = 113, List<object> initImageList = null, string removeCallback = "",int awm=0,int cityid=0, string vueName = "")
        {
            var initImageListString = "[]";
            if (initImageList != null && initImageList.Count > 0)
            {
                initImageListString = JsonConvert.SerializeObject(initImageList);
            }
            return FileUploadHtmlString(imgsName, maxFilesize, maxFiles, btnText, controllerWidth, initImageListString, removeCallback,awm, cityid, vueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgsName"></param>
        /// <param name="maxFilesize"></param>
        /// <param name="maxFiles"></param>
        /// <param name="btnText"></param>
        /// <param name="controllerWidth"></param>
        /// <param name="initImageList"></param>
        /// <param name="removeCallback"></param>
        /// <param name="vueName">外部vue的图片计数器</param>
        /// <returns></returns>
        private static MvcHtmlString FileUploadHtmlString(string imgsName, int maxFilesize, int maxFiles, string btnText, int controllerWidth, string initImageList, string removeCallback,int awm,int cityid, string vueName = "")
        {
            var divId = "FileUpload_" + Guid.NewGuid().ToString();
            var builder = new StringBuilder();
            removeCallback = removeCallback.IsNullOrWhiteSpace() ? "jQuery" : removeCallback;
            builder.AppendLine("<div id='" + divId + "' class='dropzone' style='width: " + controllerWidth + "px;'></div>");
            //builder.AppendLine("var imgcount = 0;");
            builder.AppendLine("<script>");
            builder.AppendLine("$(document).ready(function() {");
            builder.AppendLine("var maxFilesize = " + maxFilesize + ";");
            builder.AppendLine("var maxFiles = " + maxFiles + ";");
            builder.AppendLine("var acceptedFiles = 'image/jpeg,image/png,image/gif';");
            builder.AppendLine("var imgsName = '" + imgsName + "';");
            builder.AppendLine("var imageList = " + initImageList + ";");

            builder.AppendLine("$('div#" + divId + "').dropzone({");
            builder.AppendLine("url: '/Upload/UploadImg?awm="+ awm + "&cityid="+cityid+"',");
            builder.AppendLine("addRemoveLinks: true,");
            builder.AppendLine("dictRemoveFile: '',");
            builder.AppendLine("maxFilesize: maxFilesize,");
            builder.AppendLine("maxFiles: maxFiles,");
            builder.AppendLine("acceptedFiles: acceptedFiles,");
            builder.AppendLine("dictDefaultMessage: '" + btnText + "',");
            builder.AppendLine("dictFileTooBig: '文件大小超过 ' + maxFilesize + ' M',");
            builder.AppendLine("dictMaxFilesExceeded: '文件数量超过 ' + maxFiles + ' 个',");
            builder.AppendLine("dictInvalidInputType: '不支持此类型文件',");
            builder.AppendLine("dictFallbackText: '',");
            builder.AppendLine("success: function(file, result) {");
            builder.AppendLine("if (result && result.Success)");
            builder.AppendLine("{");
            builder.AppendLine("$(file.previewTemplate).append('<input type=\"hidden\" value=\"' + result.Path + '\" name=\"' + imgsName + '\"/>');");
            if (vueName != "")
            {
                builder.AppendLine($"{vueName}++");
            }
            builder.AppendLine("}");
            builder.AppendLine("else if (result)");
            builder.AppendLine("{");
            builder.AppendLine("AppTools.Alert('文件 ' + file.name + ' 上传失败 !');");
            builder.AppendLine("}");
            builder.AppendLine("},");
            builder.AppendLine("maxfilesexceeded: function(file) {");
            builder.AppendLine("bootbox.hideAll();");
            builder.AppendLine("AppTools.Alert('文件数量超过限制 ' + maxFiles + ' 个');");
            builder.AppendLine("if($('#label_'+maxFiles + '_'+ imgsName).length==0)");
            builder.AppendLine("{");
            builder.AppendLine("$(file.previewTemplate.parentNode.parentNode).append('<label id=\"label_'+maxFiles+'_'+ imgsName +'\" style=\"color:red;\" >文件数量已满最大限制'+maxFiles+'个</label>');");
            builder.AppendLine("}");
            builder.AppendLine("this.removeFile(file);");
            builder.AppendLine("},");
            builder.AppendLine("init: function() {");
            builder.AppendLine("this.emit('initimage', imageList);");
            builder.AppendLine("this.on('success', function(file) {");
            builder.AppendLine("//console.log('File ' + file.name + 'uploaded');");
            builder.AppendLine("});");
            builder.AppendLine("this.on('removedfile', function(file) {");
            builder.AppendLine("if (typeof(" + removeCallback + ") == 'function')");
            builder.AppendLine("{");
            builder.AppendLine(removeCallback + "(file);");
            builder.AppendLine("if (file.status == 'success')");
            builder.AppendLine("{");
            builder.AppendLine("$('#label_'+maxFiles+'_'+ imgsName).remove();");
            if (vueName != "")
            {
                builder.AppendLine($"{vueName}--");
            }
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine("});");
            builder.AppendLine("}");
            builder.AppendLine("});");
            builder.AppendLine("});");

            builder.AppendLine("</script>");

            return new MvcHtmlString(builder.ToString());
        }
        #endregion

    }
}