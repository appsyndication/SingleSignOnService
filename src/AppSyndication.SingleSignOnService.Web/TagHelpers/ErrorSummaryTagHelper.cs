using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AppSyndication.SingleSignOnService.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = ErrorSummaryModelAttributeName)]
    public class ErrorSummaryTagHelper : TagHelper
    {
        private const string ErrorSummaryModelAttributeName = "error-summary";
        private const string ErrorSummaryClass = "alert alert-danger";

        [HtmlAttributeName(ErrorSummaryModelAttributeName)]
        public ModelStateDictionary ErrorSummaryModel { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.ErrorSummaryModel.IsValid)
            {
                output.SuppressOutput();
                return;
            }

            var messages = this.GetErrorMessage().ToArray();

            if (!messages.Any())
            {
                messages = new [] { "Please contact support." };
            }

            var html = String.Join("<br/>", messages.Select(m => $"<strong>Error:</strong> {m}"));
            output.Content.AppendHtml(html);

            var classAttribute = output.Attributes["class"];
            var classValue = (classAttribute == null) ? ErrorSummaryClass : classAttribute.Value + " " + ErrorSummaryClass;
            output.Attributes.SetAttribute("class", classValue);
        }

        private IEnumerable<string> GetErrorMessage()
        {
            ModelStateEntry entry;

            if (!this.ErrorSummaryModel.TryGetValue(String.Empty, out entry) || !entry.Errors.Any())
            {
                return this.ErrorSummaryModel.Values.Where(e => e.Errors.Any()).SelectMany(e => e.Errors).Select(e => e.ErrorMessage);
            }

            return entry?.Errors.Select(e => e.ErrorMessage);
        }
    }
}
