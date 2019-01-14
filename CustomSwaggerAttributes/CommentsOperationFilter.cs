namespace SwaggerAttributes.CustomSwaggerAttributes
{
    using System.Linq;
    using System.Text;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class CommentsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var commentAttributes = context.ApiDescription
                .ActionAttributes()
                .OfType<SwaggerCommentAttribute>()
                .ToList();

            if (!commentAttributes.Any())
            {
                return;
            }

            if (commentAttributes.Any())
            {
                var commentsSummary = new StringBuilder();
                var comments = commentAttributes.Select(attr => attr.Comment.Trim()).ToList();
                operation.Summary += commentsSummary.Append($"{string.Join("\n", comments)}").ToString();
            }
        }
    }
}
