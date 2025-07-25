using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace YourProjectNamespace.Extensions
{
    public static class ViewRenderingExtensions
    {
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller,
            string viewName, TModel model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            var viewEngine = controller.HttpContext.RequestServices
                .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            var tempDataProvider = controller.HttpContext.RequestServices
                .GetService(typeof(ITempDataProvider)) as ITempDataProvider;

            using (var writer = new StringWriter())
            {
                var viewResult = viewEngine.FindView(
                    controller.ControllerContext,
                    viewName,
                    false);

                if (!viewResult.Success)
                {
                    throw new InvalidOperationException(
                        $"Couldn't find view '{viewName}'");
                }

                var viewDictionary = new ViewDataDictionary<TModel>(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = model
                };

                var tempData = new TempDataDictionary(
                    controller.HttpContext,
                    tempDataProvider);

                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    viewDictionary,
                    tempData,
                    writer,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return writer.ToString();
            }
        }
    }
}